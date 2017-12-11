//<<<include=using_text.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
//>>>


public partial class Converter
{
    static bool convert_include(string[] args, out string val)
    {
        val = string.Empty;
        if (args==null || args.Length==0) return false;
        var w = args[0];
        if (w!="include") return false;
        if (args.Length<2) return false;

        var filepath = Path.Combine( Environment.CurrentDirectory, args[1]);

        val = File.ReadAllText(filepath, Encoding.GetEncoding( DefineConverter.Program.m_encode) );
        for(var i = 2; i<args.Length; i++)
        {
            val = Replace(val,i-2,args[i]);
        }

        return true;
    }

}
