# JUMPS
Stack based Turing complete programming language and runtime

## Status
WIP. More features to be added when I get the time to play with it more. Mind that it was created on a Sunday afternoon. Source code looks unrefined. Deal with it.

## Features
Two stacks. One common register. Conditional jumps with labels. Support for integers (signed int32) in decimal, binary and hex numeral systems. Support for chars. Stopwatch for measuring performance.

### Future plans
* Stack to buffer stdout
* Stack to read stdin
* Common register refac to stack
* Tidy up the code
* Optimizations

#### Endgame
* Rewrite runtime in C and run on bare metal (no OS, boot directly into JUMPS runtime)
* Write a game or something fun in JUMPS

## How to use
### Run
```
dotnet run showcase.jmps
```
or
```
dotnet run showcase.jmps -c Release
```

### Debug
```
dotnet run showcase.jmps -d
```
or
```
dotnet run showcase.jmps -d > out.txt
```

### Debug interactive (step by step)
```
dotnet run showcase.jmps -di
```

### Stopwatch (performance)
```
dotnet run showcase.jmps -w -c Release
```

## Instruction set

| Instruction | Description | Has param? |
| ----------- | ----------- | ---------- |
| NOP         | Does nothing. | No |
| PUSHa       | Pushes a value on stack __a__. | Optional. If not provided the value is read from register __c__. |
| PUSHb       | Pushes a value on stack __b__. | Optional. If not provided the value is read from register __c__. |
| POPa        | Pops a value from stack __a__ into register __c__. | No |
| POPb        | Pops a value from stack __b__ into register __c__. | No |
| LBL         | Label (marker) for jump location. | Yes |
| JUMP        | Unconditional jump. | Yes (jump label) |
| JUMPSa      | Conditional jump. Pops two values from stack __a__ and jumps when the second value is smaller ie topmost value is bigger. | Yes (jump label) |
| JUMPSb      | Conditional jump. Pops two values from stack __b__ and jumps when the second value is smaller ie topmost value is bigger. | Yes (jump label) |
| ADDa        | Adds two values from stack __a__. Pops two values from stack __a__ and pushes the result back on stack __a__. | No |
| ADDb        | Adds two values from stack __b__. Pops two values from stack __b__ and pushes the result back on stack __b__. | No |
| NEGa        | Negates a value on stack __a__. Pops value from stack __a__ and pushes the negated result back on stack __a__. | No |
| NEGb        | Negates a value on stack __b__. Pops value from stack __b__ and pushes the negated result back on stack __b__. | No |
| WRITEa      | Writes a value to stdout. Value is popped from stack __a__. | No |
| WRITEb      | Writes a value to stdout. Value is popped from stack __b__. | No |
| WRITECa     | Writes a value as char to stdout. Value is popped from stack __a__. | No |
| WRITECb     | Writes a value as char to stdout. Value is popped from stack __b__. | No |
| CLR         | Clears registers. Throws an error if stacks are not empty or if the stopwatch is running. | No |
| HLT         | Halts the execution. | Optional. Interval in milliseconds. Indefinitely if not provided. |
| STARTW      | Starts the stopwatch. | No |
| STOPW       | Stops the stopwatch. | No |

## Showcase program
```
// This showcase contains 4 programs that you can run back to back (at once)

// #1: Measures time using stopwatch while doing nothing
STARTW
NOP
STOPW

LBL 'S'

// #2: Hello world
PUSHa 'H'
PUSHa 'e'
PUSHa 'l'
PUSHa 'l'
PUSHa 'o'
PUSHa ','
PUSHa ' '
PUSHa 'W'
PUSHa 'o'
PUSHa 'r'
PUSHa 'l'
PUSHa 'd'
PUSHa '!'
PUSHa 0x0A // LF char
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
POPa
PUSHb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
WRITECb
CLR

// #3: Math
PUSHa 0b1001
PUSHa 0b1101
PUSHa 0xFF
PUSHa 0xFF
NEGa
PUSHa 255
PUSHa 'X'
PUSHa -69
PUSHa 'E'
ADDa
ADDa
ADDa
ADDa
ADDa
ADDa
ADDa
WRITEa
PUSHa 0x0A
WRITECa
CLR

// #4: Fibonacci sequence up to 1B
STARTW
PUSHa 0
POPa
PUSHa
PUSHa
WRITEa
PUSHa ' '
WRITECa
PUSHa 1
POPa
PUSHa
PUSHa
WRITEa
PUSHa ' '
WRITECa
PUSHb 1
LBL 0xFACA
ADDa
POPa
PUSHa
PUSHa
WRITEa
PUSHa ' '
WRITECa
POPa
PUSHa
PUSHa
POPa
PUSHb
ADDb
POPb
PUSHb
PUSHb
WRITEb
PUSHa ' '
WRITECa
POPb
PUSHb
PUSHb
POPb
PUSHa
POPa
PUSHa
PUSHa
PUSHa 1000000000
JUMPSa 0xFACA
POPa
POPa
POPb
PUSHa 0x0A
WRITECa
STOPW
CLR

JUMP 'E' // comment this line out to go in a loop
HLT 1000
JUMP 'S'

LBL 'E'
CLR
```

### Expected result
```
Hello, World!
365
0 1 1 2 3 5 8 13 21 34 55 89 144 233 377 610 987 1597 2584 4181 6765 10946 17711 28657 46368 75025 121393 196418 317811 514229 832040 1346269 2178309 3524578 5702887 9227465 14930352 24157817 39088169 63245986 102334155 165580141 267914296 433494437 701408733 1134903170
```

#### Stopwatch result
```
sw parser: 00:00:00.0038227 (3ms) // 'showcase.jmps' syntax parsing time
sw jumps: 00:00:00.0000022 (0ms) // resolving jump locations time
sw: 00:00:00.0000004 (0ms) // NOP time
Hello, World!
365
0 1 1 2 3 5 8 13 21 34 55 89 144 233 377 610 987 1597 2584 4181 6765 10946 17711 28657 46368 75025 121393 196418 317811 514229 832040 1346269 2178309 3524578 5702887 9227465 14930352 24157817 39088169 63245986 102334155 165580141 267914296 433494437 701408733 1134903170 
sw: 00:00:00.0001504 (0ms) // Fibonacci sequence time
```
