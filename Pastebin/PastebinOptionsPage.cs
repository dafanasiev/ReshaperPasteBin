using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.UI.Application;
using JetBrains.UI.CrossFramework;
using JetBrains.UI.Options;
using JetBrains.UI.Options.Helpers;
using JetBrains.UI.Options.OptionPages;

namespace Pastebin
{
    [OptionsPage(Pid, "Pastebin Settings", null, ParentId = EnvironmentPage.Pid, Sequence = 100)]
    public class PastebinOptionsPage : AOptionsPage
    {
        public const string Pid = "PastebinSettings";

        public PastebinOptionsPage([NotNull] Lifetime lifetime, IUIApplication environment, OptionsSettingsSmartContext settings)
            : base(lifetime, environment, Pid)
        {
            if (lifetime == null)
                throw new ArgumentNullException("lifetime");

            TextBox apiKeBox;
            Control = InitView(out apiKeBox);

            settings.SetBinding(lifetime, (PastebinSettings s) => s.ApiKey, apiKeBox, TextBox.TextProperty);
        }

        private EitherControl InitView(out TextBox apiKeBox)
        {
            var grid = new Grid { Background = SystemColors.ControlBrush };

            var colDef1 = new ColumnDefinition { Width = GridLength.Auto };
            var colDef2 = new ColumnDefinition { Width = GridLength.Auto, MinWidth = 200 };
            grid.ColumnDefinitions.Add(colDef1);
            grid.ColumnDefinitions.Add(colDef2);

            // Define the Rows
            var rowDef1 = new RowDefinition { Height = GridLength.Auto };
            var rowDef2 = new RowDefinition { Height = GridLength.Auto };

            grid.RowDefinitions.Add(rowDef1);
            grid.RowDefinitions.Add(rowDef2);

            var header = new Label { Content = "Pastebin access" };
            Grid.SetColumn(header, 0);
            Grid.SetColumnSpan(header, 2);
            Grid.SetRow(header, 0);

            var apiKeyLabel = new Label { Content = "API Key:" };
            Grid.SetColumn(apiKeyLabel, 0);
            Grid.SetRow(apiKeyLabel, 1);

            apiKeBox = new TextBox();
            Grid.SetColumn(apiKeBox, 1);
            Grid.SetRow(apiKeBox, 1);

            ((IAddChild)grid).AddChild(header);
            ((IAddChild)grid).AddChild(apiKeyLabel);
            ((IAddChild)grid).AddChild(apiKeBox);

            return grid;
        }
    }
}