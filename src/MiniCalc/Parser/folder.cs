public class ConstantFolder(Tree tree)
{
    public Tree Fold() =>
        new(tree.Statements.Select(FoldStatement).ToList());

    private static Statement FoldStatement(Statement statement) => statement switch
    {
        VarDeclarationStatement s => s with { Value = FoldExpression(s.Value) },
        AssignmentStatement s => s with { Value = FoldExpression(s.Value) },
        PrintStatement s => s with { Expression = FoldExpression(s.Expression) },
        ExpressionStatement s => s with { Expression = FoldExpression(s.Expression) },
        _ => throw new ArgumentOutOfRangeException(nameof(statement))
    };

    private static Expression FoldExpression(Expression expression) => expression switch
    {
        BinaryExpression binary => FoldBinary(binary),
        _ => expression
    };

    private static Expression FoldBinary(BinaryExpression binary)
    {
        var left = FoldExpression(binary.Left);
        var right = FoldExpression(binary.Right);

        if (left is NumberLiteralExpression(var a) && right is NumberLiteralExpression(var b))
        {
            switch (binary.Operator)
            {
                case TokenType.Plus: return new NumberLiteralExpression(a + b);
                case TokenType.Minus: return new NumberLiteralExpression(a - b);
                case TokenType.Star: return new NumberLiteralExpression(a * b);
                case TokenType.Slash when b != 0 && !(a == int.MinValue && b == -1):
                    return new NumberLiteralExpression(a / b);
            }
        }

        return binary with { Left = left, Right = right };
    }
}
