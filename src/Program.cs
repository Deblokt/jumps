using System.Collections.Immutable;
using System.Diagnostics;

if (args.Length == 0)
{
    Console.Error.WriteLine("ERR! Provide path to file");
    return;
}

string path = args[0];

bool debug = false;
bool stopwatch = false;
bool interactive = false;

if (args.Length == 2 && args[1].Equals("-w", StringComparison.OrdinalIgnoreCase))
{
    stopwatch = true;
}

if (args.Length == 2 && args[1].Equals("-d", StringComparison.OrdinalIgnoreCase))
{
    debug = true;
    stopwatch = true;
}

if (args.Length == 2 && args[1].Equals("-di", StringComparison.OrdinalIgnoreCase))
{
    debug = true;
    stopwatch = true;
    interactive = true;
}

Span<int?> a = new int?[256];
Span<int?> b = new int?[256];
Span<Instruction> inst = new Instruction[512];

int aPtr = -1, bPtr = -1, instPtr = 0, instSpace = 0;
int? cReg = null;

using StreamReader input = new(path);
ReadOnlySpan<char> line;

Stopwatch sw = Stopwatch.StartNew();

while ((line = input.ReadLine().AsSpan()) != null)
{
    if (line.Length == 0 || line.TrimStart().StartsWith("//"))
    {
        continue;
    }

    line = line.Trim();

    (Op? op, string? opError) = GetOperation(line);
    (int? param, string? paramError) = GetParam(line);

    if (!string.IsNullOrWhiteSpace(opError))
    {
        Console.Error.WriteLine(opError);
        return;
    }

    if (!string.IsNullOrWhiteSpace(paramError))
    {
        Console.Error.WriteLine(paramError);
        return;
    }

    inst[instSpace] = new Instruction() { Operation = op!.Value, Param = param };
    instSpace++;
}

