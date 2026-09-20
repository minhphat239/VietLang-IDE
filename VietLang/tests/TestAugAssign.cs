using System;
using System.IO;

namespace VietLang;

/// <summary>Self-test += operator: chạy source → assert output, hoặc assert lỗi runtime.</summary>
public static class TestAugAssign
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "aug_cong_so", Run = TestCongSo },
        new LexTest { Name = "aug_cong_chuoi", Run = TestCongChuoi },
        new LexTest { Name = "aug_cong_nhieu_lan", Run = TestCongNhieuLan },
        new LexTest { Name = "aug_cong_loi_chua_khai_bao", Run = TestLoiChuaKhaiBao },
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

    // 1. x += 5 số
    private static bool TestCongSo()
    {
        string src = "x = 1\nx += 5\nin_ra(x)";
        return KiemTra(Chay(src) == "6", "x = 1, x += 5 → 6");
    }

    // 2. s += "World" chuỗi
    private static bool TestCongChuoi()
    {
        string src = "s = \"Hello\"\ns += \" World\"\nin_ra(s)";
        return KiemTra(Chay(src) == "Hello World", "s = Hello, s += World");
    }

    // 3. += nhiều lần
    private static bool TestCongNhieuLan()
    {
        string src = "x = 0\nx += 1\nx += 2\nx += 3\nin_ra(x)";
        return KiemTra(Chay(src) == "6", "0+1+2+3 = 6");
    }

    // 4. Lỗi: += biến chưa khai báo
    private static bool TestLoiChuaKhaiBao()
    {
        ChayLoi("y += 1", "chưa được gán");
        return true;
    }
}
