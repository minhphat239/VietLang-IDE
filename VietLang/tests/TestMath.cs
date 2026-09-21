using System;

namespace VietLang;

public static class TestMath
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "math_can_bac_hai", Run = () => Eq(ChayMath("căn_bac_hai(9)"), 3.0) },
        new LexTest { Name = "math_tuyet_doi", Run = () => Eq(ChayMath("tuyệt_đối(-5)"), 5.0) },
        new LexTest { Name = "math_toi_da", Run = () => Eq(ChayMath("tối_đa(3, 7)"), 7.0) },
        new LexTest { Name = "math_toi_thieu", Run = () => Eq(ChayMath("tối_thiểu(3, 7)"), 3.0) },
        new LexTest { Name = "math_sin", Run = () => Eq(ChayMath("sin(0)"), 0.0) },
        new LexTest { Name = "math_cos", Run = () => Eq(ChayMath("cos(0)"), 1.0) },
        new LexTest { Name = "math_log", Run = () => Eq(ChayMath("log(E)"), 1.0) },
        new LexTest { Name = "math_log2", Run = () => Eq(ChayMath("log2(8)"), 3.0) },
        new LexTest { Name = "math_log10", Run = () => Eq(ChayMath("log10(100)"), 2.0) },
        new LexTest { Name = "math_lam_tron", Run = () => Eq(ChayMath("làm_tròn(1.5)"), 2.0) },
        new LexTest { Name = "math_lam_nguyen", Run = () => Eq(ChayMath("làm_nguyên(1.7)"), 1.0) },
        new LexTest { Name = "math_so_nguyen_true", Run = () => ChayMath("so_nguyen(5)") is bool b && b == true },
        new LexTest { Name = "math_so_nguyen_false", Run = () => ChayMath("so_nguyen(1.5)") is bool b2 && b2 == false },
        new LexTest { Name = "math_PI_range", Run = () => ChayMath("PI") is double pi && pi > 3.14 && pi < 3.15 },
    };

    private static bool Eq(object actual, double expected)
        => actual is double d && Math.Abs(d - expected) < 1e-9;

    private static object ChayMath(string expr)
    {
        var cu = Console.Out;
        Console.SetOut(new System.IO.StringWriter());
        try
        {
            var env = new PhamVi();
            Builtins.DangKy(env);
            var interp = new Interpreter();
            interp.ChayVoiGlobal(env, Parser.Parse($"kq = {expr}"));
            if (env.Lay("kq", out var val)) return val;
            return null;
        }
        finally { Console.SetOut(cu); }
    }
}
