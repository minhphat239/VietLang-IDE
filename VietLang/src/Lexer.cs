using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VietLang;

/// <summary>Lỗi từ vựng: thông báo tiếng Việt kèm dòng/cột.</summary>
public sealed class LexError : Exception
{
    public int Line { get; }
    public int Col { get; }

    public LexError(string message, int line, int col) : base(message)
    {
        Line = line;
        Col = col;
    }
}

public sealed class Lexer
{
    // Bảng từ khóa — từ khóa BẮT BUỘC có dấu (trừ `this`).
    public static readonly IReadOnlyDictionary<string, TokenKind> Keywords =
        new Dictionary<string, TokenKind>
        {
            ["hàm"] = TokenKind.HAM,
            ["lớp"] = TokenKind.LOP,
            ["trả_về"] = TokenKind.TRA_VE,
            ["nếu"] = TokenKind.NEU,
            ["còn_nếu"] = TokenKind.CON_NEU,
            ["không_thì"] = TokenKind.KHONG_THI,
            ["trong_lúc"] = TokenKind.TRONG_LUC,
            ["với"] = TokenKind.VOI,
            ["trong"] = TokenKind.TRONG,
            ["dừng"] = TokenKind.DUNG_BREAK,
            ["tiếp"] = TokenKind.TIEP,
            ["và"] = TokenKind.VA,
            ["hoặc"] = TokenKind.HOAC,
            ["không"] = TokenKind.KHONG,
            ["đúng"] = TokenKind.DUNG_TRUE,
            ["sai"] = TokenKind.SAI,
            ["rỗng"] = TokenKind.RONG,
            ["this"] = TokenKind.THIS,
            ["thử"] = TokenKind.THU,
            ["ngoại_lệ"] = TokenKind.NGOAI_LE,
            ["cuối_cùng"] = TokenKind.CUOI_CUNG,
            ["ném"] = TokenKind.NEM,
            ["khai_báo"] = TokenKind.KHAI_BAO,
            ["tìm"] = TokenKind.TIM,
            ["thay"] = TokenKind.THAY,
            ["cắt"] = TokenKind.CAT,
            ["chứa"] = TokenKind.CHUA,
            ["phân_tách"] = TokenKind.PHAN_TACH,
            ["lọc"] = TokenKind.LOC,
            ["map"] = TokenKind.MAP,
            ["gộp"] = TokenKind.GOP,
            ["chuẩn_hóa"] = TokenKind.CHUAN_HOA,
            ["tìm_từ"] = TokenKind.TIM_TU,
            ["tách_từ"] = TokenKind.TACH_TU,
            ["tách_câu"] = TokenKind.TACH_CAU,
            ["đếm_từ"] = TokenKind.DEM_TU,
            ["chuẩn_hóa_tìm_kiếm"] = TokenKind.CHUAN_HOA_TIM_KIEM,
        };

    // Bảng gợi ý từ khóa thiếu dấu (ASCII -> gợi ý đúng). `ham`, `lop`, `va`... vẫn là
    // identifier hợp lệ; gợi ý CHỈ xuất hiện trong thông báo lỗi ở tầng parse/runtime.
    public static readonly IReadOnlyDictionary<string, string> SuggestionTable =
        new Dictionary<string, string>
        {
            ["ham"] = "hàm",
            ["lop"] = "lớp",
            ["tra_ve"] = "trả_về",
            ["neu"] = "nếu",
            ["con_neu"] = "còn_nếu",
            ["khong_thi"] = "không_thì",
            ["trong_luc"] = "trong_lúc",
            ["dung"] = "đúng",
            ["rong"] = "rỗng",
            ["sai"] = "sai",
            ["hoac"] = "hoặc",
            ["va"] = "và",
            ["thu"] = "thử",
            ["ngoai_le"] = "ngoại_lệ",
            ["cuoi_cung"] = "cuối_cùng",
            ["nem"] = "ném",
            ["khai_bao"] = "khai_báo",
            ["tim"] = "tìm",
            ["thay"] = "thay",
            ["cat"] = "cắt",
            ["chua"] = "chứa",
            ["phan_tach"] = "phân_tách",
            ["loc"] = "lọc",
            ["gop"] = "gộp",
            ["chuan_hoa"] = "chuẩn_hóa",
            ["tim_tu"] = "tìm_từ",
            ["tach_tu"] = "tách_từ",
            ["tach_cau"] = "tách_câu",
            ["dem_tu"] = "đếm_từ",
            ["chuan_hoa_tim_kiem"] = "chuẩn_hóa_tìm_kiếm",
        };

