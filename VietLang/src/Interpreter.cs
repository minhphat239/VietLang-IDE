using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VietLang;

/// <summary>Lỗi runtime: format `Lỗi thực thi dòng &lt;Dong&gt;: &lt;chi tiết&gt;`.</summary>
public sealed class RuntimeError : Exception
{
    public RuntimeError(string message) : base(message) { }
}

/// <summary>Exception do người dùng ném qua `ném(...)`.</summary>
public sealed class VietLangException : Exception
{
    public VietLangException(string message) : base(message) { }
}

/// <summary>Tín hiệu `trả_về` — exception nội bộ, không lộ ra ngoài.</summary>
internal sealed class ReturnSignal : Exception
{
    public object Value { get; }
    public int Dong { get; }
    public ReturnSignal(object value, int dong) { Value = value; Dong = dong; }
}

/// <summary>Tín hiệu `dừng` — exception nội bộ.</summary>
internal sealed class BreakSignal : Exception { }

/// <summary>Tín hiệu `tiếp` — exception nội bộ.</summary>
internal sealed class ContinueSignal : Exception { }

/// <summary>Phạm vi biến: Dictionary + cha. Tra biến đi ngược chain; gán: thấy ở đâu set đó, không thấy tạo ở hiện tại.</summary>
public sealed class PhamVi
{
    private readonly Dictionary<string, object> _cuc = new();

    public PhamVi Cha { get; }

    public PhamVi(PhamVi cha = null) { Cha = cha; }

    public bool Chua(string ten)
    {
        PhamVi v = this;
        while (v != null)
        {
            if (v._cuc.ContainsKey(ten)) return true;
            v = v.Cha;
        }
        return false;
    }

    public bool Lay(string ten, out object giaTri)
    {
        PhamVi v = this;
        while (v != null)
        {
            if (v._cuc.TryGetValue(ten, out giaTri)) return true;
            v = v.Cha;
        }
        giaTri = null;
        return false;
    }

    public void Gan(string ten, object giaTri)
    {
        PhamVi v = this;
        while (v != null)
        {
            if (v._cuc.ContainsKey(ten)) { v._cuc[ten] = giaTri; return; }
            v = v.Cha;
        }
        _cuc[ten] = giaTri;
    }

    // Gán trực tiếp vào phạm vi này (không đi chain) — dùng cho khai báo/biến phạm vi cục bộ.
    public void GanDay(string ten, object giaTri) => _cuc[ten] = giaTri;

    /// <summary>Trả về dictionary chứa tất cả biến từ phạm vi này lên đến gốc (global scope).</summary>
    public Dictionary<string, object> LayTatCa()
    {
        var result = new Dictionary<string, object>();
        PhamVi v = this;
        while (v != null)
        {
            foreach (var kv in v._cuc)
            {
                if (!result.ContainsKey(kv.Key))
                    result[kv.Key] = kv.Value;
            }
            v = v.Cha;
        }
        return result;
    }
}

/// <summary>Giá trị hàm (function), ôm closure.</summary>
public sealed class FunctionValue
{
    public FuncDeclStmt KhaiBao { get; }
    public PhamVi Closure { get; }
    public FunctionValue(FuncDeclStmt khaiBao, PhamVi closure) { KhaiBao = khaiBao; Closure = closure; }
    public override string ToString() => $"<hàm {KhaiBao.Name}>";
}

/// <summary>Giá trị lớp (class): tên + các method.</summary>
public sealed class ClassValue
{
    public string Ten { get; }
    public IReadOnlyDictionary<string, FunctionValue> Methods { get; }
    public ClassValue(string ten, Dictionary<string, FunctionValue> methods)
    {
        Ten = ten;
        Methods = methods;
    }
    public override string ToString() => $"<lớp {Ten}>";
}

/// <summary>Đối tượng (instance của lớp).</summary>
public sealed class InstanceValue
{
    public ClassValue Lop { get; }
    public Dictionary<string, object> Fields { get; } = new();
    public InstanceValue(ClassValue lop) { Lop = lop; }
    public override string ToString() => $"<đối tượng {Lop.Ten}>";
}

/// <summary>Giá trị dict: dictionary string → object.</summary>
public sealed class DictValue
{
    public Dictionary<string, object> Pairs { get; } = new();
    public override string ToString() => $"<dict ({Pairs.Count} phần tử)>";
}

/// <summary>Method đã bind trên dict (gộp dict + tên method).</summary>
internal sealed class DictMethodValue
{
    public DictValue Dict { get; }
    public string Ten { get; }
    public DictMethodValue(DictValue dict, string ten) { Dict = dict; Ten = ten; }
}

/// <summary>Method đã bind trên chuỗi/mảng (gộp object + tên method).</summary>
internal sealed class BoundMethodValue
{
    public object Obj { get; }
    public string Ten { get; }
    public BoundMethodValue(object obj, string ten) { Obj = obj; Ten = ten; }
}

