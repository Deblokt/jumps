# JUMPS
Stack based Turing complete programming language

## Status
WIP. More features to be added when I get the time to play with it more. Mind that it was created on a Sunday afternoon. Source code looks unrefined. Deal with it.

## Features
Two stacks. One common register. Conditional jumps with labels. Support for decimal, binary and hex numeric systems. Support for whole numbers (signed int32). Support for chars. Stopwatch for measuring performance.

## How to use
### Run
```
dotnet run showcase.jmps
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
dotnet run showcase.jmps -w
```

## Instruction set
* NOP
* PUSHa
* PUSHb
* POPa
* POPb
* LBL
* JUMP
* JUMPSa
* JUMPSb
* DUPa
* DUPb
* ADDa
* ADDb
* NEGa
* NEGb
* WRITEa
* WRITEb
* WRITECa
* WRITECb
* CLR
* HLT
* STARTW
* STOPW

## Showcase program
```
STARTW
NOP
STOPW

LBL 'S'

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


STARTW
PUSHa 0
DUPa
WRITEa
PUSHa ' '
WRITECa
PUSHa 1
DUPa
WRITEa
PUSHa ' '
WRITECa
PUSHb 1
LBL 0xFACA
ADDa
DUPa
WRITEa
PUSHa ' '
WRITECa
DUPa
POPa
PUSHb
ADDb
DUPb
WRITEb
PUSHa ' '
WRITECa
DUPb
POPb
PUSHa
DUPa
PUSHa 1000000000
JUMPSa 0xFACA
POPa
POPa
POPb
PUSHa 0x0A
WRITECa
STOPW
CLR

JUMP 'E'
HLT 1000
JUMP 'S'

LBL 'E'
CLR
```
