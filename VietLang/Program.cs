using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VietLang;

/// <summary>
/// Entry point VietLang v0.2.
/// `dotnet run -- test`      → chạy self-test (lexer + parser + interpreter + golden).
/// `dotnet run -- <tệp.vl>`  → chạy chương trình VietLang.
/// `dotnet run -- --version` → in phiên bản.
/// Không đối số → in hướng dẫn + phiên bản.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length > 0 && args[0] == "test")
        {
            return ChaySelfTest();
        }

        if (args.Length > 0 && args[0] == "--version")
        {
            Console.WriteLine("VietLang v0.2.0");
            return 0;
        }

        if (args.Length > 0)
        {
            return ChayTep(args[0]);
        }

        Console.WriteLine("VietLang v0.2.0");
        Console.WriteLine("Cách dùng: dotnet run -- <tệp.vl>");
        Console.WriteLine("dotnet run -- test");
        return 0;
    }

    private static int ChayTep(string path)
    {
        string noidung;
        try
        {
            noidung = File.ReadAllText(path, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Không đọc được tệp '{path}': {ex.Message}");
            return 1;
        }

        try
        {
            var lexer = new Lexer(noidung);
            var parser = new Parser(lexer.LexAll());
            var program = parser.PhanTichKieu();
            new Interpreter().Run(program);
            return 0;
        }
        catch (LexError e)
        {
            Console.WriteLine(e.Message);
            return 1;
        }
        catch (ParseError e)
        {
            Console.WriteLine(e.Message);
            return 1;
        }
        catch (RuntimeError e)
        {
            Console.WriteLine(e.Message);
            return 1;
        }
    }

    private static int ChaySelfTest()
    {
        var tests = new List<LexTest>();
        tests.AddRange(TestLexer.GetAll());
        tests.AddRange(TestParser.GetAll());
        tests.AddRange(TestInterpreter.GetAll());
        tests.AddRange(TestDict.GetAll());
        tests.AddRange(TestException.GetAll());
        tests.AddRange(TestModule.GetAll());
        tests.AddRange(TestAugAssign.GetAll());
        tests.AddRange(TestMethods.GetAll());
        tests.AddRange(TestVietNamese.GetAll());
        tests.AddRange(TestReplBuiltins2.GetAll());
        tests.AddRange(TestReplBuiltins.GetAll());
        tests.AddRange(TestSessionBuiltins.GetAll());
        tests.AddRange(TestEval.GetAll());
        tests.AddRange(TestMath.GetAll());
        tests.AddRange(TestGolden.GetAll());
        tests.AddRange(StdlibLoader.LayTatCaTest());

        int pass = 0;
        foreach (var t in tests)
        {
            bool ok;
            string err = null;
            try
            {
                ok = t.Run();
            }
            catch (Exception ex)
            {
                ok = false;
                err = ex.Message;
            }
            if (ok)
            {
                pass++;
                Console.WriteLine("PASS " + t.Name);
            }
            else
            {
                Console.WriteLine("FAIL " + t.Name + (err == null ? "" : " — " + err));
            }
        }

        Console.WriteLine($"TOTAL: {pass}/{tests.Count} PASS");
        return pass == tests.Count ? 0 : 1;
    }
}