/// <summary>Method đã bind this.</summary>
public sealed class BoundMethod
{
    public InstanceValue Instance { get; }
    public FunctionValue Method { get; }
    public BoundMethod(InstanceValue instance, FunctionValue method) { Instance = instance; Method = method; }
}

/// <summary>Giá trị hàm built-in (đăng ký vào môi trường toàn cục).</summary>
public sealed class BuiltinValue
{
    public string Ten { get; }
    public Func<Interpreter, List<object>, int, object> Ham { get; }
    public BuiltinValue(string ten, Func<Interpreter, List<object>, int, object> ham) { Ten = ten; Ham = ham; }
    public override string ToString() => $"<hàm {Ten}>";
}

/// <summary>Tree-walking interpreter cho VietLang v0.2.</summary>
public sealed class Interpreter
{
    /// <summary>Bảng chuẩn hóa dấu tiếng Việt → ASCII (134 ký tự).</summary>
    public static readonly IReadOnlyDictionary<char, char> BangChuanHoa = new Dictionary<char, char>
    {
        ['à'] = 'a', ['á'] = 'a', ['ả'] = 'a', ['ã'] = 'a', ['ạ'] = 'a',
        ['ă'] = 'a', ['ằ'] = 'a', ['ắ'] = 'a', ['ẳ'] = 'a', ['ẵ'] = 'a', ['ặ'] = 'a',
        ['â'] = 'a', ['ầ'] = 'a', ['ấ'] = 'a', ['ẩ'] = 'a', ['ẫ'] = 'a', ['ậ'] = 'a',
        ['è'] = 'e', ['é'] = 'e', ['ẻ'] = 'e', ['ẽ'] = 'e', ['ẹ'] = 'e',
        ['ê'] = 'e', ['ề'] = 'e', ['ế'] = 'e', ['ể'] = 'e', ['ễ'] = 'e', ['ệ'] = 'e',
        ['ì'] = 'i', ['í'] = 'i', ['ỉ'] = 'i', ['ĩ'] = 'i', ['ị'] = 'i',
        ['ò'] = 'o', ['ó'] = 'o', ['ỏ'] = 'o', ['õ'] = 'o', ['ọ'] = 'o',
        ['ô'] = 'o', ['ồ'] = 'o', ['ố'] = 'o', ['ổ'] = 'o', ['ỗ'] = 'o', ['ộ'] = 'o',
        ['ơ'] = 'o', ['ờ'] = 'o', ['ớ'] = 'o', ['ở'] = 'o', ['ỡ'] = 'o', ['ợ'] = 'o',
        ['ù'] = 'u', ['ú'] = 'u', ['ủ'] = 'u', ['ũ'] = 'u', ['ụ'] = 'u',
        ['ư'] = 'u', ['ừ'] = 'u', ['ứ'] = 'u', ['ử'] = 'u', ['ữ'] = 'u', ['ự'] = 'u',
        ['ỳ'] = 'y', ['ý'] = 'y', ['ỷ'] = 'y', ['ỹ'] = 'y', ['ỵ'] = 'y',
        ['đ'] = 'd',
        ['Đ'] = 'D',
        ['À'] = 'A', ['Á'] = 'A', ['Ả'] = 'A', ['Ã'] = 'A', ['Ạ'] = 'A',
        ['Ằ'] = 'A', ['Ắ'] = 'A', ['Ẳ'] = 'A', ['Ẵ'] = 'A', ['Ặ'] = 'A',
        ['Ầ'] = 'A', ['Ấ'] = 'A', ['Ẩ'] = 'A', ['Ẫ'] = 'A', ['Ậ'] = 'A',
        ['È'] = 'E', ['É'] = 'E', ['Ẻ'] = 'E', ['Ẽ'] = 'E', ['Ẹ'] = 'E',
        ['Ề'] = 'E', ['Ế'] = 'E', ['Ể'] = 'E', ['Ễ'] = 'E', ['Ệ'] = 'E',
        ['Ì'] = 'I', ['Í'] = 'I', ['Ỉ'] = 'I', ['Ĩ'] = 'I', ['Ị'] = 'I',
        ['Ò'] = 'O', ['Ó'] = 'O', ['Ỏ'] = 'O', ['Õ'] = 'O', ['Ọ'] = 'O',
        ['Ồ'] = 'O', ['Ố'] = 'O', ['Ổ'] = 'O', ['Ỗ'] = 'O', ['Ộ'] = 'O',
        ['Ờ'] = 'O', ['Ớ'] = 'O', ['Ở'] = 'O', ['Ỡ'] = 'O', ['Ợ'] = 'O',
        ['Ù'] = 'U', ['Ú'] = 'U', ['Ủ'] = 'U', ['Ũ'] = 'U', ['Ụ'] = 'U',
        ['Ừ'] = 'U', ['Ứ'] = 'U', ['Ử'] = 'U', ['Ữ'] = 'U', ['Ự'] = 'U',
        ['Ỳ'] = 'Y', ['Ý'] = 'Y', ['Ỷ'] = 'Y', ['Ỹ'] = 'Y', ['Ỵ'] = 'Y',
    };

