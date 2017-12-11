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
    /*
        他の言語内でも使えるように
        開始 #:
        終了 :#

        idのみ    #:hoge:#
        引数あり  #:include("file")#
        改行も許す #:hoge(
                     vvv,
                     ggg
                   ):#
        コメント機能
        　　　C系のコメントを内包可能
               #:hoge ／＊コメント＊／ :#
               #:hohe ／／コメント
               :#

        予約語
        include - ファイルを読込む

        マクロ機能
        変換文字列の{0},{1}等が引数に変換される。
        {~0}とすると、引数のダブルクォートが外される

        高速化のためConvert部分を再帰的に使う
    */
    static string BM = "#:";
    static string EM = ":#";
    
    private static  string getDicValue(string id) { return DefineConverter.Program.m_defdic.GetValue(id);  }
    public static string Convert(string input)
    {
        var str = input;
        bool b=true;
        while(b)
        {
            string newstr;
            b = convert_main(str,out newstr);
            if (!b) break;
            str = newstr;
        }

        return str;
    }
}
