public record Tree(List<Statement> Statements);
 
public abstract record Statement;
public record VarDeclarationStatement(Token Name, Expression Value) : Statement;
public record AssignmentStatement(Token Name, Expression Value) : Statement;
public record PrintStatement(Expression Expression) : Statement;
public record ExpressionStatement(Expression Expression) : Statement;
 
public abstract record Expression;
public record NumberLiteralExpression(int Value) : Expression;
public record VariableExpression(Token Name) : Expression;
public record BinaryExpression(Expression Left, TokenType Operator, Expression Right) : Expression;