    /// <summary>Bỏ dấu tiếng Việt: chuẩn_hóa(text) → string.</summary>
    public static string BoDauTiengViet(string text)
    {
        if (text == null) return null;
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (char c in text)
            sb.Append(BangChuanHoa.TryGetValue(c, out char mapped) ? mapped : c);
        return sb.ToString();
    }

    /// <summary>Độ sâu vòng lặp hiện tại — để nhận diện `dừng`/`tiếp` ngoài vòng lặp.</summary>
    private int _loopDepth;

    /// <summary>Môi trường hiện tại — để builtin `thực_thi` có thể truy cập env của caller.</summary>
    internal PhamVi CurrentEnv { get; private set; }

    /// <summary>Thư mục hiện tại (dùng để resolve đường dẫn import tương đối).</summary>
    private string _currentDir;

    /// <summary>Các đường dẫn đã visit — chống circular import.</summary>
    private HashSet<string> _visitedPaths;

    /// <summary>ToString của số: không có hậu tố thừa ("120" thay vì "120.0").</summary>
    private static string So(double d) => d.ToString("G", System.Globalization.CultureInfo.InvariantCulture);

    private static RuntimeError Loi(int dong, string chiTiet)
        => new RuntimeError($"Lỗi thực thi dòng {dong}: {chiTiet}");

