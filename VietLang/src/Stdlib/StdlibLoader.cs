using System;
using System.Collections.Generic;
using System.Reflection;

namespace VietLang;

public static class StdlibLoader
{
    public static void DangKyTatCa(PhamVi global)
    {
        var asm = typeof(StdlibLoader).Assembly;
        foreach (var type in asm.GetTypes())
        {
            if (!type.IsClass) continue;
            if (type.GetCustomAttribute<StdlibModuleAttribute>() == null) continue;
            var method = type.GetMethod("DangKy", BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(PhamVi) }, null);
            if (method == null) continue;
            method.Invoke(null, new object[] { global });
        }
    }

    public static List<LexTest> LayTatCaTest()
    {
        var ketQua = new List<LexTest>();
        var asm = typeof(StdlibLoader).Assembly;
        foreach (var type in asm.GetTypes())
        {
            if (type == typeof(StdlibLoader)) continue;
            if (!type.IsClass) continue;
            if (type.GetCustomAttribute<StdlibModuleAttribute>() == null) continue;
            var method = type.GetMethod("GetAll", BindingFlags.Public | BindingFlags.Static,
                null, Type.EmptyTypes, null);
            if (method == null) continue;
            if (method.ReturnType != typeof(List<LexTest>)) continue;
            if (method.Invoke(null, null) is List<LexTest> ds)
                ketQua.AddRange(ds);
        }
        return ketQua;
    }
}
