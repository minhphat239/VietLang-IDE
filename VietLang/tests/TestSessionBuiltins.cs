using System;

namespace VietLang;

public static class TestSessionBuiltins
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "session_lay_tat_ca_bien", Run = TestLayTatCaBien },
        new LexTest { Name = "session_gan_bien_so", Run = TestGanBienSo },
        new LexTest { Name = "session_lay_tat_ca_bien_chua_key", Run = TestLayTatCaBienChuaKey },
        new LexTest { Name = "session_gan_bien_chuoi_in_ra", Run = TestGanBienChuoiInRa },
    };

    private static bool KT(bool c, string m) { if (!c) throw new Exception(m); return true; }

    private static object ChayGiaTri(string src)
    {
        var cu = Console.Out;
        Console.SetOut(new System.IO.StringWriter());
        try
        {
            var env = new PhamVi();
            Builtins.DangKy(env);
            var interp = new Interpreter();
            interp.ChayVoiGlobal(env, Parser.Parse(src));
            if (env.Lay("kq", out var val)) return val;
            return null;
        }
        finally { Console.SetOut(cu); }
    }

    private static string Chay(string src)
    {
        var cu = Console.Out;
        var sw = new System.IO.StringWriter();
        Console.SetOut(sw);
        try
        {
            var env = new PhamVi();
            Builtins.DangKy(env);
            var interp = new Interpreter();
            interp.ChayVoiGlobal(env, Parser.Parse(src));
        }
        finally { Console.SetOut(cu); }
        return sw.ToString().Replace("\r\n", "\n").Trim('\n');
    }

    private static bool TestLayTatCaBien()
    {
        object r = ChayGiaTri("x = 10\nt = lấy_tất_cả_biến()\nkq = t");
        KT(r is DictValue, "mong DictValue, nhan " + r?.GetType());
        var d = (DictValue)r;
        KT(d.Pairs.ContainsKey("x"), "khong co key 'x'");
        KT(d.Pairs["x"] is double && (double)d.Pairs["x"] == 10.0, "x sai");
        return true;
    }

    private static bool TestGanBienSo()
    {
        var cu = Console.Out;
        Console.SetOut(new System.IO.StringWriter());
        try
        {
            var env = new PhamVi();
            Builtins.DangKy(env);
            var interp = new Interpreter();
            interp.ChayVoiGlobal(env, Parser.Parse("gán_biến(\"y\", 42)"));
            env.Lay("y", out var val);
            KT(val is double && (double)val == 42.0, "y phai la 42, nhan " + val);
            return true;
        }
        finally { Console.SetOut(cu); }
    }

    private static bool TestLayTatCaBienChuaKey()
    {
        var cu = Console.Out;
        Console.SetOut(new System.IO.StringWriter());
        try
        {
            var env = new PhamVi();
            Builtins.DangKy(env);
            var interp = new Interpreter();
            interp.ChayVoiGlobal(env, Parser.Parse("x = 10\nt = lấy_tất_cả_biến()"));
            env.Lay("t", out var val);
            KT(val is DictValue, "mong DictValue, nhan " + val?.GetType());
            var d = (DictValue)val;
            KT(d.Pairs.ContainsKey("y") == false, "khong nen co key 'y'");
            return true;
        }
        finally { Console.SetOut(cu); }
    }

    private static bool TestGanBienChuoiInRa()
    {
        string output = Chay("gán_biến(\"z\", \"hello\")\nin_ra(z)");
        KT(output == "hello", "mong 'hello', nhan '" + output + "'");
        return true;
    }
}
