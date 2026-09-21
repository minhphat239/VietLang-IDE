using System;
using System.IO;

namespace VietLang;

/// <summary>Self-test 4 builtin REPL TB+THẤP: chay_lenh, lay_tham_so, in_mau, xoa_man_hinh + in_ra sep/end + nhap prompt.</summary>
public static class TestReplBuiltins2
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "repl2_chay_lenh_echo", Run = TestChayLenhEcho },
        new LexTest { Name = "repl2_chay_lenh_dir", Run = TestChayLenhDir },
        new LexTest { Name = "repl2_lay_tham_so", Run = TestLayThamSo },
        new LexTest { Name = "repl2_in_mau_red", Run = TestInMauRed },
        new LexTest { Name = "repl2_in_mau_green", Run = TestInMauGreen },
        new LexTest { Name = "repl2_in_mau_invalid", Run = TestInMauInvalid },
        new LexTest { Name = "repl2_xoa_man_hinh", Run = TestXoaManHinh },
        new LexTest { Name = "repl2_in_ra_sep", Run = TestInRaSep },
        new LexTest { Name = "repl2_in_ra_end", Run = TestInRaEnd },
        new LexTest { Name = "repl2_in_ra_sep_end", Run = TestInRaSepEnd },
        new LexTest { Name = "repl2_nhap_prompt", Run = TestNhapPrompt },
        new LexTest { Name = "repl2_nhap_khong_prompt", Run = TestNhapKhongPrompt },
    };

    private static bool KiemTra(bool cond, string msg)
    {
        if (!cond) throw new Exception(msg);
        return true;
    }

    private static string Chay(string src)
    {
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            new Interpreter().Run(Parser.Parse(src));
        }
        finally
        {
            Console.SetOut(cu);
        }
        return sw.ToString().Replace("\r\n", "\n").Trim('\n');
    }

    private static void ChayLoi(string src, string maCu = null)
    {
        try
        {
            new Interpreter().Run(Parser.Parse(src));
        }
        catch (RuntimeError e)
        {
            if (maCu != null)
                KiemTra(e.Message.Contains(maCu), $"Lỗi thiếu cụm '{maCu}': {e.Message}");
            return;
        }
        throw new Exception(maCu == null ? "phải ném RuntimeError" : $"phải ném RuntimeError chua '{maCu}'");
    }

    // 1. chay_lenh("echo hello") → chua "hello"
    private static bool TestChayLenhEcho()
    {
        string output = Chay("in_ra(chay_lenh(\"echo hello\"))");
        return KiemTra(output.Contains("hello"), $"output phải chua 'hello': '{output}'");
    }

    // 2. chay_lenh("echo test123") → chua "test123"
    private static bool TestChayLenhDir()
    {
        string output = Chay("in_ra(chay_lenh(\"echo test123\"))");
        return KiemTra(output.Contains("test123"), $"output phải chua 'test123': '{output}'");
    }

    // 3. lay_tham_so() → trả về mảng
    private static bool TestLayThamSo()
    {
        string output = Chay("kq = lay_tham_so()\nin_ra(do_dai(kq))");
        return KiemTra(output.Length > 0, $"do_dai trả về số ≥ 0: '{output}'");
    }

    // 4. in_mau("test", "red") → output chua \x1b[31m
    private static bool TestInMauRed()
    {
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            new Interpreter().Run(Parser.Parse("in_mau(\"test\", \"red\")"));
        }
        finally
        {
            Console.SetOut(cu);
        }
        string raw = sw.ToString();
        return KiemTra(raw.Contains("\x1b[31m"), $"output phải chua ANSI red: '{raw}'");
    }

    // 5. in_mau("hello", "green") → output chua \x1b[32m
    private static bool TestInMauGreen()
    {
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            new Interpreter().Run(Parser.Parse("in_mau(\"hello\", \"green\")"));
        }
        finally
        {
            Console.SetOut(cu);
        }
        string raw = sw.ToString();
        return KiemTra(raw.Contains("\x1b[32m"), $"output phải chua ANSI green: '{raw}'");
    }

    // 6. in_mau("test", "invalid") → lỗi runtime
    private static bool TestInMauInvalid()
    {
        ChayLoi("in_mau(\"test\", \"invalid\")", "không hỗ trợ màu");
        return true;
    }

    // 7. xoa_man_hinh() → output chua \x1b[2J\x1b[H
    private static bool TestXoaManHinh()
    {
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            new Interpreter().Run(Parser.Parse("xoa_man_hinh()"));
        }
        finally
        {
            Console.SetOut(cu);
        }
        string raw = sw.ToString();
        return KiemTra(raw.Contains("\x1b[2J") && raw.Contains("\x1b[H"), $"output phải chua ANSI clear: '{raw}'");
    }

    // 8. in_ra("a", "b", { "sep": ", " }) → "a, b"
    private static bool TestInRaSep()
    {
        string output = Chay("in_ra(\"a\", \"b\", { \"sep\": \", \" })");
        return KiemTra(output == "a, b", $"phải là 'a, b', nhận '{output}'");
    }

    // 9. in_ra("x", { "end": "!" }) → "x!"
    private static bool TestInRaEnd()
    {
        string output = Chay("in_ra(\"x\", { \"end\": \"!\" })");
        return KiemTra(output == "x!", $"phải là 'x!', nhận '{output}'");
    }

    // 10. in_ra("a", "b", { "sep": ", ", "end": "!" }) → "a, b!"
    private static bool TestInRaSepEnd()
    {
        string output = Chay("in_ra(\"a\", \"b\", { \"sep\": \", \", \"end\": \"!\" })");
        return KiemTra(output == "a, b!", $"phải là 'a, b!', nhận '{output}'");
    }

    // 11. nhap("test") → accepts 1 arg (prompt displayed, can't test interactivity)
    private static bool TestNhapPrompt()
    {
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        var cuIn = Console.In;
        Console.SetIn(new StringReader("input_value\n"));
        try
        {
            new Interpreter().Run(Parser.Parse("kq = nhap(\">>> \")\nin_ra(kq)"));
        }
        finally
        {
            Console.SetOut(cu);
            Console.SetIn(cuIn);
        }
        string raw = sw.ToString();
        return KiemTra(raw.Contains(">>> "), $"output phải chua prompt '>>> ': '{raw}'");
    }

    private static bool TestNhapKhongPrompt()
    {
        var cuIn = Console.In;
        Console.SetIn(new StringReader("\n"));
        try
        {
            var env = new PhamVi();
            Builtins.DangKy(env);
            var interp = new Interpreter();
            interp.ChayVoiGlobal(env, Parser.Parse("kq = nhap()"));
            env.Lay("kq", out var val);
            return KiemTra(val is string, $"nhap voi input empty tra string, nhan {val?.GetType()}");
        }
        finally
        {
            Console.SetIn(cuIn);
        }
    }
}
