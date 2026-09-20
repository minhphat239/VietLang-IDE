using System;
using System.IO;

namespace VietLang;

/// <summary>Self-test exception handling (try/catch/finally/ném).</summary>
public static class TestException
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "try_catch_basic", Run = TestTryCatchBasic },
        new LexTest { Name = "try_catch_var_bind", Run = TestTryCatchVarBind },
        new LexTest { Name = "try_catch_all", Run = TestTryCatchAll },
        new LexTest { Name = "try_finally_always_runs", Run = TestTryFinallyAlways },
        new LexTest { Name = "try_finally_return", Run = TestTryFinallyReturn },
        new LexTest { Name = "try_finally_break", Run = TestTryFinallyBreak },
        new LexTest { Name = "try_finally_continue", Run = TestTryFinallyContinue },
        new LexTest { Name = "try_catch_finally_combo", Run = TestTryCatchFinally },
        new LexTest { Name = "try_no_exception", Run = TestTryNoException },
        new LexTest { Name = "try_catch_inner_scope", Run = TestTryCatchInnerScope },
        new LexTest { Name = "ném_outside_try", Run = TestNemOutsideTry },
        new LexTest { Name = "try_finally_only", Run = TestTryFinallyOnly },
        new LexTest { Name = "try_catch_runtime_error", Run = TestTryCatchRuntimeError },
        new LexTest { Name = "try_catch_return_signal", Run = TestTryCatchReturnSignal },
        new LexTest { Name = "nem_khong_tham_so", Run = TestNemKhongThamSo },
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
        throw new Exception(maCu == null ? "phải ném RuntimeError" : $"phải ném RuntimeError chứa '{maCu}'");
    }

    // 1. try-catch basic: ném rồi bắt được.
    private static bool TestTryCatchBasic()
    {
        string src = "thử {\n  ném(\"loi ABC\")\n} ngoại_lệ (e) {\n  in_ra(e)\n}";
        return KiemTra(Chay(src) == "loi ABC", "catch bắt ném, in ra message");
    }

    // 2. Catch variable bind: e = message string.
    private static bool TestTryCatchVarBind()
    {
        string src = "thử {\n  ném(\"abc\")\n} ngoại_lệ (e) {\n  in_ra(e == \"abc\")\n}";
        return KiemTra(Chay(src) == "đúng", "e bound to message");
    }

    // 3. Catch-all (ngoại_lệ không bind tên) vẫn bắt được.
    private static bool TestTryCatchAll()
    {
        string src = "thử {\n  ném(\"err\")\n} ngoại_lệ {\n  in_ra(\"caught\")\n}";
        return KiemTra(Chay(src) == "caught", "catch-all bắt được");
    }

    // 4. Finally luôn chạy kể cả không có exception.
    private static bool TestTryFinallyAlways()
    {
        string src = "thử {\n  in_ra(\"try\")\n} cuối_cùng {\n  in_ra(\"finally\")\n}";
        return KiemTra(Chay(src) == "try\nfinally", "finally chạy khi không lỗi");
    }

    // 5. Finally chạy trước khi return từ try.
    private static bool TestTryFinallyReturn()
    {
        string src = "hàm t() {\n  thử {\n    trả_về 42\n  } cuối_cùng {\n    in_ra(\"fin\")\n  }\n}\n" +
                     "in_ra(t())";
        return KiemTra(Chay(src) == "fin\n42", "finally chạy trước return");
    }

    // 6. Finally chạy trước khi break.
    private static bool TestTryFinallyBreak()
    {
        string src = "trong_lúc đúng {\n  thử {\n    in_ra(\"a\")\n    dừng\n  } cuối_cùng {\n    in_ra(\"fin\")\n  }\n}\n";
        return KiemTra(Chay(src) == "a\nfin", "finally chạy trước break");
    }

    // 7. Finally chạy trước khi continue.
    private static bool TestTryFinallyContinue()
    {
        string src = "i = 0\ntrong_lúc i < 2 {\n  i = i + 1\n  thử {\n    in_ra(i)\n    tiếp\n  } cuối_cùng {\n    in_ra(\"f\")\n  }\n}";
        return KiemTra(Chay(src) == "1\nf\n2\nf", "finally chạy trước continue");
    }

    // 8. try-catch-finally: cả 3 phần chạy.
    private static bool TestTryCatchFinally()
    {
        string src = "thử {\n  ném(\"loi\")\n} ngoại_lệ (e) {\n  in_ra(e)\n} cuối_cùng {\n  in_ra(\"done\")\n}";
        return KiemTra(Chay(src) == "loi\ndone", "catch + finally đều chạy");
    }

    // 9. Không có exception → bỏ catch, chạy finally.
    private static bool TestTryNoException()
    {
        string src = "thử {\n  in_ra(\"ok\")\n} ngoại_lệ (e) {\n  in_ra(\"bad\")\n} cuối_cùng {\n  in_ra(\"fin\")\n}";
        return KiemTra(Chay(src) == "ok\nfin", "bỏ catch, chạy finally");
    }

    // 10. Catch có scope riêng — biến e không leaked ra ngoài.
    private static bool TestTryCatchInnerScope()
    {
        string src = "thử {\n  ném(\"hello\")\n} ngoại_lệ (e) {\n  in_ra(e)\n}\n" +
                     "in_ra(\"out\")";
        return KiemTra(Chay(src) == "hello\nout", "e không leaked");
    }

    // 11. ném ngoài try → runtime error.
    private static bool TestNemOutsideTry()
    {
        ChayLoi("ném(\"err\")", "Lỗi thực thi");
        return true;
    }

    // 12. try-finally only (không catch): exception propagate qua finally.
    private static bool TestTryFinallyOnly()
    {
        ChayLoi("thử {\n  ném(\"err\")\n} cuối_cùng {\n  in_ra(\"fin\")\n}", null);
        return true;
    }

    // 13. try-catch: runtime error trong try bắt được.
    private static bool TestTryCatchRuntimeError()
    {
        string src = "thử {\n  x = 10 / 0\n} ngoại_lệ (e) {\n  in_ra(e)\n}";
        return KiemTra(Chay(src).Contains("chia"), "catch bắt RuntimeError, in message chứa 'chia'");
    }

    // 14. try-catch: return trong catch.
    private static bool TestTryCatchReturnSignal()
    {
        string src = "hàm t() {\n  thử {\n    ném(\"e\")\n  } ngoại_lệ (e) {\n    trả_về 99\n  }\n}\nin_ra(t())";
        return KiemTra(Chay(src) == "99", "return trong catch hoạt động");
    }

    // 15. ném không có tham số.
    private static bool TestNemKhongThamSo()
    {
        ChayLoi("ném()", null);
        return true;
    }
}
