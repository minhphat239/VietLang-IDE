using System;
using System.IO;

namespace VietLang;

/// <summary>Self-test interpreter: chạy source → assert output, hoặc assert lỗi runtime.</summary>
public static class TestInterpreter
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "so_hoc_va_in_ra", Run = TestSoHocInRa },
        new LexTest { Name = "bien_va_gan_lai", Run = TestBienGanLai },
        new LexTest { Name = "neu_chan_chuoi_logic", Run = TestNeuChuoiLogic },
        new LexTest { Name = "trong_luc_1_4", Run = TestTrongLuc },
        new LexTest { Name = "ham_de_quy_giai_thua", Run = TestDeQuy },
        new LexTest { Name = "ham_nhieu_tham_so_chuoi", Run = TestHamChuoi },
        new LexTest { Name = "lop_khoi_tao_this_method", Run = TestLop },
        new LexTest { Name = "mang_day_du", Run = TestMang },
        new LexTest { Name = "chuoi_do_dai_index", Run = TestChuoi },
        new LexTest { Name = "chan_tri_falsy", Run = TestChanTri },
        new LexTest { Name = "tra_ve_som_break_continue", Run = TestTraVeBreakContinue },
        new LexTest { Name = "loi_runtime", Run = TestLoiRuntime },
        new LexTest { Name = "unary_khong", Run = TestUnaryKhong },
        new LexTest { Name = "bound_method_this", Run = TestBoundMethodThis },
    };

    // ─── Helper ────────────────────────────────────────────────────

    private static bool KiemTra(bool cond, string msg)
    {
        if (!cond) throw new Exception(msg);
        return true;
    }

    /// <summary>Chạy source trong Interpreter mới, trả về toàn bộ output in_ra (chuẩn hóa \n).</summary>
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

    /// <summary>Chạy source, phải ném RuntimeError có chua cụm mong đợi.</summary>
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

    // ─── Test ────────────────────────────────────────────────────────

    // 1. `in_ra(1 + 2 * 3)` → "7".
    private static bool TestSoHocInRa() => KiemTra(Chay("in_ra(1 + 2 * 3)") == "7", "1+2*3 phải = 7");

    // 2. Biến + gán lại: x = 1; x = x + 2 → 3.
    private static bool TestBienGanLai() => KiemTra(Chay("x = 1\nx = x + 2\nin_ra(x)") == "3", "x = 3");

    // 3. nếu/còn_nếu/không_thì + và/hoặc/không → đúng nhánh.
    private static bool TestNeuChuoiLogic()
    {
        string src = "nếu sai hoặc sai { in_ra(\"A\"); } " +
                     "còn_nếu không đúng { in_ra(\"B\"); } " +
                     "còn_nếu đúng và sai { in_ra(\"C\"); } " +
                     "còn_nếu không sai { in_ra(\"D\"); } " +
                     "không_thì { in_ra(\"E\"); }";
        return KiemTra(Chay(src) == "D", "chuỗi nếu-còn_nếu đi đúng nhánh D");
    }

    // 4. trong_lúc: in 1..4.
    private static bool TestTrongLuc()
    {
        string src = "i = 1\n" +
                     "trong_lúc i <= 4 {\n" +
                     "  in_ra(i)\n" +
                     "  i = i + 1\n" +
                     "}";
        return KiemTra(Chay(src) == "1\n2\n3\n4", "trong_lúc in 1..4");
    }

    // 5. Hàm đệ quy giai thừa.
    private static bool TestDeQuy()
    {
        string src = "hàm gt(n) {\n  nếu n <= 1 { trả_về 1 }\n  trả_về n * gt(n - 1)\n}\n" +
                     "in_ra(gt(5))";
        return KiemTra(Chay(src) == "120", "gt(5) = 120");
    }

    // 6. Hàm nhiều tham số + chuỗi (dùng chuyen_chuoi vì `+` không nối số vào chuỗi).
    private static bool TestHamChuoi()
    {
        string src = "hàm chào(t, tuoi) {\n  trả_về \"Xin chào \" + t + \", \" + chuyen_chuoi(tuoi)\n}\n" +
                     "in_ra(chào(\"An\", 20))";
        return KiemTra(Chay(src) == "Xin chào An, 20", "chào(An, 20)");
    }

    // 7. Lớp + khởi_tạo + this + method.
    private static bool TestLop()
    {
        string src = "lớp Nguoi {\n" +
                     "  hàm khởi_tạo(ten) { this.ten = ten }\n" +
                     "  hàm xin_chao() { trả_về \"Xin chào \" + this.ten }\n" +
                     "}\n" +
                     "n = Nguoi(\"Minh\")\n" +
                     "in_ra(n.xin_chao())";
        return KiemTra(Chay(src) == "Xin chào Minh", "Nguoi('Minh').xin_chao()");
    }

    // 8. Mảng đầy đủ: gán a[i], them, do_dai, for-in.
    private static bool TestMang()
    {
        string src = "a = [1, 2, 3]\n" +
                     "a[1] = 9\n" +
                     "them(a, 4)\n" +
                     "in_ra(do_dai(a))\n" +
                     "với x trong a {\n  in_ra(x)\n}";
        return KiemTra(Chay(src) == "4\n1\n9\n3\n4", "do_dai=4, các phần tử 1 9 3 4");
    }

    // 9. Chuỗi: do_dai + a[0].
    private static bool TestChuoi()
    {
        string src = "a = \"hello\"\n" +
                     "in_ra(do_dai(a))\n" +
                     "in_ra(a[0])";
        return KiemTra(Chay(src) == "5\nh", "do_dai('hello')=5, 'hello'[0]='h'");
    }

    // 10. Chân trị: rỗng / "" / 0 / [] đều falsy → không_thì.
    private static bool TestChanTri()
    {
        string src = "nếu rỗng { in_ra(\"S1\"); } " +
                     "còn_nếu \"\" { in_ra(\"S2\"); } " +
                     "còn_nếu 0 { in_ra(\"S3\"); } " +
                     "còn_nếu [] { in_ra(\"S4\"); } " +
                     "không_thì { in_ra(\"đúng\"); }";
        return KiemTra(Chay(src) == "đúng", "4 giá trị falsy bị bỏ");
    }

    // 11. trả_về sớm + tiếp/dừng trong vòng lặp.
    private static bool TestTraVeBreakContinue()
    {
        string src = "hàm dem() {\n  nếu đúng { trả_về 7 }\n  trả_về 0\n}\n" +
                     "x = 0\n" +
                     "trong_lúc đúng {\n" +
                     "  x = x + 1\n" +
                     "  nếu x == 2 { tiếp }\n" +
                     "  nếu x == 4 { dừng }\n" +
                     "  in_ra(x)\n" +
                     "}\n" +
                     "in_ra(x)\n" +
                     "in_ra(dem())";
        return KiemTra(Chay(src) == "1\n3\n4\n7", "tiếp bỏ 2, dừng tại 4, dem()=7");
    }

    // 12. Lỗi runtime: chia 0, biến chưa gán, index ngoài mảng, chuyen_so fail.
    private static bool TestLoiRuntime()
    {
        ChayLoi("x = 1 / 0", "chia cho số 0");
        ChayLoi("in_ra(biến_lạ)", "chưa được gán");
        ChayLoi("a = [1, 2, 3]\nin_ra(a[5])", "ngoài phạm vi");
        ChayLoi("in_ra(chuyen_so(\"abc\"))", "không chuyển được");
        return true;
    }

    // 13. Unary '-' và 'không'.
    private static bool TestUnaryKhong()
    {
        string src = "a = -3\nin_ra(a)\nin_ra(không đúng)\nin_ra(-5)";
        return KiemTra(Chay(src) == "-3\nsai\n-5", "a=-3, không đúng=sai, -5");
    }

    // 14. Bound method: gọi method khác bên trong method qua this.
    private static bool TestBoundMethodThis()
    {
        string src = "lớp HinhChuNhat {\n" +
                     "  hàm khởi_tạo(w, h) { this.w = w; this.h = h }\n" +
                     "  hàm dien_tich() { trả_về this.w * this.h }\n" +
                     "  hàm mo_ta() { trả_về \"Diện tích: \" + chuyen_chuoi(this.dien_tich()) }\n" +
                     "}\n" +
                     "r = HinhChuNhat(3, 4)\n" +
                     "in_ra(r.dien_tich())\n" +
                     "in_ra(r.mo_ta())";
        return KiemTra(Chay(src) == "12\nDiện tích: 12", "HinhChuNhat(3,4)");
    }
}