using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace Khalid.Core.Framework
{
    public static class ExcelReportGenerator
    {
        public static async Task<Stream> GenerateExcelReport(List<string> headers, List<List<object>> data)
        {

            return await GenerateExcelReport(new List<ExcelSheetData> { new ExcelSheetData { Headers = headers, Data = data, SheetName = "Report" } });
            // Create a new Excel package and workbook
            //using (var package = new ExcelPackage())
            //{
            //    // Add a new worksheet to the Excel workbook
            //    var worksheet = package.Workbook.Worksheets.Add("Report");

            //    // Set headers in the first row
            //    for (var i = 1; i <= headers.Count; i++)
            //    {
            //        worksheet.Cells[1, i].Value = headers[i - 1];
            //        worksheet.Cells[1, i].Style.Font.Bold = true;
            //    }

            //    // Populate data
            //    for (var row = 0; row < data.Count; row++)
            //    {
            //        for (var col = 0; col < data[row].Count; col++)
            //        {
            //            worksheet.Cells[row + 2, col + 1].Value = data[row][col];
            //        }
            //    }

            //    // Auto-fit columns for better readability
            //    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            //    // Create a memory stream to store the Excel data
            //    var stream = new MemoryStream(await package.GetAsByteArrayAsync());

            //    return stream;
            //}

        }



        public static async Task<Stream> GenerateExcelReport(List<ExcelSheetData> sheets)
        {
            using (var package = new ExcelPackage())
            {
                foreach (var sheetData in sheets)
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheetData.SheetName);

                    // Set headers in the first row
                    for (var i = 1; i <= sheetData.Headers.Count; i++)
                    {
                        worksheet.Cells[1, i].Value = sheetData.Headers[i - 1];
                        worksheet.Cells[1, i].Style.Font.Bold = true;
                    }

                    // Populate data
                    for (var row = 0; row < sheetData.Data.Count; row++)
                    {
                        for (var col = 0; col < sheetData.Data[row].Count; col++)
                        {
                            worksheet.Cells[row + 2, col + 1].Value = sheetData.Data[row][col];
                        }
                    }

                    // Auto-fit columns for better readability
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var stream = new MemoryStream(await package.GetAsByteArrayAsync());
                return stream;
            }
        }

        public class ExcelSheetData
        {
            public string SheetName { get; set; }
            public List<string> Headers { get; set; }
            public List<List<object>> Data { get; set; }
        }


    }
}
