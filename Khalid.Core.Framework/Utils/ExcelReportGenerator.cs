using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions;

namespace Khalid.Core.Framework
{
    public static class ExcelReportGenerator
    {

        public static List<T> ImportExcelSheet<T>(Stream stream) where T : class, new()
        {
            var sheetData = ImportExcelSheet(stream);

            var result = new List<T>();

            for (int i = 0; i < sheetData.Data.Count; i++)
            {
                var d = new T();

                for (int j = 0; j < sheetData.Data[i].Count; j++)
                {
                    try
                    {
                        ReflectionHelper.SetPropertyIgnoreCase(d, sheetData.Headers[j], sheetData.Data[i][j]);

                    }
                    catch
                    {
                        throw new InvalidDataException($"Invalid data or invalid property (header) in row: {i + 2}, column: {sheetData.Headers[j]}");
                    }
                }
                result.Add(d);
            }
            return result;
        }

        public static ExcelSheetData ImportExcelSheet(Stream stream)
        {
            var data = new List<List<object>>();

            var headers = new List<string>();

            string sheetName = null;

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet != null)
                {
                    sheetName = worksheet.Name;

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;


                    for (int row = 1; row <= rowCount; row++)
                    {
                        var rowData = new List<object>();

                        for (int col = 1; col <= colCount; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Value;
                            rowData.Add(cellValue);
                        }

                        if (row == 1) headers = rowData.ConvertAll(x => x.ToString());
                        else data.Add(rowData);
                    }
                }
            }

            return new ExcelSheetData
            {
                SheetName = sheetName,
                Data = data,
                Headers = headers

            };
        }
        public static async Task<Stream> GetImportTemplate<T>()
        {
            return await GenerateExcelReport(ReflectionHelper.GetReadablePropertyNames(typeof(T)).ToList(), new List<T> { });
        }

        public static async Task<Stream> GenerateExcelReport<T>(List<T> data)
        {
            return await GenerateExcelReport(ReflectionHelper.GetReadablePropertyNames(typeof(T)).ToList(), data);
        }
        public static async Task<Stream> GenerateExcelReport<T>(List<string> headers, List<T> data)
        {
            return await GenerateExcelReport(new List<ExcelSheetData> {
                new ExcelSheetData
                {
                    Headers = headers,
                    Data = data.Select(s => headers.Select(ss => ReflectionHelper.GetPropertyValueIgnoreCase(s, ss)).ToList()).ToList(),
                    SheetName =ReflectionHelper.ConvertToTitleCase( " Report")
                } }
            );

        }
        public static async Task<Stream> GenerateExcelReport(List<string> headers, List<List<object>> data)
        {
            return await GenerateExcelReport(new List<ExcelSheetData> { new ExcelSheetData { Headers = headers, Data = data, SheetName = "Report" } });
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


    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreExcelFieldAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelFieldAttribute : Attribute
    {
        public int Order { get; }

        public string HeaderName { get; }

        public ExcelFieldAttribute(string headerName = null, int order = 0)
        {
            Order = order;
            HeaderName = headerName;
        }

    }

}
