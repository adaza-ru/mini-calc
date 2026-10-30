public class ConstantFolder(Tree tree)
{
    public Tree Fold() =>
        new(tree.Statements.Select(FoldStatement).ToList());

    private static Statement FoldStatement(Statement statement)
    {
        switch (statement)
        {
            case VarDeclarationStatement s:
                return s with { Value = FoldExpression(s.Value) };

            case AssignmentStatement s:
                return s with { Value = FoldExpression(s.Value) };

            case PrintStatement s:
                return s with { Expression = FoldExpression(s.Expression) };

            case ExpressionStatement s:
                return s with { Expression = FoldExpression(s.Expression) };

            default:
                throw new ArgumentOutOfRangeException(nameof(statement));
        }
    }

    private static Expression FoldExpression(Expression expression)
    {
        if (expression is BinaryExpression binary)
            return FoldBinary(binary);
                
        return expression;
    }

    private static Expression FoldBinary(BinaryExpression binary)
    {
        var left = FoldExpression(binary.Left);
        var right = FoldExpression(binary.Right);

        if (left is NumberLiteralExpression(var a) && right is NumberLiteralExpression(var b))
        {
            switch (binary.Operator)
            {
                case TokenType.Plus:
                    return new NumberLiteralExpression(a + b);

                case TokenType.Minus:
                    return new NumberLiteralExpression(a - b);

                case TokenType.Star:
                    return new NumberLiteralExpression(a * b);

                case TokenType.Slash when b != 0 && !(a == int.MinValue && b == -1):
                    return new NumberLiteralExpression(a / b);
            }
        }
        return binary with { Left = left, Right = right };
    }
}
