using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using CursorModeler;
using Newtonsoft.Json;
using TexturePacker.Lib;

namespace GeneratedContent
{
    public partial class GlobalCursorDB
    {
        private const string ATLAS_URL = "https://gist.githubusercontent.com/z3nth10n/61389f7b6bf37d25ea4cc22eb57231ff/raw/77225b1e67da47630a1132ec369b53c0708a10cf/atlas.json",
                             Replace = "E:\\VISUAL STUDIO\\Visual Studio Projects\\TexturePacker\\bin\\Debug\\png\\";

        private static bool LOAD_FROM_INTERNET = true;

        static GlobalCursorDB()
        {
            if (Map == null)
                Map = new Dictionary<string, List<string>>();

            if (CurrentDB == null)
                CurrentDB = Activator.CreateInstance<GlobalCursorDB>();

            if (m_KeyPairMap == null && LOAD_FROM_INTERNET)
            {
                string json;
                using (var wc = new WebClient())
                    json = wc.DownloadString(ATLAS_URL);

                m_KeyPairMap = new Dictionary<string, string>();

                JsonConvert.DeserializeObject<Atlas>(json).Nodes.Select(n => new
                {
                    Name = GetFormattedName(n.Texture.Source),
                    FileName = Path.GetFileNameWithoutExtension(n.Texture.Source)
                }).ToList().ForEach(anon =>
                {
                    if (!m_KeyPairMap.ContainsKey(anon.Name))
                        m_KeyPairMap.Add(anon.Name, anon.FileName);
                });
            }
        }

        public static Dictionary<string, List<string>> Map { get; set; }
        public static GlobalCursorDB CurrentDB { get; set; }

        private static Dictionary<string, string> m_KeyPairMap;

        // TODO: Generate a map from reflection indexing classes into a Dictionary<MouseCursor, List<string>> (save it to a JSON file, for a later load)
        // TODO: Where the List of strings will store the full reflected path (example: <MouseCursor.Move, Comix.Black.Move>)
        public static string GetCursorReference(GlobalCursorDB cursorDB, MouseCursor cursorMode, int matchIndex = -1)
        {
            return InternalGetCursorReference(cursorDB, cursorMode, matchIndex);
        }

        public static string GetCursorReference(MouseCursor cursorMode, int matchIndex = -1)
        {
            return InternalGetCursorReference(CurrentDB, cursorMode, matchIndex);
        }

        private static string InternalGetCursorReference(GlobalCursorDB cursorDB, MouseCursor cursorMode, int matchIndex = -1)
        {
            var key = cursorMode.ToString();

            if (!Map.ContainsKey(key))
                throw new ArgumentException("Cursor mode not available on the map!");

            var type = cursorDB.GetType();
            string typeName = type.FullName,
                   fullName = Map[key].Where(m => m.Contains(typeName))
                       .ElementAt(matchIndex == -1 ? 0 : matchIndex),
                   fieldName = fullName.Split('/')[1],
                   fieldValue = (string)type.GetField(fieldName).GetValue(null);

            return fieldValue;
        }

        // TODO: Get namespace+type from the expression and the parse it
        // The name must be one from the desired class class
        public static string GetCursorReference<T>(Expression<Func<string>> expression)
           where T : GlobalCursorDB
        {
            var type = typeof(T);
            return InternalGetCursorReference(type, expression);
        }

        public static string GetCursorReference(Expression<Func<string>> expression)
        {
            var type = CurrentDB.GetType();
            return InternalGetCursorReference(type, expression);
        }

        private static string InternalGetCursorReference(Type type, Expression<Func<string>> expression)
        {
            string memberName = GetMemberName(expression),
                   fieldValue = (string)type.GetField(memberName).GetValue(null);

            return fieldValue;
        }

        public static string GetCursorReference<T>(string name)
            where T : GlobalCursorDB
        {
            var type = typeof(T);
            return InternalGetCursorReference(type, name);
        }

        public static string GetCursorReference(string name)
        {
            var type = CurrentDB.GetType();
            return InternalGetCursorReference(type, name);
        }

        private static string InternalGetCursorReference(Type type, string name)
        {
            return (string)type.GetField(name).GetValue(null);
        }

        public static string GetGlobalCursorReference(string path)
        {
            // TODO: Check if path is valid, if it's valid then search from a Dictionary<string, string> (deserialized, [path, fileName]) generated from json, and return fileName
            if (m_KeyPairMap.ContainsKey(path))
                return m_KeyPairMap[path];

            return string.Empty;
        }

        public static string GetMemberName<T>(Expression<Func<T>> expression)
        {
            if (expression.Body is MemberExpression)
                return ((MemberExpression)expression.Body).Member.Name;

            var op = ((UnaryExpression)expression.Body).Operand;
            return ((MemberExpression)op).Member.Name;
        }

        public static string GetFormattedName(string name)
        {
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

            // TODO: Review this
            name = name.Replace(".", string.Empty)
                       .Replace("&", "_");

            return name;
        }

        private static string RemoveBy(string title)
        {
            return Regex.Replace(title, @"By_.+?\\", "\\");
        }
    }
}