using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VietLang;

    /// <summary>Đăng ký tất cả builtin vào môi trường toàn cục.</summary>
public static class Builtins
{
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