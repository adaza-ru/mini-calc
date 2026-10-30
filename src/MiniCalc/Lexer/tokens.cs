public record Token(TokenType Type, string Text, int Line, int Column);

public enum TokenType
{
    Equal,
    Plus,
    Minus,
    Star,
    Slash,
    Semicolon,
    Num,
    Ident,
    Var,
    Print,
    LeftParen,
    RightParen,
    EndOfFile
}
