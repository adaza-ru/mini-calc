public class CompileException(string message, int line, int column) : Exception(message)
{
    public int Line { get; } = line;
    public int Column { get; } = column;
    
    public CompileException(string message, Token token)
        : this(message, token.Line, token.Column) { }
}