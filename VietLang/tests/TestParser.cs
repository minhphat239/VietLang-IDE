using System;
using System.Collections.Generic;

namespace VietLang;

/// <summary>Self-test parser: chạy cùng vòng `dotnet run -- test` với TestLexer.</summary>
public static class TestParser
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "ham_don_gian", Run = TestHamDonGian },
        new LexTest { Name = "uu_tien_so_hoc", Run = TestUuTienSoHoc },
        new LexTest { Name = "unary_am", Run = TestUnaryAm },
        new LexTest { Name = "neu_con_neu_khong_thi", Run = TestNeuChuoi3Nhanh },
        new LexTest { Name = "trong_luc", Run = TestTrongLuc },
        new LexTest { Name = "voi_trong", Run = TestVoiTrong },
        new LexTest { Name = "lop_2_ham", Run = TestLop },
        new LexTest { Name = "this_member", Run = TestThisMember },
        new LexTest { Name = "goi_ham_nhieu_doi_so", Run = TestGoiHam },
        new LexTest { Name = "postfix_chuoi", Run = TestPostfixChuoi },
        new LexTest { Name = "logic_so_sanh", Run = TestLogicSoSanh },
        new LexTest { Name = "semi_va_khoi_nhieu_dong", Run = TestSemiKhoi },
        new LexTest { Name = "mang", Run = TestMang },
        new LexTest { Name = "bool_va_rong", Run = TestBoolVaRong },
        new LexTest { Name = "loi_ngu_phap", Run = TestLoiNguPhap },
        new LexTest { Name = "dong_cua_lenh", Run = TestDongCuaLenh },
        new LexTest { Name = "neu_chain_nhieu_dong", Run = TestNeuChainNhieuDong },
    };

    private static Program P(string src) => Parser.Parse(src);

    private static bool KiemTra(bool cond, string msg)
    {
        if (!cond) throw new Exception(msg);
        return true;
    }

    private static T Ep<T>(object o, string msg)
    {
        KiemTra(o is T, msg);
        return (T)o;
    }

    private static bool BangSo(double a, double b) => Math.Abs(a - b) < 1e-9;

    // 1. hàm f() { trả_về 1 } -> FuncDeclStmt + ReturnStmt(NumLit).
    private static bool TestHamDonGian()
    {
        var stmts = P("hàm f() { trả_về 1 }").Stmts;
        KiemTra(stmts.Count == 1, "1 lệnh");
        var fn = Ep<FuncDeclStmt>(stmts[0], "lệnh đầu là FuncDeclStmt");
        KiemTra(fn.Name == "f", "tên hàm = f");
        KiemTra(fn.Params.Count == 0, "không tham số");
        KiemTra(fn.Body.Count == 1, "body 1 lệnh");
        var ret = Ep<ReturnStmt>(fn.Body[0], "body là ReturnStmt");
        var num = Ep<NumLit>(ret.Value, "trả_về có NumLit");
        KiemTra(BangSo(num.Value, 1), "giá trị 1");
        return true;
    }

    // 2. a = 1 + 2 * 3 -> BinaryExpr(CONG, 1, BinaryExpr(NHAN, 2, 3)).
    private static bool TestUuTienSoHoc()
    {
        var assign = Ep<AssignStmt>(P("a = 1 + 2 * 3").Stmts[0], "AssignStmt");
        KiemTra(Ep<NameExpr>(assign.Target, "target NameExpr").Name == "a", "gán cho a");
        var cong = Ep<BinaryExpr>(assign.Value, "giá trị BinaryExpr");
        KiemTra(cong.Op == "CONG", "phép +");
        KiemTra(BangSo(Ep<NumLit>(cong.Left, "trái số").Value, 1), "trái = 1");
        var nhan = Ep<BinaryExpr>(cong.Right, "phải BinaryExpr");
        KiemTra(nhan.Op == "NHAN", "phép *");
        KiemTra(BangSo(Ep<NumLit>(nhan.Left, "l").Value, 2), "2 *");
        KiemTra(BangSo(Ep<NumLit>(nhan.Right, "r").Value, 3), "* 3");
        return true;
    }

    // 3. a = -3 -> UnaryExpr("TRU", NumLit(3)).
    private static bool TestUnaryAm()
    {
        var assign = Ep<AssignStmt>(P("a = -3").Stmts[0], "AssignStmt");
        var tru = Ep<UnaryExpr>(assign.Value, "UnaryExpr");
        KiemTra(tru.Op == "TRU", "toán tử TRU");
        KiemTra(BangSo(Ep<NumLit>(tru.Operand, "toán hạng số").Value, 3), "toán hạng 3");
        return true;
    }

    // 4. nếu/còn_nếu/không_thì chuỗi 3 nhánh -> còn_nếu là IfStmt lồng trong Otherwise.
    private static bool TestNeuChuoi3Nhanh()
    {
        var neu = Ep<IfStmt>(P("nếu a { } còn_nếu b { } không_thì { }").Stmts[0], "IfStmt");
        KiemTra(Ep<NameExpr>(neu.Cond, "đk a").Name == "a", "điều kiện a");
        KiemTra(neu.Then.Count == 0, "nhánh nếu rỗng");
        KiemTra(neu.Otherwise != null && neu.Otherwise.Count == 1, "có nhánh còn_nếu");
        var conNeu = Ep<IfStmt>(neu.Otherwise[0], "còn_nếu là IfStmt");
        KiemTra(Ep<NameExpr>(conNeu.Cond, "đk b").Name == "b", "điều kiện b");
        KiemTra(conNeu.Otherwise != null, "còn_nếu có nhánh không_thì");
        return true;
    }

    // 5. trong_lúc a < 10 { b = b + 1\n c = 2 } -> WhileStmt cond + body 2 lệnh.
    private static bool TestTrongLuc()
    {
        var w = Ep<WhileStmt>(P("trong_lúc a < 10 {\nb = b + 1\nc = 2\n}").Stmts[0], "WhileStmt");
        var cond = Ep<BinaryExpr>(w.Cond, "điều kiện BinaryExpr");
        KiemTra(cond.Op == "NHO_HON", "phép <");
        KiemTra(Ep<NameExpr>(cond.Left, "a").Name == "a", "vế trái a");
        KiemTra(w.Body.Count == 2, "body 2 lệnh");
        Ep<AssignStmt>(w.Body[0], "lệnh 1 gán");
        Ep<AssignStmt>(w.Body[1], "lệnh 2 gán");
        return true;
    }

    // 6. với m trong danh_sách { in_ra(m) } -> ForInStmt Var/Iterable/Body.
    private static bool TestVoiTrong()
    {
        var f = Ep<ForInStmt>(P("với m trong danh_sách { in_ra(m) }").Stmts[0], "ForInStmt");
        KiemTra(f.Var == "m", "biến lặp m");
        KiemTra(Ep<NameExpr>(f.Iterable, "iterable").Name == "danh_sách", "iterable danh_sách");
        KiemTra(f.Body.Count == 1, "body 1 lệnh");
        var call = Ep<CallExpr>(Ep<ExprStmt>(f.Body[0], "lệnh gọi").Expr, "gọi hàm");
        KiemTra(Ep<NameExpr>(call.Callee, "callee").Name == "in_ra", "gọi in_ra");
        return true;
    }

    // 7. lớp 2 phương thức.
    private static bool TestLop()
    {
        var lop = Ep<ClassDeclStmt>(P("lớp ChuGia {\nhàm noi() { }\nhàm cười() { }\n}").Stmts[0], "ClassDeclStmt");
        KiemTra(lop.Name == "ChuGia", "tên lớp");
        KiemTra(lop.Methods.Count == 2, "2 phương thức");
        KiemTra(lop.Methods[0].Name == "noi", "phương thức 1 là noi");
        KiemTra(lop.Methods[1].Name == "cười", "phương thức 2 là cười");
        return true;
    }

    // 8. trả_về this.tên -> ReturnStmt(GetExpr(ThisExpr, "tên")).
    private static bool TestThisMember()
    {
        var p = P("hàm chào() { trả_về this.tên }");
        var fn = Ep<FuncDeclStmt>(p.Stmts[0], "FuncDeclStmt");
        var ret = Ep<ReturnStmt>(fn.Body[0], "ReturnStmt");
        var get = Ep<GetExpr>(ret.Value, "GetExpr");
        KiemTra(get.Name == "tên", "thành viên tên");
        Ep<ThisExpr>(get.Obj, "đối tượng this");
        return true;
    }

    // 9. in_ra("x", 1 + 2) -> CallExpr 2 đối số (chuỗi + biểu thức).
    private static bool TestGoiHam()
    {
        var expr = Ep<ExprStmt>(P("in_ra(\"x\", 1 + 2)").Stmts[0], "ExprStmt");
        var call = Ep<CallExpr>(expr.Expr, "CallExpr");
        KiemTra(Ep<NameExpr>(call.Callee, "callee").Name == "in_ra", "gọi in_ra");
        KiemTra(call.Args.Count == 2, "2 đối số");
        KiemTra(Ep<StrLit>(call.Args[0], "đối 1 chuỗi").Value == "x", "đối 1 = 'x'");
        var cong = Ep<BinaryExpr>(call.Args[1], "đối 2 BinaryExpr");
        KiemTra(cong.Op == "CONG", "đối 2 là 1 + 2");
        return true;
    }

    // 10. a.b[0].c -> GetExpr(IndexExpr(GetExpr(a, b), 0), c).
    private static bool TestPostfixChuoi()
    {
        var expr = Ep<ExprStmt>(P("a.b[0].c").Stmts[0], "ExprStmt");
        var getC = Ep<GetExpr>(expr.Expr, "ngoài cùng GetExpr(.c)");
        KiemTra(getC.Name == "c", "thành viên c");
        var idx = Ep<IndexExpr>(getC.Obj, "index [0]");
        KiemTra(BangSo(Ep<NumLit>(idx.Index, "chỉ mục").Value, 0), "chỉ mục 0");
        var getB = Ep<GetExpr>(idx.Obj, "index.Obj GetExpr(.b)");
        KiemTra(getB.Name == "b", "thành viên b");
        KiemTra(Ep<NameExpr>(getB.Obj, "gốc a").Name == "a", "gốc là a");
        return true;
    }

    // 11. nếu a >= 1 và b != 2 hoặc không c -> hoặc( và( >=, != ), không(c) ).
    private static bool TestLogicSoSanh()
    {
        var neu = Ep<IfStmt>(P("nếu a >= 1 và b != 2 hoặc không c { }").Stmts[0], "IfStmt");
        var hoac = Ep<BinaryExpr>(neu.Cond, "gốc là hoặc");
        KiemTra(hoac.Op == "HOAC", "phép hoặc ngoài cùng");
        var va = Ep<BinaryExpr>(hoac.Left, "vế trái hoặc là và");
        KiemTra(va.Op == "VA", "phép và");
        var ge = Ep<BinaryExpr>(va.Left, "vế trái và là >=");
        KiemTra(ge.Op == "LON_HON_HOAC_BANG", "phép >=");
        KiemTra(Ep<NameExpr>(ge.Left, "a").Name == "a", "a >= 1");
        var ne = Ep<BinaryExpr>(va.Right, "vế phải và là !=");
        KiemTra(ne.Op == "KHAC", "phép !=");
        KiemTra(Ep<NameExpr>(ne.Left, "b").Name == "b", "b != 2");
        KiemTra(BangSo(Ep<NumLit>(ne.Right, "2").Value, 2), "b != 2");
        var khong = Ep<UnaryExpr>(hoac.Right, "vế phải hoặc là không");
        KiemTra(khong.Op == "KHONG", "phép không");
        KiemTra(Ep<NameExpr>(khong.Operand, "c").Name == "c", "không c");
        return true;
    }

    // 12. `;` kết thúc lệnh + khối {} nhiều dòng.
    private static bool TestSemiKhoi()
    {
        var stmts = P("x = 1; y = 2\nnếu x {\na = 1\nb = 2\n}").Stmts;
        KiemTra(stmts.Count == 3, "3 lệnh: 2 gán + 1 nếu");
        Ep<AssignStmt>(stmts[0], "lệnh 0 gán (x = 1; kết thúc bằng ;)");
        Ep<AssignStmt>(stmts[1], "lệnh 1 gán (y = 2)");
        var neu = Ep<IfStmt>(stmts[2], "lệnh 2 nếu");
        KiemTra(neu.Then.Count == 2, "khối nếu 2 lệnh nhiều dòng");
        return true;
    }

    // 13. Mảng rỗng [] và mảng có phần tử [1, "hai"].
    private static bool TestMang()
    {
        var assign1 = Ep<AssignStmt>(P("x = []").Stmts[0], "AssignStmt x = []");
        var arr1 = Ep<ArrayLit>(assign1.Value, "ArrayLit rỗng");
        KiemTra(arr1.Items.Count == 0, "mảng rỗng 0 phần tử");

        var stmts = P("x = [1, \"hai\"]").Stmts;
        var assign2 = Ep<AssignStmt>(stmts[0], "AssignStmt x = [1, 'hai']");
        var arr2 = Ep<ArrayLit>(assign2.Value, "ArrayLit 2 phần tử");
        KiemTra(arr2.Items.Count == 2, "mảng 2 phần tử");
        KiemTra(BangSo(Ep<NumLit>(arr2.Items[0], "phần tử 0").Value, 1), "phần tử 0 = 1");
        KiemTra(Ep<StrLit>(arr2.Items[1], "phần tử 1").Value == "hai", "phần tử 1 = 'hai'");
        return true;
    }

    // 15. a = đúng\nb = sai\nc = rỗng -> BoolLit(true), BoolLit(false), NullLit.
    private static bool TestBoolVaRong()
    {
        var stmts = P("a = đúng\nb = sai\nc = rỗng").Stmts;
        KiemTra(stmts.Count == 3, "3 lệnh gán");
        var a1 = Ep<AssignStmt>(stmts[0], "a = đúng là AssignStmt");
        var a2 = Ep<AssignStmt>(stmts[1], "b = sai là AssignStmt");
        var a3 = Ep<AssignStmt>(stmts[2], "c = rỗng là AssignStmt");
        KiemTra(Ep<NameExpr>(a1.Target, "target a").Name == "a", "gán cho a");
        KiemTra(Ep<NameExpr>(a2.Target, "target b").Name == "b", "gán cho b");
        KiemTra(Ep<NameExpr>(a3.Target, "target c").Name == "c", "gán cho c");
        KiemTra(Ep<BoolLit>(a1.Value, "đúng là BoolLit").Value, "đúng = true");
        KiemTra(!Ep<BoolLit>(a2.Value, "sai là BoolLit").Value, "sai = false");
        Ep<NullLit>(a3.Value, "rỗng là NullLit");
        return true;
    }

    // 14. Lỗi cú pháp: từ khóa thiếu dấu có gợi ý + thiếu '}' có chỉ dòng mở.
    private static bool TestLoiNguPhap()
    {
        bool coLoi(string src)
        {
            try
            {
                P(src);
                return false;
            }
            catch (ParseError)
            {
                return true;
            }
        }

        KiemTra(coLoi("ham f() { trả_về 1 }"), "'ham' ở đầu lệnh phải là lỗi cú pháp");
        try
        {
            P("ham f() { trả_về 1 }");
            throw new Exception("không ném ParseError cho 'ham'");
        }
        catch (ParseError e)
        {
            KiemTra(e.Message.Contains("hàm"), $"gợi ý 'hàm' — thực tế: {e.Message}");
        }

        try
        {
            P("dung khi x");
            throw new Exception("không ném ParseError cho 'dung'");
        }
        catch (ParseError e)
        {
            KiemTra(e.Message.Contains("đúng"), $"gợi ý có 'đúng' — thực tế: {e.Message}");
        }

        try
        {
            P("hàm f() { trả_về 1");
            throw new Exception("không ném ParseError khi thiếu '}'");
        }
        catch (ParseError e)
        {
            KiemTra(e.Message.Contains("thiếu dấu"), $"báo thiếu dấu — thực tế: {e.Message}");
            KiemTra(e.Message.Contains("'}'"), $"nhắc dấu '}}' — thực tế: {e.Message}");
            KiemTra(e.Message.Contains("dòng 1"), $"kèm dòng mở — thực tế: {e.Message}");
        }
        return true;
    }

    // 16. Mỗi lệnh mang số dòng: `trả_về` ở dòng 2 -> ReturnStmt.Dong == 2 (1-based).
    private static bool TestDongCuaLenh()
    {
        var fn = Ep<FuncDeclStmt>(P("hàm f() {\n  trả_về 1\n}").Stmts[0], "FuncDeclStmt");
        var ret = Ep<ReturnStmt>(fn.Body[0], "ReturnStmt");
        KiemTra(ret.Dong == 2, $"ReturnStmt.Dong phải = 2 — thực tế: {ret.Dong}");
        return true;
    }

    // 17. còn_nếu/không_thì đứng sau dấu xuống dòng (kiểu Python) — `}` dòng riêng.
    private static bool TestNeuChainNhieuDong()
    {
        var src =
            "nếu a {\n" +
            "  x = 1\n" +
            "}\n" +
            "còn_nếu b {\n" +
            "  y = 2\n" +
            "}\n" +
            "không_thì {\n" +
            "  z = 3\n" +
            "}\n";
        var stmts = P(src).Stmts;
        KiemTra(stmts.Count == 1, $"1 lệnh — thực tế: {stmts.Count}");
        var neu = Ep<IfStmt>(stmts[0], "IfStmt gốc");
        KiemTra(Ep<NameExpr>(neu.Cond, "đk a").Name == "a", "điều kiện a");
        KiemTra(neu.Dong == 1, $"nếu ở dòng 1 — thực tế: {neu.Dong}");
        KiemTra(neu.Then.Count == 1, "nhánh nếu 1 lệnh (x = 1)");
        KiemTra(neu.Otherwise != null && neu.Otherwise.Count == 1, "gốc có nhánh còn_nếu");
        var conNeu = Ep<IfStmt>(neu.Otherwise[0], "còn_nếu là IfStmt lồng");
        KiemTra(Ep<NameExpr>(conNeu.Cond, "đk b").Name == "b", "điều kiện b");
        KiemTra(conNeu.Dong == 4, $"còn_nếu ở dòng 4 — thực tế: {conNeu.Dong}");
        KiemTra(conNeu.Then.Count == 1, "nhánh còn_nếu 1 lệnh (y = 2)");
        KiemTra(conNeu.Otherwise != null && conNeu.Otherwise.Count == 1, "còn_nếu có nhánh không_thì");
        var khongThi = Ep<AssignStmt>(conNeu.Otherwise[0], "không_thì là AssignStmt (z = 3)");
        KiemTra(Ep<NameExpr>(khongThi.Target, "target không_thì").Name == "z", "không_thì gán z");
        return true;
    }
}