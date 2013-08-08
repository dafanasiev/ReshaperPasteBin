using System.IO;

namespace PasteBinSharp
{
    public static class PasteBinUtil
    {
        public static string FormatFromFileName(string filename)
        {
            if (filename == null)
                return null;

            string extension = Path.GetExtension(filename);
            if (extension == null)
                return null;

            extension = extension.ToLower();
            switch (extension)
            {
                case ".cs":
                    return "csharp";
                case ".vb":
                    return "vbnet";
                case ".cpp":
                case ".hpp":
                case ".h":
                    return "cpp";
                case ".xml":
                case ".xaml":
                case ".settings":
                case ".config":
                    return "xml";
                default:
                    return null;
            }
        }
    }
}
