using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            bool isTopLevelCall = string.IsNullOrEmpty(currentParent);

            string lastHandle = "";
            foreach (string item in arrs)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

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
                            GeneralNode = new RecursiveNode(GENERAL_CLASS, string.Empty);

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

            if (isTopLevelCall)
                yield return GeneralNode;
        }

        private static string OutputRecursiveNode(IEnumerable<RecursiveNode> nodes, Func<string, string> getFieldValue, int count = -1)
        {
            var sb = new StringBuilder();

            ++count;

            foreach (var node in nodes)
            {
                string indenter = new string('\t', count);

                var @class = indenter + GenerateClass(node.Value, out bool isAlreadyAdded).Replace(Environment.NewLine, Environment.NewLine + indenter);

                if (node.Childs.Count > 0)
                {
                    sb.AppendLine(@class);
                    sb.AppendLine(OutputRecursiveNode(node.Childs, getFieldValue, count));

                    if (!string.IsNullOrEmpty(@class))
                        sb.AppendLine(indenter + "}" + (isAlreadyAdded ? "*/" : string.Empty));

                    sb.AppendLine();
                }
                else
                {
                    string realPath = node.CurrentParent + UsedSeparator + node.Value,
                           field = GenerateField(node.Value, getFieldValue(realPath), node.CurrentParent + UsedSeparator);

                    sb.AppendLine(indenter + field);
                }
            }

            return sb.ToString();
        }

        private static HashSet<string> classNames = new HashSet<string>(),
                                       fieldNames = new HashSet<string>();

        // We don't use CodeDom due to simplicity
        private static string GenerateClass(string name, out bool isAlreadyAdded)
        {
            if (string.IsNullOrEmpty(name))
            {
                isAlreadyAdded = false;
                return string.Empty;
            }

            string cleanedName = GetCleanClassName(name);
            isAlreadyAdded = !classNames.Add(cleanedName);

            // {(isAlreadyAdded ? "partial" : string.Empty)}
            return $"{(isAlreadyAdded ? "/*" : string.Empty)}public static class {cleanedName}{Environment.NewLine}{{";
        }

        private static string GetCleanClassName(string name)
        {
            name = name.Replace(".", string.Empty)
                       .Replace("&", "_");

            return name;
        }

        private static string GenerateField(string name, string fieldValue, string realPath)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            bool isNumber = int.TryParse(name, out var digit);

            if (Regex.IsMatch(name, @"^\d"))
                name = Regex.Replace(name, @"^\d+", string.Empty);

            if (name.StartsWith("_"))
                name = name.Substring(1);

            string finalName = isNumber ? $"UndefinedFieldName_{digit}" : name;
            bool isAlreadyAdded = !fieldNames.Add(realPath + name);

            if (isAlreadyAdded && debug)
                Console.WriteLine($"Already added field '{realPath + name}'");

            return (isAlreadyAdded ? "// " : string.Empty) + $@"public static string {finalName} = ""{fieldValue}"";";
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