// #define NAMEDEBUG

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
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
            // Console.WriteLine("Hello World");
            string json;
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString(ATLAS_URL);
            }

            var atlas = JsonConvert.DeserializeObject<Atlas>(json);

            // Console.WriteLine(LevelTest.CreateLevels());
            // LevelTest.GenerateClassesWithoutSplitting();

            //#if NAMEDEBUG
            //            atlas.Nodes.ForEach(node => GetName(node.Texture.Source));
            //#else
            //            atlas.Nodes.ForEach(node => Console.WriteLine(GetName(node.Texture.Source)));
            //#endif
            var nameMapping = atlas.Nodes.Select(node => GetName(node.Texture.Source));
            string classMap = MapClasses(nameMapping);

            Console.WriteLine(classMap);

            // TODO: Think about how to link names to this enum (https://stephenhodgson.github.io/UnityCsReference/api/UnityEditor.MouseCursor.html)
            // TODO: Think about how to get a value from this enum (Maybe by inheriting a global class (GlobalCursorDB), and inheriting only on the last class)
            // This class will have a method 'public string GetCursor(MouseCursor cursorType)'
            // All last classes will inherit from Singleton (own implementation repasted)
            // We need to add the constructor and a virtual Dictionary (global class) ¿overriding it on the derived class?, and on the derived class we will Add manually values to this overrided Dictionary
            // TODO: Don't do classes static

            Console.Read();

            // Console.WriteLine(json.Length);
        }

        private static string MapClasses(IEnumerable<Tuple<string, string>> mapping)
        {
            // var dict = mapping.ToDictionary(t => t.Item2, t => t.Item1);
            var arr = mapping.Select(t => t.Item2).ToArray();

            return LevelTest.GenerateAllClasses(arr, arg => GetFieldValue(mapping, arg));

            //StringBuilder sb = new StringBuilder("public static class CursorsDB {");

            //foreach (var item in arr)
            //{
            //    // TODO: Get items like this:

            //    // Input

            //    // aaaa/item1.png
            //    // aaaa/item2.png
            //    // aaaa/item3.png
            //    // bbbb/item1.png
            //    // bbbb/item2.png

            //    // Output

            //    // aaaa/item1.png
            //    // item2.png
            //    // item3.png
            //    // bbbb/item1.png
            //    // item2.png

            //    // TODO: Then create a class and its variables with the values (easy task)

            //    // Idea sobre como podría hacer esto:
            //    // Básicamente hay que tener un string lastHandle (se forma haciendo substring hasta el primer '/'), y comprobar que el lastHandle y el handleActual (tengo q hacer un metodo para el tema del substring)
            //    // sean iguales, entonces se hará replace (para eliminar la parte del 'aaaa/')
            //    // sean diferentes, se cambiará el valor del lastHandle para ser usado por los proximos replaces
            //    // El único problema que a esto le veo es tener varios niveles... Asi que en ese caso, tendré que hacer un metodo que genere el input de arriba, y otro metodo que realice los cambios pertinentes

            //    //string hierarchy = tuple.Item2;
            //    //string[] subNodes = hierarchy.Split("\\".ToCharArray());

            //    //var classNodes = subNodes.TakeAllButLast();
            //    //string varName =
            //}

            // sb.AppendLine("}");
            // return sb.ToString();
        }

        private static string GetFieldValue(IEnumerable<Tuple<string, string>> mapping, string arg)
        {
            return mapping.First(tuple => tuple.Item2 == arg).Item1;

            // return dict[arg];
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
                //#if NAMEDEBUG
                //                Console.WriteLine($"Match: {match} (Name: {name})");
                //#endif

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
                .Replace(@"Cd_Busy_For_User32.Dll\Cd_Busy", "Cd_Busy")
                .Replace(@"A0x_Curset_1__Cur_And_Ani_Ver_\A0x_Cursors", "A0x");

            name = name.Replace(@".Png", string.Empty);

            // name = string.Join(" ", name.ToCharArray().Distinct());

            // name = string.Join(" ", name).Split(new Char[] { ' ' }).Distinct());

#if NAMEDEBUG
            // Console.WriteLine($"Subpaths: " + Regex.Matches(name, @"\\").Count);
            // Console.WriteLine($"Matches: {Regex.Matches(name, match).Count} || Name: {name}");
            Console.WriteLine(name);
#endif

            return new Tuple<string, string>(fileName, name);
        }

        private static string RemoveBy(string title)
        {
            return Regex.Replace(title, @"By_.+?\\", "\\");
        }
    }

    public static class Ext
    {
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> source)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems = false;
            bool isFirst = true;
            T item = default(T);

            do
            {
                hasRemainingItems = it.MoveNext();
                if (hasRemainingItems)
                {
                    if (!isFirst) yield return item;
                    item = it.Current;
                    isFirst = false;
                }
            } while (hasRemainingItems);
        }
    }
}