using EP.ECertificate.Presentation.Web.Models;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace EP.ECertificate.Presentation.Web.Utility
{
    public class ExcelFileManager
    {
        ExcelQueryFactory excelFactory;
        public ExcelFileManager(string fileName)
        {
            excelFactory = new ExcelQueryFactory(FileManager.GetFileNameWithFullPath(fileName, FileType.Data.ToString()));

        }

        public List<string> GetExcelHeaders()
        {
            var headers = excelFactory.WorksheetNoHeader(0).FirstOrDefault();
            List<string> headerList = new List<string>();
            headers.ForEach(p => headerList.Add(p.Value.ToString()));
            return headerList;
        }

        public Row GetExcelRow(int rowNumber)
        {
            //var columns = from c in excelFactory.Worksheet() select c[header];
            var row = excelFactory.Worksheet(0).Skip(rowNumber).ToList().First();
            return row;
            
        }

        public Row[] GetExcelRows()
        {
            //var columns = from c in excelFactory.Worksheet() select c[header];
            var rows = excelFactory.Worksheet(0).ToArray();
            return rows;

        }

        public string[] GetExcelColumn(string header)
        {
            var columns = from c in excelFactory.Worksheet(0) select c[header];
            return columns.Select(p => p.Value.ToString()).ToArray();

        }

        public int GetExcelRowCount()
        {
            return excelFactory.Worksheet().Count();

        }


      

        


        
    }
}