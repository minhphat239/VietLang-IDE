using System;
using System.Collections.Generic;

namespace VietLang;

/// <summary>Lỗi cú pháp: thông báo tiếng Việt kèm dòng/cột, format `Lỗi cú pháp dòng &lt;N&gt;: ...`.</summary>
public sealed class ParseError : Exception
{
    public int Line { get; }
    public int Col { get; }

    public ParseError(string message, int line, int col) : base(message)
    {
        Line = line;
        Col = col;
    }
}

/// <summary>
/// Recursive-descent parser: ăn danh sách token từ Lexer, trả về AST (Program).
/// Lệnh kết thúc bằng NEWLINE hoặc `;` (SOL_SEMI); bỏ qua NEWLINE/`;` dư.
/// Độ ưu tiên biểu thức (thấp → cao): hoặc &lt; và &lt; ==/!= &lt; &lt;/&gt;/&lt;=/&gt;= &lt; +/− &lt; */% &lt; unary &lt; postfix &lt; primary.
/// </summary>
public sealed class Parser
{
    private readonly List<Token> _toks;
    private int _i;

    public Parser(List<Token> tokens)
    {
        _toks = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    /// <summary>Tiện ích: lex + parse một chuỗi nguồn thành Program.</summary>
    public static Program Parse(string source) => new Parser(new Lexer(source).LexAll()).PhanTichKieu();

    private Token Cur => _toks[_i];
    private Token Next()
    {
        Token t = _toks[_i];
        if (_i < _toks.Count - 1) _i++;
        return t;
    }

    private bool KiemTra(TokenKind k) => Cur.Kind == k;
    private bool Cuon(TokenKind k)
    {
        if (Cur.Kind != k) return false;
        Next();
        return true;
    }

    private Token Nhan(TokenKind k, string moTa)
    {
        if (Cur.Kind != k) throw Loi(Cur, $"mong đợi {moTa}, gặp {MoTaTK(Cur)}");
        return Next();
    }

    private static string MoTaTK(Token t) =>
        string.IsNullOrEmpty(t.Lexeme) ? t.Kind.ToString() : $"'{t.Lexeme}'";

    private static ParseError Loi(Token t, string chiTiet) =>
        new ParseError($"Lỗi cú pháp dòng {t.Line}: {chiTiet}", t.Line, t.Col);

    private void BoQuaNganCach()
    {
        while (KiemTra(TokenKind.NEWLINE) || KiemTra(TokenKind.SOL_SEMI)) Next();
    }

    /// <summary>Kiểm lệnh vừa parse xong phải được ngăn cách bởi NEWLINE/`;`/`}`/EOF.</summary>
    private void DoiNganCach(Token dauLenh)
    {
        if (KiemTra(TokenKind.NEWLINE) || KiemTra(TokenKind.SOL_SEMI)
            || KiemTra(TokenKind.EOF) || KiemTra(TokenKind.DAU_DONG_NGOAC_NHON))
            return;
        if (dauLenh.Kind == TokenKind.DAU_DONG_NGOAC_NHON)
            return;
        if (dauLenh.Kind == TokenKind.TEN)
        {
            string sug = Lexer.GoiYThieuDau(dauLenh.Lexeme);
            if (sug != null)
                throw Loi(dauLenh, $"'{dauLenh.Lexeme}' không phải từ khóa — có ý '{sug}' không?");
        }
        throw Loi(Cur, $"thiếu dấu kết thúc lệnh (newline hoặc ';') trước {MoTaTK(Cur)}");
    }

    // ─── Cấu trúc lệnh ──────────────────────────────────────────────

    public Program PhanTichKieu()
    {
        var stmts = new List<Stmt>();
        while (true)
        {
            BoQuaNganCach();
            if (KiemTra(TokenKind.EOF)) break;
            if (KiemTra(TokenKind.DAU_DONG_NGOAC_NHON))
                throw Loi(Cur, "dấu '}' không có dấu '{' mở tương ứng");
            stmts.Add(PhanTichCauLenh());
        }
        return new Program(stmts);
    }

    private Stmt PhanTichCauLenh()
    {
        Token dauLenh = Cur;
        Stmt stmt;
        switch (Cur.Kind)
        {
            case TokenKind.HAM: stmt = PhanTichHam(); break;
            case TokenKind.LOP: stmt = PhanTichLop(); break;
            case TokenKind.NEU: stmt = PhanTichNeu(); break;
            case TokenKind.TRONG_LUC: stmt = PhanTichTrongLuc(); break;
            case TokenKind.VOI: stmt = PhanTichVoi(); break;
            case TokenKind.TRA_VE: stmt = PhanTichTraVe(); break;
            case TokenKind.THU: stmt = PhanTichThu(); break;
            case TokenKind.NEM: stmt = PhanTichNem(); break;
            case TokenKind.KHAI_BAO: stmt = PhanTichNhap(); break;
            case TokenKind.DUNG_BREAK: Next(); stmt = new BreakStmt { Dong = dauLenh.Line }; break;
            case TokenKind.TIEP: Next(); stmt = new ContinueStmt { Dong = dauLenh.Line }; break;
            default: stmt = PhanTichBieuThucHoacGan(); break;
        }
        DoiNganCach(dauLenh);
        return stmt;
    }

    private Stmt PhanTichBieuThucHoacGan()
    {
        var dong = Cur.Line;
        var lhs = BieuThuc();
        if (KiemTra(TokenKind.CONG_BANG))
        {
            Token op = Next();
            var rhs = BieuThuc();
            if (lhs is not NameExpr)
                throw Loi(op, "vế trái phép gán cộng phải là biến");
            return new AugAssignStmt((NameExpr)lhs, rhs) { Dong = dong };
        }
        if (KiemTra(TokenKind.BANG))
        {
            Token bangTok = Next();
            var rhs = BieuThuc();
            if (lhs is IndexExpr ix)
                return new IndexAssignStmt(ix.Obj, ix.Index, rhs) { Dong = dong };
            if (lhs is not NameExpr and not GetExpr)
                throw Loi(bangTok, "vế trái phép gán không hợp lệ — chỉ được gán cho biến, a.b hoặc a[i]");
            return new AssignStmt(lhs, rhs) { Dong = dong };
        }
        return new ExprStmt(lhs) { Dong = dong };
    }

    private FuncDeclStmt PhanTichHam()
    {
        var dong = Cur.Line;
        Next(); // hàm
        var nameTok = Nhan(TokenKind.TEN, "tên hàm");
        Nhan(TokenKind.DAU_MO_NGOAC_TRON, "dấu '('");
        var @params = new List<string>();
        if (!KiemTra(TokenKind.DAU_DONG_NGOAC_TRON))
        {
            do
            {
                @params.Add(Nhan(TokenKind.TEN, "tên tham số").Lexeme);
            } while (Cuon(TokenKind.DAU_PHAY));
        }
        Nhan(TokenKind.DAU_DONG_NGOAC_TRON, "dấu ')' sau danh sách tham số");
        var body = PhanTichKhoi();
        return new FuncDeclStmt(nameTok.Lexeme, @params, body) { Dong = dong };
    }

    private Stmt PhanTichLop()
    {
        var dong = Cur.Line;
        Next(); // lớp
        var nameTok = Nhan(TokenKind.TEN, "tên lớp");
        var moNgoac = Nhan(TokenKind.DAU_MO_NGOAC_NHON, "dấu '{' sau tên lớp");
        var methods = new List<FuncDeclStmt>();
        while (true)
        {
            BoQuaNganCach();
            if (KiemTra(TokenKind.EOF))
                throw Loi(moNgoac, $"thiếu dấu '}}' — khối 'lớp' mở ở dòng {moNgoac.Line}");
            if (KiemTra(TokenKind.DAU_DONG_NGOAC_NHON)) { Next(); break; }
            Token dauHam = Cur;
            if (!KiemTra(TokenKind.HAM))
                throw Loi(Cur, "trong khối 'lớp' chỉ được khai báo hàm — mong đợi từ khóa 'hàm'");
            methods.Add(PhanTichHam());
            DoiNganCach(dauHam);
        }
        return new ClassDeclStmt(nameTok.Lexeme, methods) { Dong = dong };
    }

    private Stmt PhanTichNeu()
    {
        var dong = Cur.Line;
        Next(); // nếu
        var cond = BieuThuc();
        var thenBody = PhanTichKhoi();
        var top = new IfStmt(cond, thenBody) { Dong = dong };
        var current = top;
        while (NhinCauNoiTiep())
        {
            if (KiemTra(TokenKind.CON_NEU))
            {
                var dongConNeu = Cur.Line;
                Next(); // còn_nếu
                var inner = new IfStmt(BieuThuc(), PhanTichKhoi()) { Dong = dongConNeu };
                current.Otherwise = new List<Stmt> { inner };
                current = inner;
            }
            else // KHONG_THI
            {
                Next(); // không_thì
                current.Otherwise = PhanTichKhoi();
                break;
            }
        }
        return top;
    }

    /// <summary>
    /// Gọi ngay sau `}` của nhánh nếu/còn_nếu: chỉ bỏ qua NEWLINE/`;` rỗng để tìm từ khóa
    /// còn_nếu/không_thì ở dòng kế. Tìm thấy → _i đứng tại từ khóa; không → rollback về
    /// ngay sau `}` (NEWLINE sau đó do vòng lệnh ngoài tiêu thụ như dấu kết thúc lệnh).
    /// Không bỏ qua ký tự khác để không nuốt lệnh kế tiếp.
    /// </summary>
    private bool NhinCauNoiTiep()
    {
        int save = _i;
        while (KiemTra(TokenKind.NEWLINE) || KiemTra(TokenKind.SOL_SEMI)) Next();
        if (KiemTra(TokenKind.CON_NEU) || KiemTra(TokenKind.KHONG_THI)) return true;
        _i = save;
        return false;
    }

    private Stmt PhanTichTrongLuc()
    {
        var dong = Cur.Line;
        Next(); // trong_lúc
        var cond = BieuThuc();
        var body = PhanTichKhoi();
        return new WhileStmt(cond, body) { Dong = dong };
    }

    private Stmt PhanTichVoi()
    {
        var dong = Cur.Line;
        Next(); // với
        var varTok = Nhan(TokenKind.TEN, "tên biến lặp");
        Nhan(TokenKind.TRONG, "từ khóa 'trong'");
        var iterable = BieuThuc();
        var body = PhanTichKhoi();
        return new ForInStmt(varTok.Lexeme, iterable, body) { Dong = dong };
    }

    private Stmt PhanTichTraVe()
    {
        var dong = Cur.Line;
        Next(); // trả_về
        if (KiemTra(TokenKind.NEWLINE) || KiemTra(TokenKind.SOL_SEMI)
            || KiemTra(TokenKind.EOF) || KiemTra(TokenKind.DAU_DONG_NGOAC_NHON))
            return new ReturnStmt(null) { Dong = dong };
        return new ReturnStmt(BieuThuc()) { Dong = dong };
    }

    private TryStmt PhanTichThu()
    {
        var dong = Cur.Line;
        Next(); // thử
        var body = PhanTichKhoi();
        string catchVar = null;
        List<Stmt> catchBody = null;
        List<Stmt> finallyBody = null;

        int save = _i;
        BoQuaNganCach();
        if (KiemTra(TokenKind.NGOAI_LE))
        {
            Next(); // ngoại_lệ
            if (KiemTra(TokenKind.DAU_MO_NGOAC_TRON))
            {
                Next(); // (
                catchVar = Nhan(TokenKind.TEN, "tên biến ngoại_lệ").Lexeme;
                Nhan(TokenKind.DAU_DONG_NGOAC_TRON, "dấu ')'");
            }
            catchBody = PhanTichKhoi();
            save = _i;
        }
        else
        {
            _i = save;
        }

        BoQuaNganCach();
        if (KiemTra(TokenKind.CUOI_CUNG))
        {
            Next(); // cuối_cùng
            finallyBody = PhanTichKhoi();
        }
        else
        {
            _i = save;
        }

        return new TryStmt(body, catchVar, catchBody, finallyBody) { Dong = dong };
    }

    private RaiseStmt PhanTichNem()
    {
        var dong = Cur.Line;
        Next(); // ném
        Nhan(TokenKind.DAU_MO_NGOAC_TRON, "dấu '(' sau ném");
        Expr msg;
        if (KiemTra(TokenKind.DAU_DONG_NGOAC_TRON))
        {
            msg = new StrLit("");
        }
        else
        {
            msg = BieuThuc();
        }
        Nhan(TokenKind.DAU_DONG_NGOAC_TRON, "dấu ')'");
        return new RaiseStmt(msg) { Dong = dong };
    }

    private ImportStmt PhanTichNhap()
    {
        var dong = Cur.Line;
        Next(); // khai_báo
        if (!KiemTra(TokenKind.CHUOI))
            throw Loi(Cur, "khai_báo cần đường dẫn dạng chuỗi");
        var path = PhanTichPrimary();
        return new ImportStmt(path) { Dong = dong };
    }

    /// <summary>Parse khối `{ ... }`, mỗi lệnh ngăn cách bằng NEWLINE hoặc `;`.</summary>
    private List<Stmt> PhanTichKhoi()
    {
        var moNgoac = Nhan(TokenKind.DAU_MO_NGOAC_NHON, "dấu '{'");
        var body = new List<Stmt>();
        while (true)
        {
            BoQuaNganCach();
            if (KiemTra(TokenKind.EOF))
                throw Loi(moNgoac, $"thiếu dấu '}}' — khối lệnh mở ở dòng {moNgoac.Line}");
            if (KiemTra(TokenKind.DAU_DONG_NGOAC_NHON)) { Next(); break; }
            body.Add(PhanTichCauLenh());
        }
        return body;
    }

    // ─── Biểu thức ─────────────────────────────────────────────────

    private Expr BieuThuc() => PhanTichHoac();

    private Expr PhanTichNhiPhan(Func<Expr> capDuoi, params TokenKind[] toanTu)
    {
        var left = capDuoi();
        while (Array.IndexOf(toanTu, Cur.Kind) >= 0)
        {
            Token op = Next();
            var right = capDuoi();
            left = new BinaryExpr(op.Kind.ToString(), left, right);
        }
        return left;
    }

    private Expr PhanTichHoac() => PhanTichNhiPhan(PhanTichVa, TokenKind.HOAC);
    private Expr PhanTichVa() => PhanTichNhiPhan(PhanTichSoSanhBang, TokenKind.VA);
    private Expr PhanTichSoSanhBang() => PhanTichNhiPhan(PhanTichSoSanhThuTu, TokenKind.SO_SANH_BANG, TokenKind.KHAC);
    private Expr PhanTichSoSanhThuTu() => PhanTichNhiPhan(PhanTichCongTru,
        TokenKind.NHO_HON, TokenKind.LON_HON, TokenKind.NHO_HON_HOAC_BANG, TokenKind.LON_HON_HOAC_BANG);
    private Expr PhanTichCongTru() => PhanTichNhiPhan(PhanTichNhanChia, TokenKind.CONG, TokenKind.TRU);
    private Expr PhanTichNhanChia() => PhanTichNhiPhan(PhanTichUnary, TokenKind.NHAN, TokenKind.CHIA, TokenKind.CHIA_DU);

    private Expr PhanTichUnary()
    {
        if (KiemTra(TokenKind.KHONG) || KiemTra(TokenKind.TRU))
        {
            Token op = Next();
            return new UnaryExpr(op.Kind.ToString(), PhanTichUnary());
        }
        return PhanTichPostfix();
    }

    private Expr PhanTichPostfix()
    {
        var e = PhanTichPrimary();
        while (true)
        {
            if (Cuon(TokenKind.DAU_MO_NGOAC_TRON))
            {
                var args = new List<Expr>();
                if (!KiemTra(TokenKind.DAU_DONG_NGOAC_TRON))
                {
                    do
                    {
                        args.Add(BieuThuc());
                    } while (Cuon(TokenKind.DAU_PHAY));
                }
                Nhan(TokenKind.DAU_DONG_NGOAC_TRON, "dấu ')' sau danh sách đối số");
                e = new CallExpr(e, args);
            }
            else if (Cuon(TokenKind.DAU_CHAM))
            {
                string nameLexeme;
                if (KiemTra(TokenKind.TEN))
                {
                    nameLexeme = Next().Lexeme;
                }
                else if (TokenKinds.IsKeyword(Cur.Kind))
                {
                    nameLexeme = Next().Lexeme;
                }
                else
                {
                    throw Loi(Cur, "mong đợi tên thành phần sau dấu '.'");
                }
                e = new GetExpr(e, nameLexeme);
            }
            else if (Cuon(TokenKind.DAU_MO_NGOAC_VUONG))
            {
                var idx = BieuThuc();
                Nhan(TokenKind.DAU_DONG_NGOAC_VUONG, "dấu ']'");
                e = new IndexExpr(e, idx);
            }
            else break;
        }
        return e;
    }

    private Expr PhanTichPrimary()
    {
        Token t = Cur;
        switch (t.Kind)
        {
            case TokenKind.SO:
                Next();
                return new NumLit(t.So);
            case TokenKind.CHUOI:
                Next();
                return new StrLit((string)t.Value);
            case TokenKind.DUNG_TRUE:
                Next();
                return new BoolLit(true);
            case TokenKind.SAI:
                Next();
                return new BoolLit(false);
            case TokenKind.RONG:
                Next();
                return new NullLit();
            case TokenKind.THIS:
                Next();
                return new ThisExpr();
            case TokenKind.DAU_MO_NGOAC_TRON:
                Next();
                var e = BieuThuc();
                Nhan(TokenKind.DAU_DONG_NGOAC_TRON, "dấu ')'");
                return e;
            case TokenKind.DAU_MO_NGOAC_VUONG:
                return PhanTichMang();
            case TokenKind.DAU_MO_NGOAC_NHON:
                return PhanTichDict();
            case TokenKind.HAM:
                return PhanTichHamExpr();
            case TokenKind.TEN:
                Next();
                return new NameExpr(t.Lexeme);
            case TokenKind.CHUAN_HOA:
            case TokenKind.TIM_TU:
            case TokenKind.TACH_TU:
            case TokenKind.TACH_CAU:
            case TokenKind.DEM_TU:
            case TokenKind.CHUAN_HOA_TIM_KIEM:
                Next();
                return new NameExpr(t.Lexeme);
            default:
                throw Loi(t, $"không mong đợi {MoTaTK(t)} ở vị trí biểu thức");
        }
    }

    private Expr PhanTichMang()
    {
        // '[' đã nằm ở Cur theo enum DAU_MO_NGOAC_VUONG ở primary; tiêu thụ tại đây.
        Next();
        var items = new List<Expr>();
        if (!KiemTra(TokenKind.DAU_DONG_NGOAC_VUONG))
        {
            do
            {
                items.Add(BieuThuc());
            } while (Cuon(TokenKind.DAU_PHAY));
        }
        Nhan(TokenKind.DAU_DONG_NGOAC_VUONG, "dấu ']'");
        return new ArrayLit(items);
    }

    private Expr PhanTichDict()
    {
        Next(); // tiêu thụ '{'
        // Kiểm tra pattern dict: token kế là CHUOI hoặc TEN + DAU_HAI_CHAM
        if (LaPatternDict())
        {
            var entries = new List<(Expr Key, Expr Value)>();
            do
            {
                Expr key = BieuThuc();
                Nhan(TokenKind.DAU_HAI_CHAM, "dấu ':' sau key trong dict");
                Expr value = BieuThuc();
                entries.Add((key, value));
                BoQuaNganCach();
            } while (Cuon(TokenKind.DAU_PHAY));
            Nhan(TokenKind.DAU_DONG_NGOAC_NHON, "dấu '}' dong dict");
            return new DictLit(entries);
        }
        // Không match pattern dict → dict rỗng
        Nhan(TokenKind.DAU_DONG_NGOAC_NHON, "dấu '}' dong dict rỗng");
        return new DictLit();
    }

    /// <summary>Parse biểu thức hàm: hàm(tên) { ... } hoặc hàm(…) { ... } (anonymous).</summary>
    private Expr PhanTichHamExpr()
    {
        Next(); // hàm
        string name = null;
        if (KiemTra(TokenKind.TEN))
        {
            name = Next().Lexeme;
        }
        Nhan(TokenKind.DAU_MO_NGOAC_TRON, "dấu '('");
        var @params = new List<string>();
        if (!KiemTra(TokenKind.DAU_DONG_NGOAC_TRON))
        {
            do
            {
                @params.Add(Nhan(TokenKind.TEN, "tên tham số").Lexeme);
            } while (Cuon(TokenKind.DAU_PHAY));
        }
        Nhan(TokenKind.DAU_DONG_NGOAC_TRON, "dấu ')' sau danh sách tham số");
        var body = PhanTichKhoi();
        var decl = new FuncDeclStmt(name ?? "_anon", @params, body);
        return new FuncExpr(decl);
    }

    private bool LaPatternDict()
    {
        // Dict pattern: CHUOI hoặc TEN kế tiếp là DAU_HAI_CHAM
        if (KiemTra(TokenKind.CHUOI) || KiemTra(TokenKind.TEN))
        {
            int save = _i;
            Next(); // bỏ qua CHUOI/TEN
            bool ok = KiemTra(TokenKind.DAU_HAI_CHAM);
            _i = save;
            return ok;
        }
        return false;
    }
}