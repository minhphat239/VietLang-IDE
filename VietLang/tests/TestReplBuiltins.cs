using System;
using System.IO;

namespace VietLang;

public static class TestReplBuiltins
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "thoat_dang_ky", Run = TestThoatDangKy },
        new LexTest { Name = "doc_file_co_ban", Run = TestDocFileCoBan },
        new LexTest { Name = "doc_file_khong_ton_tai", Run = TestDocFileKhongTonTai },
        new LexTest { Name = "ghi_file_tra_noi_dung", Run = TestGhiFileTraNoiDung },
        new LexTest { Name = "ghi_file_ghi_de", Run = TestGhiFileGhiDe },
        new LexTest { Name = "ton_tai_file", Run = TestTonTaiFile },
        new LexTest { Name = "ton_tai_thu_muc", Run = TestTonTaiThuMuc },
        new LexTest { Name = "ton_tai_khong_ton_tai", Run = TestTonTaiKhongTonTai },
        new LexTest { Name = "json_phan_tach_object", Run = TestJsonPhanTachObject },
        new LexTest { Name = "json_phan_tach_array", Run = TestJsonPhanTachArray },
        new LexTest { Name = "json_gop_dict", Run = TestJsonGopDict },
        new LexTest { Name = "json_round_trip", Run = TestJsonRoundTrip },
        new LexTest { Name = "json_nested", Run = TestJsonNested },
        new LexTest { Name = "json_bool_null", Run = TestJsonBoolNull },
    };

    private static bool KT(bool c, string m) { if (!c) throw new Exception(m); return true; }

    private static string Chay(string src)
    {
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try { new Interpreter().Run(Parser.Parse(src)); }
        finally { Console.SetOut(cu); }
        return sw.ToString().Replace("\r\n", "\n").Trim('\n');
    }

    private static object ChayGiaTri(string src)
    {
        var cu = Console.Out;
        Console.SetOut(new StringWriter());
        try
        {
            var env = new PhamVi();
            Builtins.DangKy(env);
            var interp = new Interpreter();
            interp.ChayVoiGlobal(env, Parser.Parse(src));
            if (env.Lay("kq", out var val)) return val;
            return null;
        }
        finally { Console.SetOut(cu); }
    }

    private static void ChayLoi(string src, string maCu = null)
    {
        try { new Interpreter().Run(Parser.Parse(src)); }
        catch (RuntimeError e)
        {
            if (maCu != null && !e.Message.Contains(maCu))
                throw new Exception("mong loi chua '" + maCu + "', nhan '" + e.Message + "'");
            return;
        }
        throw new Exception("mong RuntimeError nhung khong co");
    }

    private static string FT() => Path.Combine(Path.GetTempPath(), "vl_test_" + Guid.NewGuid().ToString("N") + ".txt");
    private static string DT() => Path.Combine(Path.GetTempPath(), "vl_dir_" + Guid.NewGuid().ToString("N"));
    private static string Q(string s) => "\"" + s.Replace("\\", "/") + "\"";

    private static bool TestThoatDangKy()
    {
        var env = new PhamVi();
        Builtins.DangKy(env);
        KT(env.Lay("thoát", out _), "thoát chua duoc dang ky");
        return true;
    }

    private static bool TestDocFileCoBan()
    {
        string p = FT();
        File.WriteAllText(p, "xin chao", System.Text.Encoding.UTF8);
        try
        {
            string o = Chay("in_ra(đọc_file(" + Q(p) + "))");
            KT(o == "xin chao", "mong 'xin chao', nhan '" + o + "'");
            return true;
        }
        finally { try { File.Delete(p); } catch { } }
    }

    private static bool TestDocFileKhongTonTai()
    {
        ChayLoi("đọc_file(\"nonexistent_file_vl.txt\")", "không tìm thấy tệp");
        return true;
    }

    private static bool TestGhiFileTraNoiDung()
    {
        string p = FT();
        try
        {
            Chay("ghi_file(" + Q(p) + ", \"hello world\")");
            string c = File.ReadAllText(p, System.Text.Encoding.UTF8);
            KT(c == "hello world", "mong 'hello world', nhan '" + c + "'");
            return true;
        }
        finally { try { File.Delete(p); } catch { } }
    }

    private static bool TestGhiFileGhiDe()
    {
        string p = FT();
        try
        {
            File.WriteAllText(p, "old", System.Text.Encoding.UTF8);
            Chay("ghi_file(" + Q(p) + ", \"new\")");
            string c = File.ReadAllText(p, System.Text.Encoding.UTF8);
            KT(c == "new", "mong 'new', nhan '" + c + "'");
            return true;
        }
        finally { try { File.Delete(p); } catch { } }
    }

    private static bool TestTonTaiFile()
    {
        string p = FT();
        File.WriteAllText(p, "x", System.Text.Encoding.UTF8);
        try
        {
            string o = Chay("in_ra(tồn_tại(" + Q(p) + "))");
            KT(o == "đúng", "mong 'đúng', nhan '" + o + "'");
            return true;
        }
        finally { try { File.Delete(p); } catch { } }
    }

    private static bool TestTonTaiThuMuc()
    {
        string d = DT();
        Directory.CreateDirectory(d);
        try
        {
            string o = Chay("in_ra(tồn_tại(" + Q(d) + "))");
            KT(o == "đúng", "mong 'đúng', nhan '" + o + "'");
            return true;
        }
        finally { try { Directory.Delete(d); } catch { } }
    }

    private static bool TestTonTaiKhongTonTai()
    {
        string o = Chay("in_ra(tồn_tại(\"nonexistent_vl_path.txt\"))");
        KT(o == "sai", "mong 'sai', nhan '" + o + "'");
        return true;
    }

    private static bool TestJsonPhanTachObject()
    {
        object r = ChayGiaTri("kq = json_phân_tách(\"{\\\"ten\\\":\\\"An\\\",\\\"tuoi\\\":25}\")");
        KT(r is DictValue, "mong DictValue, nhan " + r?.GetType());
        var dv = (DictValue)r;
        KT(dv.Pairs["ten"] as string == "An", "ten sai");
        KT(dv.Pairs["tuoi"] is double && (double)dv.Pairs["tuoi"] == 25.0, "tuoi sai");
        return true;
    }

    private static bool TestJsonPhanTachArray()
    {
        object r = ChayGiaTri("kq = json_phân_tách(\"[1,2,3]\")");
        KT(r is List<object>, "mong List, nhan " + r?.GetType());
        var l = (List<object>)r;
        KT(l.Count == 3, "mong 3 ptu, nhan " + l.Count);
        KT(l[0] is double && (double)l[0] == 1.0, "ptu 0 sai");
        return true;
    }

    private static bool TestJsonGopDict()
    {
        object r = ChayGiaTri("d = {\"a\": 1, \"b\": 2}\nkq = json_gộp(d)");
        KT(r is string, "mong string, nhan " + r?.GetType());
        string json = (string)r;
        KT(json.Contains("\"a\""), "json khong chua 'a': " + json);
        KT(json.Contains("1"), "json khong chua 1: " + json);
        return true;
    }

    private static bool TestJsonRoundTrip()
    {
        object r = ChayGiaTri("d = {\"x\": 10, \"y\": \"hello\"}\nkq = json_phân_tách(json_gộp(d))");
        KT(r is DictValue, "mong DictValue, nhan " + r?.GetType());
        var d2 = (DictValue)r;
        KT(d2.Pairs["x"] is double && (double)d2.Pairs["x"] == 10.0, "x sai");
        KT(d2.Pairs["y"] as string == "hello", "y sai");
        return true;
    }

    private static bool TestJsonNested()
    {
        object r = ChayGiaTri("kq = json_phân_tách(\"{\\\"a\\\":[1,2],\\\"b\\\":{\\\"c\\\":true}}\")");
        KT(r is DictValue, "mong DictValue, nhan " + r?.GetType());
        var d = (DictValue)r;
        KT(d.Pairs["a"] is List<object>, "a khong phai list");
        KT(d.Pairs["b"] is DictValue, "b khong phai dict");
        var d2 = (DictValue)d.Pairs["b"];
        KT(d2.Pairs["c"] is bool && (bool)d2.Pairs["c"] == true, "c sai");
        return true;
    }

    private static bool TestJsonBoolNull()
    {
        object r = ChayGiaTri("kq = json_phân_tách(\"{\\\"ok\\\":true,\\\"no\\\":false,\\\"empty\\\":null}\")");
        KT(r is DictValue, "mong DictValue, nhan " + r?.GetType());
        var d = (DictValue)r;
        KT(d.Pairs["ok"] is bool && (bool)d.Pairs["ok"] == true, "ok sai");
        KT(d.Pairs["no"] is bool && (bool)d.Pairs["no"] == false, "no sai");
        KT(d.Pairs["empty"] == null, "empty khong null");
        return true;
    }
}