    /// <summary>Tra gợi ý cho identifier ThiếuDau (bỏ dấu + lowercase), không khớp trả null.</summary>
    public static string GoiYThieuDau(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        string ascii = BoDau(id);
        return SuggestionTable.TryGetValue(ascii, out string sug) ? sug : null;
    }

    private static string BoDau(string s)
    {
        var norm = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in norm)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().ToLowerInvariant();
    }

    private readonly string _src;
    private int _pos;
    private int _line = 1;
    private int _col = 1;
    // Quyết định triển khai (contract test 8/9/10): NEWLINE chỉ bị bỏ khi độ sâu của
    // `(` và `[` > 0 (giống Python). Trong khối `{}` phát NEWLINE bình thường vì đó là
    // dấu kết thúc câu lệnh trong khối.
    private int _parenDepth;

    public Lexer(string source)
    {
        _src = source ?? "";
    }

    private bool End => _pos >= _src.Length;
    private char Cur => _src[_pos];
    private char Peek(int ahead = 1) => _pos + ahead < _src.Length ? _src[_pos + ahead] : '\0';

    public List<Token> LexAll()
    {
        var list = new List<Token>();
        while (!End)
        {
            char c = Cur;
            if (c == '\n')
            {
                PhaitNewline(list);
                Consume();
            }
            else if (c == '\r')
            {
                Consume(); // \r chỉ là khoảng trắng; \n sau đó vẫn cho đúng 1 NEWLINE
            }
            else if (char.IsWhiteSpace(c))
            {
                Consume();
            }
            else if (c == '#')
            {
                SkipComment();
            }
            else if (char.IsDigit(c))
            {
                LexSo(list);
            }
            else if (isBatDauTen(c))
            {
                LexTen(list);
            }
            else if (c == '"' || c == '\'')
            {
                LexChuoi(list, c);
            }
            else
            {
                LexDon(list);
            }
        }
        list.Add(new Token(TokenKind.EOF, "", null, _line, _col));
        return list;
    }

    private void Consume()
    {
        if (_src[_pos] == '\n')
        {
            _line++;
            _col = 1;
        }
        else
        {
            _col++;
        }
        _pos++;
    }

    private void SkipComment()
    {
        while (!End && Cur != '\n') Consume();
        // newline đứng sau comment sẽ được xử lý bình thường (phát NEWLINE).
    }

    private void PhaitNewline(List<Token> list)
    {
        if (_parenDepth == 0)
            list.Add(new Token(TokenKind.NEWLINE, "\n", null, _line, _col));
        // depth > 0: bỏ qua, không phát (dòng vẫn được đếm nhờ Consume).
    }

    private void LexSo(List<Token> list)
    {
        int line = _line, col = _col, start = _pos;
        while (!End && char.IsDigit(Cur)) Consume();
        // Phần thập phân tùy chọn `4.5`; bắt buộc có chữ số sau dấu chấm.
        if (!End && Cur == '.' && char.IsDigit(Peek()))
        {
            Consume();
            while (!End && char.IsDigit(Cur)) Consume();
        }
        string lexeme = _src.Substring(start, _pos - start);
        double value = double.Parse(lexeme, CultureInfo.InvariantCulture);
        list.Add(new Token(TokenKind.SO, lexeme, value, line, col));
    }

    private static bool isBatDauTen(char c) => c == '_' || char.IsLetter(c);

    private static bool isTiepTucTen(char c) => c == '_' || char.IsLetterOrDigit(c);

    private void LexTen(List<Token> list)
    {
        int line = _line, col = _col, start = _pos;
        Consume();
        while (!End && isTiepTucTen(Cur)) Consume();
        string lexeme = _src.Substring(start, _pos - start);
        if (Keywords.TryGetValue(lexeme, out TokenKind kind))
            list.Add(new Token(kind, lexeme, null, line, col));
        else
            list.Add(new Token(TokenKind.TEN, lexeme, null, line, col));
    }

    private void LexChuoi(List<Token> list, char quote)
    {
        int line = _line, col = _col, start = _pos;
        Consume(); // nháy mở
        var sb = new StringBuilder();
        while (true)
        {
            if (End || Cur == '\n' || Cur == '\r')
                throw new LexError($"Lỗi từ vựng dòng {line}: chuỗi chưa kết thúc", line, col);
            char c = Cur;
            if (c == quote)
            {
                Consume();
                break;
            }
            if (c == '\\')
            {
                Consume();
                if (End)
                    throw new LexError($"Lỗi từ vựng dòng {line}: chuỗi chưa kết thúc", line, col);
                char e = Cur;
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case '\\': sb.Append('\\'); break;
                    case '"': sb.Append('"'); break;
                    case '\'': sb.Append('\''); break;
                    default:
                        throw new LexError($"Lỗi từ vựng dòng {line}: escape '\\{e}' không hợp lệ", line, col);
                }
                Consume();
            }
            else
            {
                sb.Append(c);
                Consume();
            }
        }
        string lexeme = _src.Substring(start, _pos - start);
        list.Add(new Token(TokenKind.CHUOI, lexeme, sb.ToString(), line, col));
    }

    private void LexDon(List<Token> list)
    {
        int line = _line, col = _col;
        char c = Cur;
        void them(TokenKind kind, string lexeme)
        {
            Consume();
            list.Add(new Token(kind, lexeme, null, line, col));
        }

        switch (c)
        {
            case '+':
                Consume();
                if (!End && Cur == '=') { Consume(); list.Add(new Token(TokenKind.CONG_BANG, "+=", null, line, col)); }
                else list.Add(new Token(TokenKind.CONG, "+", null, line, col));
                break;
            case '-': them(TokenKind.TRU, "-"); break;
            case '*': them(TokenKind.NHAN, "*"); break;
            case '/': them(TokenKind.CHIA, "/"); break;
            case '%': them(TokenKind.CHIA_DU, "%"); break;

            case '=':
                Consume();
                if (!End && Cur == '=') { Consume(); list.Add(new Token(TokenKind.SO_SANH_BANG, "==", null, line, col)); }
                else list.Add(new Token(TokenKind.BANG, "=", null, line, col));
                break;
            case '!':
                Consume();
                if (!End && Cur == '=') { Consume(); list.Add(new Token(TokenKind.KHAC, "!=", null, line, col)); }
                else throw new LexError($"Lỗi từ vựng dòng {line}: ký tự '!' không hợp lệ", line, col);
                break;
            case '<':
                Consume();
                if (!End && Cur == '=') { Consume(); list.Add(new Token(TokenKind.NHO_HON_HOAC_BANG, "<=", null, line, col)); }
                else list.Add(new Token(TokenKind.NHO_HON, "<", null, line, col));
                break;
            case '>':
                Consume();
                if (!End && Cur == '=') { Consume(); list.Add(new Token(TokenKind.LON_HON_HOAC_BANG, ">=", null, line, col)); }
                else list.Add(new Token(TokenKind.LON_HON, ">", null, line, col));
                break;

            case '(': them(TokenKind.DAU_MO_NGOAC_TRON, "("); _parenDepth++; break;
            case ')': them(TokenKind.DAU_DONG_NGOAC_TRON, ")"); _parenDepth = Math.Max(0, _parenDepth - 1); break;
            case '[': them(TokenKind.DAU_MO_NGOAC_VUONG, "["); _parenDepth++; break;
            case ']': them(TokenKind.DAU_DONG_NGOAC_VUONG, "]"); _parenDepth = Math.Max(0, _parenDepth - 1); break;
            case '{': them(TokenKind.DAU_MO_NGOAC_NHON, "{"); break;
            case '}': them(TokenKind.DAU_DONG_NGOAC_NHON, "}"); break;
            case ',': them(TokenKind.DAU_PHAY, ","); break;
            case ':': them(TokenKind.DAU_HAI_CHAM, ":"); break;
            case '.': them(TokenKind.DAU_CHAM, "."); break;
            case ';': them(TokenKind.SOL_SEMI, ";"); break;

            default:
                throw new LexError($"Lỗi từ vựng dòng {line}: ký tự '{c}' không hợp lệ", line, col);
        }
    }
}