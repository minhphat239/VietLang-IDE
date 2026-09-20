using System;
using System.IO;

namespace VietLang;

public static class TestEval
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "thuc_thi_bieu_thuc", Run = TestBieuThuc },
        new LexTest { Name = "thuc_thi_chia_se_state", Run = TestChiaSeState },
        new LexTest { Name = "thuc_thi_dinh_nghia_ham", Run = TestDinhNghiaHam },
        new LexTest { Name = "thuc_thi_loi_bien_chua_gan", Run = TestLoiBienChuaGan },
        new LexTest { Name = "thuc_thi_loi_cu_phap", Run = TestLoiCuPhap },
        new LexTest { Name = "thuc_thi_chuoi_rong", Run = TestChuoiRong },
        new LexTest { Name = "thuc_thi_in_ra", Run = TestInRa },
        new LexTest { Name = "thuc_thi_side_effect_mang", Run = TestSideEffectMang },
    };

    private static bool KT(bool c, string m) { if (!c) throw new Exception(m); return true; }

    private static object ChayGiaTri(string src)
    {
        var cu = Console.Out;
        Console.SetOut(new StringWriter());
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
        var sw = new StringWriter();
        Console.SetOut(sw);
        try { new Interpreter().Run(Parser.Parse(src)); }
        finally { Console.SetOut(cu); }
        return sw.ToString().Replace("\r\n", "\n").Trim('\n');
    }

    private static bool TestBieuThuc()
    {
        object r = ChayGiaTri("kq = thực_thi(\"1 + 2\")");
        KT(r is double && (double)r == 3.0, "mong 3, nhan " + r);
        return true;
    }

    private static bool TestChiaSeState()
    {
        object r = ChayGiaTri("x = 5\nkq = thực_thi(\"x + 1\")");
        KT(r is double && (double)r == 6.0, "mong 6, nhan " + r);
        return true;
    }

    private static bool TestDinhNghiaHam()
    {
        object r = ChayGiaTri("thực_thi(\"hàm f() { trả_về 42 }\")\nkq = thực_thi(\"f()\")");
        KT(r is double && (double)r == 42.0, "mong 42, nhan " + r);
        return true;
    }

    private static bool TestLoiBienChuaGan()
    {
        object r = ChayGiaTri("kq = thực_thi(\"xyz\")");
        KT(r is string, "mong string error, nhan " + r?.GetType());
        string s = (string)r;
        KT(s.Contains("chưa được gán"), "mong 'chưa được gán' trong error, nhan: " + s);
        return true;
    }

    private static bool TestLoiCuPhap()
    {
        object r = ChayGiaTri("kq = thực_thi(\"1 +\")");
        KT(r is string, "mong string error, nhan " + r?.GetType());
        string s = (string)r;
        KT(s.Contains("lỗi cú pháp"), "mong 'lỗi cú pháp' trong error, nhan: " + s);
        return true;
    }

    private static bool TestChuoiRong()
    {
        object r = ChayGiaTri("kq = thực_thi(\"\")");
        KT(r == null, "mong null, nhan " + r);
        return true;
    }

    private static bool TestInRa()
    {
        string o = Chay("thực_thi(\"in_ra(99)\")");
        KT(o == "99", "mong '99', nhan '" + o + "'");
        return true;
    }

    private static bool TestSideEffectMang()
    {
        string o = Chay("a = []\nthực_thi(\"thêm(a, 1)\")\nin_ra(a)");
        KT(o == "[1]", "mong '[1]', nhan '" + o + "'");
        return true;
    }
}
