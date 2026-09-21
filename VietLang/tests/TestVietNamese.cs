using System;
using System.IO;

namespace VietLang;

/// <summary>Self-test 6 builtin tiếng Việt (M3.1).</summary>
public static class TestVietNamese
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "vn_chuan_hoa_co_dau", Run = TestChuanHoaCoDau },
        new LexTest { Name = "vn_chuan_hoa_duong_sat", Run = TestChuanHoaDuongSat },
        new LexTest { Name = "vn_chuan_hoa_already", Run = TestChuanHoaAlready },
        new LexTest { Name = "vn_chuan_hoa_error", Run = TestChuanHoaError },
        new LexTest { Name = "vn_tim_tu", Run = TestTimTu },
        new LexTest { Name = "vn_tim_tu_khong_tim_thay", Run = TestTimTuKhongTimThay },
        new LexTest { Name = "vn_tach_tu", Run = TestTachTu },
        new LexTest { Name = "vn_tach_cau", Run = TestTachCau },
        new LexTest { Name = "vn_tach_cau_no_punct", Run = TestTachCauNoPunct },
        new LexTest { Name = "vn_dem_tu", Run = TestDemTu },
        new LexTest { Name = "vn_dem_tu_rong", Run = TestDemTuRong },
        new LexTest { Name = "vn_chuan_hoa_tim_kiem", Run = TestChuanHoaTimKiem },
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

    // 1. chuan_hoa("Xin Chào Thế Giới") → "Xin Chao The Gioi"
    private static bool TestChuanHoaCoDau()
        => KiemTra(Chay("in_ra(chuan_hoa(\"Xin Chào Thế Giới\"))") == "Xin Chao The Gioi", "chuan_hoa bỏ dấu cơ bản");

    // 2. chuan_hoa("Đường sắt") → "Duong sat"
    private static bool TestChuanHoaDuongSat()
        => KiemTra(Chay("in_ra(chuan_hoa(\"Đường sắt\"))") == "Duong sat", "chuan_hoa Đường sắt");

    // 3. chuan_hoa("already") → "already"
    private static bool TestChuanHoaAlready()
        => KiemTra(Chay("in_ra(chuan_hoa(\"already\"))") == "already", "chuan_hoa giữ nguyên ASCII");

    // 4. chuan_hoa(123) → RuntimeError
    private static bool TestChuanHoaError()
    {
        ChayLoi("chuan_hoa(123)", "chuan_hoa cần chuỗi");
        return true;
    }

    // 5. tim_tu("xin chào chào", "chào") → [4, 10]
    private static bool TestTimTu()
    {
        string src = "vitri = tim_tu(\"xin chào chào\", \"chào\")\nin_ra(vitri)";
        string out_ = Chay(src);
        return KiemTra(out_ == "[4, 9]", $"tim_tu multiple matches: got '{out_}'");
    }

    // 6. tim_tu("hello", "xyz") → []
    private static bool TestTimTuKhongTimThay()
    {
        string src = "vitri = tim_tu(\"hello\", \"xyz\")\nin_ra(vitri)";
        return KiemTra(Chay(src) == "[]", "tim_tu no match");
    }

    // 7. tach_tu("xin chào thế giới") → ["xin", "chào", "thế", "giới"]
    private static bool TestTachTu()
    {
        string src = "từ_list = tach_tu(\"xin chào thế giới\")\nvới t trong từ_list {\n  in_ra(t)\n}";
        return KiemTra(Chay(src) == "xin\nchào\nthế\ngiới", "tach_tu phân tách đúng");
    }

    // 8. tach_cau("A. B? C!") → ["A.", "B?", "C!"]
    private static bool TestTachCau()
    {
        string src = "câu_list = tach_cau(\"A. B? C!\")\nvới c trong câu_list {\n  in_ra(c)\n}";
        return KiemTra(Chay(src) == "A.\nB?\nC!", "tach_cau giữ delimiter");
    }

    // 9. tach_cau("no punct") → ["no punct"]
    private static bool TestTachCauNoPunct()
    {
        string src = "câu_list = tach_cau(\"no punct\")\nvới c trong câu_list {\n  in_ra(c)\n}";
        return KiemTra(Chay(src) == "no punct", "tach_cau không có dấu câu");
    }

    // 10. dem_tu("xin chào") → 2
    private static bool TestDemTu()
        => KiemTra(Chay("in_ra(dem_tu(\"xin chào\"))") == "2", "dem_tu('xin chào') = 2");

    // 11. dem_tu("") → 0
    private static bool TestDemTuRong()
        => KiemTra(Chay("in_ra(dem_tu(\"\"))") == "0", "dem_tu('') = 0");

    // 12. chuan_hoa_tim_kiem("Xin Chào") → "xin chao"
    private static bool TestChuanHoaTimKiem()
        => KiemTra(Chay("in_ra(chuan_hoa_tim_kiem(\"Xin Chào\"))") == "xin chao", "chuan_hoa_tim_kiem lowercase+bo_dau");
}
