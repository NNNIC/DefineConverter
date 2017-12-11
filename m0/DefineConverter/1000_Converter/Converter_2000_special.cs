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
    static bool convert_special(string[] args, out string val)
    {
        val = string.Empty;
        if (args==null || args.Length==0) return false;
        var w = args[0];
        switch(w)
        {
        case "include": return convert_include(args, out val);
        }
        return false;
    }

}
