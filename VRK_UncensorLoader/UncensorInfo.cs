using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace VRK_Plugins
{
    internal class UncensorInfo
    {
        public UncensorType Type;
        public string Name;
        public string Author;

        public Dictionary<string, string> ReplacementBundles = new Dictionary<string, string>();

        public static UncensorInfo LoadFromFile(FileInfo infofile)
        {
            var xml = XDocument.Load(infofile.FullName);
            var root = xml.Root;

            var info = new UncensorInfo();
            info.Type = root.Element("Type").Value == "Male" ? UncensorType.Male : UncensorType.Female;
            info.Author = root.Element("Author")?.Value;
            info.Name = root.Element("Name").Value;

            var dirPath = infofile.DirectoryName;

            foreach (var replacementBundle in infofile.Directory.GetFiles("*.unity3d", SearchOption.AllDirectories))
            {
                var relativePath = replacementBundle.FullName.Substring(dirPath.Length).ToLower().Replace('/', '\\').TrimStart('\\');

                info.ReplacementBundles.Add(relativePath, replacementBundle.FullName);
            }

            return info;
        }
    }
}