    /// <summary>Chạy toàn bộ chương trình với môi trường toàn cục mới.</summary>
    public void Run(Program program)
    {
        var global = new PhamVi();
        Builtins.DangKy(global);
        _currentDir = Directory.GetCurrentDirectory();
        _visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            ThucHien(ListMoi(program.Stmts), global);
        }
        catch (ReturnSignal r)
        {
            throw Loi(r.Dong, "trả_về nằm ngoài hàm");
        }
        catch (VietLangException ex)
        {
            throw Loi(0, "Lỗi thực thi: " + ex.Message);
        }
    }

    /// <summary>Chạy với đường dẫn file — hỗ trợ import tương đối.</summary>
    public void Run(Program program, string filePath, PhamVi sharedGlobal, HashSet<string> visited)
    {
        _currentDir = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? Directory.GetCurrentDirectory();
        _visitedPaths = visited ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var env = sharedGlobal ?? new PhamVi();
        if (sharedGlobal == null) Builtins.DangKy(env);
        ThucHien(ListMoi(program.Stmts), env);
    }

    /// <summary>Chạy với chương trình + môi trường toàn cục sẵn (hữu ích cho test REPL sau này).</summary>
    public void ChayVoiGlobal(PhamVi global, Program program)
    {
        ThucHien(ListMoi(program.Stmts), global);
    }

    private static List<Stmt> ListMoi(List<Stmt> stmts)
    {
        foreach (Stmt s in stmts) if (s.Dong <= 0) s.Dong = 1; // an toàn cho AST dựng bằng tay (parser luôn set).
        return stmts;
    }

    /// <summary>Parse + eval code VietLang từ chuỗi. Trả kết quả cuối cùng (expression) hoặc null. Bắt lỗi an toàn.</summary>
    public object ThucThiChuoi(string code, PhamVi env)
    {
        if (string.IsNullOrEmpty(code)) return null;
        try
        {
            var program = Parser.Parse(code);
            var stmts = ListMoi(program.Stmts);
            if (stmts.Count == 0) return null;
            for (int i = 0; i < stmts.Count - 1; i++)
                ThucHienCauLenh(stmts[i], env);
            var last = stmts[stmts.Count - 1];
            if (last is ExprStmt expr)
                return Eval(expr.Expr, env, expr.Dong);
            ThucHienCauLenh(last, env);
            return null;
        }
        catch (LexError e) { return "lỗi cú pháp: " + e.Message; }
        catch (ParseError e) { return "lỗi cú pháp: " + e.Message; }
        catch (RuntimeError e) { return e.Message; }
    }

    // ─── LỆNH ─────────────────────────────────────────────────────

    private void ThucHien(List<Stmt> stmts, PhamVi env)
    {
        CurrentEnv = env;
        foreach (Stmt s in stmts) ThucHienCauLenh(s, env);
    }

    private void ThucHienCauLenh(Stmt s, PhamVi env)
    {
        switch (s)
        {
            case AssignStmt a: ThucHienGan(a, env); break;
            case AugAssignStmt aug: ThucHienAugGan(aug, env); break;
            case IndexAssignStmt ix: ThucHienGanIndex(ix, env); break;
            case ExprStmt x: Eval(x.Expr, env, x.Dong); break;
            case IfStmt i: ThucHienNeu(i, env); break;
            case WhileStmt w: ThucHienTrongLuc(w, env); break;
            case ForInStmt f: ThucHienVoiTrong(f, env); break;
            case ReturnStmt r:
                throw new ReturnSignal(r.Value == null ? null : Eval(r.Value, env, r.Dong), r.Dong);
            case BreakStmt b:
                if (_loopDepth == 0) throw Loi(b.Dong, "dừng nằm ngoài vòng lặp");
                throw new BreakSignal();
            case ContinueStmt c:
                if (_loopDepth == 0) throw Loi(c.Dong, "tiếp nằm ngoài vòng lặp");
                throw new ContinueSignal();
            case FuncDeclStmt f:
                // parser không tạo VarDeclStmt; phòng hờ nếu gặp thì gán như khai báo.
                env.GanDay(f.Name, new FunctionValue(f, env));
                break;
            case ClassDeclStmt cls: ThucHienLop(cls, env); break;
            case TryStmt t: ThucHienThu(t, env); break;
            case RaiseStmt r: ThucHienNem(r, env); break;
            case ImportStmt im: ThucHienNhap(im, env); break;
            case VarDeclStmt v:
                env.GanDay(v.Name, v.Init == null ? null : Eval(v.Init, env, v.Dong));
                break;
            default:
                throw Loi(s.Dong, "lệnh không xác định");
        }
    }

    private void ThucHienLop(ClassDeclStmt cls, PhamVi env)
    {
        var methods = new Dictionary<string, FunctionValue>();
        foreach (var m in cls.Methods)
            methods[m.Name] = new FunctionValue(m, env);
        env.GanDay(cls.Name, new ClassValue(cls.Name, methods));
    }

    private void ThucHienThu(TryStmt t, PhamVi env)
    {
        ReturnSignal retSig = null;
        BreakSignal brkSig = null;
        ContinueSignal cntSig = null;
        try
        {
            ThucHien(t.Body, env);
        }
        catch (VietLangException ex)
        {
            if (t.CatchBody != null)
            {
                var catchEnv = new PhamVi(env);
                if (t.CatchVar != null)
                    catchEnv.GanDay(t.CatchVar, ex.Message);
                ThucHien(t.CatchBody, catchEnv);
            }
            else
            {
                throw;
            }
        }
        catch (RuntimeError ex)
        {
            if (t.CatchBody != null)
            {
                var catchEnv = new PhamVi(env);
                if (t.CatchVar != null)
                    catchEnv.GanDay(t.CatchVar, ex.Message);
                ThucHien(t.CatchBody, catchEnv);
            }
            else
            {
                throw;
            }
        }
        catch (ReturnSignal r)
        {
            retSig = r;
        }
        catch (BreakSignal b)
        {
            brkSig = b;
        }
        catch (ContinueSignal c)
        {
            cntSig = c;
        }
        finally
        {
            if (t.FinallyBody != null)
                ThucHien(t.FinallyBody, env);
        }
        if (retSig != null) throw retSig;
        if (brkSig != null) throw brkSig;
        if (cntSig != null) throw cntSig;
    }

    private void ThucHienNem(RaiseStmt r, PhamVi env)
    {
        object msg = Eval(r.Message, env, r.Dong);
        string msgStr = msg == null ? "" : ChuoiHoa(msg);
        throw new VietLangException(msgStr);
    }

    private void ThucHienNhap(ImportStmt im, PhamVi env)
    {
        object pathVal = Eval(im.Path, env, im.Dong);
        if (pathVal is not string path)
            throw Loi(im.Dong, "đường dẫn nhập phải là chuỗi");

        string fullPath = Path.GetFullPath(Path.Combine(_currentDir, path));

        if (_visitedPaths.Contains(fullPath))
            return; // circular import — skip

        _visitedPaths.Add(fullPath);

        string source;
        try
        {
            source = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
        }
        catch (Exception)
        {
            throw Loi(im.Dong, $"không tìm thấy tệp '{path}'");
        }

        var importedProgram = Parser.Parse(source);
        var sub = new Interpreter();
        sub.Run(importedProgram, fullPath, env, _visitedPaths);
    }

    private void ThucHienNeu(IfStmt i, PhamVi env)
    {
        if (Chantri(Eval(i.Cond, env, i.Dong)))
            ThucHien(i.Then, env);
        else if (i.Otherwise != null)
            ThucHien(i.Otherwise, env);
    }

    private void ThucHienTrongLuc(WhileStmt w, PhamVi env)
    {
        int dong = w.Dong;
        while (Chantri(Eval(w.Cond, env, dong)))
        {
            _loopDepth++;
            try { ThucHien(w.Body, env); }
            catch (BreakSignal) { break; }
            catch (ContinueSignal) { /* tiếp tục vòng */ }
            finally { _loopDepth--; }
        }
    }

    private void ThucHienVoiTrong(ForInStmt f, PhamVi env)
    {
        int dong = f.Dong;
        object iterable = Eval(f.Iterable, env, dong);
        if (iterable is List<object> list)
        {
            foreach (object item in list.ToList())
            {
                env.Gan(f.Var, item);
                _loopDepth++;
                try { ThucHien(f.Body, env); }
                catch (BreakSignal) { break; }
                catch (ContinueSignal) { /* tiếp */ }
                finally { _loopDepth--; }
            }
        }
        else if (iterable is string chuoi)
        {
            foreach (char c in chuoi)
            {
                env.Gan(f.Var, c.ToString());
                _loopDepth++;
                try { ThucHien(f.Body, env); }
                catch (BreakSignal) { break; }
                catch (ContinueSignal) { /* tiếp */ }
                finally { _loopDepth--; }
            }
        }
        else if (iterable is DictValue dict)
        {
            foreach (string key in dict.Pairs.Keys.ToList())
            {
                env.Gan(f.Var, key);
                _loopDepth++;
                try { ThucHien(f.Body, env); }
                catch (BreakSignal) { break; }
                catch (ContinueSignal) { /* tiếp */ }
                finally { _loopDepth--; }
            }
        }
        else
        {
            throw Loi(dong, $"không lặp được qua kiểu {MoTaKieu(iterable)}");
        }
    }

    private void ThucHienGan(AssignStmt a, PhamVi env)
    {
        int dong = a.Dong;
        object giaTri = Eval(a.Value, env, dong);
        switch (a.Target)
        {
            case NameExpr n:
                env.Gan(n.Name, giaTri);
                break;
            case GetExpr g:
                var obj = Eval(g.Obj, env, dong);
                if (obj is InstanceValue inst)
                {
                    inst.Fields[g.Name] = giaTri;
                }
                else
                {
                    throw Loi(dong, $"không gán được thuộc tính '{g.Name}' cho {MoTaKieu(obj)}");
                }
                break;
            case IndexExpr ix:
                var mang = Eval(ix.Obj, env, dong);
                if (mang is List<object> arr)
                {
                    int i = ChiSoNguyen(Eval(ix.Index, env, dong), dong);
                    if (i < 0 || i >= arr.Count)
                        throw Loi(dong, $"chỉ số {i} ngoài phạm vi mảng (0..{arr.Count - 1})");
                    arr[i] = giaTri;
                }
                else if (mang is DictValue dict)
                {
                    object key = Eval(ix.Index, env, dong);
                    if (key is not string ks)
                        throw Loi(dong, $"chỉ số dict phải là chuỗi, nhận {MoTaKieu(key)}");
                    dict.Pairs[ks] = giaTri;
                }
                else
                {
                    throw Loi(dong, $"không đánh chỉ số được kiểu {MoTaKieu(mang)}");
                }
                break;
            default:
                throw Loi(dong, "vế trái phép gán không hợp lệ");
        }
    }

    private void ThucHienAugGan(AugAssignStmt a, PhamVi env)
    {
        int dong = a.Dong;
        string name = a.Target.Name;
        if (!env.Lay(name, out var cur))
            throw Loi(dong, $"biến '{name}' chưa được gán");
        object rhs = Eval(a.Value, env, dong);
        if (cur is double cd && rhs is double rd)
            env.Gan(name, cd + rd);
        else if (cur is string cs && rhs is string rs)
            env.Gan(name, cs + rs);
        else
            throw Loi(dong, $"phép += chỉ hỗ trợ số hoặc chuỗi, nhận {MoTaKieu(cur)} và {MoTaKieu(rhs)}");
    }

    private void ThucHienGanIndex(IndexAssignStmt ix, PhamVi env)
    {
        int dong = ix.Dong;
        object obj = Eval(ix.Obj, env, dong);
        object giaTri = Eval(ix.Value, env, dong);
        if (obj is DictValue dict)
        {
            object key = Eval(ix.Index, env, dong);
            if (key is not string ks)
                throw Loi(dong, $"chỉ số dict phải là chuỗi, nhận {MoTaKieu(key)}");
            dict.Pairs[ks] = giaTri;
        }
        else if (obj is List<object> arr)
        {
            int i = ChiSoNguyen(Eval(ix.Index, env, dong), dong);
            if (i < 0 || i >= arr.Count)
                throw Loi(dong, $"chỉ số {i} ngoài phạm vi mảng (0..{arr.Count - 1})");
            arr[i] = giaTri;
        }
        else
        {
            throw Loi(dong, $"không đánh chỉ số được kiểu {MoTaKieu(obj)}");
        }
    }

    // ─── BIỂU THỨC ─────────────────────────────────────────────────

    private object Eval(Expr e, PhamVi env, int dong)
    {
        switch (e)
        {
            case NumLit n: return n.Value;
            case StrLit s: return s.Value;
            case BoolLit b: return b.Value;
            case NullLit: return null;
            case ArrayLit arr: return arr.Items.Select(x => Eval(x, env, dong)).ToList();
            case DictLit dict:
                var dp = new DictValue();
                foreach (var (k, v) in dict.Entries)
                {
                    object key = Eval(k, env, dong);
                    if (key is not string ks)
                        throw Loi(dong, $"key trong dict phải là chuỗi, nhận {MoTaKieu(key)}");
                    dp.Pairs[ks] = Eval(v, env, dong);
                }
                return dp;
            case NameExpr name:
                if (env.Lay(name.Name, out var gtri)) return gtri;
                throw Loi(dong, $"biến '{name.Name}' chưa được gán");
            case ThisExpr:
                if (env.Lay("this", out var thisVal)) return thisVal;
                throw Loi(dong, "'this' ngoài phương thức");
            case UnaryExpr u: return EvalUnary(u, env, dong);
            case BinaryExpr b: return EvalBinary(b, env, dong);
            case CallExpr c: return EvalCall(c, env, dong);
            case GetExpr g: return EvalGet(g, env, dong);
            case IndexExpr ix: return EvalIndex(ix, env, dong);
            case FuncExpr fe: return new FunctionValue(fe.Decl, env);
            default: throw Loi(dong, "biểu thức không xác định");
        }
    }

    private object EvalUnary(UnaryExpr u, PhamVi env, int dong)
    {
        if (u.Op == "KHONG")
            return !Chantri(Eval(u.Operand, env, dong));
        if (u.Op == "TRU")
        {
            object v = Eval(u.Operand, env, dong);
            if (!(v is double d)) throw Loi(dong, "phép toán '-' yêu cầu số");
            return -d;
        }
        throw Loi(dong, $"toán tử một ngôi '{u.Op}' không xác định");
    }

    private object EvalBinary(BinaryExpr b, PhamVi env, int dong)
    {
        object l = Eval(b.Left, env, dong);

        if (b.Op == "VA")
        {
            if (!Chantri(l)) return false;
            return Chantri(Eval(b.Right, env, dong));
        }
        if (b.Op == "HOAC")
        {
            if (Chantri(l)) return true;
            return Chantri(Eval(b.Right, env, dong));
        }

        object r = Eval(b.Right, env, dong);

        switch (b.Op)
        {
            case "CONG":
                if (l is double ld && r is double rd) return ld + rd;
                if (l is string ls && r is string rs) return ls + rs;
                throw Loi(dong, $"không cộng được {ChuoiHoa(l)} và {ChuoiHoa(r)}");
            case "TRU":
                if (l is double lt && r is double rt) return lt - rt;
                throw Loi(dong, $"phép toán '-' chỉ áp dụng cho số");
            case "NHAN":
                if (l is double ln && r is double rn) return ln * rn;
                throw Loi(dong, $"phép toán '*' chỉ áp dụng cho số");
            case "CHIA":
                if (l is double lc && r is double rc)
                {
                    if (rc == 0.0) throw Loi(dong, "chia cho số 0");
                    return lc / rc;
                }
                throw Loi(dong, $"phép toán '/' chỉ áp dụng cho số");
            case "CHIA_DU":
                if (l is double lm && r is double rm)
                {
                    if (rm == 0.0) throw Loi(dong, "chia cho số 0");
                    return lm % rm;
                }
                throw Loi(dong, $"phép toán '%' chỉ áp dụng cho số");
            case "SO_SANH_BANG": return BangBang(l, r);
            case "KHAC": return !BangBang(l, r);
            case "NHO_HON":
                if (l is double lx && r is double rx) return lx < rx;
                throw Loi(dong, "chỉ so sánh được số với số");
            case "LON_HON":
                if (l is double ly && r is double ry) return ly > ry;
                throw Loi(dong, "chỉ so sánh được số với số");
            case "NHO_HON_HOAC_BANG":
                if (l is double lz && r is double rz) return lz <= rz;
                throw Loi(dong, "chỉ so sánh được số với số");
            case "LON_HON_HOAC_BANG":
                if (l is double lw && r is double rw) return lw >= rw;
                throw Loi(dong, "chỉ so sánh được số với số");
        }
        throw Loi(dong, $"toán tử '{b.Op}' không xác định");
    }

    private static bool BangBang(object l, object r)
    {
        if (l == null && r == null) return true;
        if (l == null || r == null) return false;
        if (IsDouble(l) && IsDouble(r)) return (double)l == (double)r;
        if (l is string sl && r is string sr) return sl == sr;
        if (l is bool bl && r is bool br) return bl == br;
        return ReferenceEquals(l, r);
    }

    private static bool IsDouble(object o) => o is double;

    private int ChiSoNguyen(object v, int dong)
    {
        if (!(v is double d) || Math.Floor(d) != d)
            throw Loi(dong, "chỉ số phải là số nguyên");
        return (int)d;
    }

    private object EvalIndex(IndexExpr ix, PhamVi env, int dong)
    {
        object obj = Eval(ix.Obj, env, dong);

        if (obj is DictValue dict)
        {
            object key = Eval(ix.Index, env, dong);
            if (key is not string ks)
                throw Loi(dong, $"chỉ số dict phải là chuỗi, nhận {MoTaKieu(key)}");
            if (dict.Pairs.TryGetValue(ks, out var val)) return val;
            return null;
        }

        int i = ChiSoNguyen(Eval(ix.Index, env, dong), dong);

        if (obj is List<object> list)
        {
            if (i < 0 || i >= list.Count)
                throw Loi(dong, $"chỉ số {i} ngoài phạm vi mảng (0..{list.Count - 1})");
            return list[i];
        }
        if (obj is string s)
        {
            if (i < 0 || i >= s.Length)
                throw Loi(dong, $"chỉ số {i} ngoài phạm vi chuỗi (0..{s.Length - 1})");
            return s.Substring(i, 1);
        }
        throw Loi(dong, $"không đánh chỉ số được kiểu {MoTaKieu(obj)}");
    }

    private object EvalGet(GetExpr g, PhamVi env, int dong)
    {
        object obj = Eval(g.Obj, env, dong);
        if (obj is InstanceValue inst)
        {
            if (inst.Fields.TryGetValue(g.Name, out var fld)) return fld;
            if (inst.Lop.Methods.TryGetValue(g.Name, out var met))
                return new BoundMethod(inst, met);
            throw Loi(dong, $"thuộc tính '{g.Name}' không tồn tại");
        }
        if (obj is DictValue dict)
        {
            return new DictMethodValue(dict, g.Name);
        }
        if (obj is string || obj is List<object>)
        {
            return new BoundMethodValue(obj, g.Name);
        }
        throw Loi(dong, $"kiểu {MoTaKieu(obj)} không có thuộc tính '{g.Name}'");
    }

    private object EvalCall(CallExpr c, PhamVi env, int dong)
    {
        object callee = Eval(c.Callee, env, dong);
        var args = c.Args.Select(a => Eval(a, env, dong)).ToList();
        return Goi(callee, args, dong);
    }

    /// <summary>Gọi một giá trị (hàm/lớp/bound method/builtin).</summary>
    private object Goi(object callee, List<object> args, int dong)
    {
        switch (callee)
        {
            case FunctionValue fn: return GoiHam(fn, null, args, dong);
            case BuiltinValue b: return b.Ham(this, args, dong);
            case ClassValue cls:
                var inst = new InstanceValue(cls);
                if (cls.Methods.TryGetValue("khởi_tạo", out var ctor))
                    GoiHam(ctor, inst, args, dong);
                return inst;
            case BoundMethod bm: return GoiHam(bm.Method, bm.Instance, args, dong);
            case DictMethodValue dm: return GoiDictMethod(dm, args, dong);
            case BoundMethodValue bmv: return GoiBoundMethodValue(bmv, args, dong);
            default:
                throw Loi(dong, $"'{ChuoiHoa(callee)}' không phải hàm");
        }
    }

    private object GoiDictMethod(DictMethodValue dm, List<object> args, int dong)
    {
        var dict = dm.Dict;
        switch (dm.Ten)
        {
            case "có":
                if (args.Count != 1) throw Loi(dong, $"hàm 'có' cần 1 tham số, nhận {args.Count}");
                if (args[0] is not string kCo) throw Loi(dong, "tham số của 'có' phải là chuỗi");
                return dict.Pairs.ContainsKey(kCo);
            case "lấy":
                if (args.Count != 1) throw Loi(dong, $"hàm 'lấy' cần 1 tham số, nhận {args.Count}");
                if (args[0] is not string kLay) throw Loi(dong, "tham số của 'lấy' phải là chuỗi");
                return dict.Pairs.TryGetValue(kLay, out var vLay) ? vLay : null;
            case "xóa":
                if (args.Count != 1) throw Loi(dong, $"hàm 'xóa' cần 1 tham số, nhận {args.Count}");
                if (args[0] is not string kXoa) throw Loi(dong, "tham số của 'xóa' phải là chuỗi");
                if (dict.Pairs.TryGetValue(kXoa, out var vXoa)) { dict.Pairs.Remove(kXoa); return vXoa; }
                return null;
            case "tất_cả":
                if (args.Count != 0) throw Loi(dong, $"hàm 'tất_cả' cần 0 tham số, nhận {args.Count}");
                return dict.Pairs.Keys.ToList<object>();
            case "kích_thước":
                if (args.Count != 0) throw Loi(dong, $"hàm 'kích_thước' cần 0 tham số, nhận {args.Count}");
                return (double)dict.Pairs.Count;
            default:
                throw Loi(dong, $"dict không có method '{dm.Ten}'");
        }
    }

    private object GoiBoundMethodValue(BoundMethodValue bmv, List<object> args, int dong)
    {
        if (bmv.Obj is string s)
        {
            switch (bmv.Ten)
            {
                case "tìm":
                    if (args.Count != 1) throw Loi(dong, $"hàm 'tìm' cần 1 tham số, nhận {args.Count}");
                    if (args[0] is not string sub) throw Loi(dong, "tham số của 'tìm' phải là chuỗi");
                    return (double)s.IndexOf(sub);
                case "thay":
                    if (args.Count != 2) throw Loi(dong, $"hàm 'thay' cần 2 tham số, nhận {args.Count}");
                    if (args[0] is not string oldS) throw Loi(dong, "tham số 1 của 'thay' phải là chuỗi");
                    if (args[1] is not string newS) throw Loi(dong, "tham số 2 của 'thay' phải là chuỗi");
                    return s.Replace(oldS, newS);
                case "cắt":
                    if (args.Count != 0) throw Loi(dong, $"hàm 'cắt' cần 0 tham số, nhận {args.Count}");
                    return s.Trim();
                case "chứa":
                    if (args.Count != 1) throw Loi(dong, $"hàm 'chứa' cần 1 tham số, nhận {args.Count}");
                    if (args[0] is not string subC) throw Loi(dong, "tham số của 'chứa' phải là chuỗi");
                    return s.Contains(subC);
                case "phân_tách":
                    if (args.Count != 1) throw Loi(dong, $"hàm 'phân_tách' cần 1 tham số, nhận {args.Count}");
                    if (args[0] is not string delim) throw Loi(dong, "tham số của 'phân_tách' phải là chuỗi");
                    return s.Split(new[] { delim }, StringSplitOptions.None).ToList<object>();
                default:
                    throw Loi(dong, $"chuỗi không có method '{bmv.Ten}'");
            }
        }
        if (bmv.Obj is List<object> arr)
        {
            switch (bmv.Ten)
            {
                case "lọc":
                    if (args.Count != 0) throw Loi(dong, $"hàm 'lọc' cần 0 tham số, nhận {args.Count}");
                    return arr.Where(x => Chantri(x)).ToList();
                case "map":
                    if (args.Count != 1) throw Loi(dong, $"hàm 'map' cần 1 tham số, nhận {args.Count}");
                    if (args[0] is not FunctionValue fnMap) throw Loi(dong, "tham số của 'map' phải là hàm");
                    return arr.Select(x => GoiHam(fnMap, null, new List<object> { x }, dong)).ToList();
                case "gộp":
                    if (args.Count != 1) throw Loi(dong, $"hàm 'gộp' cần 1 tham số, nhận {args.Count}");
                    if (args[0] is not List<object> other) throw Loi(dong, "tham số của 'gộp' phải là mảng");
                    return arr.Concat(other).ToList();
                default:
                    throw Loi(dong, $"mảng không có method '{bmv.Ten}'");
            }
        }
        throw Loi(dong, $"kiểu {MoTaKieu(bmv.Obj)} không có method '{bmv.Ten}'");
    }

    private object GoiHam(FunctionValue fn, InstanceValue thisVal, List<object> args, int dong)
    {
        int n = fn.KhaiBao.Params.Count;
        if (args.Count != n)
            throw Loi(dong, $"hàm '{fn.KhaiBao.Name}' cần {n} tham số, nhận {args.Count}");

        var local = new PhamVi(fn.Closure);
        if (thisVal != null) local.GanDay("this", thisVal);
        for (int i = 0; i < n; i++)
            local.GanDay(fn.KhaiBao.Params[i], args[i]);

        try
        {
            ThucHien(fn.KhaiBao.Body, local);
        }
        catch (ReturnSignal r)
        {
            return r.Value;
        }
        catch (BreakSignal)
        {
            throw Loi(dong, "dừng nằm ngoài vòng lặp");
        }
        catch (ContinueSignal)
        {
            throw Loi(dong, "tiếp nằm ngoài vòng lặp");
        }
        return null;
    }

    // ─── CHÂN TRỊ & CHUỖI HOÁ ──────────────────────────────────────

    private static bool Chantri(object v)
    {
        if (v == null) return false;
        if (v is bool b) return b;
        if (v is double d) return d != 0.0;
        if (v is string s) return s.Length > 0;
        if (v is List<object> l) return l.Count > 0;
        if (v is DictValue dv) return dv.Pairs.Count > 0;
        return true;
    }

    /// <summary>'rỗng' nếu null (chỉ cho đối tượng).</summary>
    private static string MoTaKieu(object v)
    {
        if (v is double) return "số";
        if (v is string) return "chuỗi";
        if (v is bool) return "đúng/sai";
        if (v is List<object>) return "mảng";
        if (v is DictValue) return "dict";
        if (v is FunctionValue) return "hàm";
        if (v is ClassValue) return "lớp";
        if (v is InstanceValue) return "đối tượng";
        if (v is BuiltinValue) return "hàm";
        if (v == null) return "rỗng";
        return "giá trị";
    }

    /// <summary>Stringify giá trị cho in_ra/chuyển_chuỗi/thông báo lỗi.</summary>
    public static string ChuoiHoa(object v)
    {
        if (v == null) return "rỗng";
        if (v is double d) return So(d);
        if (v is string s) return s;
        if (v is bool b) return b ? "đúng" : "sai";
        if (v is List<object> list)
            return "[" + string.Join(", ", list.Select(ChuoiHoa)) + "]";
        if (v is DictValue dict)
        {
            var pairs = dict.Pairs.Select(kv => $"\"{kv.Key}\": {ChuoiHoa(kv.Value)}");
            return "{" + string.Join(", ", pairs) + "}";
        }
        return v.ToString();
    }
}