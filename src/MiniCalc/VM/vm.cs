public class RuntimeException(string message) : Exception(message);

public class VirtualMachine(List<Instruction> instructions)
{
    private readonly Stack<int> _stack = new();
    private readonly int[] _registers = new int[256];

    public void Run()
    {
        foreach (var instruction in instructions)
        {
            switch (instruction)
            {
                case LoadIntInstruction loadInt:
                    _stack.Push(loadInt.Value);
                    break;

                case LoadVarInstruction loadVar:
                    _stack.Push(_registers[loadVar.Index]);
                    break;

                case StoreVarInstruction storeVar:
                    _registers[storeVar.Index] = _stack.Pop();
                    break;

                case AddInstruction:
                    BinaryOperation((left, right) => left + right);
                    break;

                case SubInstruction:
                    BinaryOperation((left, right) => left - right);
                    break;

                case MulInstruction:
                    BinaryOperation((left, right) => left * right);
                    break;

                case DivInstruction:
                    BinaryOperation(Divide);
                    break;

                case PrintInstruction:
                    Console.WriteLine(_stack.Pop());
                    break;

                case PopInstruction:
                    _stack.Pop();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(instruction));
            }
        }
    }

    private void BinaryOperation(Func<int, int, int> operation)
    {
        var right = _stack.Pop();
        var left = _stack.Pop();
        _stack.Push(operation(left, right));
    }

    private static int Divide(int left, int right)
    {
        if (right == 0)
            throw new RuntimeException("division by zero");

        if (left == int.MinValue && right == -1)
            throw new RuntimeException("integer overflow in division");

        return left / right; 
    }
}
