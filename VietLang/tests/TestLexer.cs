using System;
using System.Collections.Generic;
using System.Linq;

namespace VietLang;

public sealed class LexTest
{
    public string Name { get; set; }
    public Func<bool> Run { get; set; }
}

/// <summary>Self-test tokenizer: chạy bằng `dotnet run -- test`.</summary>
public static class TestLexer
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "tu_khoa_co_dau", Run = TestTuKhoaCoDau },
        new LexTest { Name = "ident_co_dau", Run = TestIdentCoDau },
        new LexTest { Name = "so", Run = TestSo },
        new LexTest { Name = "tru_nhi_phan", Run = TestTruNhiPhan },
        new LexTest { Name = "chuoi_escape", Run = TestChuoiEscape },
        new LexTest { Name = "chuoi_nhay_don", Run = TestChuoiNhayDon },
        new LexTest { Name = "comment", Run = TestComment },
        new LexTest { Name = "toan_tu", Run = TestToanTu },
        new LexTest { Name = "newline_trong_ngoac", Run = TestNewlineTrongNgoac },
        new LexTest { Name = "newline_ngoai_ngoac", Run = TestNewlineNgoaiNgoac },
        new LexTest { Name = "newline_trong_khoi", Run = TestNewlineTrongKhoi },
        new LexTest { Name = "truy_cap_thanh_phan", Run = TestTruyCapThanhPhan },
        new LexTest { Name = "ky_tu_la", Run = TestKyTuLa },
        new LexTest { Name = "bang_goi_y_thieu_dau", Run = TestBangGoiY },
    };

    private static List<Token> Tok(string src) =>
        new Lexer(src).LexAll().Where(t => t.Kind != TokenKind.EOF).ToList();

    private static bool KiemTra(bool cond, string msg)
    {
        if (!cond) throw new Exception(msg);
        return true;
    }

    // 1. Từ khóa đúng dấu.
    private static bool TestTuKhoaCoDau()
    {
        var t = Tok("hàm");
        KiemTra(t.Count == 1, "hàm -> 1 token");
        KiemTra(t[0].Kind == TokenKind.HAM, "hàm -> TokenKind.HAM (TUKHOA)");
        KiemTra(t[0].IsKeyword, "HAM phải được nhận diện là keyword");
        KiemTra(t[0].Lexeme == "hàm", "lexeme phải là 'hàm'");

        var d = Tok("đúng");
        KiemTra(d.Count == 1 && d[0].Kind == TokenKind.DUNG_TRUE && d[0].Lexeme == "đúng",
            "đúng -> TUKHOA đúng (DUNG_TRUE)");

        var dung = Tok("dừng");
        KiemTra(dung.Count == 1 && dung[0].Kind == TokenKind.DUNG_BREAK, "dừng -> DUNG_BREAK");
        return true;
    }

    // 2. Identifier có dấu.
    private static bool TestIdentCoDau()
    {
        var t = Tok("tổng");
        KiemTra(t.Count == 1, "tổng -> 1 token");
        KiemTra(t[0].Kind == TokenKind.TEN, "tổng -> TEN");
        KiemTra(t[0].Lexeme == "tổng", "lexeme = 'tổng'");
        return true;
    }

    // 3. Số nguyên + thập phân; `-` luôn là toán tử (số âm = UnaryExpr của parser).
    private static bool TestSo()
    {
        var t = Tok("42 4.5 -3");
        KiemTra(t.Count == 4, "4 token: 42, 4.5, TRU(-), 3");
        KiemTra(t[0].Kind == TokenKind.SO && Math.Abs(t[0].So - 42) < 1e-9 && t[0].Lexeme == "42", "42 -> SO(42)");
        KiemTra(t[1].Kind == TokenKind.SO && Math.Abs(t[1].So - 4.5) < 1e-9 && t[1].Lexeme == "4.5", "4.5 -> SO(4.5)");
        KiemTra(t[2].Kind == TokenKind.TRU && t[2].Lexeme == "-", "- -> TRU (toán tử)");
        KiemTra(t[3].Kind == TokenKind.SO && Math.Abs(t[3].So - 3) < 1e-9 && t[3].Lexeme == "3", "3 -> SO(3)");
        return true;
    }

    // Regression: phép trừ nhị phân không vỡ thành 2 literal âm.
    private static bool TestTruNhiPhan()
    {
        var t = Tok("5-3");
        KiemTra(t.Count == 3, "5-3 -> 3 token");
        KiemTra(t[0].Kind == TokenKind.SO && Math.Abs(t[0].So - 5) < 1e-9 && t[0].Lexeme == "5", "5 -> SO");
        KiemTra(t[1].Kind == TokenKind.TRU && t[1].Lexeme == "-", "TRU ở giữa hai số");
        KiemTra(t[2].Kind == TokenKind.SO && Math.Abs(t[2].So - 3) < 1e-9 && t[2].Lexeme == "3", "3 -> SO");
        return true;
    }

    // 4. Chuỗi nháy kép + escape \n.
    private static bool TestChuoiEscape()
    {
        var t = Tok("\"chào\\nbạn\"");
        KiemTra(t.Count == 1, "1 token chuỗi");
        KiemTra(t[0].Kind == TokenKind.CHUOI, "CHUOI");
        KiemTra((string)t[0].Value == "chào\nbạn", "escape \\n thành xuống dòng");
        KiemTra(t[0].Lexeme == "\"chào\\nbạn\"", "lexeme giữ gốc cả nháy");
        return true;
    }

    // 5. Chuỗi nháy đơn.
    private static bool TestChuoiNhayDon()
    {
        var t = Tok("'xin chào'");
        KiemTra(t.Count == 1, "1 token chuỗi");
        KiemTra(t[0].Kind == TokenKind.CHUOI, "CHUOI");
        KiemTra((string)t[0].Value == "xin chào", "giá trị 'xin chào'");
        return true;
    }

    // 6. Comment # ... bị bỏ; newline sau comment vẫn là NEWLINE.
    private static bool TestComment()
    {
        var t = Tok("# ghi chú\nsố = 1");
        KiemTra(t.Count == 4, "NEWLINE + 3 token lệnh");
        KiemTra(t[0].Kind == TokenKind.NEWLINE, "newline sau comment có mặt");
        KiemTra(t[1].Kind == TokenKind.TEN && t[1].Lexeme == "số", "token sau comment đúng (số)");
        KiemTra(t[2].Kind == TokenKind.BANG, "dấu =");
        KiemTra(t[3].Kind == TokenKind.SO && Math.Abs(t[3].So - 1) < 1e-9, "số 1");
        KiemTra(t.All(x => x.Lexeme != "ghi"), "comment không phát token nào");
        return true;
    }

    // 7. Phép toán.
    private static bool TestToanTu()
    {
        var expect = new[]
        {
            TokenKind.CONG, TokenKind.TRU, TokenKind.NHAN, TokenKind.CHIA, TokenKind.CHIA_DU,
            TokenKind.SO_SANH_BANG, TokenKind.KHAC, TokenKind.NHO_HON, TokenKind.LON_HON,
            TokenKind.NHO_HON_HOAC_BANG, TokenKind.LON_HON_HOAC_BANG, TokenKind.BANG,
        };
        var t = Tok("+ - * / % == != < > <= >= =");
        KiemTra(t.Count == expect.Length, "đủ 12 toán tử");
        for (int i = 0; i < expect.Length; i++)
            KiemTra(t[i].Kind == expect[i], $"toán tử thứ {i} = {expect[i]}");
        return true;
    }

    // 8. Newline trong () bị bỏ; sau ) vẫn phát.
    private static bool TestNewlineTrongNgoac()
    {
        var t = Tok("hàm f(\n a,\n b\n)\n");
        KiemTra(t.Count(x => x.Kind == TokenKind.NEWLINE) == 1, "chỉ 1 NEWLINE (sau dấu đóng ) — trong () bị bỏ");
        KiemTra(t[^1].Kind == TokenKind.NEWLINE, "NEWLINE nằm sau ')'");
        KiemTra(t[0].Kind == TokenKind.HAM, "hàm");
        KiemTra(t[1].Kind == TokenKind.TEN && t[1].Lexeme == "f", "f");
        return true;
    }

    // 9. Newline ngoài ngoặc được phát.
    private static bool TestNewlineNgoaiNgoac()
    {
        var t = Tok("a = 1\nb = 2");
        KiemTra(t.Count(x => x.Kind == TokenKind.NEWLINE) == 1, "1 NEWLINE giữa 2 lệnh");
        KiemTra(t.Count == 7, "7 token");
        return true;
    }

    // 10. Trong {} nhiều lệnh nhiều dòng -> có đủ số NEWLINE.
    private static bool TestNewlineTrongKhoi()
    {
        var t = Tok("{\nx = 1\ny = 2\n}");
        KiemTra(t.Count(x => x.Kind == TokenKind.NEWLINE) == 3, "3 NEWLINE trong khối {} (newline vẫn là dấu kết thúc lệnh)");
        return true;
    }

    // 11. a.b[0] -> TEN, '.', TEN, '[', SO, ']'
    private static bool TestTruyCapThanhPhan()
    {
        var t = Tok("a.b[0]");
        KiemTra(t.Count == 6, "6 token");
        KiemTra(t[0].Kind == TokenKind.TEN && t[0].Lexeme == "a", "a");
        KiemTra(t[1].Kind == TokenKind.DAU_CHAM && t[1].Lexeme == ".", ".");
        KiemTra(t[2].Kind == TokenKind.TEN && t[2].Lexeme == "b", "b");
        KiemTra(t[3].Kind == TokenKind.DAU_MO_NGOAC_VUONG, "[");
        KiemTra(t[4].Kind == TokenKind.SO && Math.Abs(t[4].So - 0) < 1e-9, "0");
        KiemTra(t[5].Kind == TokenKind.DAU_DONG_NGOAC_VUONG, "]");
        return true;
    }

    // 12. Ký tự lạ -> lỗi từ vựng có dòng/cột, không crash.
    private static bool TestKyTuLa()
    {
        try
        {
            new Lexer("a b @").LexAll();
            throw new Exception("phải ném LexError");
        }
        catch (LexError e)
        {
            KiemTra(e.Message.Contains("dòng 1"), "lỗi có số dòng");
            KiemTra(e.Message.Contains("không hợp lệ"), "có cụm 'không hợp lệ'");
            KiemTra(e.Message.Contains("@"), "lỗi nhắc ký tự '@'");
            KiemTra(e.Line == 1 && e.Col == 5, $"dòng/cột đúng (1:5) — thực tế {e.Line}:{e.Col}");
            return true;
        }
    }

    // 13. Bảng gợi ý thiếu dấu tồn tại.
    private static bool TestBangGoiY()
    {
        KiemTra(Lexer.SuggestionTable.Count >= 5, $"bảng ≥ 5 phần tử (thực tế {Lexer.SuggestionTable.Count})");
        KiemTra(Lexer.GoiYThieuDau("ham") == "hàm", "ham -> hàm");
        KiemTra(Lexer.GoiYThieuDau("lop") == "lớp", "lop -> lớp");
        var dung = Lexer.GoiYThieuDau("dung");
        KiemTra(dung == "đúng", "dung -> đúng");
        KiemTra(Lexer.GoiYThieuDau("rong") == "rỗng", "rong -> rỗng");
        KiemTra(Lexer.GoiYThieuDau("Sai") == "sai", "Sai -> sai");
        KiemTra(Lexer.GoiYThieuDau("va") == "và", "va -> và");
        KiemTra(Lexer.GoiYThieuDau("tổng") == null, "tổng không phải từ khóa thiếu dấu");
        return true;
    }
}