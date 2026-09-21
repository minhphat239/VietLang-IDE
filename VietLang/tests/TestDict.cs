using System;
using System.IO;

namespace VietLang;

/// <summary>Self-test dict: chạy source → assert output, hoặc assert lỗi runtime.</summary>
public static class TestDict
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "dict_tao_va_truy_cap", Run = TestDictTaoVaTruyCap },
        new LexTest { Name = "dict_rong_kich_thuoc", Run = TestDictRong },
        new LexTest { Name = "dict_long", Run = TestDictLong },
        new LexTest { Name = "dict_co_key", Run = TestDictCoKey },
        new LexTest { Name = "dict_lay_key", Run = TestDictLayKey },
        new LexTest { Name = "dict_xoa_key", Run = TestDictXoaKey },
        new LexTest { Name = "dict_tat_ca", Run = TestDictTatCa },
        new LexTest { Name = "dict_kich_thuoc", Run = TestDictKichThuoc },
        new LexTest { Name = "dict_gan_index", Run = TestDictGanIndex },
        new LexTest { Name = "dict_voi_trong", Run = TestDictVoiTrong },
        new LexTest { Name = "dict_ham_trong_dict", Run = TestDictHamTrongDict },
        new LexTest { Name = "dict_loi_runtime", Run = TestDictLoiRuntime },
        new LexTest { Name = "dict_nhieu_key_nlp", Run = TestDictNhieuKeyNLP },
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

    // 1. Tạo dict + truy cập
    private static bool TestDictTaoVaTruyCap()
    {
        string src = "t = { \"a\": 1 }\nin_ra(t[\"a\"])";
        return KiemTra(Chay(src) == "1", "t[\"a\"] == 1");
    }

    // 2. Dict rỗng + kích thước 0
    private static bool TestDictRong()
    {
        string src = "t = { }\nin_ra(do_dai(t))";
        return KiemTra(Chay(src) == "0", "dict rỗng do_dai = 0");
    }

    // 3. Dict lồng
    private static bool TestDictLong()
    {
        string src = "t = { \"a\": { \"b\": 2 } }\nin_ra(t[\"a\"][\"b\"])";
        return KiemTra(Chay(src) == "2", "dict lồng t[\"a\"][\"b\"] == 2");
    }

    // 4. .có(): có → true, không có → false
    private static bool TestDictCoKey()
    {
        string src = "t = { \"ten\": \"An\", \"tuoi\": 20 }\nin_ra(t.có(\"ten\"))\nin_ra(t.có(\"khong_co\"))";
        return KiemTra(Chay(src) == "đúng\nsai", ".có() đúng/sai");
    }

    // 5. .lay(): có → value, không có → rỗng
    private static bool TestDictLayKey()
    {
        string src = "t = { \"a\": 1 }\nin_ra(t.lay(\"a\"))\nin_ra(t.lay(\"khong\") == rỗng)";
        return KiemTra(Chay(src) == "1\nđúng", ".lay() value hoặc rỗng");
    }

    // 6. .xóa(): xóa key tồn tại → trả value, xóa key không có → rỗng
    private static bool TestDictXoaKey()
    {
        string src = "t = { \"a\": 1, \"b\": 2 }\n" +
                     "r1 = t.xóa(\"a\")\n" +
                     "in_ra(r1)\n" +
                     "in_ra(t.kich_thuoc())\n" +
                     "r2 = t.xóa(\"khong\")\n" +
                     "in_ra(r2 == rỗng)";
        return KiemTra(Chay(src) == "1\n1\nđúng", ".xóa() value hoặc rỗng");
    }

    // 7. .tat_ca(): trả mảng keys
    private static bool TestDictTatCa()
    {
        string src = "t = { \"a\": 1, \"b\": 2, \"c\": 3 }\n" +
                     "k = t.tat_ca()\n" +
                     "in_ra(do_dai(k))";
        return KiemTra(Chay(src) == "3", ".tat_ca() trả 3 keys");
    }

    // 8. .kich_thuoc(): số đúng
    private static bool TestDictKichThuoc()
    {
        string src = "t = { \"a\": 1, \"b\": 2 }\nin_ra(t.kich_thuoc())";
        return KiemTra(Chay(src) == "2", ".kich_thuoc() == 2");
    }

    // 9. Gán index: t["x"] = 5 → t["x"] == 5; gán đè
    private static bool TestDictGanIndex()
    {
        string src = "t = { }\n" +
                     "t[\"x\"] = 5\n" +
                     "in_ra(t[\"x\"])\n" +
                     "t[\"x\"] = 10\n" +
                     "in_ra(t[\"x\"])";
        return KiemTra(Chay(src) == "5\n10", "gán index + gán đè");
    }

    // 10. Với k trong t → duyệt keys
    private static bool TestDictVoiTrong()
    {
        string src = "t = { \"a\": 1, \"b\": 2 }\n" +
                     "dem = 0\n" +
                     "với k trong t {\n" +
                     "  dem = dem + 1\n" +
                     "  in_ra(k)\n" +
                     "}\n" +
                     "in_ra(dem)";
        string output = Chay(src);
        // Kiểm tra 2 key được duyệt
        return KiemTra(output.Contains("a") && output.Contains("b") && output.EndsWith("2"),
            "với k trong t duyệt 2 keys");
    }

    // 11. Dict có value là hàm → gọi được
    private static bool TestDictHamTrongDict()
    {
        string src = "hàm mot() { trả_về 1 }\nt = { \"f\": mot }\nin_ra(t[\"f\"]())";
        return KiemTra(Chay(src) == "1", "gọi hàm trong dict");
    }

    // 12. Lỗi: truy cập dict không tồn tại; gán index trên số
    private static bool TestDictLoiRuntime()
    {
        ChayLoi("in_ra(x[\"k\"])", "chưa được gán");
        ChayLoi("5[\"k\"] = 1", "không đánh chỉ số");
        return true;
    }

    // 13. Dict nhiều key + NLP demo
    private static bool TestDictNhieuKeyNLP()
    {
        string src = "t = { \"chuan_hoa\": \"normalize\", \"tach_tu\": \"segment\" }\n" +
                     "in_ra(t.có(\"chuan_hoa\"))\n" +
                     "in_ra(t.lay(\"tach_tu\"))\n" +
                     "in_ra(t.kich_thuoc())";
        return KiemTra(Chay(src) == "đúng\nsegment\n2", "NLP dict keys");
    }
}