sw.Stop();
if (debug || stopwatch)
{
    Console.WriteLine($"sw parser: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
}

sw = Stopwatch.StartNew();

for (int jumpPtr = 0; jumpPtr < instSpace; jumpPtr++)
{
    Instruction jumpInst = inst[jumpPtr];
    if (jumpInst.Operation == Op.JUMP || jumpInst.Operation == Op.JUMPSa || jumpInst.Operation == Op.JUMPSb)
    {
        if (!jumpInst.Param.HasValue)
        {
            Console.Error.WriteLine($"[{jumpPtr}]: {jumpInst.Operation} ERR! Jump parameter undefined");
            return;
        }

        for (int locPtr = 0; locPtr < instSpace; locPtr++)
        {
            Instruction locInst = inst[locPtr];
            if (locInst.Operation == Op.LBL)
            {
                if (!locInst.Param.HasValue)
                {
                    Console.Error.WriteLine($"[{locPtr}]: {locInst.Operation} ERR! Label parameter undefined");
                    return;
                }

                if (locInst.Param!.Value == jumpInst.Param!.Value)
                {
                    jumpInst.JumpLocation = locPtr;
                    if (debug)
                    {
                        Console.WriteLine($"jump[{jumpInst.Param!.Value}]: {jumpPtr} -> {locPtr}");
                    }
                    break;
                }
            }
        }

        if (jumpInst.JumpLocation is null)
        {
            Console.Error.WriteLine($"[{jumpPtr}]: {jumpInst.Operation} ERR! Missing jump label '{jumpInst.Param!.Value}'");
            return;
        }

        inst[jumpPtr] = jumpInst;
    }
}

sw.Stop();
if (debug || stopwatch)
{
    Console.WriteLine($"sw jumps: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
}

for (instPtr = 0; instPtr < instSpace; instPtr++)
{
    Instruction i = inst[instPtr];
    if (debug)
    {
        Console.WriteLine($"[{instPtr}]: {i.Operation} {i.Param}");
    }

    switch (i.Operation)
    {
        case Op.NOP:
            break;
        case Op.PUSHa:
            string? push_a_error = Push(i.Param, ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(push_a_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {push_a_error}");
                return;
            }
            break;
        case Op.PUSHb:
            string? push_b_error = Push(i.Param, ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(push_b_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {push_b_error}");
                return;
            }
            break;
        case Op.POPa:
            (cReg, string? pop_a_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(pop_a_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {pop_a_error}");
                return;
            }
            break;
        case Op.POPb:
            (cReg, string? pop_b_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(pop_b_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {pop_b_error}");
                return;
            }
            break;
        case Op.JUMP:
            string? jump_error = Jump(i.JumpLocation);
            if (!string.IsNullOrWhiteSpace(jump_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {jump_error}");
                return;
            }
            break;
        case Op.LBL:
            break;
        case Op.JUMPSa:
            (int? jumps_a_second, string? jumps_a_second_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(jumps_a_second_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {jumps_a_second_error}");
                return;
            }

            (int? jumps_a_first, string? jumps_a_first_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(jumps_a_first_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {jumps_a_first_error}");
                return;
            }

            if (jumps_a_first!.Value < jumps_a_second!.Value)
            {
                string? jumps_a_error = Jump(i.JumpLocation);
                if (!string.IsNullOrWhiteSpace(jumps_a_error))
                {
                    Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {jumps_a_error}");
                    return;
                }
            }
            break;
        case Op.JUMPSb:
            (int? jumps_b_second, string? jumps_b_second_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(jumps_b_second_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {jumps_b_second_error}");
                return;
            }

            (int? jumps_b_first, string? jumps_b_first_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(jumps_b_first_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {jumps_b_first_error}");
                return;
            }

            if (jumps_b_first!.Value < jumps_b_second!.Value)
            {
                string? jumps_b_error = Jump(i.JumpLocation);
                if (!string.IsNullOrWhiteSpace(jumps_b_error))
                {
                    Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} {jumps_b_error}");
                    return;
                }
            }
            break;
        case Op.ADDa:
            (int? add_a_second, string? add_a_second_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(add_a_second_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {add_a_second_error}");
                return;
            }

            (int? add_a_first, string? add_a_first_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(add_a_first_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {add_a_first_error}");
                return;
            }

            string? add_a_push_error = Push(add_a_first!.Value + add_a_second!.Value, ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(add_a_push_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {add_a_push_error}");
                return;
            }
            break;
        case Op.ADDb:
            (int? add_b_second, string? add_b_second_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(add_b_second_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {add_b_second_error}");
                return;
            }

            (int? add_b_first, string? add_b_first_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(add_b_first_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {add_b_first_error}");
                return;
            }

            string? add_b_push_error = Push(add_b_first!.Value + add_b_second!.Value, ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(add_b_push_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {add_b_push_error}");
                return;
            }
            break;
        case Op.NEGa:
            (int? neg_a_value, string? neg_a_value_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(neg_a_value_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {neg_a_value_error}");
                return;
            }

            string? neg_a_value_push_error = Push(-neg_a_value!.Value, ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(neg_a_value_push_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {neg_a_value_push_error}");
                return;
            }
            break;
        case Op.NEGb:
            (int? neg_b_value, string? neg_b_value_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(neg_b_value_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {neg_b_value_error}");
                return;
            }

            string? neg_b_value_push_error = Push(-neg_b_value!.Value, ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(neg_b_value_push_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {neg_b_value_push_error}");
                return;
            }
            break;
        case Op.WRITEa:
            (int? write_a_value, string? write_a_value_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(write_a_value_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {write_a_value_error}");
                return;
            }

            if (debug)
            {
                Console.WriteLine($"OUTa: {write_a_value!.Value}");
            }
            else
            {
                Console.Write(write_a_value!.Value);
            }
            break;
        case Op.WRITEb:
            (int? write_b_value, string? write_b_value_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(write_b_value_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {write_b_value_error}");
                return;
            }

            if (debug)
            {
                Console.WriteLine($"OUTb: {write_b_value!.Value}");
            }
            else
            {
                Console.Write(write_b_value!.Value);
            }
            break;
        case Op.WRITECa:
            (int? writec_a_value, string? writec_a_value_error) = Pop(ref a, ref aPtr);
            if (!string.IsNullOrWhiteSpace(writec_a_value_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {writec_a_value_error}");
                return;
            }

            if (debug)
            {
                Console.WriteLine($"OUTCa: {(char)writec_a_value!.Value}");
            }
            else
            {
                Console.Write((char)writec_a_value!.Value);
            }
            break;
        case Op.WRITECb:
            (int? writec_b_value, string? writec_b_value_error) = Pop(ref b, ref bPtr);
            if (!string.IsNullOrWhiteSpace(writec_b_value_error))
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {writec_b_value_error}");
                return;
            }

            if (debug)
            {
                Console.WriteLine($"OUTCb: {(char)writec_b_value!.Value}");
            }
            else
            {
                Console.Write((char)writec_b_value!.Value);
            }
            break;
        case Op.CLR:
            cReg = null;

            if (aPtr > -1 || bPtr > -1)
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} ERR! Stack size non zero. a: {aPtr + 1}, b: {bPtr + 1}");
                return;
            }

            if (sw.IsRunning)
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} ERR! Stopwatch left running");
                return;
            }
            break;
        case Op.HLT:
            if (!i.Param.HasValue)
            {
                Thread.Sleep(Timeout.Infinite);
            }
            Thread.Sleep(i.Param!.Value);
            break;
        case Op.STARTW:
            sw = Stopwatch.StartNew();
            break;
        case Op.STOPW:
            if (!sw.IsRunning)
            {
                Console.Error.WriteLine($"[{instPtr}]: {i.Operation} ERR! Stopwatch is not running");
                return;
            }
            sw.Stop();
            if (debug || stopwatch)
            {
                Console.WriteLine($"sw: {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
            }
            break;
        default:
            Console.Error.WriteLine($"[{instPtr}]: {i.Operation} {i.Param} ERR! Unhandled operation");
            return;
    }

    if (debug)
    {
        string aDebug = string.Join(' ', a.ToArray().Reverse()).TrimEnd();
        if (!string.IsNullOrWhiteSpace(aDebug))
        {
            Console.WriteLine($"a: {aDebug}");
        }

        string bDebug = string.Join(' ', b.ToArray().Reverse()).TrimEnd();
        if (!string.IsNullOrWhiteSpace(bDebug))
        {
            Console.WriteLine($"b: {bDebug}");
        }

        if (cReg.HasValue)
        {
            Console.WriteLine($"c: {cReg}");
        }
    }

    if (interactive)
    {
        Console.ReadLine();
    }
}

string? Push(int? value, ref Span<int?> stack, ref int stackPtr)
{
    if (!value.HasValue)
    {
        if (!cReg.HasValue)
        {
            return "ERR! Null push value with an empty register";
        }
        value = cReg;
    }

    if (stackPtr == stack.Length - 1)
    {
        return "ERR! Stack overflow";
    }

    stackPtr++;
    stack[stackPtr] = value.Value;

    return null;
}

(int? value, string? error) Pop(ref Span<int?> stack, ref int stackPtr)
{
    if (stackPtr == -1)
    {
        return (null, "ERR! Stack underflow");
    }

    int value = stack[stackPtr]!.Value;
    stack[stackPtr] = null;
    stackPtr--;
    return (value, null);
}

string? Jump(int? value)
{
    if (!value.HasValue)
    {
        return "ERR! Jump location not set";
    }

    if (value < 0 || value > instSpace)
    {
        return "ERR! Invalid jump location";
    }

    instPtr = value.Value - 1;
    return null;
}

(Op? operation, string? error) GetOperation(ReadOnlySpan<char> line)
{
    try
    {
        int index = line.IndexOf(' ');
        if (index < 0)
        {
            index = line.Length;
        }

        var op = line[..index];
        if (!Enum.TryParse<Op>(op, true, out Op result))
        {
            return (null, $"ERR! Unknown op '{op}'");
        }
        return (result, null);
    }
    catch (Exception ex)
    {
        return (null, $"ERR! Parsing operation failed. {ex.GetBaseException().Message}");
    }
}

(int? param, string? error) GetParam(ReadOnlySpan<char> line)
{
    try
    {
        int indexStart = line.IndexOf(' ');
        if (indexStart < 0)
        {
            return (null, null);
        }

        var param = line[++indexStart..];
        int indexEnd = param.IndexOf("//");
        if (indexEnd >= 0)
        {
            param = param[..indexEnd];
        }

        string value = param.ToString().Trim();
        int? result = value switch
        {
            var _ when value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) =>
                Convert.ToInt32(value.Replace("0x", String.Empty, StringComparison.OrdinalIgnoreCase), 16),
            var _ when value.StartsWith("0b", StringComparison.OrdinalIgnoreCase) =>
                Convert.ToInt32(value.Replace("0b", String.Empty, StringComparison.OrdinalIgnoreCase), 2),
            var _ when (value.StartsWith("'") || value.StartsWith("\"")) && value.Length == 3 =>
                value.ToCharArray()[1],
            var _ when int.TryParse(value, out int number) => number,
            var _ => null
        };

        if (result is null)
        {
            return (null, $"ERR! Parameter value '{value}' has invalid format");
        }

        return (result, null);
    }
    catch (Exception ex)
    {
        return (null, $"ERR! Parsing operation parameter value failed. {ex.GetBaseException().Message}");
    }
}

enum Op
{
    NOP,
    PUSHa,
    PUSHb,
    POPa,
    POPb,
    LBL,
    JUMP,
    JUMPSa,
    JUMPSb,
    ADDa,
    ADDb,
    NEGa,
    NEGb,
    WRITEa,
    WRITEb,
    WRITECa,
    WRITECb,
    CLR,
    HLT,
    STARTW,
    STOPW
}

struct Instruction
{
    public Op Operation;
    public int? Param;
    public int? JumpLocation;
}
