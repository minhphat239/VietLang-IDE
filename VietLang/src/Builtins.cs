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

    /// <summary>Thêm builtin: in_ra, độ_dài, chuyển_chuỗi, chuyển_số, nhập, thêm + 6 NLP + 3 regex + 7 REPL (file I/O, JSON, system, ANSI, sep/end).</summary>
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

        global.GanDay("độ_dài", new BuiltinValue("độ_dài", (i, a, d) =>
        {
            YeucauSoLuongThamSo("độ_dài", 1, a.Count, d);
            if (a[0] is List<object> list) return (double)list.Count;
            if (a[0] is string s) return (double)s.Length;
            if (a[0] is DictValue dict) return (double)dict.Pairs.Count;
            throw Loi(d, "độ_dài chỉ áp dụng cho mảng, chuỗi hoặc dict");
        }));

        global.GanDay("chuyển_chuỗi", new BuiltinValue("chuyển_chuỗi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuyển_chuỗi", 1, a.Count, d);
            return Interpreter.ChuoiHoa(a[0]);
        }));

        global.GanDay("chuyển_số", new BuiltinValue("chuyển_số", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuyển_số", 1, a.Count, d);
            string chuoiRa = Interpreter.ChuoiHoa(a[0]);
            if (double.TryParse(chuoiRa, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                return v;
            throw Loi(d, $"không chuyển được '{chuoiRa}' thành số");
        }));

        global.GanDay("nhập", new BuiltinValue("nhập", (i, a, d) =>
        {
            if (a.Count > 1) throw Loi(d, $"hàm 'nhập' cần 0 hoặc 1 tham số, nhận {a.Count}");
            if (a.Count == 1)
                Console.Write(Interpreter.ChuoiHoa(a[0]));
            return Console.ReadLine();
        }));

        global.GanDay("thêm", new BuiltinValue("thêm", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thêm", 2, a.Count, d);
            if (!(a[0] is List<object> mang))
                throw Loi(d, "tham số đầu của 'thêm' phải là mảng");
            mang.Add(a[1]);
            return mang;
        }));

        // ─── 6 builtin tiếng Việt (M3.1) ─────────────────────────────

        global.GanDay("chuẩn_hóa", new BuiltinValue("chuẩn_hóa", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuẩn_hóa", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"chuẩn_hóa cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Interpreter.BoDauTiengViet(s);
        }));

        global.GanDay("tìm_từ", new BuiltinValue("tìm_từ", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tìm_từ", 2, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"tham số 1 của tìm_từ phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string tu)
                throw Loi(d, $"tham số 2 của tìm_từ phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
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

        global.GanDay("tách_từ", new BuiltinValue("tách_từ", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tách_từ", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"tách_từ cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return s.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList<object>();
        }));

        global.GanDay("tách_câu", new BuiltinValue("tách_câu", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tách_câu", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"tách_câu cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
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

        global.GanDay("đếm_từ", new BuiltinValue("đếm_từ", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đếm_từ", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"đếm_từ cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (string.IsNullOrWhiteSpace(s)) return (double)0;
            return (double)s.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }));

        global.GanDay("chuẩn_hóa_tìm_kiếm", new BuiltinValue("chuẩn_hóa_tìm_kiếm", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chuẩn_hóa_tìm_kiếm", 1, a.Count, d);
            if (a[0] is not string s)
                throw Loi(d, $"chuẩn_hóa_tìm_kiếm cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Interpreter.BoDauTiengViet(s).ToLowerInvariant().Trim();
        }));

        // ─── 3 builtin regex tiếng Việt (M3.2) ─────────────────────────

        global.GanDay("tìm_kiếm", new BuiltinValue("tìm_kiếm", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tìm_kiếm", 2, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"tham số 1 của tìm_kiếm phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string pattern)
                throw Loi(d, $"tham số 2 của tìm_kiếm phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
            var matches = Regex.Matches(text, pattern);
            var result = new List<object>();
            foreach (Match m in matches)
                result.Add(m.Value);
            return result;
        }));

        global.GanDay("khớp_pattern", new BuiltinValue("khớp_pattern", (i, a, d) =>
        {
            YeucauSoLuongThamSo("khớp_pattern", 2, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"tham số 1 của khớp_pattern phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string pattern)
                throw Loi(d, $"tham số 2 của khớp_pattern phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
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

        // ─── REPL cao: thoát + File I/O + JSON ─────────────────────

        global.GanDay("thoát", new BuiltinValue("thoát", (i, a, d) =>
        {
            Environment.Exit(0);
            return null;
        }));

        global.GanDay("đọc_file", new BuiltinValue("đọc_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đọc_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"đọc_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
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

        global.GanDay("tồn_tại", new BuiltinValue("tồn_tại", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tồn_tại", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"tồn_tại cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return File.Exists(path) || Directory.Exists(path);
        }));

        // ─── File System builtins ──────────────────────────────────

        global.GanDay("đọc_thu_muc", new BuiltinValue("đọc_thu_muc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đọc_thu_muc", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"đọc_thu_muc cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (!Directory.Exists(path))
                throw Loi(d, $"không tìm thấy thư mục '{path}'");
            var result = new List<object>();
            foreach (var entry in Directory.GetFileSystemEntries(path))
                result.Add(Path.GetFileName(entry));
            return result;
        }));

        global.GanDay("tạo_thu_muc", new BuiltinValue("tạo_thu_muc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tạo_thu_muc", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"tạo_thu_muc cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            try
            {
                Directory.CreateDirectory(path);
                return "đã tạo";
            }
            catch (Exception ex)
            {
                throw Loi(d, $"tạo_thu_muc thất bại: {ex.Message}");
            }
        }));

        global.GanDay("xóa_file", new BuiltinValue("xóa_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("xóa_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"xóa_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
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
                throw Loi(d, $"xóa_file thất bại: {ex.Message}");
            }
        }));

        global.GanDay("sao_copy", new BuiltinValue("sao_copy", (i, a, d) =>
        {
            YeucauSoLuongThamSo("sao_copy", 2, a.Count, d);
            if (a[0] is not string src)
                throw Loi(d, $"sao_copy tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[1] is not string dst)
                throw Loi(d, $"sao_copy tham số 2 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[1])}");
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
                throw Loi(d, $"sao_copy thất bại: {ex.Message}");
            }
        }));

        global.GanDay("đi_tường", new BuiltinValue("đi_tường", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đi_tường", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"đi_tường cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Path.GetFullPath(path);
        }));

        global.GanDay("kích_thước_file", new BuiltinValue("kích_thước_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("kích_thước_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"kích_thước_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (!File.Exists(path))
                throw Loi(d, $"không tìm thấy tệp '{path}'");
            return (double)new FileInfo(path).Length;
        }));

        global.GanDay("là_thu_muc", new BuiltinValue("là_thu_muc", (i, a, d) =>
        {
            YeucauSoLuongThamSo("là_thu_muc", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"là_thu_muc cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return Directory.Exists(path);
        }));

        global.GanDay("là_file", new BuiltinValue("là_file", (i, a, d) =>
        {
            YeucauSoLuongThamSo("là_file", 1, a.Count, d);
            if (a[0] is not string path)
                throw Loi(d, $"là_file cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return File.Exists(path);
        }));

        global.GanDay("json_phân_tách", new BuiltinValue("json_phân_tách", (i, a, d) =>
        {
            YeucauSoLuongThamSo("json_phân_tách", 1, a.Count, d);
            if (a[0] is not string text)
                throw Loi(d, $"json_phân_tách cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            using var doc = JsonDocument.Parse(text);
            return ChuyenTuJson(doc.RootElement);
        }));

        global.GanDay("json_gộp", new BuiltinValue("json_gộp", (i, a, d) =>
        {
            YeucauSoLuongThamSo("json_gộp", 1, a.Count, d);
            object value = a[0];
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            {
                GhiJson(writer, value);
            }
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }));

        // ─── 4 builtin REPL TB+THẤP (Tính năng 4–7) ────────────────

        global.GanDay("chạy_lệnh", new BuiltinValue("chạy_lệnh", (i, a, d) =>
        {
            YeucauSoLuongThamSo("chạy_lệnh", 1, a.Count, d);
            if (a[0] is not string cmd)
                throw Loi(d, $"chạy_lệnh cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
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
                throw Loi(d, $"chạy_lệnh thất bại: {ex.Message}");
            }
        }));

        global.GanDay("lấy_tham_số", new BuiltinValue("lấy_tham_số", (i, a, d) =>
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

        global.GanDay("xóa_màn_hình", new BuiltinValue("xóa_màn_hình", (i, a, d) =>
        {
            Console.Write("\x1b[2J\x1b[H");
            return null;
        }));

        // ─── thực_thi: eval code VietLang từ chuỗi ──────────────

        global.GanDay("thực_thi", new BuiltinValue("thực_thi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thực_thi", 1, a.Count, d);
            if (a[0] is not string code)
                throw Loi(d, $"thực_thi cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            return i.ThucThiChuoi(code, i.CurrentEnv);
        }));

        // ─── REPL session: lấy_tất_cả_biến + gán_biến ──────────────────

        global.GanDay("lấy_tất_cả_biến", new BuiltinValue("lấy_tất_cả_biến", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lấy_tất_cả_biến", 0, a.Count, d);
            var dict = new DictValue();
            var allVars = i.CurrentEnv.LayTatCa();
            foreach (var kv in allVars)
            {
                if (kv.Value is BuiltinValue) continue;
                dict.Pairs[kv.Key] = kv.Value;
            }
            return dict;
        }));

        global.GanDay("gán_biến", new BuiltinValue("gán_biến", (i, a, d) =>
        {
            YeucauSoLuongThamSo("gán_biến", 2, a.Count, d);
            if (a[0] is not string ten)
                throw Loi(d, $"gán_biến tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            i.CurrentEnv.Gan(ten, a[1]);
            return null;
        }));

        // ─── Math library builtins ────────────────────────────────

        global.GanDay("căn_bac_hai", new BuiltinValue("căn_bac_hai", (i, a, d) =>
        {
            YeucauSoLuongThamSo("căn_bac_hai", 1, a.Count, d);
            return Math.Sqrt(Convert.ToDouble(a[0]));
        }));

        global.GanDay("tuyệt_đối", new BuiltinValue("tuyệt_đối", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tuyệt_đối", 1, a.Count, d);
            return Math.Abs(Convert.ToDouble(a[0]));
        }));

        global.GanDay("tối_đa", new BuiltinValue("tối_đa", (i, a, d) =>
        {
            if (a.Count < 1 || a.Count > 2)
                throw Loi(d, $"hàm 'tối_đa' cần 1 hoặc 2 tham số, nhận {a.Count}");
            double result = Convert.ToDouble(a[0]);
            for (int idx = 1; idx < a.Count; idx++)
                result = Math.Max(result, Convert.ToDouble(a[idx]));
            return result;
        }));

        global.GanDay("tối_thiểu", new BuiltinValue("tối_thiểu", (i, a, d) =>
        {
            if (a.Count < 1 || a.Count > 2)
                throw Loi(d, $"hàm 'tối_thiểu' cần 1 hoặc 2 tham số, nhận {a.Count}");
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

        global.GanDay("làm_tròn", new BuiltinValue("làm_tròn", (i, a, d) =>
        {
            YeucauSoLuongThamSo("làm_tròn", 1, a.Count, d);
            return (double)Math.Round(Convert.ToDouble(a[0]));
        }));

        global.GanDay("làm_nguyên", new BuiltinValue("làm_nguyên", (i, a, d) =>
        {
            YeucauSoLuongThamSo("làm_nguyên", 1, a.Count, d);
            return (double)Math.Floor(Convert.ToDouble(a[0]));
        }));

        global.GanDay("làm_tròn_lên", new BuiltinValue("làm_tròn_lên", (i, a, d) =>
        {
            YeucauSoLuongThamSo("làm_tròn_lên", 1, a.Count, d);
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
            return "hết giờ";
        }));

        // ─── HTTP builtins ─────────────────────────────────────────

        global.GanDay("lấy", new BuiltinValue("lấy", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lấy", 1, a.Count, d);
            if (a[0] is not string url)
                throw Loi(d, $"lấy cần chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            try
            {
                var response = _http.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw Loi(d, $"lấy thất bại: {ex.Message}");
            }
        }));

        global.GanDay("gửi", new BuiltinValue("gửi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("gửi", 2, a.Count, d);
            if (a[0] is not string url)
                throw Loi(d, $"gửi tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
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
                throw Loi(d, $"gửi thất bại: {ex.Message}");
            }
        }));

        global.GanDay("gửi_chuỗi", new BuiltinValue("gửi_chuỗi", (i, a, d) =>
        {
            YeucauSoLuongThamSo("gửi_chuỗi", 3, a.Count, d);
            if (a[0] is not string url)
                throw Loi(d, $"gửi_chuỗi tham số 1 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[0])}");
            if (a[2] is not string contentType)
                throw Loi(d, $"gửi_chuỗi tham số 3 phải là chuỗi, nhận {Interpreter.ChuoiHoa(a[2])}");
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
                throw Loi(d, $"gửi_chuỗi thất bại: {ex.Message}");
            }
        }));

        // ─── GUI: tạo_cửa_sổ ─────────────────────────────────────
        global.GanDay("tạo_cửa_sổ", new BuiltinValue("tạo_cửa_sổ", (i, a, d) =>
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

        // ─── GUI: tạo_nút ─────────────────────────────────────────
        global.GanDay("tạo_nút", new BuiltinValue("tạo_nút", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tạo_nút", 1, a.Count, d);
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

        // ─── GUI: tạo_ô_văn_bản ───────────────────────────────────
        global.GanDay("tạo_ô_văn_bản", new BuiltinValue("tạo_ô_văn_bản", (i, a, d) =>
        {
            EnsureForm();
            var txt = new System.Windows.Forms.TextBox();
            txt.Width = 200;
            txt.Margin = new System.Windows.Forms.Padding(5);
            if (a.Count >= 1) txt.Text = Interpreter.ChuoiHoa(a[0]);
            _panel.Controls.Add(txt);
            return new WidgetValue(txt, "ô_văn_bản");
        }));

        // ─── GUI: tạo_nhãn ────────────────────────────────────────
        global.GanDay("tạo_nhãn", new BuiltinValue("tạo_nhãn", (i, a, d) =>
        {
            YeucauSoLuongThamSo("tạo_nhãn", 1, a.Count, d);
            EnsureForm();
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = Interpreter.ChuoiHoa(a[0]);
            lbl.AutoSize = true;
            lbl.Margin = new System.Windows.Forms.Padding(5);
            _panel.Controls.Add(lbl);
            return new WidgetValue(lbl, "nhãn");
        }));

        // ─── GUI: tạo_dòng_chữ ────────────────────────────────────
        global.GanDay("tạo_dòng_chữ", new BuiltinValue("tạo_dòng_chữ", (i, a, d) =>
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

        // ─── GUI: thêm ────────────────────────────────────────────
        global.GanDay("thêm_widget", new BuiltinValue("thêm_widget", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thêm_widget", 1, a.Count, d);
            EnsureForm();
            var ctrl = GetWidget(a[0]);
            _panel.Controls.Add(ctrl);
            return null;
        }));

        // ─── GUI: chạy ────────────────────────────────────────────
        global.GanDay("chạy", new BuiltinValue("chạy", (i, a, d) =>
        {
            EnsureForm();
            _guiInterpreter = i;
            _form.FormClosing += (s, e) => { _form = null; };
            System.Windows.Forms.Application.Run(_form);
            _form = null;
            return null;
        }));

        // ─── GUI: đặt_title ───────────────────────────────────────
        global.GanDay("đặt_title", new BuiltinValue("đặt_title", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_title", 1, a.Count, d);
            EnsureForm();
            _form.Text = Interpreter.ChuoiHoa(a[0]);
            return null;
        }));

        // ─── GUI: đặt_kích_thước ──────────────────────────────────
        global.GanDay("đặt_kích_thước", new BuiltinValue("đặt_kích_thước", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_kích_thước", 2, a.Count, d);
            EnsureForm();
            if (a[0] is double w && a[1] is double h)
                _form.Size = new System.Drawing.Size((int)w, (int)h);
            return null;
        }));

        // ─── GUI: lấy_văn_bản ─────────────────────────────────────
        global.GanDay("lấy_văn_bản", new BuiltinValue("lấy_văn_bản", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lấy_văn_bản", 1, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (ctrl is System.Windows.Forms.TextBox tb) return tb.Text;
            if (ctrl is System.Windows.Forms.RichTextBox rtb) return rtb.Text;
            throw Loi(d, "lấy_văn_bản chỉ áp dụng cho ô_văn_bản hoặc dòng_chữ");
        }));

        // ─── GUI: đặt_văn_bản ─────────────────────────────────────
        global.GanDay("đặt_văn_bản", new BuiltinValue("đặt_văn_bản", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_văn_bản", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var text = Interpreter.ChuoiHoa(a[1]);
            if (ctrl is System.Windows.Forms.TextBox tb) { tb.Text = text; return null; }
            if (ctrl is System.Windows.Forms.RichTextBox rtb) { rtb.Text = text; return null; }
            if (ctrl is System.Windows.Forms.Label lbl) { lbl.Text = text; return null; }
            if (ctrl is System.Windows.Forms.Button btn) { btn.Text = text; return null; }
            throw Loi(d, "đặt_văn_bản không hỗ trợ kiểu widget này");
        }));

        // ─── GUI: thêm_dòng ────────────────────────────────────────
        global.GanDay("thêm_dòng", new BuiltinValue("thêm_dòng", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thêm_dòng", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var text = Interpreter.ChuoiHoa(a[1]);
            if (ctrl is System.Windows.Forms.RichTextBox rtb) { rtb.AppendText(text + "\n"); return null; }
            throw Loi(d, "thêm_dòng chỉ áp dụng cho dòng_chữ");
        }));

        // ─── GUI: xóa_trống ───────────────────────────────────────
        global.GanDay("xóa_trống", new BuiltinValue("xóa_trống", (i, a, d) =>
        {
            YeucauSoLuongThamSo("xóa_trống", 1, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (ctrl is System.Windows.Forms.TextBox tb) { tb.Text = ""; return null; }
            if (ctrl is System.Windows.Forms.RichTextBox rtb) { rtb.Text = ""; return null; }
            throw Loi(d, "xóa_trống chỉ áp dụng cho ô_văn_bản hoặc dòng_chữ");
        }));

        // ─── GUI: đóng ─────────────────────────────────────────────
        global.GanDay("đóng", new BuiltinValue("đóng", (i, a, d) =>
        {
            if (_form != null)
            {
                _form.Invoke(new Action(() => _form.Close()));
            }
            return null;
        }));

        // ─── GUI: đặt_kích_thước_widget ───────────────────────────
        global.GanDay("đặt_kích_thước_widget", new BuiltinValue("đặt_kích_thước_widget", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_kích_thước_widget", 3, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double w && a[2] is double h)
                ctrl.Size = new System.Drawing.Size((int)w, (int)h);
            return null;
        }));

        // ─── GUI: đặt_vị_trí ─────────────────────────────────────
        global.GanDay("đặt_vị_trí", new BuiltinValue("đặt_vị_trí", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_vị_trí", 3, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double x && a[2] is double y)
                ctrl.Location = new System.Drawing.Point((int)x, (int)y);
            return null;
        }));

        // ─── GUI: đặt_font ────────────────────────────────────────
        global.GanDay("đặt_font", new BuiltinValue("đặt_font", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_font", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double size)
                ctrl.Font = new System.Drawing.Font("Consolas", (float)size);
            return null;
        }));

        // ─── GUI: đặt_màu_nền ─────────────────────────────────────
        global.GanDay("đặt_màu_nền", new BuiltinValue("đặt_màu_nền", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_màu_nền", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var colorName = Interpreter.ChuoiHoa(a[1]);
            ctrl.BackColor = System.Drawing.Color.FromName(colorName);
            return null;
        }));

        // ─── GUI: đặt_màu_chữ ─────────────────────────────────────
        global.GanDay("đặt_màu_chữ", new BuiltinValue("đặt_màu_chữ", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_màu_chữ", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            var colorName = Interpreter.ChuoiHoa(a[1]);
            ctrl.ForeColor = System.Drawing.Color.FromName(colorName);
            return null;
        }));

        // ─── GUI: tạo_text_editor ──────────────────────────────────
        global.GanDay("tạo_text_editor", new BuiltinValue("tạo_text_editor", (i, a, d) =>
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
        global.GanDay("tạo_output", new BuiltinValue("tạo_output", (i, a, d) =>
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
        global.GanDay("tạo_panel", new BuiltinValue("tạo_panel", (i, a, d) =>
        {
            EnsureForm();
            var panel = new System.Windows.Forms.Panel();
            panel.Width = 600;
            panel.Height = 50;
            panel.Margin = new System.Windows.Forms.Padding(2);
            _panel.Controls.Add(panel);
            return new WidgetValue(panel, "panel");
        }));

        // ─── GUI: thêm_vào_panel ──────────────────────────────────
        global.GanDay("thêm_vào_panel", new BuiltinValue("thêm_vào_panel", (i, a, d) =>
        {
            YeucauSoLuongThamSo("thêm_vào_panel", 2, a.Count, d);
            var panelCtrl = GetWidget(a[0]);
            var childCtrl = GetWidget(a[1]);
            if (panelCtrl is System.Windows.Forms.Panel panel)
                panel.Controls.Add(childCtrl);
            return null;
        }));

        // ─── GUI: hộp_thoại_mở_file ────────────────────────────────
        global.GanDay("hộp_thoại_mở_file", new BuiltinValue("hộp_thoại_mở_file", (i, a, d) =>
        {
            var filter = a.Count >= 1 ? Interpreter.ChuoiHoa(a[0]) : "VietLang (*.vl)|*.vl|All (*.*)|*.*";
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = filter;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return ofd.FileName;
            return "";
        }));

        // ─── GUI: hộp_thoại_lưu_file ───────────────────────────────
        global.GanDay("hộp_thoại_lưu_file", new BuiltinValue("hộp_thoại_lưu_file", (i, a, d) =>
        {
            var filter = a.Count >= 1 ? Interpreter.ChuoiHoa(a[0]) : "VietLang (*.vl)|*.vl|All (*.*)|*.*";
            var sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = filter;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return sfd.FileName;
            return "";
        }));

        // ─── GUI: lấy_dòng ─────────────────────────────────────────
        global.GanDay("lấy_dòng", new BuiltinValue("lấy_dòng", (i, a, d) =>
        {
            YeucauSoLuongThamSo("lấy_dòng", 2, a.Count, d);
            var ctrl = GetWidget(a[0]);
            if (a[1] is double lineNo && ctrl is System.Windows.Forms.RichTextBox rtb)
            {
                int ln = (int)lineNo;
                if (ln < 0 || ln >= rtb.Lines.Length) return "";
                return rtb.Lines[ln];
            }
            return "";
        }));

        // ─── GUI: đặt_dòng ─────────────────────────────────────────
        global.GanDay("đặt_dòng", new BuiltinValue("đặt_dòng", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đặt_dòng", 3, a.Count, d);
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

        // ─── GUI: đếm_dòng ─────────────────────────────────────────
        global.GanDay("đếm_dòng", new BuiltinValue("đếm_dòng", (i, a, d) =>
        {
            YeucauSoLuongThamSo("đếm_dòng", 1, a.Count, d);
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