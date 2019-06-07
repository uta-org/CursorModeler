// #define REFLECTION
#define TESTING

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using GeneratedContent;
using Newtonsoft.Json.Linq;
using TexturePacker.Lib;

namespace CursorModeler
{
    using Tests;

    public class Program
    {
        private const string ATLAS_URL = "https://gist.githubusercontent.com/z3nth10n/61389f7b6bf37d25ea4cc22eb57231ff/raw/77225b1e67da47630a1132ec369b53c0708a10cf/atlas.json";

        public static void Main()
        {
            string jsonFile = Path.Combine(Environment.CurrentDirectory, "mouse_cursors.json");

#if !REFLECTION && !TESTING
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
#elif REFLECTION
            {
            }
            var assembly = Assembly.GetExecutingAssembly();

            int ocurrenceCount = 0;
            var enums = Enum.GetValues(typeof(MouseCursor)).Cast<MouseCursor>().Select(e => e.ToString());

            foreach (var type in assembly.GetTypes())
            {
                if(type.Namespace == "GeneratedContent")
                {
                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                    foreach (var field in fields)
                    {
                        string enumOcurrence = enums.FirstOrDefault(e => field.Name.Contains(e) || FindAlias(field.Name, e, type.FullName));
                        if(enumOcurrence != null)
                        {
                            // Console.WriteLine($"[Type={type.FullName} | FieldName={field.Name}] EnumOcurrence={enumOcurrence}");
                            var mouseCursor = (MouseCursor)Enum.Parse(typeof(MouseCursor), enumOcurrence);
                            string value = $"{type.FullName}/{field.Name}";

                            if (!GlobalCursorDB.Map.ContainsKey(mouseCursor))
                                GlobalCursorDB.Map.Add(mouseCursor, new List<string>());

                            GlobalCursorDB.Map[mouseCursor].Add(value);
                            ++ocurrenceCount;
                        }
                    }
                    // Console.WriteLine(type.FullName);
                }
            }

            File.WriteAllText(jsonFile, JsonConvert.SerializeObject(GlobalCursorDB.Map, Formatting.Indented));
            Console.WriteLine($"Count: {ocurrenceCount} || CheckCount: {checkCount}");
#elif TESTING
            GlobalCursorDB.Map =
                // (Dictionary<string, List<string>>)
                JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(jsonFile)); //, typeof(Dictionary<string, List<string>>));

            Console.WriteLine($"From Enum: {GlobalCursorDB.GetCursorReference(MouseCursor.Move)}");
            Console.WriteLine($"From Generic Type: {GlobalCursorDB.GetCursorReference<Comix.Black>("Diagonal2")}");
#endif

            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        private static int checkCount = 0;

        private static bool FindAlias(string fieldName, string enumName, string typeName)
        {
            ++checkCount;

            string tName = typeName.ToLowerInvariant();
            string fName = fieldName.ToLowerInvariant();
            string eName = enumName.ToLowerInvariant();

            if (enumName.Contains("Resize") && fName.Contains("size") && !tName.Contains("oxygen") || tName.Contains("globalcursordb"))
                enumName = enumName.Replace("_Resize", string.Empty);

            if (eName == "all_scroll" && fName.Contains("all"))
                return true;

            if (eName == "crosshair" && fName.Contains("cross"))
                return true;

            if ((eName == "e_resize" || eName == "ew_resize") && fName.Contains("hor"))
                return true;

            if ((eName == "ns_resize" || eName == "n_resize") && fName.Contains("ver"))
                return true;

            if ((eName == "nw_resize" || eName == "nwse_resize") && fName.Contains("fdiag"))
                return true;

            if ((eName == "ne_resize" || eName == "nesw_resize") && fName.Contains("bdiag"))
                return true;

            return fieldName.Contains(enumName);
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
            name = GlobalCursorDB.GetFormattedName(name);

            return new Tuple<string, string>(name, fileName);
        }
    }
}