using System.Globalization;
using System.Text;

namespace Riok.Mapperly.Helpers;

public static class MemberNamingUtil
{
    private static readonly Processor _camelCaseProcessor = new(string.Empty, LetterCase.Lower, LetterCase.Upper, LetterCase.Lower);
    private static readonly Processor _pascalCaseProcessor = new(string.Empty, LetterCase.Upper, LetterCase.Upper, LetterCase.Lower);
    private static readonly Processor _snakeCaseProcessor = new("_", LetterCase.Lower);
    private static readonly Processor _upperSnakeCaseProcessor = new("_", LetterCase.Upper);
    private static readonly Processor _kebabCaseProcessor = new("-", LetterCase.Lower);
    private static readonly Processor _upperKebabCaseProcessor = new("-", LetterCase.Upper);

    public static string ToCamelCase(this string str) => _camelCaseProcessor.Process(str);

    public static string ToPascalCase(this string str) => _pascalCaseProcessor.Process(str);

    public static string ToSnakeCase(this string str) => _snakeCaseProcessor.Process(str);

    public static string ToUpperSnakeCase(this string str) => _upperSnakeCaseProcessor.Process(str);

    public static string ToKebabCase(this string str) => _kebabCaseProcessor.Process(str);

    public static string ToUpperKebabCase(this string str) => _upperKebabCaseProcessor.Process(str);

    private readonly struct Processor(
        string separator,
        LetterCase startLetterCase,
        LetterCase wordStartLetterCase,
        LetterCase wordLetterCase
    )
    {
        public string Separator { get; } = separator;
        public Func<char, char> ModifyStartLetter { get; } = GetCharModifier(startLetterCase);
        public Func<char, char> ModifyWordStartLetter { get; } = GetCharModifier(wordStartLetterCase);
        public Func<char, char> ModifyLetter { get; } = GetCharModifier(wordLetterCase);

        public Processor(string separator, LetterCase letterCase)
            : this(separator, letterCase, letterCase, letterCase) { }

        public string Process(string str)
        {
            if (str.Length == 0)
                return str;

            var capacity = str.Length;
            if (Separator.Length > 0)
            {
                // average word length is around 5 chars
                // add 20% capacity to account for the separator
                capacity = (int)(capacity * 1.2);
            }

            var state = new ProcessorState(capacity, this);
            foreach (var c in str)
            {
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.UppercaseLetter:
                        state.AppendUpper(c);
                        break;
                    case UnicodeCategory.LowercaseLetter:
                        state.AppendLower(c);
                        break;
                    case UnicodeCategory.DecimalDigitNumber:
                        state.AppendDigit(c);
                        break;
                    case UnicodeCategory.ConnectorPunctuation when !state.IsAtStart:
                        state.AppendSeparator();
                        break;
                }
            }

            return state.Done();
        }

        private static Func<char, char> GetCharModifier(LetterCase letterCase) =>
            letterCase switch
            {
                LetterCase.Lower => char.ToLowerInvariant,
                LetterCase.Upper => char.ToUpperInvariant,
                _ => throw new ArgumentOutOfRangeException(nameof(letterCase), letterCase, "Unknown letter case"),
            };
    }

    private struct ProcessorState(int capacity, Processor processor)
    {
        private readonly StringBuilder _sb = new(capacity);
        private State _state = State.Start;
        private State _prevState = State.Start;

        public readonly bool IsAtStart => _state == State.Start;

        public readonly string Done() => _sb.ToString();

        public void AppendSeparator() => UpdateState(State.Separator);

        public void AppendUpper(char c)
        {
            switch (_state)
            {
                case State.Start:
                    AppendStart(c, State.UppercaseLetter);
                    break;
                case State.LowercaseLetterOrDigit:
                case State.Separator:
                    AppendNewWord(c, State.UppercaseLetter);
                    break;
                case State.UppercaseLetter:
                    Append(c, State.UppercaseLetter);
                    break;
            }
        }

        public void AppendLower(char c)
        {
            switch (_state)
            {
                case State.Start:
                    AppendStart(c, State.LowercaseLetterOrDigit);
                    break;
                case State.Separator:
                    AppendNewWord(c, State.LowercaseLetterOrDigit);
                    break;
                case State.LowercaseLetterOrDigit:
                    Append(c, State.LowercaseLetterOrDigit);
                    break;
                case State.UppercaseLetter:
                    if (_prevState == State.UppercaseLetter)
                    {
                        var prevC = _sb[^1];
                        _sb.Remove(_sb.Length - 1, 1);
                        _sb.Append(processor.Separator);
                        _sb.Append(processor.ModifyWordStartLetter(prevC));
                    }

                    Append(c, State.LowercaseLetterOrDigit);
                    break;
            }
        }

        public void AppendDigit(char c)
        {
            switch (_state)
            {
                case State.Start:
                    AppendStart(c, State.LowercaseLetterOrDigit);
                    break;
                case State.Separator:
                    AppendNewWord(c, State.LowercaseLetterOrDigit);
                    break;
                case State.LowercaseLetterOrDigit:
                case State.UppercaseLetter:
                    Append(c, State.LowercaseLetterOrDigit);
                    break;
            }
        }

        private void AppendNewWord(char c, State newState)
        {
            _sb.Append(processor.Separator);
            _sb.Append(processor.ModifyWordStartLetter(c));
            UpdateState(newState);
        }

        private void AppendStart(char c, State newState)
        {
            _sb.Append(processor.ModifyStartLetter(c));
            UpdateState(newState);
        }

        private void Append(char c, State newState)
        {
            _sb.Append(processor.ModifyLetter(c));
            UpdateState(newState);
        }

        private void UpdateState(State newState)
        {
            _prevState = _state;
            _state = newState;
        }

        private enum State
        {
            Start,
            UppercaseLetter,
            LowercaseLetterOrDigit,
            Separator,
        }
    }

    private enum LetterCase
    {
        Lower,
        Upper,
    }
}
