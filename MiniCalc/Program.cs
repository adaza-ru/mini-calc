const string usage = """
    Usage: MiniCalc <file> [options]

    Options:
      --tokens      print the token stream
      --ast         print the syntax tree
      --bytecode    print the generated instructions
      -h, --help    show this help

    If any of --tokens, --ast or --bytecode is given, the program is
    compiled but not executed.
    """;

string[] knownFlags = ["--tokens", "--ast", "--bytecode"];

if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
{
    Console.WriteLine(usage);
    if (args.Length == 0)
        return 2;
    return 0;
}

var flags = args.Where(a => a.StartsWith('-')).ToHashSet();
var paths = args.Where(a => !a.StartsWith('-')).ToList();

foreach (var flag in flags.Except(knownFlags))
{
    Console.Error.WriteLine($"error: unknown option '{flag}'");
    return 2;
}

if (paths.Count != 1)
{
    Console.Error.WriteLine("error: expected exactly one input file");
    return 2;
}

var path = paths[0];
if (!File.Exists(path))
{
    Console.Error.WriteLine($"error: file not found: {path}");
    return 2;
}

var source = File.ReadAllText(path);

try
{
    List<Token> tokens = new Scanner(source).Scan();
    if (flags.Contains("--tokens"))
    {
        foreach (var token in tokens)
            Console.WriteLine(token);
    }

    var tree = new Parser(tokens).Parse();
    if (flags.Contains("--ast"))
    {
        foreach (var statement in tree.Statements)
            Console.WriteLine(statement);
    }

    List<Instruction> instructions = new Compiler(tree).Compile();
    if (flags.Contains("--bytecode"))
    {
        for (var i = 0; i < instructions.Count; i++)
            Console.WriteLine($"{i,4}  {instructions[i]}");
    }

    if (flags.Count > 0)
        return 0;

    new VirtualMachine(instructions).Run();
    return 0;
}

catch (CompileException e)
{
    Console.Error.WriteLine($"{path}:{e.Line}:{e.Column}: error: {e.Message}");
    return 1;
}

catch (RuntimeException e)
{
    Console.Error.WriteLine($"{path}: runtime error: {e.Message}");
    return 1;
}
