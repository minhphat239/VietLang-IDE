using System;
using System.IO;
using System.Text;

namespace VietLang;

/// <summary>Golden tests: chạy file .vl thật, so sánh output stdout với expected.</summary>
public static class TestGolden
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "golden_hello", Run = GoldenHello },
        new LexTest { Name = "golden_dict", Run = GoldenDict },
        new LexTest { Name = "golden_exception", Run = GoldenException },
        new LexTest { Name = "golden_module", Run = GoldenModule },
        new LexTest { Name = "golden_math", Run = GoldenMath },
        new LexTest { Name = "golden_filesystem", Run = GoldenFilesystem },
        new LexTest { Name = "golden_datetime", Run = GoldenDatetime },
    };

    private static void KiemTra(bool cond, string msg)
    {
        if (!cond) throw new Exception(msg);
    }

    private static string FixturesDir =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "tests", "fixtures"));

    private static (string output, int exitCode) ChayFile(string relativePath)
    {
        string fullPath = Path.Combine(FixturesDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
        string source = File.ReadAllText(fullPath, Encoding.UTF8);
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        int exitCode = 0;
        try
        {
            var lexer = new Lexer(source);
            var parser = new Parser(lexer.LexAll());
            var program = parser.PhanTichKieu();
            var interp = new Interpreter();
            interp.Run(program, fullPath, null, null);
        }
        catch (LexError e)
        {
            Console.WriteLine(e.Message);
            exitCode = 1;
        }
        catch (ParseError e)
        {
            Console.WriteLine(e.Message);
            exitCode = 1;
        }
        catch (RuntimeError e)
        {
            Console.WriteLine(e.Message);
            exitCode = 1;
        }
        finally
        {
            Console.SetOut(cu);
        }
        string output = sw.ToString().Replace("\r\n", "\n").TrimEnd('\n');
        return (output, exitCode);
    }

    private static bool GoldenHello()
    {
        var (output, exit) = ChayFile("golden_hello.vl");
        KiemTra(exit == 0, $"exit = {exit}");
        KiemTra(output == "Xin chào VietLang", $"output = [{output}]");
        return true;
    }

    private static bool GoldenDict()
    {
        var (output, exit) = ChayFile("golden_dict.vl");
        KiemTra(exit == 0, $"exit = {exit}");
        KiemTra(output == "An\nđúng\n21\n2\nsai", $"output = [{output}]");
        return true;
    }

    private static bool GoldenException()
    {
        var (output, exit) = ChayFile("golden_exception.vl");
        KiemTra(exit == 0, $"exit = {exit}");
        KiemTra(output == "loiABC\nfinallyChay", $"output = [{output}]");
        return true;
    }

    private static bool GoldenModule()
    {
        var (output, exit) = ChayFile("golden_module.vl");
        KiemTra(exit == 0, $"exit = {exit}");
        KiemTra(output == "Xin chào VietLang", $"output = [{output}]");
        return true;
    }

    private static bool GoldenMath()
    {
        var (output, exit) = ChayFile("test_math.vl");
        KiemTra(exit == 0, $"exit = {exit}");
        KiemTra(output == "PASS: all math tests", $"output = [{output}]");
        return true;
    }

    private static bool GoldenFilesystem()
    {
        var (output, exit) = ChayFile("test_filesystem.vl");
        KiemTra(exit == 0, $"exit = {exit}");
        KiemTra(output == "PASS: filesystem tests", $"output = [{output}]");
        return true;
    }

    private static bool GoldenDatetime()
    {
        var (output, exit) = ChayFile("test_datetime.vl");
        KiemTra(exit == 0, $"exit = {exit}");
        KiemTra(output == "PASS: datetime tests", $"output = [{output}]");
        return true;
    }
}
