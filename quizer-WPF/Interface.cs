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


        private static readonly bool?[] n_t_f = [null, true, false];
        public static PartConfigNull ReadConfig(in string head)
        {
            var groups = HeadPattern().Match(head).Groups;
            return new PartConfigNull
            {
                partType = groups[4].Value,
                markQuestionNumber = groups[1].Length == 0,
                index = groups[2].Length != 0 ? int.Parse(groups[2].Value) - 1 : null,
                questionNumberAlignment = groups[3].Length != 0 ? groups[3].Value[0] : null,
                underscoreLength = groups[5].Length != 0 ? int.Parse(groups[5].Value) : null,
                underscoreLengthFixed = n_t_f[groups[6].Length],
                underscoreInSentenceLength = groups[7].Length != 0 ? int.Parse(groups[7].Value) : null,
                provideCode = n_t_f[groups[8].Length],
                codes = groups[9].Length != 0 ? groups[9].Value switch
                {
                    "AZ" => Constants.AZ,
                    "CF" => Constants.CARD_F,
                    _ => Constants.CARD_E
                } : null,
                lowerCase = groups[10].Length != 0 ? groups[10].Length==2 : null,
                wordListSeparator = groups[11].Length != 0 ? groups[11].Value : null
            };
        }

        public static string WriteConfig(in PartConfigNull config)
        {
            string partType = config.partType ?? "";
            string markQuestionNumber = config.markQuestionNumber==false ? "!" : "";
            string index = (config.index + 1)?.ToString() ?? "";
            string questionNumberAlignment = config.questionNumberAlignment?.ToString() ?? "";
            string underscoreLength = config.underscoreLength != null ? $"L{config.underscoreLength}" : "";
            string underscoreLengthFixed = config.underscoreLengthFixed == true ? "F" : config.underscoreLengthFixed == false ? "NF" : "";
            string underscoreInSentenceLength = config.underscoreInSentenceLength != null ? $"Q{config.underscoreInSentenceLength}" : "";
            string provideCode = config.provideCode == true ? "C" : config.provideCode == false ? "NC" : "";
            string codes = config.codes?.Length switch
            {
                63 => "CE",
                31 => "CF",
                26 => "AZ",
                _ => ""
            };
            string lowerCase = config.lowerCase == true ? "LC" : config.lowerCase == false ? "NLC" : "";
            string wordListSeparator = config.wordListSeparator != null ? $"SEP[{config.wordListSeparator}]" : "";
            return $"@{markQuestionNumber}{index}{questionNumberAlignment}[{partType}]{underscoreLength}{underscoreLengthFixed}{underscoreInSentenceLength}{provideCode}{codes}{lowerCase}{wordListSeparator}";

        }

        public static void ConfigOverride(in PartConfigNull over, ref PartConfig config)
        {
            config.partType = over.partType!=null && ToParts.IsPartType(over.partType) ? over.partType : config.partType;
            config.markQuestionNumber = over.markQuestionNumber ?? config.markQuestionNumber;
            config.index = over.index ?? config.index;
            config.questionNumberAlignment = over.questionNumberAlignment ?? config.questionNumberAlignment;
            config.underscoreLength = over.underscoreLength ?? config.underscoreLength;
            config.underscoreLengthFixed = over.underscoreLengthFixed ?? config.underscoreLengthFixed;
            config.underscoreInSentenceLength = over.underscoreInSentenceLength ?? config.underscoreInSentenceLength;
            config.provideCode = over.provideCode ?? config.provideCode;
            config.codes = over.codes ?? config.codes;
            config.lowerCase = over.lowerCase ?? config.lowerCase;
            config.wordListSeparator = over.wordListSeparator ?? config.wordListSeparator;
        }


        public static Func<string[], PartBase> ParsePartHead(in string head, PartConfig config)
        {
            var over = ReadConfig(head);
            ToParts.PartBaseFactoryLines configToStringToPart =
                over.partType != null && ToParts.IsPartType(over.partType) ?
                ToParts.ChoosePartType(over.partType) :
                throw new ArgumentException("doesn't have such question type.");
            ConfigOverride(over, ref config);
            return source => configToStringToPart(source, config);


        }

        [GeneratedRegex(@"^@(\!)?(\d*)?([<=>])?\[(\w*)\](?:L(\d+))?(N?F)?(?:Q(\d+))?(N?C)?(CE|CF|AZ)?(N?LC)?(?:SEP\[(.*)\])?$")]
        private static partial Regex HeadPattern();


        public class Result(
            List<string> questionParts,
            List<KeyValuePair<int, List<string>>> answersParts,
            (List<string>, List<string>, List<string>) messages
        )
        {
            public List<string> questionParts = questionParts;
            public List<KeyValuePair<int, List<string>>> answersParts = answersParts;
            public (List<string> infos, List<string> warnings, List<string> errors) messages = messages;
        }


        public static Result LinesToResult(in string[] lines)
        {
            PartConfig config = new();
            List<KeyValuePair<int, List<string>>> answer = [];
            List<string> quesText = [];
            int idx = 0, end = lines.Length, startLine, endLine, start;
            Func<string[], PartBase> parsed;
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
                    if(idx == end)
                    {
                        break;
                    }
                    config.messages.errors.Add($"Syntax error at line {++idx}");
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
                    (q, a) = parsed(lines[startLine..endLine]).Produce();
                }
                catch (Exception)
                {
                    config.messages.errors.Add($"Some error occur.\nCannot generate the quiz.");
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
