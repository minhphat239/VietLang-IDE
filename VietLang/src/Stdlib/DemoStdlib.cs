namespace VietLang;

[StdlibModule]
public static class DemoStdlib
{
    public static void DangKy(PhamVi global)
    {
        global.GanDay("phien_ban", new BuiltinValue("phien_ban", (i, a, d) => "VietLang v0.2.0"));
    }
}
