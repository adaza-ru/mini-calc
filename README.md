<div align="center">

# MiniCalc - My First Approach To Compilers

**A small compiler and stack-based virtual machine in C# — lexing, recursive descent parsing, bytecode generation and execution, [based on the compiler Erik Schierboom live-coded at NDC Oslo 2026](https://www.youtube.com/watch?v=tYFljFUaAw0).**

![C#](https://img.shields.io/badge/language-C%23-239120?logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Architecture](https://img.shields.io/badge/architecture-lexer%20%E2%86%92%20parser%20%E2%86%92%20compiler%20%E2%86%92%20VM-informational)
![Status](https://img.shields.io/badge/status-learning%20project-yellow)

</div>

---

## About this project

MiniCalc is my first compiler and my first C# project. It started from the compiler Erik Schierboom live-coded in his talk [*Writing a compiler from scratch*](https://www.youtube.com/watch?v=tYFljFUaAw0) at NDC Oslo 2026, which supports `var` declarations, single-digit integers, addition and multiplication. I coded along with the talk and then extended the language and the tooling on my own, to make sure I understood every stage of the pipeline.

```
var x = 3;
var result = 1 + 2 * x;
print result + 10;
```

```
17
```

## Features

- Integer arithmetic with `+`, `-`, `*`, `/`, unary minus and parentheses, with the usual precedence and left associativity
- Multi-digit integer literals
- Variable declaration (`var`), reassignment and `print`
- Errors reported as `file:line:column`, for both syntax and semantic problems
- Debug flags to inspect every stage: tokens, syntax tree and bytecode
- Meaningful exit codes, so it can be used from scripts

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)

## Quick start

Save the program above as `example.mini` and run:

```
dotnet run -- example.calc
```

Or build it once and run the binary:

```
dotnet build -c Release
```

### Usage

```
MiniCalc <file> [options]

Options:
  --tokens      print the token stream
  --ast         print the syntax tree
  --bytecode    print the generated instructions
  -h, --help    show this help
```

If any of `--tokens`, `--ast` or `--bytecode` is given, the program is compiled but **not executed**.

| Exit code | Meaning |
|---|---|
| `0` | Success |
| `1` | Compile error or runtime error |
| `2` | Usage error (no arguments, unknown option, wrong number of files, file not found) |

## The language

A program is a sequence of statements, and every statement ends with `;`.

```
var a = 10;
a = a * 2 + 1;
print (a - 1) / 4;
-a + 3;
```

The four statements above declare a variable, reassign it, print an expression, and evaluate an expression whose result is discarded.

- A variable must be declared before it is used, and can only be declared once.
- The language has no comments.
- Values are 32-bit signed integers.
- Integer division truncates toward zero, as in C.
- Identifiers start with a lowercase letter and may contain lowercase letters, digits and `_`.
- Reserved words: `var`, `print`.
- Whitespace (spaces, tabs, newlines) is ignored.

### Grammar

Lowest to highest precedence:

```
program   := statement* EOF
statement := "var" IDENT "=" expr ";"
           | "print" expr ";"
           | IDENT "=" expr ";"
           | expr ";"
expr      := addition
addition  := product (("+" | "-") product)*
product   := negative (("*" | "/") negative)*
negative  := "-" negative | primary
primary   := NUM | IDENT | "(" expr ")"
```

## How it works

The source goes through four stages:

```
 source  ──▶  Scanner  ──▶  Parser  ──▶  Compiler  ──▶  VirtualMachine
 text         tokens        AST          bytecode        output
```

| Stage | Class | What it does |
|---|---|---|
| Lexing | `Scanner` | Turns text into tokens, each carrying its line and column |
| Parsing | `Parser` | Recursive descent parser, one method per grammar rule, that builds the syntax tree |
| Compilation | `Compiler` | Walks the tree and emits stack-machine instructions; resolves variables to register indices |
| Execution | `VirtualMachine` | Runs the instructions with an operand stack and 256 registers |

Design notes:

- **Precedence by grammar levels.** Each precedence level is its own parser method, so `*` and `/` bind tighter than `+` and `-` without any extra tables.
- **Unary minus lives in the parser.** `-x` is desugared to `0 - x`, so the compiler and the VM don't need a new instruction.
- **The tree keeps tokens.** Variable nodes store the original token, so the compiler can still report the exact line and column of an undeclared or duplicated variable.
- **Keywords are a dictionary.** The scanner reads a whole word and looks it up in a `Keywords` table, which makes adding a keyword a one-line change.
- **Statements leave the stack clean.** Expression statements are followed by a `Pop` and `print` consumes its value, so the stack is empty between statements.

### Inspecting the pipeline

For the example above, `--bytecode` prints:

```
   0  LoadIntInstruction { Value = 3 }
   1  StoreVarInstruction { Index = 0 }
   2  LoadIntInstruction { Value = 1 }
   3  LoadIntInstruction { Value = 2 }
   4  LoadVarInstruction { Index = 0 }
   5  MulInstruction { }
   6  AddInstruction { }
   7  StoreVarInstruction { Index = 1 }
   8  LoadVarInstruction { Index = 1 }
   9  LoadIntInstruction { Value = 10 }
  10  AddInstruction { }
  11  PrintInstruction { }
```

`--tokens` and `--ast` work the same way for the earlier stages.

## Error messages

Compile errors use the `file:line:column: error: message` format:

Using an undeclared variable (`print y;`):

```
example.calc:1:7: error: variable 'y' is not declared
```

A missing expression (`var x = ;`):

```
example.calc:1:9: error: expected an expression but found ;
```

Runtime errors are reported without a position. For `print 1 / 0;`:

```
example.calc: runtime error: division by zero
```

## What changed from the original

| | Original | MiniCalc |
|---|---|---|
| Operators | `+`, `*` | `+`, `-`, `*`, `/`, unary minus, parentheses |
| Numbers | One digit | Multi-digit, with an error on `int` overflow |
| Identifiers | `a-z` | `a-z`, digits and `_` |
| Statements | `var`, expression | `var`, reassignment, `print`, expression |
| Keywords | Checked inline in the scanner | `Keywords` dictionary |
| Errors | Generic exceptions | `CompileException` with line and column, and `RuntimeException` |
| Variables | Redeclaring a variable corrupted register indices | Duplicate and undeclared variables are compile errors; limit of 256 |
| Expression statements | Value left on the stack | Value popped; output is explicit with `print` |
| Running | Script | `.csproj` project with a CLI, debug flags and exit codes |

## Known limitations

- `+`, `-` and `*` wrap around silently on 32-bit overflow. Division by zero and `int.MinValue / -1` are runtime errors.
- `-2147483648` can't be written as a literal, because `2147483648` doesn't fit in an `int`.
- Assignment is a statement, not an expression, so `a = b = 3;` is not valid.
- Identifiers are lowercase only, and there is a single global scope.
- At most 256 variables.
- There are no types other than `int`, no control flow and no functions.

## What I learned

**Compiler concepts**

- **Lexical analysis:** turning raw text into tokens, tracking line and column, and handling keywords with a lookup table.
- **Syntax analysis:** writing a recursive descent parser and expressing the language as a grammar.
- **Precedence and associativity:** getting `1 - 2 - 3` and `1 + 2 * 3` right by layering grammar rules.
- **Abstract syntax trees:** modelling the program as a tree of nodes and deciding what each node needs to carry (for example, tokens for diagnostics).
- **Desugaring:** rewriting `-x` as `0 - x` in the parser to keep the later stages simple.
- **Semantic checks:** detecting undeclared and duplicated variables at compile time.
- **Code generation:** compiling a tree into bytecode for a stack machine, and mapping variables to registers.
- **Virtual machines:** executing instructions with an operand stack, and keeping the stack balanced between statements.

- **C# and .NET:** Creating and organizing a project with the `dotnet` CLI and a `.csproj` file.

## Credits

This project is based on the compiler **Erik Schierboom** live-coded in his talk [*Writing a compiler from scratch*](https://www.youtube.com/watch?v=tYFljFUaAw0) at NDC Oslo 2026. The architecture (scanner, parser, compiler and virtual machine) comes from his talk. The extensions described above are my own work.
