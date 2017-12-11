using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;

public class ExcelWork 
{
    Excel.Application m_app;
    Excel.Workbooks   m_workbooks;
    Excel.Workbook    m_workbook;
    Excel.Worksheet   m_worksheet;


    public void Dispose() //ref https://blogs.msdn.microsoft.com/office_client_development_support_blog/2012/02/09/office-5/
    {
        if (m_worksheet!=null)
        {
            Marshal.ReleaseComObject(m_worksheet);
            m_worksheet = null;
        }
        if (m_workbook!=null)
        {
            Marshal.ReleaseComObject(m_workbook);
            m_workbook = null;
        }
        if (m_workbooks!=null)
        {
            Marshal.ReleaseComObject(m_workbooks);
            m_workbooks = null;
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        m_app.Quit();
        m_app = null;
    }


    public void Load(string file)
    {
        if (m_app!=null) throw new SystemException();
        m_app = new Excel.Application();
        m_workbooks = m_app.Workbooks;
        m_workbook = m_workbooks.Open(file);
    }

    public void Save()
    {
        m_workbook.Save();        
    }

    public void SetSheet(string name)
    {
        for(var i = 1; i <= m_workbook.Sheets.Count; i++)
        {
            var sheet = (Excel.Worksheet)m_workbook.Sheets[i];
            if (sheet.Name == name)
            {
                m_worksheet = sheet;
                break;
            }
            Marshal.ReleaseComObject(sheet);
            sheet = null;
        }   
    }

    public Excel.Worksheet GetSheet()
    {
        return m_worksheet;
    }
}