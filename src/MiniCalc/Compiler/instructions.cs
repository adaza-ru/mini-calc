public abstract record Instruction;

public record LoadIntInstruction(int Value) : Instruction;
public record LoadVarInstruction(byte Index) : Instruction;
public record StoreVarInstruction(byte Index) : Instruction;

public record AddInstruction : Instruction;
public record SubInstruction : Instruction;
public record MulInstruction : Instruction;
public record DivInstruction : Instruction;

public record PrintInstruction : Instruction;
public record PopInstruction : Instruction;