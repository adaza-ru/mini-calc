/*
Grammar (lowest to highest precedence):

    program := statement* EOF
    statement := "var" IDENT "=" expr ";"
              | "print" expr ";"
              | IDENT "=" expr ";"
              | expr ";"
    expr := addition
    addition := product (("+" | "-") product)*
    product := negative (("*" | "/") negative)*
    negative := "-" negative | primary
    primary := NUM | IDENT | "(" expr ")"
*/

public class Parser(List<Token> tokens)
{
    private int _position = 0;
    private Token Current => tokens[_position];
    private Token Previous => tokens[_position - 1];

    private Token Peek => tokens[Math.Min(_position + 1, tokens.Count - 1)];

    public Tree Parse()
    {
        var statements = new List<Statement>();

        while (Current.Type != TokenType.EndOfFile)
            statements.Add(ParseStatement());

        return new Tree(statements);
    }

    private Statement ParseStatement()
    {
        if (Match(TokenType.Var))
            return ParseVarDeclaration();
 
        if (Match(TokenType.Print))
            return ParsePrintStatement();

        if (Current.Type == TokenType.Ident && Peek.Type == TokenType.Equal)
            return ParseAssignmentExpression();
 
        return ParseExpressionStatement();
    }
    
    private Statement ParseVarDeclaration()
    {
        var name = Consume(TokenType.Ident);
        Consume(TokenType.Equal);
        var value = ParseExpression();
        Consume(TokenType.Semicolon);
        return new VarDeclarationStatement(name, value);
    }

    private Statement ParsePrintStatement()
    {
        var value = ParseExpression();
        Consume(TokenType.Semicolon);
        return new PrintStatement(value);
    }

    private Statement ParseAssignmentExpression()
    {
        var name = Consume(TokenType.Ident);
        Consume(TokenType.Equal);
        var value = ParseExpression();
        Consume(TokenType.Semicolon);
        return new AssignmentStatement(name, value);
    }

    private Statement ParseExpressionStatement()
    {
        var expression = ParseExpression();
        Consume(TokenType.Semicolon);
        return new ExpressionStatement(expression);
    }

    private Expression ParseExpression()
    {
        return ParseAdditionExpression();
    }

    private Expression ParseAdditionExpression()
    {
        var left = ParseProductExpression();
 
        while (Match(TokenType.Plus) || Match(TokenType.Minus))
        {
            var operatorType = Previous.Type;
            var right = ParseProductExpression();
            left = new BinaryExpression(left, operatorType, right);
        }
 
        return left;
    }

    private Expression ParseProductExpression()
    {
        var left = ParseNegativeExpression();
 
        while (Match(TokenType.Star) || Match(TokenType.Slash))
        {
            var operatorType = Previous.Type;
            var right = ParseNegativeExpression();
            left = new BinaryExpression(left, operatorType, right);
        }
        return left;
    }

    private Expression ParseNegativeExpression()
    {
        if (Match(TokenType.Minus))
            return new BinaryExpression(
                new NumberLiteralExpression(0), TokenType.Minus, ParseNegativeExpression()
            );
        return ParsePrimaryExpression();
    }

    private Expression ParsePrimaryExpression()
    {
        if (Match(TokenType.Num))
        {
            if (!int.TryParse(Previous.Text, out int value))
                throw new CompileException($"Overflow/Underflow Error: {Previous.Text}", Previous);
            return new NumberLiteralExpression(value);
        }
        
        if (Match(TokenType.Ident))
            return new VariableExpression(Previous);
        
        if (Match(TokenType.LeftParen))
        {
            var expression = ParseExpression();
            Consume(TokenType.RightParen);
            return expression;
        }

        throw new CompileException($"expected an expression but found {Current.Text}", Current);
    }

    private Token Consume(TokenType expected)
    {
        if(Current.Type != expected)
            throw new CompileException($"Expected {expected}, but found {Current.Type}", Current);
        _position++;
        return Previous;
    }

    private bool Match(TokenType expected)
    {
        if (Current.Type != expected)
            return false;
        _position++;
        return true;
    }
}
