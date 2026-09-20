using System;
using System.IO;

namespace VietLang;

/// <summary>Self-test string/array methods: chạy source → assert output.</summary>
public static class TestMethods
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "str_tim", Run = TestStrTim },
        new LexTest { Name = "str_tim_khong_tim_thay", Run = TestStrTimKhongThay },
        new LexTest { Name = "str_thay", Run = TestStrThay },
        new LexTest { Name = "str_cat", Run = TestStrCat },
        new LexTest { Name = "str_chua", Run = TestStrChua },
        new LexTest { Name = "str_phan_tach", Run = TestStrPhanTach },
        new LexTest { Name = "arr_loc", Run = TestArrLoc },
        new LexTest { Name = "arr_map", Run = TestArrMap },
        new LexTest { Name = "arr_gop", Run = TestArrGop },
        new LexTest { Name = "method_loi_khong_ton_tai", Run = TestMethodLoi },
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

    // 1. s.tìm("sub") — tìm thấy
    private static bool TestStrTim()
    {
        string src = "s = \"Xin Chao\"\nin_ra(s.tìm(\"Chao\"))";
        return KiemTra(Chay(src) == "4", "tìm('Chao') → 4");
    }

    // 2. s.tìm("sub") — không tìm thấy → -1
    private static bool TestStrTimKhongThay()
    {
        string src = "s = \"Xin Chao\"\nin_ra(s.tìm(\"Hello\"))";
        return KiemTra(Chay(src) == "-1", "tìm('Hello') → -1");
    }

    // 3. s.thay("old", "new")
    private static bool TestStrThay()
    {
        string src = "s = \"Xin Chao The Gioi\"\nin_ra(s.thay(\"The Gioi\", \"ban\"))";
        return KiemTra(Chay(src) == "Xin Chao ban", "thay ok");
    }

    // 4. s.cắt() — bỏ whitespace
    private static bool TestStrCat()
    {
        string src = "s = \"  Xin Chao  \"\nin_ra(s.cắt())";
        return KiemTra(Chay(src) == "Xin Chao", "cắt whitespace");
    }

    // 5. s.chứa("sub") — đúng / sai
    private static bool TestStrChua()
    {
        string src = "s = \"Xin Chao\"\nin_ra(s.chứa(\"Chao\"))\nin_ra(s.chứa(\"Hello\"))";
        return KiemTra(Chay(src) == "đúng\nsai", "chứa ok");
    }

    // 6. s.phân_tách("dấu") — tách chuỗi
    private static bool TestStrPhanTach()
    {
        string src = "s = \"Xin,Chao,The,Gioi\"\na = s.phân_tách(\",\")\nin_ra(độ_dài(a))\nin_ra(a[1])";
        return KiemTra(Chay(src) == "4\nChao", "phân_tách ok");
    }

    // 7. a.lọc() — bỏ falsy
    private static bool TestArrLoc()
    {
        string src = "a = [1, 0, 2, \"\", 3]\nb = a.lọc()\nin_ra(độ_dài(b))\nin_ra(b[0])\nin_ra(b[1])\nin_ra(b[2])";
        return KiemTra(Chay(src) == "3\n1\n2\n3", "lọc bỏ 0 và \"\"");
    }

    // 8. a.map(hàm) — áp dụng hàm
    private static bool TestArrMap()
    {
        string src = "a = [1, 2, 3]\nb = a.map(hàm(x) { trả_về x * 2 })\nin_ra(b[0])\nin_ra(b[1])\nin_ra(b[2])";
        return KiemTra(Chay(src) == "2\n4\n6", "map nhân 2");
    }

    // 9. a.gộp([4, 5]) — concat
    private static bool TestArrGop()
    {
        string src = "a = [1, 2]\nb = a.gộp([3, 4])\nin_ra(độ_dài(b))\nin_ra(b[2])\nin_ra(b[3])";
        return KiemTra(Chay(src) == "4\n3\n4", "gộp ok");
    }

    // 10. Method không tồn tại trên kiểu → RuntimeError
    private static bool TestMethodLoi()
    {
        ChayLoi("in_ra((5).tìm(\"x\"))", "không có thuộc tính");
        ChayLoi("in_ra((5).lọc())", "không có thuộc tính");
        return true;
    }
}
