# Chip8

This project is an interpreter for the CHIP-8 programming language written in C#.

## How to Use

The interpreter can be called via the CLI by executing the following command:

```
dotnet Chip8Interpreter.dll [PathToInstructions]
```

Where ***PathToInstructions*** is the path to a CHIP-8 program.

### Flags

Some programs may rely on slight changes to the interpreter's behavior. This is due to changes introduced by modern interpreters.

The default configurations (when no flags are set) are meant to resemble the original one, written in the 1970s.

You can customize these behaviors by setting CLI arguments after ***PathToInstructions***, which may help resolve compatibility issues with newer programs.

The following flags are available:

> **NNN**, **X**, and **Y** are subsections of  a CHIP-8 opcode.

- **alt-jump**: Changes the jump with offset instruction, setting  the program counter to the base address (**NNN**) + the value of register **X**.
- **alt-shift**: Changes the bit-shifting instructions to first set the value of register **Y** to the register that will have its bits shifted (register **X**).
- **alt-mem-handl**: Increments the **Index Register** when iterating and loading data from memory into registers and vice versa.

The order of the flags does not matter.