using System.Text.RegularExpressions;


namespace quizer_WPF
{

    public class PartConfig(
        int underscoreLength = 4, int underscoreInSentenceLength = 0, int index = 0,
        char questionNumberAlignment = '<',
        bool markQuestionNumber = true, bool underscoreLengthFixed = false,
        bool provideCode = true, bool lowerCase = false,
        string wordListSeparator = "  ", string[]? codes = null)
    {
        public int underscoreLength = underscoreLength, underscoreInSentenceLength = underscoreInSentenceLength, index = index;
        public char questionNumberAlignment = questionNumberAlignment;
        public bool markQuestionNumber = markQuestionNumber, underscoreLengthFixed = underscoreLengthFixed, provideCode = provideCode, lowerCase = lowerCase;
        public string wordListSeparator = wordListSeparator;
        public string[] codes = codes ?? Constants.CARD_E;
        public (List<string> infos, List<string> warnings, List<string> errors) messages = ([], [], []);
    }

    public abstract class PartBase
    {
        protected readonly string? source;
        protected readonly string[]? sourceLines;
        protected PartConfig config;
        protected List<string> ans = [];


        public PartBase(in string source, PartConfig config)
        {
            this.source = source;
            this.config = config;
        }

        public PartBase(in string[] sourceLines, PartConfig config)
        {
            this.sourceLines = sourceLines;
            this.config = config;
        }

        public abstract Tuple<string, List<string>> Produce();
    }

    public partial class PartVocSlot : PartBase
    {
        public PartVocSlot(in string source, PartConfig config) : base(source, config)
        {
        }

        public PartVocSlot(in string[] sourceLines, PartConfig config) : base(sourceLines, config)
        {
        }

        private string Repl(Match matched)
        {
            var groups = matched.Groups;
            string qi = config.markQuestionNumber ? $"({++config.index})" : "";
            var (lqi, rqi) = config.questionNumberAlignment == '>' ? ("", qi) : (qi, "");
            ans.Add($"{groups[1].Value}{groups[2].Value}{groups[3].Value}");
            int vl = config.underscoreLengthFixed ?
                config.underscoreLength :
                Math.Max(
                    config.underscoreLength,
                    groups[1].Length + groups[2].Length + groups[3].Length + 2
                );
            return string.Join("",
                lqi,
                groups[1].Value,
                new ('_', vl),
                groups[3].Value,
                rqi
            );

        }

        public override Tuple<string, List<string>> Produce()
        {
            if (source == null)
            {
                return new(string.Join('\n', from line in sourceLines select SlotPattern().Replace(line, Repl)), ans);
            }
            return new(SlotPattern().Replace(source, Repl), ans);
        }


        [GeneratedRegex(@"\[(?:(\w*)\|)?([\w -]*)(?:\|?(\w*))?\]")]
        private static partial Regex SlotPattern();
    }


    public partial class PartVocSentence : PartBase
    {
        private List<(string, string, string, string, string)> parsed_sentences;
        public int longest_v_len;
        public PartVocSentence(string source, PartConfig config) : base(source, config)
        {
            parsed_sentences = [];
            longest_v_len = 0;

            foreach (Match match in SlotPattern().Matches(source).Cast<Match>())
            {
                var parsed = (
                    match.Groups[1].Value,
                    match.Groups[2].Value,
                    match.Groups[3].Value,
                    match.Groups[4].Value,
                    match.Groups[5].Value
                );
                string ans = $"{parsed.Item2}{parsed.Item3}{parsed.Item4}";
                longest_v_len = Math.Max(longest_v_len, ans.Length);
                this.ans.Add(ans);
                parsed_sentences.Add(parsed);
            }
        }

        public PartVocSentence(in string[] sourceLines, PartConfig config) : base(sourceLines, config)
        {
            parsed_sentences = [];
            longest_v_len = 0;

            foreach (string line in sourceLines)
            {
                Match match = SlotPattern().Match(line);
                if (!match.Success)
                    continue;
                var parsed = (
                    match.Groups[1].Value,
                    match.Groups[2].Value,
                    match.Groups[3].Value,
                    match.Groups[4].Value,
                    match.Groups[5].Value
                );
                string ans = $"{parsed.Item2}{parsed.Item3}{parsed.Item4}";
                longest_v_len = Math.Max(longest_v_len, ans.Length);
                this.ans.Add(ans);
                parsed_sentences.Add(parsed);
            }
        }

