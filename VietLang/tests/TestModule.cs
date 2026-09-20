using System;
using System.IO;
using System.Text;

namespace VietLang;

public static class TestModule
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "module_co_ban", Run = TestModuleCoBan },
        new LexTest { Name = "module_nhieu_ham", Run = TestModuleNhieuHam },
        new LexTest { Name = "module_chia_se_bien", Run = TestModuleChiaSeBien },
        new LexTest { Name = "module_lop", Run = TestModuleLop },
        new LexTest { Name = "module_file_khong_ton_tai", Run = TestModuleFileKhongTonTai },
        new LexTest { Name = "module_circular", Run = TestModuleCircular },
        new LexTest { Name = "module_nested", Run = TestModuleNested },
        new LexTest { Name = "module_khong_duong_dan_chuoi", Run = TestModuleKhongDuongDanChuoi },
    };

    private static bool KiemTra(bool cond, string msg)
    {
        if (!cond) throw new Exception(msg);
        return true;
    }

    private static string FixturesDir =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "tests", "fixtures"));

    private static string ChayTep(string tenTep, string noiDung)
    {
        string dir = FixturesDir;
        string fullPath = Path.Combine(dir, tenTep);
        File.WriteAllText(fullPath, noiDung, Encoding.UTF8);
        try
        {
            var cu = Console.Out;
            var sw = new StringWriter();
            Console.SetOut(sw);
            try
            {
                string source = File.ReadAllText(fullPath, Encoding.UTF8);
                var program = Parser.Parse(source);
                var global = new PhamVi();
                Builtins.DangKy(global);
                var interp = new Interpreter();
                interp.Run(program, fullPath, global, null);
            }
            finally
            {
                Console.SetOut(cu);
            }
            return sw.ToString().Replace("\r\n", "\n").Trim('\n');
        }
        finally
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }

    private static void ChayLoi(string tenTep, string noiDung, string maCu)
    {
        string dir = FixturesDir;
        string fullPath = Path.Combine(dir, tenTep);
        File.WriteAllText(fullPath, noiDung, Encoding.UTF8);
        try
        {
            string source = File.ReadAllText(fullPath, Encoding.UTF8);
            var program = Parser.Parse(source);
            var global = new PhamVi();
            Builtins.DangKy(global);
            var interp = new Interpreter();
            interp.Run(program, fullPath, global, null);
        }
        catch (RuntimeError e)
        {
            if (maCu != null)
                KiemTra(e.Message.Contains(maCu), $"Lỗi thiếu cụm '{maCu}': {e.Message}");
            return;
        }
        finally
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        throw new Exception($"phải ném RuntimeError chứa '{maCu}'");
    }

    private static void ChayLoiParse(string src, string maCu)
    {
        try
        {
            Parser.Parse(src);
        }
        catch (ParseError e)
        {
            if (maCu != null)
                KiemTra(e.Message.Contains(maCu), $"Lỗi thiếu cụm '{maCu}': {e.Message}");
            return;
        }
        throw new Exception($"phải ném ParseError chứa '{maCu}'");
    }

    // 1. Import cơ bản: math.vl định nghĩa cộng(1,2) → 3
    private static bool TestModuleCoBan()
    {
        string output = ChayTep("test_co_ban.vl",
            "khai_báo \"math.vl\"\n" +
            "in_ra(cộng(1, 2))");
        return KiemTra(output == "3", $"cộng(1,2) phải = 3, got '{output}'");
    }

    // 2. Import nhiều hàm từ cùng file
    private static bool TestModuleNhieuHam()
    {
        string output = ChayTep("test_nhieu_ham.vl",
            "khai_báo \"math.vl\"\n" +
            "in_ra(cộng(10, 20))\n" +
            "in_ra(nhân(3, 4))");
        return KiemTra(output == "30\n12", $"30 và 12, got '{output}'");
    }

    // 3. Chia sẻ biến: file chính gán biến, file import dùng được không? 
    //    → Không, vì import chạy trước. Test biến từ file import dùng ở file chính.
    private static bool TestModuleChiaSeBien()
    {
        string output = ChayTep("test_chia_se.vl",
            "khai_báo \"math.vl\"\n" +
            "x = 5\n" +
            "y = cộng(x, 10)\n" +
            "in_ra(y)");
        return KiemTra(output == "15", $"x=5, cộng(5,10)=15, got '{output}'");
    }

    // 4. Import file có lớp
    private static bool TestModuleLop()
    {
        string output = ChayTep("test_lop.vl",
            "khai_báo \"util.vl\"\n" +
            "t = Tinh(7)\n" +
            "in_ra(t.doubled())");
        return KiemTra(output == "14", $"Tinh(7).doubled()=14, got '{output}'");
    }

    // 5. File không tồn tại → RuntimeError
    private static bool TestModuleFileKhongTonTai()
    {
        ChayLoi("test_khong_ton_tai.vl",
            "khai_báo \"khong_ton_tai.vl\"",
            "không tìm thấy tệp");
        return true;
    }

    // 6. Circular import: a.vl ↔ b.vl → skip lần 2, cả hai hàm đều có
    private static bool TestModuleCircular()
    {
        string output = ChayTep("test_circular.vl",
            "khai_báo \"circular_a.vl\"\n" +
            "in_ra(ham_a())\n" +
            "in_ra(ham_b())");
        return KiemTra(output == "A\nB", $"ham_a()=A, ham_b()=B, got '{output}'");
    }

    // 7. Nested import: file nhập file khác
    private static bool TestModuleNested()
    {
        string output = ChayTep("test_nested.vl",
            "khai_báo \"nested.vl\"\n" +
            "in_ra(vuông(5))");
        return KiemTra(output == "25", $"vuông(5)=25, got '{output}'");
    }

    // 8. Parser lỗi: nhập không phải chuỗi
    private static bool TestModuleKhongDuongDanChuoi()
    {
        ChayLoiParse("khai_báo x", "khai_báo cần đường dẫn dạng chuỗi");
        return true;
    }
}
