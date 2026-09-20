using System.Collections.Generic;

namespace VietLang;

/// <summary>AST data-only: chỉ chứa dữ liệu, không chứa logic. T2 (parser) tạo, T3 (interpreter) tiêu thụ.</summary>

public sealed class Program
{
    public List<Stmt> Stmts { get; set; } = new();

    public Program() { }
    public Program(List<Stmt> stmts) { Stmts = stmts; }
}

public abstract class Stmt
{
    /// <summary>Số dòng (1-based) của từ khóa/token đầu tiên của lệnh trong mã nguồn; 0 nếu chưa gán.</summary>
    public int Dong { get; set; }
}

public sealed class VarDeclStmt : Stmt
{
    public string Name { get; set; }
    public Expr Init { get; set; }

    public VarDeclStmt() { }
    public VarDeclStmt(string name, Expr init = null) { Name = name; Init = init; }
}

public sealed class AssignStmt : Stmt
{
    public Expr Target { get; set; }
    public Expr Value { get; set; }

    public AssignStmt() { }
    public AssignStmt(Expr target, Expr value) { Target = target; Value = value; }
}

public sealed class AugAssignStmt : Stmt
{
    public NameExpr Target { get; set; }
    public Expr Value { get; set; }

    public AugAssignStmt() { }
    public AugAssignStmt(NameExpr target, Expr value) { Target = target; Value = value; }
}

public sealed class ExprStmt : Stmt
{
    public Expr Expr { get; set; }

    public ExprStmt() { }
    public ExprStmt(Expr expr) { Expr = expr; }
}

public sealed class IfStmt : Stmt
{
    public Expr Cond { get; set; }
    public List<Stmt> Then { get; set; } = new();
    public List<Stmt> Otherwise { get; set; }

    public IfStmt() { }
    public IfStmt(Expr cond, List<Stmt> then, List<Stmt> otherwise = null)
    {
        Cond = cond;
        Then = then;
        Otherwise = otherwise;
    }
}

public sealed class WhileStmt : Stmt
{
    public Expr Cond { get; set; }
    public List<Stmt> Body { get; set; } = new();

    public WhileStmt() { }
    public WhileStmt(Expr cond, List<Stmt> body) { Cond = cond; Body = body; }
}

public sealed class ForInStmt : Stmt
{
    public string Var { get; set; }
    public Expr Iterable { get; set; }
    public List<Stmt> Body { get; set; } = new();

    public ForInStmt() { }
    public ForInStmt(string v, Expr iterable, List<Stmt> body) { Var = v; Iterable = iterable; Body = body; }
}

public sealed class FuncDeclStmt : Stmt
{
    public string Name { get; set; }
    public List<string> Params { get; set; } = new();
    public List<Stmt> Body { get; set; } = new();

    public FuncDeclStmt() { }
    public FuncDeclStmt(string name, List<string> @params, List<Stmt> body)
    {
        Name = name;
        Params = @params;
        Body = body;
    }
}

public sealed class ClassDeclStmt : Stmt
{
    public string Name { get; set; }
    public List<FuncDeclStmt> Methods { get; set; } = new();

    public ClassDeclStmt() { }
    public ClassDeclStmt(string name, List<FuncDeclStmt> methods) { Name = name; Methods = methods; }
}

public sealed class ReturnStmt : Stmt
{
    public Expr Value { get; set; }

    public ReturnStmt() { }
    public ReturnStmt(Expr value) { Value = value; }
}

public sealed class BreakStmt : Stmt { }

public sealed class ContinueStmt : Stmt { }

public sealed class TryStmt : Stmt
{
    public List<Stmt> Body { get; set; } = new();
    public string CatchVar { get; set; }
    public List<Stmt> CatchBody { get; set; }
    public List<Stmt> FinallyBody { get; set; }

    public TryStmt() { }
    public TryStmt(List<Stmt> body, string catchVar, List<Stmt> catchBody, List<Stmt> finallyBody)
    {
        Body = body;
        CatchVar = catchVar;
        CatchBody = catchBody;
        FinallyBody = finallyBody;
    }
}

public sealed class RaiseStmt : Stmt
{
    public Expr Message { get; set; }

    public RaiseStmt() { }
    public RaiseStmt(Expr message) { Message = message; }
}

public sealed class ImportStmt : Stmt
{
    public Expr Path { get; set; }

    public ImportStmt() { }
    public ImportStmt(Expr path) { Path = path; }
}

public abstract class Expr { }

public sealed class NumLit : Expr
{
    public double Value { get; set; }

    public NumLit() { }
    public NumLit(double value) { Value = value; }
}

public sealed class StrLit : Expr
{
    public string Value { get; set; }

    public StrLit() { }
    public StrLit(string value) { Value = value; }
}

public sealed class BoolLit : Expr
{
    public bool Value { get; set; }

    public BoolLit() { }
    public BoolLit(bool value) { Value = value; }
}

public sealed class NullLit : Expr
{
    public NullLit() { }
}

public sealed class ArrayLit : Expr
{
    public List<Expr> Items { get; set; } = new();

    public ArrayLit() { }
    public ArrayLit(List<Expr> items) { Items = items; }
}

public sealed class DictLit : Expr
{
    public List<(Expr Key, Expr Value)> Entries { get; set; } = new();

    public DictLit() { }
    public DictLit(List<(Expr Key, Expr Value)> entries) { Entries = entries; }
}

public sealed class NameExpr : Expr
{
    public string Name { get; set; }

    public NameExpr() { }
    public NameExpr(string name) { Name = name; }
}

public sealed class ThisExpr : Expr { }

public sealed class BinaryExpr : Expr
{
    public string Op { get; set; }
    public Expr Left { get; set; }
    public Expr Right { get; set; }

    public BinaryExpr() { }
    public BinaryExpr(string op, Expr left, Expr right) { Op = op; Left = left; Right = right; }
}

public sealed class UnaryExpr : Expr
{
    public string Op { get; set; }
    public Expr Operand { get; set; }

    public UnaryExpr() { }
    public UnaryExpr(string op, Expr operand) { Op = op; Operand = operand; }
}

public sealed class CallExpr : Expr
{
    public Expr Callee { get; set; }
    public List<Expr> Args { get; set; } = new();

    public CallExpr() { }
    public CallExpr(Expr callee, List<Expr> args) { Callee = callee; Args = args; }
}

public sealed class GetExpr : Expr
{
    public Expr Obj { get; set; }
    public string Name { get; set; }

    public GetExpr() { }
    public GetExpr(Expr obj, string name) { Obj = obj; Name = name; }
}

public sealed class FuncExpr : Expr
{
    public FuncDeclStmt Decl { get; set; }

    public FuncExpr() { }
    public FuncExpr(FuncDeclStmt decl) { Decl = decl; }
}

public sealed class IndexExpr : Expr
{
    public Expr Obj { get; set; }
    public Expr Index { get; set; }

    public IndexExpr() { }
    public IndexExpr(Expr obj, Expr index) { Obj = obj; Index = index; }
}

public sealed class IndexAssignStmt : Stmt
{
    public Expr Obj { get; set; }
    public Expr Index { get; set; }
    public Expr Value { get; set; }

    public IndexAssignStmt() { }
    public IndexAssignStmt(Expr obj, Expr index, Expr value) { Obj = obj; Index = index; Value = value; }
}