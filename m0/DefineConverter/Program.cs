//<<<include=using_text.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
//>>>

namespace DefineConverter
{
    class Program
    {
        public static string m_inputfile;
        public static string m_outputfile;
        public static string m_dicfile;

        public static DefineDic m_defdic;
       
        public static string m_encode = "utf-8";
        static void Main(string[] args) // 0 - input.txt 1 - output.txt 2  -  dic.xls  3 ... options -e encode
        {
            m_inputfile  = args[0];
            m_outputfile = args[1];
            m_dicfile    = args[2];

            for(var i = 3; i< args.Length; i++)
            {
                var a = args[i];
                if (a=="-e")
                {
                    if (i+1<args.Length)
                    {
                        m_encode = args[i+1];
                        i++;
                        continue;
                    }
                }
            }

            m_defdic = new DefineDic();
            m_defdic.Create(m_dicfile);
        }
    }
}
