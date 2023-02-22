using System.Collections.Generic;
using System.Data;

namespace BoostChampsPlatform.Services.Services.ExcelHelper
{
    public interface IExcelHelper
    {
        /// <summary>
        /// Saves to server location (NOT to CLIENT). Use .xlsx as extension
        /// </summary>
        /// <typeparam name="T">Type of List</typeparam>
        /// <param name="records">List of Records</param>
        /// <param name="fullFilePath">Full path (with file name) to where to save the file to (server path, NOT CLIENT). eg:e:\\excelfile.xlsx</param>
        void ListToExcel<T>(IList<T> records, string fullFilePath);
        /// <summary>
        /// <see cref="ListToExcel{T}(IList{T}, string)"/>
        /// For Creating Multiple Sheets
        /// </summary>
        /// <param name="sheets">Key is used as Sheet Name, Value should be of List type</param>
        /// <param name="fullFilePath">Full path (with file name) to where to save the file to (server path, NOT CLIENT). eg:e:\\excelfile.xlsx</param>
        void ListToExcel(IDictionary<string, dynamic> sheets, string fullFilePath);
        /// <summary>
        /// Returns memory stream for direct file download.
        /// </summary>
        /// <typeparam name="T">Type of List</typeparam>
        /// <param name="records">List of Records</param>
        /// <returns><see cref="System.IO.MemoryStream"/></returns>
        System.IO.MemoryStream ListToExcelStream<T>(IList<T> records);
        /// <summary>
        /// <see cref="ListToExcelStream{T}(IList{T})"/>
        /// For Creating Multiple Sheets
        /// </summary>
        /// <param name="sheets">Key is used as Sheet Name, Value should be of List type</param>
        /// <returns></returns>
        System.IO.MemoryStream ListToExcelStream(IDictionary<string, dynamic> sheets);
        DataTable ExcelToDataTable(string fileName, string sheetName = null);
        void ListToExcel<T>(IList<T> records, string fullFilePath, string ExcludeColumn);
        void DatatTableToExcelWithMultipleSheets(DataSet ds, string fileName);
        void DatatTableToExcelWithMultipleSheetsDateNumberFormat(DataSet ds, string fileName);
        void DataTableToExcelWithSubTotal(DataSet ds, string fileName);
    }
}
