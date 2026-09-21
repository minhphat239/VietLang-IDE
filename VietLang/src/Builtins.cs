using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace VietLang;

    /// <summary>Đăng ký tất cả builtin vào môi trường toàn cục.</summary>
public static class Builtins
{
    private static readonly HttpClient _http = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

    private static RuntimeError Loi(int dong, string chiTiet)
        => new RuntimeError($"Lỗi thực thi dòng {dong}: {chiTiet}");

    private static void YeucauSoLuongThamSo(string ten, int can, int nhan, int dong)
    {
        if (nhan != can) throw Loi(dong, $"hàm '{ten}' cần {can} tham số, nhận {nhan}");
    }

    /// <summary>Thêm builtin: in_ra, do_dai, chuyen_chuoi, chuyen_so, nhap, them + 6 NLP + 3 regex + 7 REPL (file I/O, JSON, system, ANSI, sep/end).</summary>
    public static void DangKy(PhamVi global)
    {
        global.GanDay("in_ra", new BuiltinValue("in_ra", (i, a, d) =>
        {
            string sep = " ";
            string end = "\n";
            var printArgs = a;
            if (a.Count > 0 && a.Last() is DictValue lastDict)
            {
                if (lastDict.Pairs.ContainsKey("sep"))
                    sep = Interpreter.ChuoiHoa(lastDict.Pairs["sep"]);
                if (lastDict.Pairs.ContainsKey("end"))
                    end = Interpreter.ChuoiHoa(lastDict.Pairs["end"]);
                printArgs = a.Take(a.Count - 1).ToList();
            }
            Console.Write(string.Join(sep, printArgs.Select(Interpreter.ChuoiHoa)) + end);
            return null;
        }));

        global.GanDay("do_dai", new BuiltinValue("do_dai", (i, a, d) =>
        {
            YeucauSoLuongThamSo("do_dai", 1, a.Count, d);
            if (a[0] is List<object> list) return (double)list.Count;
            if (a[0] is string s) return (double)s.Length;
            if (a[0] is DictValue dict) return (double)dict.Pairs.Count;
            throw Loi(d, "do_dai chỉ áp dụng cho mảng, chuỗi hoặc dict");
        }));

        global.GanDay("chuyen_chuoi", new BuiltinValue("chuyen_chuoi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuyen_chuoi", 1, a.Count, d);
            return Interpreter.ChuoiHoa(a[0]);
        }));

        global.GanDay("chuyen_so", new BuiltinValue("chuyen_so", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuyen_so", 1, a.Count, d);
            string chuoiRa = Interpreter.ChuoiHoa(a[0]);
            if (double.TryParse(chuoiRa, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                return v;
            throw Loi(d, $"không chuyển được '{chuoiRa}' thành số");
        }));

        global.GanDay("nhap", new BuiltinValue("nhap", (i, a, d) =>
        {
            if (a.Count > 1) throw Loi(d, $"hàm 'nhap' cần 0 hoặc 1 tham số, nhận {a.Count}");
            if (a.Count == 1)
                Console.Write(Interpreter.ChuoiHoa(a[0]));
            return Console.ReadLine();
        }));

        global.GanDay("them", new BuiltinValue("them", (i, a, d) =>
        {
            YeucauSoLuongThamSo("them", 2, a.Count, d);
            if (!(a[0] is List<object> mang))
                throw Loi(d, "tham số đầu của 'them' phải là mảng");
            mang.Add(a[1]);
            return mang;
        }));

        // ─── 6 builtin tiếng Việt (M3.1) ─────────────────────────────

        global.GanDay("chuan_hoa", new BuiltinValue("chuan_hoa", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuan_hoa", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"chuan_hoa cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Interpreter.BoDauTiengViet(s);
        }));

        global.GanDay("tim_tu", new BuiltinValue("tim_tu", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tim_tu", 2, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"tham số 1 của tim_tu phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string tu)
                throw Loi(d, $"tham số 2 của tim_tu phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
            var result = new List<object>();
            int idx = 0;
            while (true)
            {
                int pos = text.IndexOf(tu, idx);
                if (pos < 0) break;
                result.Add((double)pos);
                idx = pos + 1;
            }
            return result;
        }));

        global.GanDay("tach_tu", new BuiltinValue("tach_tu", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tach_tu", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"tach_tu cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return s.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList<object>();
        }));

        global.GanDay("tach_cau", new BuiltinValue("tach_cau", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tach_cau", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"tach_cau cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            var result = new List<object>();
            int start = 0;
            for (int ci = 0; ci < s.Length; ci++)
            {
                char c = s[ci];
                if (c == '.' || c == '?' || c == '!')
                {
                    result.Add(s.Substring(start, ci - start + 1));
                    start = ci + 1;
                    while (start < s.Length && s[start] == ' ') start++;
                }
            }
            if (start < s.Length)
                result.Add(s.Substring(start));
            return result;
        }));

        global.GanDay("dem_tu", new BuiltinValue("dem_tu", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dem_tu", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"dem_tu cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (string.IsNullOrWhiteSpace(s)) return (double)0;
            return (double)s.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }));

        global.GanDay("chuan_hoa_tim_kiem", new BuiltinValue("chuan_hoa_tim_kiem", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuan_hoa_tim_kiem", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"chuan_hoa_tim_kiem cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Interpreter.BoDauTiengViet(s).ToLowerInvariant().Trim();
        }));

        // ─── 3 builtin regex tiếng Việt (M3.2) ─────────────────────────

        global.GanDay("tim_kiem", new BuiltinValue("tim_kiem", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tim_kiem", 2, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"tham số 1 của tim_kiem phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string pattern)
                throw Loi(d, $"tham số 2 của tim_kiem phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
            var matches = Regex.Matches(text, pattern);
            var result = new List<object>();
            foreach (Match m in matches)
                result.Add(m.Value);
            return result;
        }));

        global.GanDay("khop_pattern", new BuiltinValue("khop_pattern", (i, a, d) =>
        {
            YeucauSoLuongThamSo("khop_pattern", 2, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"tham số 1 của khop_pattern phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string pattern)
                throw Loi(d, $"tham số 2 của khop_pattern phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
            return Regex.IsMatch(text, pattern);
        }));

        global.GanDay("thay_the", new BuiltinValue("thay_the", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thay_the", 3, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"tham số 1 của thay_the phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string pattern)
                throw Loi(d, $"tham số 2 của thay_the phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
            if (a[2] is not string replacement)
                throw Loi(d, $"tham số 3 của thay_the phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[2])}");
            return Regex.Replace(text, pattern, replacement);
        }));

        // ─── REPL cao: thoat + File I/O + JSON ─────────────────────

        global.GanDay("thoat", new BuiltinValue("thoat", (i, a, d) =>
        {
            Environment.Exit(0);
            return null;
        }));

        global.GanDay("doc_file", new BuiltinValue("doc_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("doc_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"doc_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (!File.Exists(path))
                throw Loi(d, $"không tìm thấy tệp '{path}'");
            return File.ReadAllText(path, System.Text.Encoding.UTF8);
        }));

        global.GanDay("ghi_file", new BuiltinValue("ghi_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("ghi_file", 2, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"ghi_file tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            string noiDung = Interpreter.ChuoiHoa(a[1]);
            File.WriteAllText(path, noiDung, System.Text.Encoding.UTF8);
            return noiDung;
        }));

        global.GanDay("ton_tai", new BuiltinValue("ton_tai", (i, a, d) =>
        {
            YeucauSoLuongThamSo("ton_tai", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"ton_tai cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return File.Exists(path) || Directory.Exists(path);
        }));

        // ─── File System builtins ──────────────────────────────────

        global.GanDay("doc_thu_muc", new BuiltinValue("doc_thu_muc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("doc_thu_muc", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"doc_thu_muc cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (!Directory.Exists(path))
                throw Loi(d, $"không tìm thấy thư mục '{path}'");
            var result = new List<object>();
            foreach (var entry in Directory.GetFileSystemEntries(path))
                result.Add(Path.GetFileName(entry));
            return result;
        }));

        global.GanDay("tao_thu_muc", new BuiltinValue("tao_thu_muc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tao_thu_muc", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"tao_thu_muc cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            try
            {
                Directory.CreateDirectory(path);
                return "đã tạo";
            }
            catch (Exception ex)
            {
                throw Loi(d, $"tao_thu_muc thất bại: {ex.Message}");
            }
        }));

        global.GanDay("xoa_file", new BuiltinValue("xoa_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("xoa_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"xoa_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return "đã xóa";
                }
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                    return "đã xóa";
                }
                throw Loi(d, $"không tìm thấy '{path}'");
            }
            catch (RuntimeError) { throw; }
            catch (Exception ex)
            {
                throw Loi(d, $"xoa_file thất bại: {ex.Message}");
            }
        }));

        global.GanDay("sao_chep", new BuiltinValue("sao_chep", (i, a, d) =>
        {
            YeucauSoLuongThamSo("sao_chep", 2, a.Count, d);
            if (a[0] is not string src)
                throw Loi(d, $"sao_chep tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string dst)
                throw Loi(d, $"sao_chep tham số 2 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
            try
            {
                if (File.Exists(src))
                {
                    File.Copy(src, dst, true);
                    return "đã sao chép";
                }
                throw Loi(d, $"không tìm thấy tệp nguồn '{src}'");
            }
            catch (RuntimeError) { throw; }
            catch (Exception ex)
            {
                throw Loi(d, $"sao_chep thất bại: {ex.Message}");
            }
        }));

        global.GanDay("di_tuong", new BuiltinValue("di_tuong", (i, a, d) =>
        {
            YeucauSoLuongThamSo("di_tuong", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"di_tuong cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Path.GetFullPath(path);
        }));

        global.GanDay("kich_thuoc_file", new BuiltinValue("kich_thuoc_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("kich_thuoc_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"kich_thuoc_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (!File.Exists(path))
                throw Loi(d, $"không tìm thấy tệp '{path}'");
            return (double)new FileInfo(path).Length;
        }));

        global.GanDay("la_thu_muc", new BuiltinValue("la_thu_muc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("la_thu_muc", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"la_thu_muc cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Directory.Exists(path);
        }));

        global.GanDay("la_file", new BuiltinValue("la_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("la_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"là_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return File.Exists(path);
        }));

        global.GanDay("json_phan_tach", new BuiltinValue("json_phan_tach", (i, a, d) =>
        {
            YeucauSoLuongThamSo("json_phan_tach", 1, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"json_phan_tach cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            using var doc = JsonDocument.Parse(text);
            return ChuyenTuJson(doc.RootElement);
        }));

        global.GanDay("json_gop", new BuiltinValue("json_gop", (i, a, d) =>
        {
            YeucauSoLuongThamSo("json_gop", 1, a.Count, d);
            object value = a[0];
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            {
                GhiJson(writer, value);
            }
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }));

        // ─── 4 builtin REPL TB+THẤP (Tính năng 4–7) ────────────────

        global.GanDay("chay_lenh", new BuiltinValue("chay_lenh", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chay_lenh", 1, a.Count, d);
            if (a[0] is not string cmd)
                throw Loi(d, $"chay_lenh cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c {cmd}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                if (string.IsNullOrEmpty(stderr))
                    return stdout.TrimEnd('\r', '\n');
                return stderr.TrimEnd('\r', '\n');
            }
            catch (Exception ex)
            {
                throw Loi(d, $"chay_lenh thất bại: {ex.Message}");
            }
        }));

        global.GanDay("lay_tham_so", new BuiltinValue("lay_tham_so", (i, a, d) =>
        {
            var args = Environment.GetCommandLineArgs();
            var result = new List<object>();
            // Skip program name (index 0) and script file path (index 1)
            // User args start from index 2
            for (int idx = 2; idx < args.Length; idx++)
                result.Add(args[idx]);
            return result;
        }));

        global.GanDay("in_mau", new BuiltinValue("in_mau", (i, a, d) =>
        {
            YeucauSoLuongThamSo("in_mau", 2, a.Count, d);
            string text = Interpreter.ChuoiHoa(a[0]);
            string mau = Interpreter.ChuoiHoa(a[1]);
            var mauso = new Dictionary<string, string>
            {
                ["red"] = "31", ["green"] = "32", ["yellow"] = "33",
                ["blue"] = "34", ["magenta"] = "35", ["cyan"] = "36",
                ["white"] = "37", ["reset"] = "0"
            };
            if (!mauso.ContainsKey(mau))
                throw Loi(d, $"in_mau không hỗ trợ màu '{mau}', chọn: red, green, yellow, blue, magenta, cyan, white, reset");
            Console.Write($"\x1b[{mauso[mau]}m{text}\x1b[0m");
            return null;
        }));

        global.GanDay("xoa_man_hinh", new BuiltinValue("xoa_man_hinh", (i, a, d) =>
        {
            Console.Write("\x1b[2J\x1b[H");
            return null;
        }));

        // ─── thuc_thi: eval code VietLang từ chuỗi ──────────────

        global.GanDay("thuc_thi", new BuiltinValue("thuc_thi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thuc_thi", 1, a.Count, d);
            if (a[0] is not string code)
                throw Loi(d, $"thuc_thi cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return i.ThucThiChuoi(code, i.CurrentEnv);
        }));

        // ─── REPL session: lay_tat_ca_bien + gan_bien ──────────────────

        global.GanDay("lay_tat_ca_bien", new BuiltinValue("lay_tat_ca_bien", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lay_tat_ca_bien", 0, a.Count, d);
            var dict = new DictValue();
            var allVars = i.CurrentEnv.LayTatCa();
            foreach (var kv in allVars)
            {
                if (kv.Value is BuiltinValue) continue;
                dict.Pairs[kv.Key] = kv.Value;
            }
            return dict;
        }));

        global.GanDay("gan_bien", new BuiltinValue("gan_bien", (i, a, d) =>
        {
            YeucauSoLuongThamSo("gan_bien", 2, a.Count, d);
            if (a[0] is not string ten)
                throw Loi(d, $"gan_bien tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            i.CurrentEnv.Gan(ten, a[1]);
            return null;
        }));

        // ─── Math library builtins ────────────────────────────────

        global.GanDay("can_bac_hai", new BuiltinValue("can_bac_hai", (i, a, d) =>
        {
            YeucauSoLuongThamSo("can_bac_hai", 1, a.Count, d);
            return Math.Sqrt(Convert.ToDouble(a[0]));
        }));

        global.GanDay("tuyet_doi", new BuiltinValue("tuyet_doi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tuyet_doi", 1, a.Count, d);
            return Math.Abs(Convert.ToDouble(a[0]));
        }));

        global.GanDay("toi_da", new BuiltinValue("toi_da", (i, a, d) =>
        {
            if (a.Count < 1 || a.Count > 2)
                throw Loi(d, $"hàm 'toi_da' cần 1 hoặc 2 tham số, nhận {a.Count}");
            double result = Convert.ToDouble(a[0]);
            for (int idx = 1; idx < a.Count; idx++)
                result = Math.Max(result, Convert.ToDouble(a[idx]));
            return result;
        }));

        global.GanDay("toi_thieu", new BuiltinValue("toi_thieu", (i, a, d) =>
        {
            if (a.Count < 1 || a.Count > 2)
                throw Loi(d, $"hàm 'toi_thieu' cần 1 hoặc 2 tham số, nhận {a.Count}");
            double result = Convert.ToDouble(a[0]);
            for (int idx = 1; idx < a.Count; idx++)
                result = Math.Min(result, Convert.ToDouble(a[idx]));
            return result;
        }));

        global.GanDay("sin", new BuiltinValue("sin", (i, a, d) =>
        {
            YeucauSoLuongThamSo("sin", 1, a.Count, d);
            return Math.Sin(Convert.ToDouble(a[0]));
        }));

        global.GanDay("cos", new BuiltinValue("cos", (i, a, d) =>
        {
            YeucauSoLuongThamSo("cos", 1, a.Count, d);
            return Math.Cos(Convert.ToDouble(a[0]));
        }));

        global.GanDay("tan", new BuiltinValue("tan", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tan", 1, a.Count, d);
            return Math.Tan(Convert.ToDouble(a[0]));
        }));

        global.GanDay("log", new BuiltinValue("log", (i, a, d) =>
        {
            YeucauSoLuongThamSo("log", 1, a.Count, d);
            return Math.Log(Convert.ToDouble(a[0]));
        }));

        global.GanDay("log2", new BuiltinValue("log2", (i, a, d) =>
        {
            YeucauSoLuongThamSo("log2", 1, a.Count, d);
            return Math.Log2(Convert.ToDouble(a[0]));
        }));

        global.GanDay("log10", new BuiltinValue("log10", (i, a, d) =>
        {
            YeucauSoLuongThamSo("log10", 1, a.Count, d);
            return Math.Log10(Convert.ToDouble(a[0]));
        }));

        global.GanDay("lam_tron", new BuiltinValue("lam_tron", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lam_tron", 1, a.Count, d);
            return (double)Math.Round(Convert.ToDouble(a[0]));
        }));

        global.GanDay("lam_nguyen", new BuiltinValue("lam_nguyen", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lam_nguyen", 1, a.Count, d);
            return (double)Math.Floor(Convert.ToDouble(a[0]));
        }));

        global.GanDay("lam_tron_len", new BuiltinValue("lam_tron_len", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lam_tron_len", 1, a.Count, d);
            return (double)Math.Ceiling(Convert.ToDouble(a[0]));
        }));

        global.GanDay("so_nguyen", new BuiltinValue("so_nguyen", (i, a, d) =>
        {
            YeucauSoLuongThamSo("so_nguyen", 1, a.Count, d);
            double val = Convert.ToDouble(a[0]);
            return val == Math.Floor(val);
        }));

        global.GanDay("PI", Math.PI);
        global.GanDay("E", Math.E);

        // ─── DateTime library builtins ──────────────────────────

        global.GanDay("bay_gio", new BuiltinValue("bay_gio", (i, a, d) =>
        {
            YeucauSoLuongThamSo("bay_gio", 0, a.Count, d);
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        }));

        global.GanDay("ngay", new BuiltinValue("ngay", (i, a, d) =>
        {
            YeucauSoLuongThamSo("ngay", 0, a.Count, d);
            return DateTime.Now.ToString("yyyy-MM-dd");
        }));

        global.GanDay("gio", new BuiltinValue("gio", (i, a, d) =>
        {
            YeucauSoLuongThamSo("gio", 0, a.Count, d);
            return DateTime.Now.ToString("HH:mm:ss");
        }));

        global.GanDay("thoi_gian", new BuiltinValue("thoi_gian", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thoi_gian", 0, a.Count, d);
            return (double)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }));

        global.GanDay("cho", new BuiltinValue("cho", (i, a, d) =>
        {
            YeucauSoLuongThamSo("cho", 1, a.Count, d);
            int ms = Convert.ToInt32(a[0]);
            Thread.Sleep(ms);
            return "đã cho xong";
        }));

        global.GanDay("dem_nguoc", new BuiltinValue("dem_nguoc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dem_nguoc", 1, a.Count, d);
            int ms = Convert.ToInt32(a[0]);
            Thread.Sleep(ms);
            return "hết gio";
        }));

        // ─── HTTP builtins ─────────────────────────────────────────

        global.GanDay("lay", new BuiltinValue("lay", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lay", 1, a.Count, d);
            if (a[0] is not string url)
                throw Loi(d, $"lay cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            try
            {
                var response = _http.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw Loi(d, $"lay thất bại: {ex.Message}");
            }
        }));

        global.GanDay("gui", new BuiltinValue("gui", (i, a, d) =>
        {
            YeucauSoLuongThamSo("gui", 2, a.Count, d);
            if (a[0] is not string url)
                throw Loi(d, $"gui tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            try
            {
                var content = new StringContent(
                    Interpreter.ChuoiHoa(a[1]),
                    System.Text.Encoding.UTF8,
                    "application/json");
                var response = _http.PostAsync(url, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw Loi(d, $"gui thất bại: {ex.Message}");
            }
        }));

        global.GanDay("gui_chuoi", new BuiltinValue("gui_chuoi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("gui_chuoi", 3, a.Count, d);
            if (a[0] is not string url)
                throw Loi(d, $"gui_chuoi tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[2] is not string contentType)
                throw Loi(d, $"gui_chuoi tham số 3 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[2])}");
            try
            {
                var content = new StringContent(
                    Interpreter.ChuoiHoa(a[1]),
                    System.Text.Encoding.UTF8,
                    contentType);
                var response = _http.PostAsync(url, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw Loi(d, $"gui_chuoi thất bại: {ex.Message}");
            }
        }));

        // ─── GUI: tao_cua_so ─────────────────────────────────────
        global.GanDay("tao_cua_so", new BuiltinValue("tao_cua_so", (i, a, d) =>
        {
            _guiInterpreter = i;
            EnsureForm();
            if (a.Count >= 1) _form.Text = Interpreter.ChuoiHoa(a[0]);
            if (a.Count >= 2 && a[0] is double w && a[1] is double h)
                _form.Size = new System.Drawing.Size((int)w, (int)h);
            if (a.Count >= 3 && a[0] is string title && a[1] is double ww && a[2] is double hh)
            {
                _form.Text = title;
                _form.Size = new System.Drawing.Size((int)ww, (int)hh);
            }
            return null;
        }));

        // ─── GUI: tao_nut ─────────────────────────────────────────
        global.GanDay("tao_nut", new BuiltinValue("tao_nut", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tao_nut", 1, a.Count, d);
            EnsureForm();
            var btn = new System.Windows.Forms.Button();
            btn.Text = Interpreter.ChuoiHoa(a[0]);
            btn.AutoSize = true;
            btn.Margin = new System.Windows.Forms.Padding(5);
            // Store reference for callback
            var interp = _guiInterpreter;
            btn.Click += (s, e) =>
            {
                if (interp != null)
                {
                    try { interp.ThucThiChuoi("nhấn_nút()", interp.CurrentEnv); }
                    catch { /* ignore callback errors */ }
                }
            };
            _panel.Controls.Add(btn);
            return new WidgetValue(btn, "nút");
        }));

        // ─── GUI: tao_o_van_ban ───────────────────────────────────
        global.GanDay("tao_o_van_ban", new BuiltinValue("tao_o_van_ban", (i, a, d) =>
        {
            EnsureForm();
            var txt = new System.Windows.Forms.TextBox();
            txt.Width = 200;
            txt.Margin = new System.Windows.Forms.Padding(5);
            if (a.Count >= 1) txt.Text = Interpreter.ChuoiHoa(a[0]);
            _panel.Controls.Add(txt);
            return new WidgetValue(txt, "ô_văn_bản");
        }));

        // ─── GUI: tao_nhan ────────────────────────────────────────
        global.GanDay("tao_nhan", new BuiltinValue("tao_nhan", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tao_nhan", 1, a.Count, d);
            EnsureForm();
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = Interpreter.ChuoiHoa(a[0]);
            lbl.AutoSize = true;
            lbl.Margin = new System.Windows.Forms.Padding(5);
            _panel.Controls.Add(lbl);
            return new WidgetValue(lbl, "nhãn");
        }));

        // ─── GUI: tao_dong_chu ────────────────────────────────────
        global.GanDay("tao_dong_chu", new BuiltinValue("tao_dong_chu", (i, a, d) =>
        {
            EnsureForm();
            var rtb = new System.Windows.Forms.RichTextBox();
            rtb.Width = 300;
            rtb.Height = 100;
            rtb.Margin = new System.Windows.Forms.Padding(5);
            if (a.Count >= 1) rtb.Text = Interpreter.ChuoiHoa(a[0]);
            _panel.Controls.Add(rtb);
            return new WidgetValue(rtb, "dòng_chữ");
        }));

        // ─── GUI: them ────────────────────────────────────────────
        global.GanDay("them_widget", new BuiltinValue("them_widget", (i, a, d) =>
        {
            YeucauSoLuongThamSo("them_widget", 1, a.Count, d);
            EnsureForm();
            var ctrl = GetWidget(a[0]);
            _panel.Controls.Add(ctrl);
            return null;
        }));

        // ─── GUI: chạy ────────────────────────────────────────────
        global.GanDay("chay", new BuiltinValue("chay", (i, a, d) =>
        {
            EnsureForm();
            _guiInterpreter = i;
            _form.FormClosing += (s, e) => { _form = null; };
            System.Windows.Forms.Application.Run(_form);
            _form = null;
            return null;
        }));

        // ─── GUI: dat_title ───────────────────────────────────────
        global.GanDay("dat_title", new BuiltinValue("dat_title", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_title", 1, a.Count, d);
            EnsureForm();
            _form.Text = Interpreter.ChuoiHoa(a[0]);
            return null;
        }));

        // ─── GUI: dat_kich_thuoc ──────────────────────────────────
        global.GanDay("dat_kich_thuoc", new BuiltinValue("dat_kich_thuoc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_kich_thuoc", 2, a.Count, d);
            EnsureForm();
            if (a[0] is double w && a[1] is double h)
                _form.Size = new System.Drawing.Size((int)w, (int)h);
            return null;
        }));

        // ─── GUI: lay_van_ban ─────────────────────────────────────
        global.GanDay("lay_van_ban", new BuiltinValue("lay_van_ban", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lay_van_ban", 1, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (ctrl is System.Windows.Forms.TextBox tb) return tb.Text;
            if (ctrl is System.Windows.Forms.RichTextBox rtb) return rtb.Text;
            throw Loi(d, "lay_van_ban chỉ áp dụng cho ô_văn_bản hoặc dòng_chữ");
        }));

        // ─── GUI: dat_van_ban ─────────────────────────────────────
        global.GanDay("dat_van_ban", new BuiltinValue("dat_van_ban", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_van_ban", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var text = Interpreter.ChuoiHoa(a[1]);
            if (ctrl is System.Windows.Forms.TextBox tb) { tb.Text = text; return null; }
            if (ctrl is System.Windows.Forms.RichTextBox rtb) { rtb.Text = text; return null; }
            if (ctrl is System.Windows.Forms.Label lbl) { lbl.Text = text; return null; }
            if (ctrl is System.Windows.Forms.Button btn) { btn.Text = text; return null; }
            throw Loi(d, "dat_van_ban không hỗ trợ kiểu widget này");
        }));

        // ─── GUI: them_dong ────────────────────────────────────────
        global.GanDay("them_dong", new BuiltinValue("them_dong", (i, a, d) =>
        {
            YeucauSoLuongThamSo("them_dong", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var text = Interpreter.ChuoiHoa(a[1]);
            if (ctrl is System.Windows.Forms.RichTextBox rtb) { rtb.AppendText(text + "\n"); return null; }
            throw Loi(d, "them_dong chỉ áp dụng cho dòng_chữ");
        }));

        // ─── GUI: xoa_trong ───────────────────────────────────────
        global.GanDay("xoa_trong", new BuiltinValue("xoa_trong", (i, a, d) =>
        {
            YeucauSoLuongThamSo("xoa_trong", 1, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (ctrl is System.Windows.Forms.TextBox tb) { tb.Text = ""; return null; }
            if (ctrl is System.Windows.Forms.RichTextBox rtb) { rtb.Text = ""; return null; }
            throw Loi(d, "xoa_trong chỉ áp dụng cho ô_văn_bản hoặc dòng_chữ");
        }));

        // ─── GUI: dong ─────────────────────────────────────────────
        global.GanDay("dong", new BuiltinValue("dong", (i, a, d) =>
        {
            if (_form != null)
            {
                _form.Invoke(new Action(() => _form.Close()));
            }
            return null;
        }));

        // ─── GUI: dat_kich_thuoc_widget ───────────────────────────
        global.GanDay("dat_kich_thuoc_widget", new BuiltinValue("dat_kich_thuoc_widget", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_kich_thuoc_widget", 3, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double w && a[2] is double h)
                ctrl.Size = new System.Drawing.Size((int)w, (int)h);
            return null;
        }));

        // ─── GUI: dat_vi_tri ─────────────────────────────────────
        global.GanDay("dat_vi_tri", new BuiltinValue("dat_vi_tri", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_vi_tri", 3, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double x && a[2] is double y)
                ctrl.Location = new System.Drawing.Point((int)x, (int)y);
            return null;
        }));

        // ─── GUI: dat_font ────────────────────────────────────────
        global.GanDay("dat_font", new BuiltinValue("dat_font", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_font", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double size)
                ctrl.Font = new System.Drawing.Font("Consolas", (float)size);
            return null;
        }));

        // ─── GUI: dat_mau_nen ─────────────────────────────────────
        global.GanDay("dat_mau_nen", new BuiltinValue("dat_mau_nen", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_mau_nen", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var colorName = Interpreter.ChuoiHoa(a[1]);
            ctrl.BackColor = System.Drawing.Color.FromName(colorName);
            return null;
        }));

        // ─── GUI: dat_mau_chu ─────────────────────────────────────
        global.GanDay("dat_mau_chu", new BuiltinValue("dat_mau_chu", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_mau_chu", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var colorName = Interpreter.ChuoiHoa(a[1]);
            ctrl.ForeColor = System.Drawing.Color.FromName(colorName);
            return null;
        }));

        // ─── GUI: tạo_text_editor ──────────────────────────────────
        global.GanDay("tao_text_editor", new BuiltinValue("tao_text_editor", (i, a, d) =>
        {
            EnsureForm();
            var rtb = new System.Windows.Forms.RichTextBox();
            rtb.Width = 600;
            rtb.Height = 400;
            rtb.Font = new System.Drawing.Font("Consolas", 11);
            rtb.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            rtb.ForeColor = System.Drawing.Color.White;
            rtb.WordWrap = false;
            rtb.AcceptsTab = true;
            rtb.DetectUrls = false;
            rtb.Margin = new System.Windows.Forms.Padding(2);
            if (a.Count >= 1) rtb.Text = Interpreter.ChuoiHoa(a[0]);
            _panel.Controls.Add(rtb);
            return new WidgetValue(rtb, "text_editor");
        }));

        // ─── GUI: tạo_output ───────────────────────────────────────
        global.GanDay("tao_output", new BuiltinValue("tao_output", (i, a, d) =>
        {
            EnsureForm();
            var rtb = new System.Windows.Forms.RichTextBox();
            rtb.Width = 600;
            rtb.Height = 200;
            rtb.Font = new System.Drawing.Font("Consolas", 10);
            rtb.BackColor = System.Drawing.Color.FromArgb(15, 15, 15);
            rtb.ForeColor = System.Drawing.Color.LightGreen;
            rtb.ReadOnly = true;
            rtb.WordWrap = true;
            rtb.Margin = new System.Windows.Forms.Padding(2);
            _panel.Controls.Add(rtb);
            return new WidgetValue(rtb, "output");
        }));

        // ─── GUI: tạo_panel ────────────────────────────────────────
        global.GanDay("tao_panel", new BuiltinValue("tao_panel", (i, a, d) =>
        {
            EnsureForm();
            var panel = new System.Windows.Forms.Panel();
            panel.Width = 600;
            panel.Height = 50;
            panel.Margin = new System.Windows.Forms.Padding(2);
            _panel.Controls.Add(panel);
            return new WidgetValue(panel, "panel");
        }));

        // ─── GUI: them_vao_panel ──────────────────────────────────
        global.GanDay("them_vao_panel", new BuiltinValue("them_vao_panel", (i, a, d) =>
        {
            YeucauSoLuongThamSo("them_vao_panel", 2, a.Count, d);
            var panelCtrl = GetWidget(a[0]);
            var childCtrl = GetWidget(a[1]);
            if (panelCtrl is System.Windows.Forms.Panel panel)
                panel.Controls.Add(childCtrl);
            return null;
        }));

        // ─── GUI: hop_thoai_mo_file ────────────────────────────────
        global.GanDay("hop_thoai_mo_file", new BuiltinValue("hop_thoai_mo_file", (i, a, d) =>
        {
            var filter = a.Count >= 1 ? Interpreter.ChuoiHoa(a[0]) : "VietLang (*.vl)|*.vl|All (*.*)|*.*";
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = filter;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return ofd.FileName;
            return "";
        }));

        // ─── GUI: hop_thoai_luu_file ───────────────────────────────
        global.GanDay("hop_thoai_luu_file", new BuiltinValue("hop_thoai_luu_file", (i, a, d) =>
        {
            var filter = a.Count >= 1 ? Interpreter.ChuoiHoa(a[0]) : "VietLang (*.vl)|*.vl|All (*.*)|*.*";
            var sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = filter;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return sfd.FileName;
            return "";
        }));

        // ─── GUI: lay_dòng ─────────────────────────────────────────
        global.GanDay("lay_dong", new BuiltinValue("lay_dong", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lay_dong", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double lineNo && ctrl is System.Windows.Forms.RichTextBox rtb)
            {
                int ln = (int)lineNo;
                if (ln < 0 || ln >= rtb.Lines.Length) return "";
                return rtb.Lines[ln];
            }
            return "";
        }));

        // ─── GUI: dat_dong ─────────────────────────────────────────
        global.GanDay("dat_dong", new BuiltinValue("dat_dong", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dat_dong", 3, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double lineNo && ctrl is System.Windows.Forms.RichTextBox rtb)
            {
                int ln = (int)lineNo;
                var text = Interpreter.ChuoiHoa(a[2]);
                if (ln >= 0 && ln < rtb.Lines.Length)
                    rtb.Lines[ln] = text;
            }
            return null;
        }));

        // ─── GUI: dem_dong ─────────────────────────────────────────
        global.GanDay("dem_dong", new BuiltinValue("dem_dong", (i, a, d) =>
        {
            YeucauSoLuongThamSo("dem_dong", 1, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (ctrl is System.Windows.Forms.RichTextBox rtb)
                return (double)rtb.Lines.Length;
            return 0.0;
        }));
    }

    // ─── GUI builtins (WinForms) ──────────────────────────────────────

    private static System.Windows.Forms.Form _form;
    private static System.Windows.Forms.FlowLayoutPanel _panel;
    private static Interpreter _guiInterpreter;

    private static System.Windows.Forms.Control GetWidget(object obj)
    {
        if (obj is WidgetValue wv && wv.Control is System.Windows.Forms.Control ctrl)
            return ctrl;
        throw new RuntimeError($"Widget không hợp lệ: {Interpreter.ChuoiHoa(obj)}");
    }

    private static void EnsureForm()
    {
        if (_form == null)
        {
            _form = new System.Windows.Forms.Form();
            _panel = new System.Windows.Forms.FlowLayoutPanel();
            _panel.Dock = System.Windows.Forms.DockStyle.Fill;
            _panel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            _panel.AutoScroll = true;
            _form.Controls.Add(_panel);
        }
    }

    // ─── JSON helpers ───────────────────────────────────────────────

    private static object ChuyenTuJson(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new DictValue();
                foreach (var prop in el.EnumerateObject())
                    dict.Pairs[prop.Name] = ChuyenTuJson(prop.Value);
                return dict;
            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in el.EnumerateArray())
                    list.Add(ChuyenTuJson(item));
                return list;
            case JsonValueKind.String:
                return el.GetString();
            case JsonValueKind.Number:
                return el.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null;
            default:
                return null;
        }
    }

    private static void GhiJson(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case List<object> list:
                writer.WriteStartArray();
                foreach (var item in list)
                    GhiJson(writer, item);
                writer.WriteEndArray();
                break;
            case DictValue dict:
                writer.WriteStartObject();
                foreach (var kv in dict.Pairs)
                {
                    writer.WritePropertyName(kv.Key);
                    GhiJson(writer, kv.Value);
                }
                writer.WriteEndObject();
                break;
            default:
                writer.WriteStringValue(Interpreter.ChuoiHoa(value));
                break;
        }
    }
}