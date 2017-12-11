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
        find_target

        1. 入力テキストの現在地から BMを探し出す
        　　見つかったら idx_bm = 現在値
        
        2. 一時現在地を idx_bm + sizeof(BM)とする

        3. idx_bmを要素ごとに読み出す
      
            EMがあった 　　=> idx_em       idx_bmとidx_emで内部要素が決定　－－－－－－－－終了
            //があった　   => 次の行の先頭まで 一時現在地を更新
            ／＊があった   => 次の ＊／まで  一時現在地を更新
            BMがあった　   => 一時現在地を現在値として、idx_bm＝現在値 として 2へ

    */
    static string find_target(string input, out int start_idx, out int end_idx)
    {
        start_idx = -1;
        end_idx   = -1;

        if (string.IsNullOrEmpty(input)) return null;

        var beginmarkidx   = input.IndexOf(EM);
        if (beginmarkidx<0)
        {
            return null; //対象はない
        }

        var tmp = beginmarkidx + BM.Length;
        while(tmp < input.Length)
        {
            //check
            var ck_em_idx = input.IndexOf(EM,tmp);
            var ck_cl_idx = input.IndexOf("//",tmp);
            var ck_cm_idx = input.IndexOf("/*",tmp);
            var ck_bm_idx = input.IndexOf(BM,tmp);
            
            //    上記で一番手前にあるのは？
            var minidx = int.MaxValue;
            var minid  = string.Empty; //種別
            if (ck_em_idx >=tmp && ck_em_idx < minidx) { minidx = ck_em_idx;  minid = "em"; }
            if (ck_cl_idx >=tmp && ck_cl_idx < minidx) { minidx = ck_cl_idx;  minid = "cl"; }
            if (ck_cm_idx >=tmp && ck_cm_idx < minidx) { minidx = ck_cm_idx;  minid = "cm"; }
            if (ck_bm_idx >=tmp && ck_bm_idx < minidx) { minidx = ck_bm_idx;  minid = "bm"; }

            //種別未判定であればエラー
            if (minidx == int.MaxValue)
            {
                throw new SystemException("ERROR CANNOT FIND END MARK");
            }
            if (minid == "em")//内部要素が決定
            {
                var val = input.Substring(beginmarkidx+BM.Length,ck_em_idx - beginmarkidx - BM.Length);
                start_idx = beginmarkidx;
                end_idx   = ck_em_idx + EM.Length;
                return val;
            }
            if (minid == "cl")//以降はコメント行
            {
                for(;tmp < input.Length; tmp++)
                {
                    var c = input[tmp];
                    if (c=='\x0d' || c=='\x0a')
                    {
                        if (tmp+1<input.Length)
                        {
                            c = input[tmp+1];//改行コード２文字の場合
                            if (c=='\x0d' || c=='\x0a')
                            {
                                tmp = tmp+1;
                            }
                        }
                        continue;
                    }
                }
            }
            if (minid == "cm")//コメント要素
            {
                tmp+=2;
                var cmend_idx = input.IndexOf("*/",tmp);
                if (cmend_idx>0)
                {
                    tmp = cmend_idx + 2;
                    continue;
                }
                throw new SystemException("ERROR CANNOT FIND END OF COMMENT");
            }
            if (minid == "bm")
            {
                tmp = ck_bm_idx + BM.Length;
                continue;
            }
        }
        return null;
    }

    /// <summary>
    /// ターゲットの文字列を、ID名と引数に分解する
    /// args[0] は ID名
    /// フォーマットに合わないものはエラー
    /// format   id
    ///          id()
    ///          id(a1,a2,..)
    /// 
    /// 空白、
    /// コメント削除 行と／＊があるので注意
    /// ダブルクォートで囲まれた文字列を考慮せよ
    /// </summary>
    static string[] get_args(string target)
    {
        if (string.IsNullOrEmpty(target) ) return null;

        var target2 = target.Trim();
        if (string.IsNullOrEmpty(target2) ) return null;

        //要素分解
        var tokens = analize(target2);
        if (tokens==null || tokens.Length==0) return null;
        
        //コメントの削除
        var tokens2 = _delete_comment(tokens);
        if (tokens2==null || tokens2.Length==0) return null;

        //スペースの削除
        var tokens3 = _delete_comment(tokens2);
        if (tokens3==null || tokens3.Length==0) return null;

        //arg0
        Func<int,TokenItem> gi = (n)=> {
            return (n < tokens3.Length) ? tokens3[n] : null;
        };
        Func<int,TOKENGROUP> gg = (n)=> {
            var i = gi(n);
            return (i!=null) ? i.group : TOKENGROUP.NONE;
        };
        Func<int,string> gv = (n)=> {
            var i = gi(n);
            return (i!=null) ? i.val : null;
        };

        if (gg(0)!= TOKENGROUP.WORD)
        {
            throw new SystemException("ERROR UNEXPECTED FORMAT");
        }
        if (gg(1) == TOKENGROUP.END || (gg(1)==TOKENGROUP.BRACKETS_OPEN && gg(2)== TOKENGROUP.BRACKETS_CLOSE) )
        {
            return new string[1] { gv(0) };
        }
        if (gg(1)==TOKENGROUP.COMMENT_OPEN)
        {
            var arglist = new List<string>();
            arglist.Add( gv(0) );

            for(var i = 2; i<tokens3.Length;i++)
            {
                var arg = gv(i);
                var ng  = gg(i+1);
                var ng2 = gg(i+2);
                if (ng == TOKENGROUP.BRACKETS_CLOSE && ng2== TOKENGROUP.END) //最後 
                {
                    arglist.Add(arg);
                    return arglist.ToArray();
                }
                if (ng == TOKENGROUP.COMMA)
                {
                    arglist.Add(arg);
                    i++;
                    continue;
                }
                throw new SystemException("ERROR UNEXPECTED");
            }
        }
        return null;
    }

    //字句分解
    public enum TOKENGROUP
    {
        NONE,

        SPACE,
        WORD,
        DQSTRING,

        BRACKETS_OPEN,  //(
        BRACKETS_CLOSE, //)

        COMMA,         //,

        COMMENT_OPEN,   //／＊
        COMMENT_CLOSE,  //＊／

        COMMENT_LINE,   // ／／

        NEWLINE,
        END
    }
    public class TokenItem
    {
        public TOKENGROUP group;
        public string     val;
    }
    static TokenItem[] analize(string input)
    {
        Func<string,string,string> _getmatch = (rv,s)=> {
            var r = new Regex(rv);
            if (!r.IsMatch(s)) return null;
            foreach(var i in r.Matches(s))
            {
                var m = (Match)i;
                return m.Value;
            }
            return null;
        };


        List<TokenItem> toklist = new List<TokenItem>();
        for(var i = 0; i < input.Length; i++)
        {
            var c1 = input[i];
            var c2 = i+1 < input.Length ? input[i+1] : '\x00';
            if (c1==' ' || c1=='\t')//スペース
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.SPACE;
                item.val   = _getmatch(@"^[\s]+",input.Substring(i));
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
            if (c1==',') //コンマ
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.SPACE;
                item.val   = ",";
                toklist.Add(item);
                continue;
            }
            if (c1=='\"')//ダブルクォート
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.DQSTRING;
                item.val   = _getmatch("^\"[^\"]*\"",input.Substring(i));
                toklist.Add(item);
                i+=item.val.Length -1;
                continue;
            }
            if (c1=='(')
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.BRACKETS_OPEN;
                item.val   = "(";
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
            if (c1==')')
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.BRACKETS_CLOSE;
                item.val   = ")";
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
            if (c1=='/' && c2=='*')
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.COMMENT_OPEN;
                item.val   = "/*";
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
            if (c1=='*' && c2=='/')
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.COMMENT_CLOSE;
                item.val   = "*/";
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
            if (c1=='/' && c2=='/')
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.COMMENT_LINE;
                item.val   = "//";
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
            if (c1=='\x0d' || c1=='\x0a')
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.NEWLINE;
                item.val = _getmatch(@"^[\x0d\x0a]+",input.Substring(i));
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
            // else
            {
                var item = new TokenItem();
                item.group = TOKENGROUP.WORD;
                item.val   = _getmatch(@"^[a-zA-Z0-9_@$.]",input.Substring(i));
                if (string.IsNullOrEmpty(item.val))
                {
                    throw new SystemException("ERROR Unexpected characters");
                }
                toklist.Add(item);
                i+=item.val.Length - 1;
                continue;
            }
        }
        
        toklist.Add( new TokenItem() { group = TOKENGROUP.END, val = null });

        return toklist.ToArray();
    }

    static TokenItem[] _delete_comment(TokenItem[] tokens)
    {
        var newtokens = new List<TokenItem>();
        var type = 0; // 1 - comment element 2 -- comment line
        foreach(var tok in tokens)
        {
            if (type == 0)
            {
                if (tok.group == TOKENGROUP.COMMENT_OPEN)
                {
                    type = 1;
                    continue;
                }
                if (tok.group == TOKENGROUP.COMMENT_LINE)
                {
                    type = 2;
                    continue;
                }
                newtokens.Add(tok);
                continue;
            }
            if (type == 1)
            {
                if (tok.group == TOKENGROUP.COMMENT_CLOSE)
                {
                    type = 0;
                    continue;
                }
                continue;
            }
            if (type == 2)
            {
                if (tok.group == TOKENGROUP.NEWLINE)
                {
                    type = 0;
                    continue;
                }
                continue;
            }
        }
                    
        return newtokens.ToArray();
    }

    static TokenItem[] _delete_spaces(TokenItem[] tokens)
    {
        var newtokens = new List<TokenItem>();
        foreach(var tok in tokens)
        {
            if (tok.group != TOKENGROUP.SPACE)
            {
                newtokens.Add(tok);
            }
        }
        return newtokens.ToArray();
    }

    static string Replace(string input, int n, string val)
    {
        var s = input;
        var sub1 = "{" + n.ToString() + "}";
        var sub2 = "{~" + n.ToString() + "}";
        s = s.Replace(sub1,val);
       
        var val2 = val;
        if (val2.StartsWith("\"")) val2 = val2.Substring(1);
        if (val2.EndsWith("\""))   val2 = val2.Substring(0,val2.Length-1);

        s=s.Replace(sub2,val2);

        return s;
    }
}
