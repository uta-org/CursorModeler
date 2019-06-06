#define REFLECTION

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using TexturePacker.Lib;

namespace CursorModeler
{
    using Tests;

    public class Program
    {
        private const string ATLAS_URL = "https://gist.githubusercontent.com/z3nth10n/61389f7b6bf37d25ea4cc22eb57231ff/raw/77225b1e67da47630a1132ec369b53c0708a10cf/atlas.json";
        private const string Replace = "E:\\VISUAL STUDIO\\Visual Studio Projects\\TexturePacker\\bin\\Debug\\png\\";

        public static void Main()
        {
#if !REFLECTION
            string json;
            using (WebClient wc = new WebClient())
                json = wc.DownloadString(ATLAS_URL);

            var atlas = JsonConvert.DeserializeObject<Atlas>(json);
            var nameMapping = atlas.Nodes.Select(node => GetName(node.Texture.Source));
            var mappedDictionary = GetDictionary(nameMapping);

            string generatedClasses = MapClasses(mappedDictionary);

            if (Environment.CurrentDirectory.Contains("bin"))
            {
                string saveFile =
                    Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)),
                        "GeneratedContent", "Cursors.cs");

                string folderPath = Path.GetDirectoryName(saveFile);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                File.WriteAllText(saveFile, EnclosingString("#if IS_POC", generatedClasses, "#endif"));
                Console.WriteLine($"Generated output content in '{saveFile}'");
            }
            else
                Console.WriteLine("This must be called in Visual Studio!");
#else

#endif

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        private static string EnclosingString(string prefix, string str, string suffix)
        {
            return $"{prefix}\n\n{str}\n\n{suffix}";
        }

        private static Dictionary<string, string> GetDictionary(IEnumerable<Tuple<string, string>> tupledItems)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in tupledItems)
            {
                if (!dict.ContainsKey(item.Item1))
                    dict.Add(item.Item1, item.Item2);
            }

            return dict;
        }

        private static string MapClasses(Dictionary<string, string> mapping)
        {
            var arr = mapping.Keys.ToArray();

            return LevelTest.GenerateAllClasses(arr, arg => GetFieldValue(mapping, arg));
        }

        private static string GetFieldValue(Dictionary<string, string> mapping, string arg)
        {
            return mapping.ContainsKey(arg) ? mapping[arg] : string.Empty;
        }

        private static Tuple<string, string> GetName(string name)
        {
            string fileName = Path.GetFileNameWithoutExtension(name);

            name = name.Replace(Replace, "");
            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            name = RemoveBy(name);
            name = name
                .Replace(" ", "_")
                .Replace("_\\", "\\")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", string.Empty)
                .Replace("]", string.Empty);

            string match = string.Empty;

            try
            {
                match = name.Substring(0, name.IndexOf(@"\"));
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(match))
            {
                name = name.Replace(match, "");
                name = match + name;

                name = name.Replace(@"\\", @"\");
                name = Regex.Replace(name, @"\\.\\", "\\");
            }

            // Individual cases
            name = name
                .Replace("\\_", "\\")
                .Replace("-", "_")
                .Replace("Deviantart\\", "")
                .Replace(@"_1_Blue\Moonshine\", @"1\Blue\")
                .Replace(@"\Streetlight\", @"\")
                .Replace(@"Polar_Cursor_Set_For_Windows\", string.Empty)
                .Replace("Oxy_", string.Empty)
                .Replace(@"\Streetlight_", "\\")
                .Replace(@"\Deviantart_", "\\")
                .Replace(@"Ml_Blau_Cursor__Smaller_Version_\", string.Empty)
                .Replace(@"Ml.Blau.3\Ml_Blau_", @"Ml_Blau\")
                .Replace(@"Grey_Tango_Cursor_Little\Grey_Tango_Little\", @"Grey_Tango_Little\")
                .Replace(@"Google_Chrome_Os_Pointers__W_I_P__\", string.Empty)
                .Replace(@"Forma_Nova__Xp_Cursor_Set\", "Forma_Nova")
                .Replace(@"Extenza_Pro_Cursor_Pack", "Extenza_Pro")
                .Replace(@"Extenza_Cursors\EXTENZA_Cursors\EXTENZA_", "Extenza")
                .Replace(@"Denial_Cursor_Pack", "Denial")
                .Replace(@"Denial___Blue", @"Denial\Blue")
                .Replace(@"Comix_Cursors_Orange\Comixcursors_Orange", @"Comix\Orange")
                .Replace(@"Comix_Cursors_Blue\Comixcursors_Blue", @"Comix\Blue")
                .Replace(@"Comix_Cursors_Black", @"Comix\Black")
                .Replace(@"Comix_Cursors_Black_And_Red\Comixcursors_Black_And_Red", @"Comix\RedAndBlack")
                .Replace(@"Comix\Black_And_Red\Comixcursors_Black_And_Red", @"Comix\RedAndBack")
                .Replace(@"Cd_Busy_For_User32.Dll\Cd_Busy", "Cd_Busy")
                .Replace(@"A0x_Curset_1__Cur_And_Ani_Ver_\A0x_Cursors", "A0x");

            name = name.Replace(@".Png", string.Empty);
            name = name.Replace("\\", "/");

            return new Tuple<string, string>(name, fileName);
        }

        private static string RemoveBy(string title)
        {
            return Regex.Replace(title, @"By_.+?\\", "\\");
        }
    }
}