        public override Tuple<string, List<string>> Produce()
        {
            int vl = config.underscoreLengthFixed ? config.underscoreLength : longest_v_len + 2;
            int bl = config.underscoreInSentenceLength;
            int first_idx = config.index + 1;
            int question_count = ans.Count;
            int last_idx = first_idx + question_count - 1;
            int qi_len = last_idx.ToString().Length + 2;
            string qi_alignment_format = $"{{0,-{qi_len}}}";
            Func<int, string> qif =
                config.markQuestionNumber ?
                (int i) => string.Format(format: qi_alignment_format, $"{i}.") :
                (int _) => "";
            string ans_slot = new('_', vl);
            string[] questions = new string[question_count];
            for (int i = 0; i < question_count; ++i)
            {
                questions[i] = string.Join("",
                    qif(i + first_idx),
                    ans_slot,
                    ' ',
                    parsed_sentences[i].Item1,
                    parsed_sentences[i].Item2,
                    new string('_', bl > 0 ? bl : parsed_sentences[i].Item3.Length),
                    parsed_sentences[i].Item4,
                    parsed_sentences[i].Item5
                );
            }
            config.index = last_idx;
            return new(string.Join("\n", questions), ans);
        }

        [GeneratedRegex(@"(.*)\[(?:(\w*)\|)?([\w -]*)(?:\|(\w*))?\](.*)")]
        private static partial Regex SlotPattern();
    }

    public partial class PartVocMatch : PartBase
    {
        private readonly string[] hints;
        private readonly Func<int, string> qf;

        public PartVocMatch(string source, PartConfig config) : base(source, config)
        {
            List<string> words = [];
            int longest = 0;
            Func<string, string> maybe_lower = config.lowerCase ? s => s.ToLower() : s => s;
            foreach (Match match in SlotPattern().Matches(source).Cast<Match>())
            {
                words.Add(match.Groups[1].Value);
                longest = Math.Max(longest, match.Groups[1].Length);
            }
            int quantity = words.Count;
            hints = new string[quantity];
            int[] orders = Enumerable.Range(0, quantity).ToArray();
            Random random = new();
            random.Shuffle(orders);
            bool label;
            if (config.provideCode)
            {
                if (quantity > config.codes.Length)
                {
                    label = false;
                    config.messages.Item2.Add("Too many slots, choice disabled");
                }
                else
                {
                    label = true;
                }
            }
            else
            {
                label = false;
            }
            if (label)
            {
                ans = (
                    from i in Enumerable.Range(0, quantity)
                    select $"({config.codes[orders[i]]}){words[i]}"
                ).ToList();
            }
            else
            {
                ans = [.. words];
            }
            for (int i = 0; i < quantity; ++i)
            {
                hints[orders[i]] = ans[i];
            }
            int vl = config.underscoreLengthFixed || label ?
                config.underscoreLength :
                Math.Max(config.underscoreLength, longest + 2);
            (string l_us, string r_us) = config.questionNumberAlignment switch
            {
                '>' => (new string('_', vl), ""),
                '<' => ("", new string('_', vl)),
                _ => (new string('_', vl >> 1), new string('_', vl >> 1))
            };

            string qfs = config.markQuestionNumber ? $"{l_us}({{0}}){r_us}" : $"{l_us}{r_us}";
            qf = i => string.Format(qfs, i);
        }

        public PartVocMatch(in string[] sourceLines, PartConfig config) : this(string.Join('\n', sourceLines), config)
        {
        }

        private string Repl(Match match)
        {
            return qf(++config.index);
        }


        public override Tuple<string, List<string>> Produce()
        {
            return new(
                $"{string.Join(config.wordListSeparator, hints)}{"\n****\n"}{SlotPattern().Replace(source, Repl)}",
                ans
            );
        }

        [GeneratedRegex(@"\[(.*?)\]")]
        private static partial Regex SlotPattern();
    }

    class ToParts
    {
        public delegate PartBase PartBaseFactory(string source, PartConfig config);
        public delegate PartBase PartBaseFactoryLines(string[] source, PartConfig config);
        public static PartBaseFactoryLines? ChoosePartType(string type)
        {
            return type switch
            {
                "fill" => (string[] sourceLns, PartConfig config) => new PartVocSlot(sourceLns, config),
                "pair" or "match" => (string[] sourceLns, PartConfig config) => new PartVocMatch(sourceLns, config),
                "voc" => (string[] sourceLns, PartConfig config) => new PartVocSentence(sourceLns, config),
                _ => null,
            };
        }
    }
}

