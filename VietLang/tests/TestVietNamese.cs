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
        throw new Exception(maCu == null ? "phải ném RuntimeError" : $"phải ném RuntimeError chứa '{maCu}'");
    }

    // 1. chuẩn_hóa("Xin Chào Thế Giới") → "Xin Chao The Gioi"
    private static bool TestChuanHoaCoDau()
        => KiemTra(Chay("in_ra(chuẩn_hóa(\"Xin Chào Thế Giới\"))") == "Xin Chao The Gioi", "chuẩn_hóa bỏ dấu cơ bản");

    // 2. chuẩn_hóa("Đường sắt") → "Duong sat"
    private static bool TestChuanHoaDuongSat()
        => KiemTra(Chay("in_ra(chuẩn_hóa(\"Đường sắt\"))") == "Duong sat", "chuẩn_hóa Đường sắt");

    // 3. chuẩn_hóa("already") → "already"
    private static bool TestChuanHoaAlready()
        => KiemTra(Chay("in_ra(chuẩn_hóa(\"already\"))") == "already", "chuẩn_hóa giữ nguyên ASCII");

    // 4. chuẩn_hóa(123) → RuntimeError
    private static bool TestChuanHoaError()
    {
        ChayLoi("chuẩn_hóa(123)", "chuẩn_hóa cần chuỗi");
        return true;
    }

    // 5. tìm_từ("xin chào chào", "chào") → [4, 10]
    private static bool TestTimTu()
    {
        string src = "vitri = tìm_từ(\"xin chào chào\", \"chào\")\nin_ra(vitri)";
        string out_ = Chay(src);
        return KiemTra(out_ == "[4, 9]", $"tìm_từ multiple matches: got '{out_}'");
    }

    // 6. tìm_từ("hello", "xyz") → []
    private static bool TestTimTuKhongTimThay()
    {
        string src = "vitri = tìm_từ(\"hello\", \"xyz\")\nin_ra(vitri)";
        return KiemTra(Chay(src) == "[]", "tìm_từ no match");
    }

    // 7. tách_từ("xin chào thế giới") → ["xin", "chào", "thế", "giới"]
    private static bool TestTachTu()
    {
        string src = "từ_list = tách_từ(\"xin chào thế giới\")\nvới t trong từ_list {\n  in_ra(t)\n}";
        return KiemTra(Chay(src) == "xin\nchào\nthế\ngiới", "tách_từ phân tách đúng");
    }

    // 8. tách_câu("A. B? C!") → ["A.", "B?", "C!"]
    private static bool TestTachCau()
    {
        string src = "câu_list = tách_câu(\"A. B? C!\")\nvới c trong câu_list {\n  in_ra(c)\n}";
        return KiemTra(Chay(src) == "A.\nB?\nC!", "tách_câu giữ delimiter");
    }

    // 9. tách_câu("no punct") → ["no punct"]
    private static bool TestTachCauNoPunct()
    {
        string src = "câu_list = tách_câu(\"no punct\")\nvới c trong câu_list {\n  in_ra(c)\n}";
        return KiemTra(Chay(src) == "no punct", "tách_câu không có dấu câu");
    }

    // 10. đếm_từ("xin chào") → 2
    private static bool TestDemTu()
        => KiemTra(Chay("in_ra(đếm_từ(\"xin chào\"))") == "2", "đếm_từ('xin chào') = 2");

    // 11. đếm_từ("") → 0
    private static bool TestDemTuRong()
        => KiemTra(Chay("in_ra(đếm_từ(\"\"))") == "0", "đếm_từ('') = 0");

    // 12. chuẩn_hóa_tìm_kiếm("Xin Chào") → "xin chao"
    private static bool TestChuanHoaTimKiem()
        => KiemTra(Chay("in_ra(chuẩn_hóa_tìm_kiếm(\"Xin Chào\"))") == "xin chao", "chuẩn_hóa_tìm_kiếm lowercase+bo_dau");
}
