using System.Text.RegularExpressions;

namespace quizer_WPF
{
    public partial class Interface
    {
        // head pattern
        //
        // @[not show: !][start index: int][question number alignment: <=>]`[type: voc/pair/fill]`
        // [L length: int][F/NF: fixed?][Q length (in sentence): int][C/NC: choice/handwrite]
        // [CE/CF/AZ: combination A-E, A-F/ single A-Z]
        // [LC/NLC: lower cases?][SEP`[word list joining string: str]`]
        // not show idx:1, start int:2, question number alignment:3, type:4, length:5, fixed?:6,
        // blank-in-sentence length:7, choice?:8, choice type:9, force lower cases:10, separator:11
        // note if start index == 0, won't show index
        // example @5=[pair]L2CAZ
        // questions will start as Q5 and look like _(5)_, options from A-Z

        public static Func<string, PartBase> ParsePartHead(string head, PartConfig config)
        {
            var groups = HeadPattern().Match(head).Groups;
            ToParts.PartBaseFactory configToStringToPart =
                ToParts.ChoosePartType(groups[4].Value) ??
                throw new ArgumentException("doesn't have such question type.");
            config.markQuestionNumber = groups[1].Length == 0;
            if (groups[2].Length != 0)
            {
                config.index = int.Parse(groups[2].Value);
            }
            if (groups[3].Length != 0)
            {
                config.questionNumberAlignment = groups[3].Value[0];
            }
            if (groups[5].Length != 0)
            {
                config.underscoreLength = int.Parse(groups[5].Value);
            }
            if (groups[6].Length != 0)
            {
                config.underscoreLengthFixed = groups[6].Length == 1;
            }
            if (groups[7].Length != 0)
            {
                config.underscoreInSentenceLength = int.Parse(groups[7].Value);
            }
            if (groups[8].Length != 0)
            {
                config.provideCode = groups[8].Length == 1;
            }
            if (groups[9].Length != 0)
            {
                config.codes = groups[9].Value switch
                {
                    "AZ" => Constants.AZ,
                    "CF" => Constants.CARD_F,
                    _ => Constants.CARD_E
                };
            }
            if (groups[10].Length != 0)
            {
                config.lowerCase = groups[10].Length == 2;
            }
            if (groups[11].Length != 0)
            {
                config.wordListSeparator = groups[11].Value;
            }
            return source => configToStringToPart(source, config);


        }

        [GeneratedRegex(@"@(\!)?(\d*)?([<=>])?\[(\w*)\](?:L(\d+))?(N?F)?(?:Q(\d+))?(N?C)?(CE|CF|AZ)?(N?LC)?(?:SEP\[(.*)\])?")]
        private static partial Regex HeadPattern();


        public class Result(
            List<string> questionParts,
            List<KeyValuePair<int, List<string>>> answersParts,
            Tuple<List<string>, List<string>, List<string>> messages
        )
        {
            public List<string> questionParts = questionParts;
            public List<KeyValuePair<int, List<string>>> answersParts = answersParts;
            public Tuple<List<string>, List<string>, List<string>> messages = messages;
        }


        public static Result LinesToResult(string[] lines)
        {
            PartConfig config = new();
            List<KeyValuePair<int, List<string>>> answer = [];
            List<string> quesText = [];
            int idx = 0, end = lines.Length, startLine, endLine, start;
            Func<string, PartBase> parsed;
            string q;
            List<string> a;
            while (idx < end)
            {
                try
                {
                    while ((lines[idx].Length == 0) || (lines[idx][1] == '#'))
                    {
                        ++idx;
                    }

                    parsed = ParsePartHead(lines[idx], config);
                }
                catch (Exception)
                {
                    config.messages.Item3.Add($"Syntax error at line {++idx}");
                    return new Result([], [], config.messages);
                }

                startLine = endLine = ++idx;

                do
                {
                    ++endLine;
                } while (endLine < end && lines[endLine] != "@@");

                start = config.index + 1;

                try
                {
                    (q, a) = parsed(string.Join("\n", lines[startLine..endLine])).Produce();
                }
                catch (Exception)
                {
                    config.messages.Item3.Add($"Some error occur.\nCannot generate the quiz.");
                    return new Result([], [], config.messages);
                }
                answer.Add(new(start, a));
                quesText.Add(q);
                idx = ++endLine;
            }

            return new Result(quesText, answer, config.messages);
        }
    }
}
