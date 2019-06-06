using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CursorModeler.Tests
{
    // Fiddle: https://dotnetfiddle.net/zhtHbe
    public static class LevelTest
    {
        public static string Separator = string.Empty;
        public static string UsedSeparator;

        public static string GenerateAllClasses(string[] arr, Func<string, string> getFieldValue)
        {
            return OutputRecursiveNode(RecursiveSplitting(arr), getFieldValue);
        }

        private const bool debug = false;

        private static IEnumerable<RecursiveNode> RecursiveSplitting(IEnumerable<string> arrs, string currentParent = "", string splitChar = "/", bool nestedDebug = false)
        {
            if (!string.IsNullOrEmpty(Separator))
                splitChar = Separator;

            UsedSeparator = splitChar;

            var _arrs = debug && !nestedDebug
                    ? arrs.Where(a => a.Contains("Comix/"))
                    : arrs;

            if (debug)
                nestedDebug = true;

            string lastHandle = "";
            foreach (var item in _arrs)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                string parent = string.IsNullOrEmpty(currentParent) && item.Contains(splitChar)
                    ? item.Substring(0, item.LastIndexOf(splitChar))
                    : currentParent;

                if (!item.Contains(splitChar))
                {
                    yield return new RecursiveNode(item, parent);
                    continue;
                }

                var splitted = item.Split(splitChar.ToCharArray());

                if (lastHandle == splitted[0])
                    continue;

                lastHandle = splitted[0];

                var node = new RecursiveNode(lastHandle, parent);

                var subItems = arrs.Where(i => i.StartsWith(splitted[0] + splitChar) && i.Contains(splitChar))
                                   .Select(i => i.Replace(splitted[0] + splitChar, string.Empty));

                if (debug)
                    Console.WriteLine("[Item={0}, SubItem Count={1}]", item, subItems.Count());

                if (splitted.Length == 1)
                {
                    yield return new RecursiveNode(splitted[0], parent);
                    continue;
                }

                var subNodes = RecursiveSplitting(subItems, parent, splitChar, nestedDebug);
                node.Childs.AddRange(subNodes);

                yield return node;
            }
        }

        private static string OutputRecursiveNode(IEnumerable<RecursiveNode> nodes, Func<string, string> getFieldValue, int count = -1)
        {
            var sb = new StringBuilder();

            ++count;

            foreach (var node in nodes)
            {
                string indenter = new string('\t', count);

                var @class = indenter + GenerateClass(node.Value).Replace(Environment.NewLine, Environment.NewLine + indenter);

                if (node.Childs.Count > 0)
                {
                    sb.AppendLine(@class);
                    sb.AppendLine(OutputRecursiveNode(node.Childs, getFieldValue, count));

                    if (!string.IsNullOrEmpty(@class))
                        sb.AppendLine(indenter + "}");

                    sb.AppendLine();
                }
                else
                {
                    string field = GenerateField(node.Value, getFieldValue(node.CurrentParent + UsedSeparator + node.Value));
                    sb.AppendLine(indenter + field);
                }
            }

            return sb.ToString();
        }

        private static string GenerateClass(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return $"public static class {name}{Environment.NewLine}{{";
            // return $@"public static class {name}{Environment.NewLine}{{";
        }

        private static string GenerateField(string name, string fieldValue) // , Func<string> str)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return $@"public static string {name} = ""{fieldValue}"";";
        }
    }

    public class RecursiveNode
    {
        public string Value { get; }
        public string CurrentParent { get; set; }
        public List<RecursiveNode> Childs { get; }

        private RecursiveNode()
        {
            Childs = new List<RecursiveNode>();
        }

        public RecursiveNode(string value, string parent)
            : this()
        {
            Value = value;
            CurrentParent = parent;
        }
    }
}