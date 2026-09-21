using System;

namespace VietLang;

/// <summary>Mỗi từ khóa được gán một TokenKind riêng (contract: "TokenKind riêng, có dấu").</summary>
public enum TokenKind
{
    // literal
    SO,          // số (double)
    CHUOI,       // chuỗi
    TEN,         // identifier

    // từ khóa (bắt buộc có dấu, trừ `this`)
    HAM,         // hàm       (funcDef)
    LOP,         // lớp       (classDef)
    TRA_VE,      // trả_về    (return)
    NEU,         // nếu       (if)
    CON_NEU,     // còn_nếu   (else if)
    KHONG_THI,   // không_thì (else)
    TRONG_LUC,   // trong_lúc (while)
    VOI,         // với       (for)
    TRONG,       // trong     (in)
    DUNG_BREAK,  // dừng      (break)
    TIEP,        // tiếp      (continue)
    VA,          // và        (and)
    HOAC,        // hoặc      (or)
    KHONG,       // không     (not)
    DUNG_TRUE,   // đúng      (true)
    SAI,         // sai       (false)
    RONG,        // rỗng      (null)
    THIS,        // this
    THU,         // thử        (try)
    NGOAI_LE,    // ngoại_lệ   (catch)
    CUOI_CUNG,   // cuối_cùng  (finally)
    NEM,         // ném         (raise)
    KHAI_BAO,    // khai_báo    (import)
    CHUAN_HOA,            // chuan_hoa         (normalize)
    TIM_TU,               // tim_tu            (find_word)
    TACH_TU,              // tach_tu            (word_split)
    TACH_CAU,             // tach_cau            (sentence_split)
    DEM_TU,               // dem_tu            (count_words)
    CHUAN_HOA_TIM_KIEM,   // chuan_hoa_tim_kiem (normalize_for_search)

    // toán tử
    CONG,                  // +
    TRU,                   // -
    NHAN,                  // *
    CHIA,                  // /
    CHIA_DU,               // %
    CONG_BANG,             // +=
    BANG,                  // =
    SO_SANH_BANG,          // ==
    KHAC,                  // !=
    NHO_HON,               // <
    LON_HON,               // >
    NHO_HON_HOAC_BANG,     // <=
    LON_HON_HOAC_BANG,     // >=

    // dấu
    DAU_MO_NGOAC_TRON,     // (
    DAU_DONG_NGOAC_TRON,   // )
    DAU_MO_NGOAC_VUONG,    // [
    DAU_DONG_NGOAC_VUONG,  // ]
    DAU_MO_NGOAC_NHON,     // {
    DAU_DONG_NGOAC_NHON,   // }
    DAU_PHAY,              // ,
    DAU_CHAM,              // .
    DAU_HAI_CHAM,          // :
    SOL_SEMI,              // ;  (dấu kết thúc lệnh tùy chọn)

    // method name keywords
    TIM,          // tìm
    THAY,         // thay
    CAT,          // cat
    CHUA,         // chua
    PHAN_TACH,    // phan_tach
    LOC,          // loc
    MAP,          // map
    GOP,          // gop

    NEWLINE,  // kết thúc lệnh (\r?\n)
    EOF
}

/// <summary>Tiện ích nhận diện nhóm token.</summary>
public static class TokenKinds
{
    public static bool IsKeyword(TokenKind kind) => kind switch
    {
        TokenKind.HAM or TokenKind.LOP or TokenKind.TRA_VE or TokenKind.NEU
            or TokenKind.CON_NEU or TokenKind.KHONG_THI or TokenKind.TRONG_LUC
            or TokenKind.VOI or TokenKind.TRONG or TokenKind.DUNG_BREAK
            or TokenKind.TIEP or TokenKind.VA or TokenKind.HOAC or TokenKind.KHONG
            or TokenKind.DUNG_TRUE or TokenKind.SAI or TokenKind.RONG or TokenKind.THIS
            or         TokenKind.THU or TokenKind.NGOAI_LE or TokenKind.CUOI_CUNG or TokenKind.NEM
            or TokenKind.KHAI_BAO
            or TokenKind.TIM or TokenKind.THAY or TokenKind.CAT or TokenKind.CHUA
            or TokenKind.PHAN_TACH or TokenKind.LOC or TokenKind.MAP or TokenKind.GOP
            or TokenKind.CHUAN_HOA or TokenKind.TIM_TU or TokenKind.TACH_TU
            or TokenKind.TACH_CAU or TokenKind.DEM_TU or TokenKind.CHUAN_HOA_TIM_KIEM
            => true,
        _ => false,
    };
}

public sealed class Token
{
    public TokenKind Kind { get; }
    public string Lexeme { get; }
    public object Value { get; }
    public int Line { get; }
    public int Col { get; }

    public Token(TokenKind kind, string lexeme, object value, int line, int col)
    {
        Kind = kind;
        Lexeme = lexeme;
        Value = value;
        Line = line;
        Col = col;
    }

    public bool IsKeyword => TokenKinds.IsKeyword(Kind);
    public bool IsSo => Value is double;
    public double So => Value is double d ? d : double.NaN;
    public bool IsChuoi => Value is string;

    public override string ToString() => $"{Kind}(\"{Lexeme}\") {Line}:{Col}";
}