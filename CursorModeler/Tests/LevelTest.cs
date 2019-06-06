using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CursorModeler.Tests
{
    public static class LevelTest
    {
        private static string[] firstLevel = new[] { "Level1a", "Level1b", "Level1c" };
        private static string[] secondLevel = new[] { "Level2a", "Level2b", "Level2c" };
        private static string[] thirdLevel = new[] { "Level3a", "Level3b", "Level3c" };
        private static string[] fourthLevel = new[] { "Level4a", "Level4b", "Level4c" };

        public static string CreateLevels()
        {
            var sb = new StringBuilder();

            foreach (var first in firstLevel)
                foreach (var second in secondLevel)
                    foreach (var third in thirdLevel)
                        foreach (var fourth in fourthLevel)
                            sb.AppendLine(string.Format("{0}/{1}/{2}/{3}", first, second, third, fourth));

            return sb.ToString();
        }

        public static string GenerateAllClasses(string[] arr, Func<string, string> getFieldValue)
        {
            return OutputRecursiveNode(RecursiveSplitting(arr), getFieldValue);
        }

        private static IEnumerable<RecursiveNode> GenerateClassesWithoutSplitting(params string[][] levelsArr)
        {
            //if (levelsArr == null || levelsArr.Length == 0)
            //    levelsArr = new[] { firstLevel, secondLevel, thirdLevel, fourthLevel };

            //Array.Reverse(levelsArr);

            //// int count;
            //foreach (var level in levelsArr)
            //{
            //    // Console.WriteLine(level.Length);
            //    // Console.WriteLine(string.Join(", ", level));
            //}

            string levels = CreateLevels();
            var lines = levels.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            // Console.WriteLine(lines.Length);
            // Console.WriteLine("Last value: '" + lines.Last() + "'");

            return RecursiveSplitting(lines);

            // Map array
            //Dictionary<string, string> dict = new Dictionary<string, string>();
            //dict.Add();
        }

        //public static int counter;
        // private static Random rnd = new Random();

        private static IEnumerable<RecursiveNode> RecursiveSplitting(IEnumerable<string> arrs, char splitChar = '/')
        {
            //if(counter > 10)
            //	yield break;

            //++counter;

            string lastHandle = "";
            foreach (var item in arrs)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                var splitted = item.Split(splitChar);

                if (lastHandle == splitted[0])
                    continue;

                lastHandle = splitted[0];

                var node = new RecursiveNode(lastHandle);

                var subItems = arrs.Where(i => i.StartsWith(splitted[0]))
                                   .Select(i => i.Replace(splitted[0] + splitChar, string.Empty));

                // Console.WriteLine(subItems.Count());

                if (splitted.Length == 1)
                {
                    yield return new RecursiveNode(splitted[0]);
                    continue;
                }

                var subNodes = RecursiveSplitting(subItems);
                node.Childs.AddRange(subNodes);

                /*if(splitted.Length > 1 && !string.IsNullOrEmpty(splitted[0])) {
					var splittingLevel = arrs.Select(_item => _item.Replace(splitted[0], string.Empty))
                    	.ToArray();

                	node.Childs.AddRange(RecursiveSplitting(splittingLevel, splitChar));
				}*/

                yield return node;
            }
        }

        private static string OutputRecursiveNode(IEnumerable<RecursiveNode> nodes, Func<string, string> getFieldValue, int count = -1)
        {
            var sb = new StringBuilder();

            // int counter = 0;

            ++count;

            foreach (var node in nodes)
            {
                // Console.WriteLine(node.Childs.Count);

                string indenter = new string('\t', count);

                var @class = indenter + GenerateClass(node.Value).Replace(Environment.NewLine, Environment.NewLine + indenter);

                if (node.Childs.Count > 0)
                {
                    sb.AppendLine(@class);
                    sb.AppendLine(OutputRecursiveNode(node.Childs, getFieldValue, count));

                    if (!string.IsNullOrEmpty(@class))
                        sb.AppendLine(indenter + "}");
                    // () => string.Join(Environment.NewLine, node.Childs.Select(n => GenerateClass(n.Value))));
                    sb.AppendLine();
                }
                else
                {
                    string field = GenerateField(node.Value, getFieldValue);
                    sb.AppendLine(indenter + field);
                    //field.Remove(field.Length - 2));
                }

                /*foreach(var child in node.Childs)
				{
					var @class = GenerateClass(child.Value);
					sb.AppendLine(@class);

					if(!string.IsNullOrEmpty(@class))
						sb.AppendLine("}");
				}*/

                // ++counter;
            }

            return sb.ToString();
        }

        private static string GenerateClass(string name) // , Func<string> str)
        {
            if (string.IsNullOrEmpty(name))
            {
                // Console.WriteLine("Error!");
                return string.Empty;
            }

            // StringBuilder sb = new StringBuilder(, name));
            // sb.AppendLine(str())
            // sb.AppendLine("}");

            // return sb.ToString();

            return $@"public static class {name}{Environment.NewLine}{{";
        }

        private static string GenerateField(string name, Func<string, string> getFieldValue) // , Func<string> str)
        {
            if (string.IsNullOrEmpty(name))
            {
                // Console.WriteLine("Error!");
                return string.Empty;
            }

            // StringBuilder sb = new StringBuilder(, name));
            // sb.AppendLine(str())
            // sb.AppendLine("}");

            // return sb.ToString();

            return $@"public static string {name} = ""{getFieldValue(name)}"";";
        }
    }

    public class RecursiveNode
    {
        public string Value { get; set; }
        public List<RecursiveNode> Childs { get; set; }

        private RecursiveNode()
        {
            Childs = new List<RecursiveNode>();
        }

        public RecursiveNode(string value)
            : this()
        {
            Value = value;
        }
    }
}