public class Scanner(string source)
{
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["var"] = TokenType.Var,
        ["print"] = TokenType.Print,
    };

    public List<Token> Scan()
    {
        var tokens = new List<Token>();
        var position = 0;
        var line = 1;
        var lineStart = 0;
        var column = 1;



        void Add(TokenType type, string text) => tokens.Add(new Token(type, text, line, column));

        while (position < source.Length)
        {
            char character = source[position];
            column = position - lineStart + 1;

            switch (character)
            {
                case ' ' or '\t' or '\r':
                    position++;
                    continue;

                case '\n':
                    position++;
                    line++;
                    lineStart = position;
                    continue;

                case '+':
                    Add(TokenType.Plus, "+");
                    position++;
                    break;

                case '-':
                    Add(TokenType.Minus, "-");
                    position++;
                    break;

                case '*':
                    Add(TokenType.Star, "*");
                    position++;
                    break;

                case '/':
                    Add(TokenType.Slash, "/");
                    position++;
                    break;

                case ';':
                    Add(TokenType.Semicolon, ";");
                    position++;
                    break;

                case '=':
                    Add(TokenType.Equal, "=");
                    position++;
                    break;

                case '(':
                    Add(TokenType.LeftParen, "(");
                    position++;
                    break;

                case ')':
                    Add(TokenType.RightParen, ")");
                    position++;
                    break;

                case >= '0' and <= '9':
                    var numStart = position;
                    while (position < source.Length && source[position] is >= '0' and <= '9')
                        position++;
                    Add(TokenType.Num, source[numStart..position]);
                    break;

                case >= 'a' and <= 'z':
                    var identStart = position;
                    while (position < source.Length && (source[position] is >= 'a' and <= 'z' or >= '0' and <= '9' or '_'))
                        position++;
                    var identText = source[identStart..position];
                    Add(Keywords.GetValueOrDefault(identText, TokenType.Ident), identText);
                    break;

                default:
                    throw new CompileException($"Unknown character '{character}'", line, column);
            }
        }

        column = position - lineStart + 1;
        Add(TokenType.EndOfFile, "EOF");
        return tokens;
    }
}
