//<<<include=using_text.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
//>>>

using System.Runtime.InteropServices;

using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;

public class DefineDic
{
    public Dictionary<string, string> m_dic { get; private set; }

    public void Create(string file) // A列: なし　B列:ID  C列:別名IDその１ D列:別名IDその２ E列:値  ３行目から読み込む　空白ＩＤは無視 英数字以外無視
    {
        m_dic = new Dictionary<string, string>();

        var ew = new ExcelWork();
        ew.Load(file);
        ew.SetSheet("define");
        var sheet = ew.GetSheet();

        var urange = sheet.UsedRange;
        
        var row_start = urange.Row;
        var row_end   = urange.Row + urange.Rows.Count;
        var col_start = urange.Column;
        var col_end   = urange.Column + urange.Columns.Count;

        Func<int,int,string> _getvalue = (r,c)=> {
            var ret = string.Empty;
            if (
            r >= row_start && r<= row_end 
                &&
            c >= col_start && c<= col_end
                )
            {
                var cell = (Excel.Range)urange.Cells[r,c];
                if (cell!=null)
                {
                    var s = cell.Value2;
                    if (s!=null)
                    {
                        ret = ((object)s).ToString();
                    }
                }
                Marshal.ReleaseComObject(cell);
                cell = null;
            }
            return ret;
        };

        var regex_id = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*");

        for(var row = 3; row <=row_end; row++)
        {
            var id1 = _getvalue(row,2);
            if (string.IsNullOrEmpty(id1)) continue;
            if (!regex_id.IsMatch(id1)) continue;

            var id2 = _getvalue(row,3);
            if (!regex_id.IsMatch(id2)) id2 = string.Empty;

            var id3 = _getvalue(row,4);
            if (!regex_id.IsMatch(id3)) id3 = string.Empty;

            var val = _getvalue(row,5);
            
            m_dic.Add(id1,val);
            if (!string.IsNullOrEmpty(id2)) m_dic.Add(id2,val);
            if (!string.IsNullOrEmpty(id3)) m_dic.Add(id3,val);
        }

        Marshal.ReleaseComObject(urange);
        urange = null;
        ew.Dispose();
    }

    public string GetValue(string key)
    {
        if (m_dic.ContainsKey(key))
        {
            return m_dic[key];
        }
        return null;
    }
}

