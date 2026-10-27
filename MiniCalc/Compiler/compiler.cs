public class Compiler(Tree tree)
{
    private const int MaxVariables = 256;

    private readonly List<Instruction> _instructions = new();
    private readonly Dictionary<string, byte> _variableToIndex = new();

    public List<Instruction> Compile()
    {
        foreach (var statement in tree.Statements)
            Compile(statement);

        return _instructions;
    }


    private void Compile(Statement statement)
    {
        switch (statement)
        {
            case VarDeclarationStatement declaration:
                CompileVarDeclaration(declaration);
                break;

            case AssignmentStatement assignment:
                CompileAssignment(assignment);
                break;

            case PrintStatement print:
                Compile(print.Expression);
                _instructions.Add(new PrintInstruction());
                break;

            case ExpressionStatement expressionStatement:
                Compile(expressionStatement.Expression);
                _instructions.Add(new PopInstruction());
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(statement));
        }
    }

    private void CompileVarDeclaration(VarDeclarationStatement declaration)
    {
        var name = declaration.Name;

        if (_variableToIndex.ContainsKey(name.Text))
            throw new CompileException($"variable '{name.Text}' is already declared", name);

        if (_variableToIndex.Count >= MaxVariables)
            throw new CompileException($"too many variables (maximum is {MaxVariables})", name);

        Compile(declaration.Value);

        var index = (byte)_variableToIndex.Count;
        _variableToIndex[name.Text] = index;
        _instructions.Add(new StoreVarInstruction(index));
    }

    private void CompileAssignment(AssignmentStatement assignment)
    {
        var index = Lookup(assignment.Name);
        Compile(assignment.Value);
        _instructions.Add(new StoreVarInstruction(index));
    }


    private void Compile(Expression expression)
    {
        switch (expression)
        {
            case BinaryExpression binary:
                Compile(binary.Left);
                Compile(binary.Right);
                _instructions.Add(OperatorToInstruction(binary.Operator));
                break;

            case NumberLiteralExpression number:
                _instructions.Add(new LoadIntInstruction(number.Value));
                break;

            case VariableExpression variable:
                _instructions.Add(new LoadVarInstruction(Lookup(variable.Name)));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(expression));
        }
    }

    private byte Lookup(Token name)
    {
        if (!_variableToIndex.TryGetValue(name.Text, out var index))
            throw new CompileException($"variable '{name.Text}' is not declared", name);
        return index;
    }

    private static Instruction OperatorToInstruction(TokenType op) => op switch
    {
        TokenType.Plus => new AddInstruction(),
        TokenType.Minus => new SubInstruction(),
        TokenType.Star => new MulInstruction(),
        TokenType.Slash => new DivInstruction(),
        _ => throw new InvalidOperationException($"unexpected operator {op}")
    };
}
