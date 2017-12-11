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
    static bool convert_main(string input, out string output)
    {
        int start_idx = -1;
        int end_idx   = -1;
        output = string.Empty;

        var find = find_target(input, out start_idx, out end_idx); //マークのペアを抜き出す
        if (string.IsNullOrEmpty(find)) return false;              //無い

        var args = get_args(find);
        var val = string.Empty;

        

        val  = getDicValue(args[0]);
        if(args.Length > 1) //引数あり
        {
            for(var n = 1;n < args.Length;n++)
            {
                var a = args[n];
                val   = Replace(val,n-1,a);
            }
        }

        if (val.IndexOf(BM)>=0)
        {
            val = Convert(val); //再変換　　再帰的に
        }

        output  = input.Substring(0,start_idx);
        output += val;
        if (end_idx < input.Length)
        {
            output += input.Substring(end_idx+1);
        }
        return true;
    }
}
