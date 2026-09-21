using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VietLang;

[StdlibModule]
public static class TestStdlibIntegrity
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "stdlib_integrity_sach", Run = TestSach },
        new LexTest { Name = "stdlib_integrity_bat_loi", Run = TestBatLoi },
    };

    private static readonly string[] AllowList =
    {
        "TestAugAssign", "TestDict", "TestEval", "TestException", "TestGolden",
        "TestInterpreter", "TestLexer", "TestMath", "TestMethods", "TestModule",
        "TestParser", "TestReplBuiltins", "TestReplBuiltins2", "TestSessionBuiltins", "TestVietNamese",
    };

    public static List<string> KiemTra(IEnumerable<Type> types, IReadOnlyCollection<string> allowList)
    {
        var viPham = new List<string>();
        var coAttr = new HashSet<string>();
        foreach (var t in types)
        {
            bool hasAttr = t.GetCustomAttribute<StdlibModuleAttribute>() != null;
            if (hasAttr) coAttr.Add(t.Name);

            var getAll = t.GetMethod("GetAll", BindingFlags.Public | BindingFlags.Static,
                null, Type.EmptyTypes, null);
            if (getAll != null && getAll.ReturnType == typeof(List<LexTest>)
                && !hasAttr && !allowList.Contains(t.Name))
                viPham.Add(t.Name + ": GetAll() thiếu [StdlibModule]");

            var dangKy = t.GetMethod("DangKy", BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(PhamVi) }, null);
            if (dangKy != null && !hasAttr && t.Name != "Builtins")
                viPham.Add(t.Name + ": DangKy() thiếu [StdlibModule]");
        }

        foreach (var name in allowList)
            if (coAttr.Contains(name))
                viPham.Add(name + ": vừa trong allowList vừa có [StdlibModule]");

        return viPham;
    }

    private static bool TestSach()
    {
        var types = typeof(TestStdlibIntegrity).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsNested);
        var viPham = KiemTra(types, AllowList);
        if (viPham.Count != 0)
            throw new Exception("Vi phạm: " + string.Join("; ", viPham));
        return true;
    }

    private static bool TestBatLoi()
    {
        var viPham = KiemTra(new[] { typeof(ViDuXau) }, new string[0]);
        if (viPham.Count == 0)
            throw new Exception("Guard không cắn: ViDuXau thiếu [StdlibModule] nhưng KiemTra trả rỗng");
        return true;
    }

    public static class ViDuXau
    {
        public static List<LexTest> GetAll() => new List<LexTest>();
    }
}
