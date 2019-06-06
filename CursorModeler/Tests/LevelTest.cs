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
        private const string GENERAL_CLASS = "GlobalCursorDB";
        private static RecursiveNode GeneralNode;

        private static IEnumerable<RecursiveNode> RecursiveSplitting(IEnumerable<string> arrs, string currentParent = "", string splitChar = "/")
        {
            if (!string.IsNullOrEmpty(Separator))
                splitChar = Separator;

            UsedSeparator = splitChar;

            string lastHandle = "";
            foreach (string item in arrs)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                bool isTopLevelCall = string.IsNullOrEmpty(currentParent);

                string parent = isTopLevelCall && item.Contains(splitChar)
                    ? item.Substring(0, item.LastIndexOf(splitChar))
                    : currentParent;

                if (!item.Contains(splitChar))
                {
                    if (debug)
                        Console.WriteLine($"General node found {item}!");

                    // If parent isn't still defined we are on the top-level recursive call
                    if (isTopLevelCall)
                    {
                        if (GeneralNode == null)
                        {
                            GeneralNode = new RecursiveNode(GENERAL_CLASS, string.Empty);
                            yield return GeneralNode;
                        }

                        GeneralNode.Childs.Add(new RecursiveNode(item, string.Empty));
                    }
                    else
                        yield return new RecursiveNode(item, parent);

                    continue;
                }

                var splittedValues = item.Split(splitChar.ToCharArray());

                if (lastHandle == splittedValues[0])
                    continue;

                lastHandle = splittedValues[0];

                var node = new RecursiveNode(lastHandle, parent);

                var subItems = arrs.Where(i => i.StartsWith(splittedValues[0] + splitChar) && i.Contains(splitChar))
                                   .Select(i => i.Replace(splittedValues[0] + splitChar, string.Empty));

                if (debug)
                    Console.WriteLine("[Item={0}, SubItem Count={1}]", item, subItems.Count());

                if (splittedValues.Length == 1)
                {
                    yield return new RecursiveNode(splittedValues[0], parent);
                    continue;
                }

                var subNodes = RecursiveSplitting(subItems, parent, splitChar);
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

        // We don't use CodeDom due to simplicity
        private static string GenerateClass(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return $"public static class {name}{Environment.NewLine}{{";
        }

        private static string GenerateField(string name, string fieldValue)
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