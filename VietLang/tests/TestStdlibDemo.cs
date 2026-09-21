using System;
using System.IO;

namespace VietLang;

[StdlibModule]
public static class TestStdlibDemo
{
    public static List<LexTest> GetAll() => new List<LexTest>
    {
        new LexTest { Name = "stdlib_demo_phien_ban", Run = TestPhienBan },
    };

    private static string Chay(string src)
    {
        var cu = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try { new Interpreter().Run(Parser.Parse(src)); }
        finally { Console.SetOut(cu); }
        return sw.ToString().Replace("\r\n", "\n").Trim('\n');
    }

    private static bool TestPhienBan()
    {
        string o = Chay("in_ra(phien_ban())");
        if (!o.Contains("VietLang v0.2.0")) throw new Exception("mong 'VietLang v0.2.0', nhan '" + o + "'");
        return true;
    }
}
