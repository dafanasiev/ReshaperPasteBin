using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Extensions.Settings;
using JetBrains.DocumentManagers;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.TextControl;
using JetBrains.UI;
using JetBrains.UI.PopupWindowManager;
using JetBrains.UI.RichText;
using JetBrains.UI.Tooltips;
using JetBrains.Util;
using JetBrains.Util.Logging;
using JetBrains.Util.Special;
using System.Linq;
using PasteBinSharp;


namespace Pastebin
{
    [ActionHandler("Pastebin.PastebinAction")]
    public class PastebinAction : IActionHandler
    {
        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {

            // return true or false to enable/disable this action
            return
              context.CheckAllNotNull(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION, JetBrains.ProjectModel.DataContext.DataConstants.PROJECT_MODEL_ELEMENTS) ||
              context.CheckAllNotNull(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION, JetBrains.DocumentModel.DataContext.DataConstants.DOCUMENT_SELECTION);
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            // insert code to execute when action is fired
            var solution = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION);
            if (solution == null)
                return;


            IDictionary<string, string> publishData = null;
            // Publish selected text
            var documentSelection = context.GetData(JetBrains.DocumentModel.DataConstants.DOCUMENT_SELECTION);
            if (documentSelection != null)
            {
                var filename = documentSelection.Document.GetPsiSourceFile(solution).IfNotNull(_ => _.Name);
                var text = documentSelection.Document.GetText(documentSelection.TextRange);
                if (!string.IsNullOrWhiteSpace(text))
                    publishData = new Dictionary<string, string> { { filename, text } };
            }

            // Publish selected files
            var projectModelElements = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.PROJECT_MODEL_ELEMENTS);
            if ((publishData == null) && (projectModelElements != null))
            {
                var documentManager = solution.GetComponent<DocumentManager>();

                publishData = projectModelElements
                  .OfType<IProjectFile>()
                  .Concat(projectModelElements.OfType<IProjectFolder>().SelectMany(_ => _.GetAllProjectFiles()))
                  .Distinct()
                  .ToDictionary(_ => _.Name, _ => documentManager.GetOrCreateDocument(_).GetText());
            }

            if (publishData == null) return;

            List<string> urls;
            List<Exception> errors;
            SafePublish(solution.GetComponent<PastebinService>().GetClient(context), publishData, out urls, out errors);

            var allUrls = string.Join(Environment.NewLine, urls);

            solution.GetComponent<Clipboard>().SetDataObject(allUrls);
            ShowTooltip(context, solution, new RichText("Url ").Append(new RichText(allUrls, TextStyle.FromForeColor(Color.Blue))).Append(" copied to clipboard", TextStyle.Default));

            if (errors != null)
            {
                var allErrors = string.Join(Environment.NewLine, errors.Select(x => x.Message));
                ShowTooltip(context, solution, new RichText("Error posting to Pastebin:" + allErrors, TextStyle.FromForeColor(Color.Red)));
            }
        }

        private static void ShowTooltip(IDataContext context, ISolution solution, RichText tooltip)
        {
            var shellLocks = solution.GetComponent<IShellLocks>();
            var tooltipManager = solution.GetComponent<ITooltipManager>();

            tooltipManager.Show(tooltip,
              lifetime =>
              {
                  var windowContextSource = context.GetData(JetBrains.UI.DataConstants.PopupWindowContextSource);

                  if (windowContextSource != null)
                  {
                      var windowContext = windowContextSource.Create(lifetime);
                      var ctxTextControl = windowContext as TextControlPopupWindowContext;
                      return ctxTextControl == null ? windowContext :
                        ctxTextControl.OverrideLayouter(lifetime, lifetimeLayouter => new DockingLayouter(lifetimeLayouter, new TextControlAnchoringRect(lifetimeLayouter, ctxTextControl.TextControl, ctxTextControl.TextControl.Caret.Offset(), shellLocks), Anchoring2D.AnchorTopOrBottom));
                  }

                  return solution.GetComponent<MainWindowPopupWindowContext>().Create(lifetime);
              });
        }

        [CanBeNull]
        private void SafePublish(PasteBinClient client, IDictionary<string, string> content, out List<string> urls, out List<Exception> errors)
        {
            urls = new List<string>();
            errors = null;
            foreach (var contentItem in content)
            {
                try
                {
                    var entry = new PasteBinEntry();

                    entry.Private = true; //TODO?
                    entry.Text = contentItem.Value;
                    entry.Title = contentItem.Key;
                    entry.Expiration = PasteBinExpiration.OneDay;
                    entry.Format = "csharp";
                    var retVal = client.Paste(entry);

                    urls.Add(retVal);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("Pastebin error: {0}", ex);
                    if (errors == null) errors = new List<Exception>();
                    errors.Add(ex);
                }
            }

        }

    }
}
