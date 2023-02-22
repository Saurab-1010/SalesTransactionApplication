using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DataTable = System.Data.DataTable;
using NumberingFormat = DocumentFormat.OpenXml.Spreadsheet.NumberingFormat;

namespace BoostChampsPlatform.Services.Services.ExcelHelper
{
    public class ExcelHelper : IExcelHelper
    {
        private readonly ILogger<ExcelHelper> _logger;
        private readonly Dictionary<uint, NumberingFormat> builtInDateTimeNumberFormats;
        private Regex dateTimeFormatRegex = new Regex(@"((?=([^[]*\[[^[\]]*\])*([^[]*[ymdhs]+[^\]]*))|.*\[(h|mm|ss)\].*)", RegexOptions.Compiled);
        private uint[] builtInDateTimeNumberFormatIDs = new uint[] { 14, 15, 16, 17, 18, 19, 20, 21, 22, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 45, 46, 47, 50, 51, 52, 53, 54, 55, 56, 57, 58 };
        private Type[] numberTypes = new[] { typeof(int), typeof(Int16), typeof(long), typeof(int?), typeof(Int16?),
            typeof(long?), typeof(double), typeof(decimal), typeof(double?), typeof(decimal?), typeof(float), typeof(float?) };
        public ExcelHelper(ILogger<ExcelHelper> logger)
        {
            _logger = logger;
            builtInDateTimeNumberFormats = builtInDateTimeNumberFormatIDs.ToDictionary(id => id, id => new NumberingFormat { NumberFormatId = id });
        }

        public void ListToExcel<T>(IList<T> records, string fileName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                WriteExcel(records, document, null);
            }
        }
        public void ListToExcel<T>(IList<T> records, string fileName, string ExcludeColumn)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                WriteExcel(records, document, ExcludeColumn);
            }
        }
        /// <summary>
        /// Directly downloads to the client browser
        /// </summary>
        /// <typeparam name="T">Type of List</typeparam>
        /// <param name="records">List of Records</param>
        /// <returns></returns>
        public System.IO.MemoryStream ListToExcelStream<T>(IList<T> records)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
            {
                WriteExcel(records, document, null);
            }
            return stream;
        }

        public void ListToExcel(IDictionary<string, dynamic> sheetList, string fileName)
        {

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add Sheets to the Workbook.
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                UInt32Value sheetCount = 1; // Mimimum value is 1

                foreach (var data in sheetList)
                {

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Append a new worksheet and associate it with the workbook.
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = sheetCount,
                        Name = data.Key
                    };
                    sheets.Append(sheet);

                    WriteToWorkSheet(data.Value, worksheetPart, null);
                    sheetCount++;
                }

                workbookpart.Workbook.Save();

                // Close the document.
                spreadsheetDocument.Close();
            }
        }

        public DataTable ExcelToDataTable(string fileFullPath, string sheetName = null)
        {
            using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(fileFullPath, false))
            {
                WorkbookPart workbookPart = sDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = "";
                if (sheetName == null)
                    relationshipId = sheets.First().Id.Value;
                else
                    relationshipId = workbookPart.Workbook.Descendants<Sheet>().First(s => sheetName.Equals(s.Name)).Id;

                WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();
                DataTable dt = new DataTable();
                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetCellValue(sDoc, cell));
                }

                foreach (Row row in rows) //this will also include your header row...
                {
                    var totalColumns = rows.ElementAt(0).Count();

                    DataRow tempRow = dt.NewRow();

                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        Cell cell = row.Descendants<Cell>().ElementAt(i);
                        int actualCellIndex = CellReferenceToIndex(cell);
                        if (actualCellIndex < totalColumns)  //Rakshya 9283-6: this is done to read data of only expected no of columns.
                        {
                            tempRow[actualCellIndex] = GetCellValue(sDoc, cell);
                        }

                    }

                    dt.Rows.Add(tempRow);
                }
                // Remove header row
                dt.Rows.RemoveAt(0);
                return dt;
            }
        }
        public MemoryStream ListToExcelStream(IDictionary<string, dynamic> sheetList)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add Sheets to the Workbook.
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                UInt32Value sheetCount = 1; // Mimimum value is 1

                foreach (var data in sheetList)
                {

                    // Add a WorksheetPart to the WorkbookPart
                    WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    // Append a new worksheet and associate it with the workbook.
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = sheetCount,
                        Name = data.Key
                    };
                    sheets.Append(sheet);

                    WriteToWorkSheet(data.Value, worksheetPart, null);
                    sheetCount++;
                }

                workbookpart.Workbook.Save();

                // Close the document.
                spreadsheetDocument.Close();
            }
            return stream;
        }
        public void DatatTableToExcelWithMultipleSheets(DataSet ds, string fileName)
        {
            using (var workbook = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();

                uint sheetId = 1;

                foreach (DataTable table in ds.Tables)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    sheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    if (sheets.Elements<Sheet>().Count() > 0)
                    {
                        sheetId =
                            sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);

                    Row headerRow = new Row();

                    List<String> columns = new List<string>();
                    foreach (DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dsrow in table.Rows)
                    {
                        Row newRow = new Row();
                        foreach (String col in columns)
                        {
                            Cell cell = new Cell();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }
                }
                workbook.WorkbookPart.Workbook.Save();
            }
        }

        public void DataTableToExcelWithSubTotal(DataSet ds, string fileName)
        {
            using (var workbook = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();

                uint sheetId = 1;

                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();

                sheetPart.Worksheet = new Worksheet(sheetData);

                WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylePart.Stylesheet = new Stylesheet();


                Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId =
                        sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = "Table" };
                sheets.Append(sheet);

                foreach (DataTable table in ds.Tables)
                {
                    Row headerRow = new Row();

                    List<String> columns = new List<string>();
                    foreach (DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }

                    if (table.TableName == "Table")
                    {
                        sheetData.AppendChild(headerRow);
                    }

                    foreach (DataRow dsrow in table.Rows)
                    {
                        Row newRow = new Row();
                        foreach (String col in columns)
                        {
                            Cell cell = new Cell();
                            cell.DataType = (dsrow[col].GetType().Name.Contains("Int") || dsrow[col].GetType().Name.Contains("Decimal")) ? CellValues.Number : CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            newRow.AppendChild(cell);
                        }

                        //if (table.TableName == "Table1")
                        //{
                        //    var Fonts = new Fonts();
                        //    var CellFormats = new CellFormats();
                        //    stylePart.Stylesheet.Append(Fonts);
                        //    stylePart.Stylesheet.Append(CellFormats);
                        //    Font font1 = new Font(
                        //                    new Bold()
                        //                );
                        //    stylePart.Stylesheet.Fonts.Append(font1);
                        //    stylePart.Stylesheet.Save();

                        //    UInt32Value fontId = Convert.ToUInt32(stylePart.Stylesheet.Fonts.ChildElements.Count);
                        //    CellFormat cf = new CellFormat() { FontId = fontId, FillId = 0, BorderId = 0, ApplyFont = true };

                        //    stylePart.Stylesheet.CellFormats.Append(cf);
                        //    Row r = sheetData.Elements<Row>().Last<Row>();

                        //    int index1 = stylePart.Stylesheet.CellFormats.ChildElements.Count - 1;
                        //    foreach (Cell c in r.Elements<Cell>())
                        //    {
                        //        c.StyleIndex = Convert.ToUInt32(index1);
                        //        sheetPart.Worksheet.Save();
                        //    }
                        //}

                        sheetData.AppendChild(newRow);
                    }
                }
                workbook.WorkbookPart.Workbook.Save();
            }
        }
        #region " Private Heavyweight functions "
        private void WriteExcel<T>(IList<T> records, SpreadsheetDocument spreadsheet, string ExcludeColumn)
        {
            //  Create the Excel file contents.  This function is used when creating an Excel file either writing 
            //  to a file, or writing to a MemoryStream.
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new Workbook();

            //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
            spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            Stylesheet stylesheet = new Stylesheet();
            workbookStylesPart.Stylesheet = stylesheet;
            //  For each worksheet you want to create
            string workSheetID = "rId" + 1;
            string worksheetName = "Worksheet" + 1;

            WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();

            // create sheet data
            newWorksheetPart.Worksheet.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.SheetData());

            // save worksheet
            WriteToWorkSheet(records, newWorksheetPart, ExcludeColumn);
            newWorksheetPart.Worksheet.Save();

            // create the worksheet to workbook relation
            // if (worksheetNumber == 1)
            spreadsheet.WorkbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

            spreadsheet.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>().AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
                SheetId = 1,
                Name = worksheetName
            });

            spreadsheet.WorkbookPart.Workbook.Save();
        }
        private void WriteToWorkSheet<T>(IList<T> records, WorksheetPart worksheetPart, string ExcludeColumn)
        {
            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();


            var props = typeof(T).GetProperties();//GetMembers().Where(x => x.MemberType == MemberTypes.Property);
                                                  //  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
                                                  //
                                                  //  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
                                                  //  cells of data, we'll know if to write Text values or Numeric cell values.

            //Rakshya :TaskID 9595-22-2 change made not to display unnecessary columns on excel sheet. Pass the columnname in comma separated form in ExcludeColumn parameter.

            //var ExcludeColumn = "OrganizationID,SalesPersonID";
            if (ExcludeColumn != null)
            {
                var ExcludeColumnList = ExcludeColumn.Split(',');
                foreach (var item in ExcludeColumnList)
                {
                    props = props.Where(x => x.Name != item).ToArray();

                }
            }


            int numberOfColumns = props.Count();
            bool[] IsNumericColumn = new bool[numberOfColumns];

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            var colOptions = new List<ColOption>();
            //
            //  Create the Header row in our Excel Worksheet
            //
            uint rowIndex = 1;

            var headerRow = new Row { RowIndex = rowIndex, };  // add a row at the top of spreadsheet
            sheetData.Append(headerRow);
            int colIndex = 0;
            foreach (var prop in props)
            {
                var colOption = new ColOption();
                colOption.Type = GetCellValueType(prop);
                colOption.ExcelColumnName = excelColumnNames[colIndex];
                var browsableProp = prop.GetCustomAttribute(typeof(BrowsableAttribute), true) as BrowsableAttribute;
                // If the property is browsable then only show it in excel
                if (browsableProp == null || browsableProp.Browsable == true)
                {
                    string columnName = "";
                    // Check for different column Name
                    var displayNameProp = prop.GetCustomAttribute(typeof(DisplayNameAttribute), true) as DisplayNameAttribute;
                    if (displayNameProp != null)
                    {
                        columnName = displayNameProp.DisplayName;
                    }
                    else
                    {
                        columnName = prop.Name;
                    }
                    // Check for formatting
                    var formatStringProp = prop.GetCustomAttribute(typeof(DisplayFormatAttribute), true) as DisplayFormatAttribute;
                    if (formatStringProp != null)
                    {
                        colOption.DataFormatString = formatStringProp.DataFormatString;
                    }
                    AppendTextCell(excelColumnNames[colIndex] + "1", columnName, headerRow);
                    colIndex++;
                    colOption.Browsable = true;
                }
                else
                {
                    colOption.Browsable = false;
                }
                colOptions.Add(colOption);
            }
            //
            //  Now, step through each row of data in our DataTable...
            //
            foreach (var record in records)
            {
                // ...create a new row, and append a set of this row's data to it.
                ++rowIndex;
                var newExcelRow = new Row { RowIndex = rowIndex };  // add a row at the top of spreadsheet
                sheetData.Append(newExcelRow);
                colIndex = 0;
                foreach (var prop in props)
                {
                    var colOption = colOptions[colIndex];

                    if (colOption.Browsable)
                    {
                        object valueObj = prop.GetValue(record);
                        string cellValue = valueObj == null ? "" : (string.IsNullOrEmpty(colOption.DataFormatString) == true ? valueObj.ToString() : string.Format(colOption.DataFormatString, valueObj));
                        //string cellValue = "";
                        //  For text cells, just write the input data straight out to the Excel file.
                        AppendCell(colOption.ExcelColumnName + rowIndex.ToString(), cellValue, newExcelRow, colOption.Type);
                    }
                    colIndex++;
                }
            }
        }
        #endregion

        #region " Private Helper functions "
        private string hexvaluesToRemove = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
        private void AppendCell(string cellReference, string cellStringValue, Row excelRow, CellValues dataType)
        {
            //  Add a new Excel Cell to our Row 
            Cell cell = new Cell() { CellReference = cellReference, DataType = dataType };
            CellValue cellValue = new CellValue();

            if (string.IsNullOrEmpty(cellStringValue) == false)
            {
                cellStringValue = Regex.Replace(cellStringValue, hexvaluesToRemove, "");
            }
            cellValue.Text = cellStringValue;
            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private void AppendTextCell(string cellReference, string cellStringValue, Row excelRow)
        {
            //  Add a new Excel Cell to our Row 
            Cell cell = new Cell() { CellReference = cellReference, DataType = CellValues.String };
            CellValue cellValue = new CellValue();
            if (string.IsNullOrEmpty(cellStringValue) == false)
            {
                cellStringValue = Regex.Replace(cellStringValue, hexvaluesToRemove, "");
            }
            cellValue.Text = cellStringValue;
            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private CellValues GetCellValueType(PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
            {
                return CellValues.String;
            }
            else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
            {
                //  return CellValues.Boolean; // Shows error when opening excel file
                return CellValues.String;
            }
            else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                return CellValues.String;
            }
            else if (numberTypes.Contains(property.PropertyType))
            {
                return CellValues.Number;
            }
            // If not found anything

            return CellValues.String;
        }
        private string GetExcelColumnName(int columnIndex, int offset = 0)
        {
            //  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
            //
            //  eg  GetExcelColumnName(0) should return "A"
            //      GetExcelColumnName(1) should return "B"
            //      GetExcelColumnName(25) should return "Z"
            //      GetExcelColumnName(26) should return "AA"
            //      GetExcelColumnName(27) should return "AB"
            //      ..etc..
            //
            if ((columnIndex - offset) < 26)
                return ((char)('A' + (columnIndex - offset))).ToString();

            char firstChar = (char)('A' + ((columnIndex - offset) / 26) - 1);
            char secondChar = (char)('A' + ((columnIndex - offset) % 26));

            return string.Format("{0}{1}", firstChar, secondChar);
        }
        private class ColOption
        {
            public CellValues Type = CellValues.String;
            public bool Browsable = true;
            public string ExcelColumnName = "";
            public string DataFormatString = "";
        }
        #endregion
        //sushin: need to replace Datatable.
        //public DataTable ReadExcelSheetDataTable(string fileName)
        //{
        //    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
        //    {
        //        ////Read the first Sheet from Excel file.
        //        //Sheet sheet = spreadsheetDocument.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
        //        ////Get the Worksheet instance.
        //        //Worksheet worksheet = (spreadsheetDocument.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
        //        ////Fetch all the rows present in the Worksheet.
        //        //IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
        //        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
        //        WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
        //        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
        //        IEnumerable<Row> rows = sheetData.Descendants<Row>();
        //        DataTable dt = new DataTable();
        //        //Loop through the Worksheet rows.
        //        foreach (Row row in rows)
        //        {
        //            //Use the first row to add columns to DataTable.
        //            if (row.RowIndex.Value == 1)
        //            {
        //                foreach (Cell cell in row.Descendants<Cell>())
        //                {
        //                    dt.Columns.Add(GetValue(spreadsheetDocument, cell));
        //                }
        //            }
        //            else
        //            {
        //                //Add rows to DataTable.
        //                dt.Rows.Add();
        //                int i = 0;
        //                foreach (Cell cell in row.Descendants<Cell>())
        //                {
        //                    dt.Rows[dt.Rows.Count - 1][i] = GetValue(spreadsheetDocument, cell);
        //                    i++;
        //                }
        //            }
        //        }
        //        return dt;
        //    }
        //}

        private int CellReferenceToIndex(Cell cell)
        {
            int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();

            if (string.IsNullOrEmpty(reference))
            {
                return 0;
            }

            //remove digits
            string columnReference = Regex.Replace(reference.ToUpper(), @"[\d]", string.Empty);

            int columnNumber = -1;
            int mulitplier = 1;

            //working from the end of the letters take the ASCII code less 64 (so A = 1, B =2...etc)
            //then multiply that number by our multiplier (which starts at 1)
            //multiply our multiplier by 26 as there are 26 letters
            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * ((int)c - 64);

                mulitplier = mulitplier * 26;
            }

            //the result is zero based so return columnnumber + 1 for a 1 based answer
            //this will match Excel's COLUMN function
            index = columnNumber;

            return index;
        }

        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue == null)
            {
                return "";
            }
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                if (cell.StyleIndex == null)
                {

                    return value;
                }
                var cellformat = GetCellFormat(cell);
                if (cellformat.NumberFormatId == 14)
                {
                    return DateTime.FromOADate(Convert.ToDouble(value)).ToShortDateString();
                }

                return value;

            }
        }

        private CellFormat GetCellFormat(Cell cell)
        {
            Worksheet workSheet = cell.Ancestors<Worksheet>().FirstOrDefault();
            SpreadsheetDocument doc = workSheet.WorksheetPart.OpenXmlPackage as SpreadsheetDocument;
            WorkbookPart workbookPart = doc.WorkbookPart;
            int styleIndex = (int)cell.StyleIndex.Value;
            CellFormat cellFormat = (CellFormat)workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ElementAt(styleIndex);
            return cellFormat;
        }

        private string GetValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }

        // Test for dynamic return from excel
        //public List<object> ReadExcelSheet(string fileName)
        //{
        //    var listColumnHeaders = new List<dynamic>();
        //    var listData = new List<dynamic>();
        //    var listReturnData = new List<dynamic>();
        //    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
        //    {
        //        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
        //        WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
        //        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
        //        IEnumerable<Row> rows = sheetData.Descendants<Row>();
        //        //Loop through the Worksheet rows.
        //        foreach (Row row in rows)
        //        {
        //            //Use the first row to add columns Header. 
        //            if (row.RowIndex.Value == 1)
        //            {
        //                foreach (Cell cell in row.Descendants<Cell>())
        //                {
        //                    listColumnHeaders.Add(GetValue(spreadsheetDocument, cell));
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    if (listColumnHeaders != null)
        //                    {
        //                        int i = 0;
        //                        foreach (Cell cell in row.Descendants<Cell>())
        //                        {
        //                            string KeyName = listColumnHeaders[i];
        //                            string ValueName = GetValue(spreadsheetDocument, cell);
        //                            var data = new { KeyName, ValueName };
        //                            listData.Add(data);
        //                            i++;
        //                        }
        //                        string newJson = JsonConvert.SerializeObject(listData);
        //                        listReturnData.Add(newJson);
        //                        listData.Clear();
        //                    }
        //                }
        //                catch (Exception)
        //                {

        //                }
        //            }
        //        }
        //        return listReturnData;
        //    }
        //}
        public void DatatTableToExcelWithMultipleSheetsDateNumberFormat(DataSet ds, string fileName)
        {
            using (var workbook = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();

                uint sheetId = 1;

                foreach (DataTable table in ds.Tables)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    sheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    if (sheets.Elements<Sheet>().Count() > 0)
                    {
                        sheetId =
                            sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    if (ds.DataSetName == "RecognitionReport")
                    {
                        WorkbookStylesPart wspContractImport = workbookPart.AddNewPart<WorkbookStylesPart>();
                        wspContractImport.Stylesheet = GenerateStylesheetDefault();
                        wspContractImport.Stylesheet.Save();
                    }

                    Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);

                    Row headerRow = new Row();

                    List<String> columns = new List<string>();
                    foreach (DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dsrow in table.Rows)
                    {
                        Row newRow = new Row();
                        foreach (String col in columns)
                        {
                            Cell cell = new Cell();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            if (col == "Accounting Closing Date" || col == "Start Date" || col == "End Date" || col == "Date Entered" || col == "Actual Posting Date")
                            {
                                Cell cellStyleIndx = CellStyleIndex("DateFormat");
                                cell.StyleIndex = cellStyleIndx.StyleIndex;
                            }

                            if (dsrow[col].GetType() == typeof(decimal))
                            {
                                Cell cellStyleIndx = CellStyleIndex("DecimalFormat");
                                cell.StyleIndex = cellStyleIndx.StyleIndex;
                                cell.DataType = CellValues.Number;
                            }

                            if (dsrow[col].GetType() == typeof(int) || col == "Line Item" || col == "Agency Commission Percent")
                            {
                                Cell cellStyleIndx = CellStyleIndex("NumberFormat");
                                cell.StyleIndex = cellStyleIndx.StyleIndex;
                                cell.DataType = CellValues.Number;
                            }
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }
                }
                workbook.WorkbookPart.Workbook.Save();
            }
        }

        public static Stylesheet GenerateStylesheetDefault()
        {
            var StyleSheet = new Stylesheet();

            // Create "fonts" node.
            var Fonts = new Fonts();
            Fonts.Append(new DocumentFormat.OpenXml.Spreadsheet.Font()
            {
                FontName = new FontName() { Val = "Calibri" },
                FontSize = new FontSize() { Val = 11 },
                FontFamilyNumbering = new FontFamilyNumbering() { Val = 2 },
            });

            Fonts.Count = (uint)Fonts.ChildElements.Count;

            // Create "fills" node.
            var Fills = new Fills();
            Fills.Append(new Fill()
            {
                PatternFill = new PatternFill() { PatternType = PatternValues.None }
            });
            Fills.Append(new Fill()
            {
                PatternFill = new PatternFill() { PatternType = PatternValues.Gray125 }
            });

            Fills.Count = (uint)Fills.ChildElements.Count;

            // Create "borders" node.
            var Borders = new Borders();
            Borders.Append(new Border()
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder(),
                BottomBorder = new BottomBorder(),
                DiagonalBorder = new DiagonalBorder()
            });

            Borders.Count = (uint)Borders.ChildElements.Count;

            // Create "cellStyleXfs" node.
            var CellStyleFormats = new CellStyleFormats();
            CellStyleFormats.Append(new CellFormat()
            {
                NumberFormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0
            });

            CellStyleFormats.Count = (uint)CellStyleFormats.ChildElements.Count;

            // Create "cellXfs" node.
            var CellFormats = new CellFormats();

            // StyleIndex = 0, A default style that works for most things (But not strings? )
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 0,
                FormatId = 0,
                ApplyNumberFormat = true
            });

            // StyleIndex = 1, A style that works for DateTime (just the date)
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 14, //Date
                FormatId = 0,
                ApplyNumberFormat = true
            });

            // StyleIndex = 2, A style that works for Decimal (just for Decimal)
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 2, //Decimal
                FormatId = 0,
                ApplyNumberFormat = true,

            });

            // StyleIndex = 3, A style that works for Decimal (just for Decimal)
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 1, //Number
                FormatId = 0,
                ApplyNumberFormat = true
            });

            // StyleIndex = 4, A style that works for DateTime (Date and Time)
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 22, //Date Time
                FormatId = 0,
                ApplyNumberFormat = true
            });

            CellFormats.Count = (uint)CellFormats.ChildElements.Count;

            // Create "cellStyles" node.
            var CellStyles = new CellStyles();
            CellStyles.Append(new CellStyle()
            {
                Name = "Normal",
                FormatId = 0,
                BuiltinId = 0
            });
            CellStyles.Count = (uint)CellStyles.ChildElements.Count;

            // Append all nodes in order.
            StyleSheet.Append(Fonts);
            StyleSheet.Append(Fills);
            StyleSheet.Append(Borders);
            StyleSheet.Append(CellStyleFormats);
            StyleSheet.Append(CellFormats);
            StyleSheet.Append(CellStyles);

            return StyleSheet;
        }
        public static Cell CellStyleIndex(string cellName)
        {
            Cell cell = new Cell();
            if (cellName == "DateFormat")
            {
                cell.StyleIndex = 1U;
            }
            if (cellName == "DecimalFormat")
            {
                cell.StyleIndex = 2U;
            }
            if (cellName == "NumberFormat")
            {
                cell.StyleIndex = 3U;
            }
            return cell;
        }

        public static DataTable GetDataTableFromSpreadsheet(string MyExcelStream, bool ReadOnly, string SheetName, out string ErrorMessage)
        {
            DataTable dt = new DataTable();
            using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly))
            {
                WorkbookPart workbookPart = sDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                if (!string.IsNullOrEmpty(SheetName))
                {
                    sheets = sheets.Where(x => x.Name == SheetName);
                }

                ErrorMessage = "";

                if (sheets.Count() < 1)
                {
                    ErrorMessage = SheetName;
                    return dt;
                }

                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;

                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetDynamicCellValue(sDoc, cell));
                }

                foreach (Row row in rows) //this will also include your header row...
                {
                    var totalColumns = rows.ElementAt(0).Count();

                    DataRow tempRow = dt.NewRow();

                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        Cell cell = row.Descendants<Cell>().ElementAt(i);
                        int actualCellIndex = CellDynamicReferenceToIndex(cell);
                        if (actualCellIndex < totalColumns)  //Rakshya 9283-6: this is done to read data of only expected no of columns.
                        {
                            tempRow[actualCellIndex] = GetDynamicCellValue(sDoc, cell);
                        }

                    }

                    dt.Rows.Add(tempRow);
                }
            }
            dt.Rows.RemoveAt(0);
            return dt;
        }

        private static int CellDynamicReferenceToIndex(Cell cell)
        {
            int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();

            reference = reference.ToUpper();
            for (int ix = 0; ix < reference.Length && reference[ix] >= 'A'; ix++)
            {
                index = (index * 26) + ((int)reference[ix] - 64);
            }

            return index - 1;
        }

        public static string GetDynamicCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue == null)
            {
                return "";
            }
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText.Trim();
            }
            else
            {
                if (cell.StyleIndex == null)
                {

                    return value.Trim();
                }
                var cellformat = GetDynamicCellFormat(cell);

                ////Swoyuj: Cell Format commented for Date values >>>> Added Hardcoded check for Start Date And End Date in Equinox Contract Import in bsImportCOntroller
                /////https: //stackoverflow.com/questions/11781210/c-sharp-open-xml-2-0-numberformatid-range

                //if ((cellformat.NumberFormatId >= 14 && cellformat.NumberFormatId <= 22) ||
                //            (cellformat.NumberFormatId >= 165 && cellformat.NumberFormatId <= 180) ||
                //                cellformat.NumberFormatId == 278 || cellformat.NumberFormatId == 185 || cellformat.NumberFormatId == 196 ||
                //                cellformat.NumberFormatId == 217 || cellformat.NumberFormatId == 326) // Dates
                //{
                //    return DateTime.FromOADate(Convert.ToDouble(value)).ToShortDateString();
                //}

                return value.Trim();
            }
        }

        private static CellFormat GetDynamicCellFormat(Cell cell)
        {
            Worksheet workSheet = cell.Ancestors<Worksheet>().FirstOrDefault();
            SpreadsheetDocument doc = workSheet.WorksheetPart.OpenXmlPackage as SpreadsheetDocument;
            WorkbookPart workbookPart = doc.WorkbookPart;
            int styleIndex = (int)cell.StyleIndex.Value;
            CellFormat cellFormat = (CellFormat)workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ElementAt(styleIndex);
            return cellFormat;
        }

        public static void AppendMultipleSheetWhileDoingExportToExcel(DataSet ds, string Savepath)
        {
            using (var workbook = SpreadsheetDocument.Create(Savepath, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();
                MergeCells mergeCells = new MergeCells();

                uint sheetId = 1;
                int count = 0;
                int head = 7;
                foreach (DataTable table in ds.Tables)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    sheetPart.Worksheet = new Worksheet(sheetData);

                    Columns lstColumns = workbook.WorkbookPart.Workbook.GetFirstChild<Columns>();
                    if (table.TableName == "Site Data List" || table.TableName == "Face Data List")
                    {
                        string tblName = table.TableName;
                        ChangetheColumnWidth(lstColumns, sheetPart, tblName, 0);
                    }

                    Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);
                    uint rowIndex = 1;

                    int numberofCols = table.Columns.Count;
                    bool[] IsNumericColumn = new bool[numberofCols];
                    string[] excelColumnNames = new string[numberofCols];
                    for (int n = 0; n < numberofCols; n++)
                        excelColumnNames[n] = GetDynamicExcelColumnName(n);

                    if (count == 0)
                    {
                        WorkbookStylesPart wspContractImport = workbookPart.AddNewPart<WorkbookStylesPart>();
                        wspContractImport.Stylesheet = GenerateDynamicStylesheetDefault();
                        wspContractImport.Stylesheet.Save();
                        count = count + 1;
                    }

                    if (sheets.Elements<Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);
                    Row headerRow = new Row();
                    List<String> columns = new List<string>();
                    int colIndex = 0;
                    foreach (DataColumn column in table.Columns)
                    {
                        // Below cell.StyleIndex always start from 1 you can add font as well as cellFormat or Color fill but adding it on Font and CellFormat it always start from index 0 but for cell.StyleIndex it start from 1 index 
                        columns.Add(column.ColumnName);
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        cell.CellReference = excelColumnNames[colIndex] + "1";

                        Cell cellStyleIndx = DynamicCellStyleIndex(column.ColumnName, table.TableName);
                        cell.StyleIndex = cellStyleIndx.StyleIndex;

                        headerRow.AppendChild(cell);
                        colIndex++;
                    }
                    sheetData.AppendChild(headerRow);


                    if (table.TableName == "Site Data List" || table.TableName == "Face Data List")
                    {
                        var newTable = new DataTable();
                        var tempRow = new[] { "" };

                        if (table.TableName == "Site Data List")
                            tempRow = new[] { "Code", "Name", "CountryCode", "CountryCode", "CountryName", "Id", "Description", "Id", "Description", "Name", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description" };
                        else if (table.TableName == "Face Data List")
                            tempRow = new[] { "Id", "Description", "Id", "Description", "Id", "Description", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Id", "Description", "Description", "Description", "Id", "Description" };

                        foreach (var array in columns)
                            newTable.Columns.Add(array);

                        newTable.Rows.Add(tempRow);

                        foreach (DataRow dsrow in newTable.Rows)
                        {
                            int innerColIndex = 0;
                            rowIndex++;
                            Row newRow = new Row();
                            foreach (String col in columns)
                            {
                                Stylesheet stylesheet1 = new Stylesheet();
                                Cell cell = new Cell();
                                cell.DataType = CellValues.String;
                                cell.CellValue = new CellValue(dsrow[col].ToString());
                                cell.CellReference = excelColumnNames[innerColIndex] + rowIndex.ToString();

                                Cell cellStyleIndx = DynamicCellStyleIndex(dsrow[col].ToString(), table.TableName);
                                cell.StyleIndex = cellStyleIndx.StyleIndex;

                                newRow.AppendChild(cell);
                                innerColIndex++;
                            }

                            sheetData.AppendChild(newRow);
                        }
                    }

                    foreach (DataRow dsrow in table.Rows)
                    {
                        int innerColIndex = 0;
                        rowIndex++;
                        Row newRow = new Row();
                        foreach (String col in columns)
                        {
                            Stylesheet stylesheet1 = new Stylesheet();
                            Cell cell = new Cell();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            cell.CellReference = excelColumnNames[innerColIndex] + rowIndex.ToString();

                            Cell cellStyleIndx = DynamicCellStyleIndex(dsrow[col].ToString(), table.TableName);
                            cell.StyleIndex = cellStyleIndx.StyleIndex;

                            newRow.AppendChild(cell);
                            innerColIndex++;
                        }

                        sheetData.AppendChild(newRow);
                    }
                }
                workbook.WorkbookPart.Workbook.Save();
            }
        }

        private static string GetDynamicExcelColumnName(int columnIndex, int offset = 0)
        {
            if ((columnIndex - offset) < 26)
                return ((char)('A' + (columnIndex - offset))).ToString();

            char firstChar = (char)('A' + ((columnIndex - offset) / 26) - 1);
            char secondChar = (char)('A' + ((columnIndex - offset) % 26));

            return string.Format("{0}{1}", firstChar, secondChar);
        }

        public static void ChangetheColumnWidth(Columns cols, WorksheetPart sheetPart, string tableName, uint number)
        {
            Columns lstColumns = cols;
            Boolean needToInsertColumns = false;
            if (lstColumns == null)
            {
                lstColumns = new Columns();
                needToInsertColumns = true;
            }
            if (tableName == "Site Data List")
            {
                MergeCells mergeCells = new MergeCells();
                mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:C1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("D1:E1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("F1:G1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("H1:I1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("K1:L1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("M1:N1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("O1:P1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("Q1:R1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("S1:T1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("U1:V1") });
                sheetPart.Worksheet.InsertAfter(mergeCells, sheetPart.Worksheet.Elements<SheetData>().First());

                lstColumns.Append(new Column() { Min = 1, Max = 1, Width = 15, CustomWidth = true });
                lstColumns.Append(new Column() { Min = 2, Max = 2, Width = 15, CustomWidth = true });
                if (needToInsertColumns)
                    sheetPart.Worksheet.InsertAt(lstColumns, 0);
            }

            if (tableName == "Face Data List")
            {
                MergeCells mergeCells = new MergeCells();
                mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:B1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("C1:D1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("E1:F1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("H1:I1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("J1:K1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("L1:M1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("N1:O1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("P1:Q1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("R1:S1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("T1:U1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("V1:W1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("X1:Y1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("Z1:AA1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AB1:AC1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AE1:AF1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AG1:AH1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AI1:AJ1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AK1:AL1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AM1:AN1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AO1:AP1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AQ1:AR1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AS1:AT1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AU1:AV1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("AY1:AZ1") });
                sheetPart.Worksheet.InsertAfter(mergeCells, sheetPart.Worksheet.Elements<SheetData>().First());

                lstColumns.Append(new Column() { Min = 1, Max = 1, Width = 15, CustomWidth = true });
                lstColumns.Append(new Column() { Min = 2, Max = 2, Width = 15, CustomWidth = true });
                if (needToInsertColumns)
                    sheetPart.Worksheet.InsertAt(lstColumns, 0);
            }

        }

        public static Stylesheet GenerateDynamicStylesheetDefault()
        {
            Stylesheet styleSheet = null;

            Fonts fonts = new Fonts(
                new DocumentFormat.OpenXml.Spreadsheet.Font( // Index 0 - default
                     new FontSize() { Val = 11 },
                     new FontName() { Val = "Calibri" }
                ),
                new Font( // Index 1 - header
                    new FontSize() { Val = 11 },
                    new FontName() { Val = "Calibri" },
                    new Bold()
                )
            );

            Fills fills = new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }), // Index 0 - default
                    new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // Index 1 - default
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = TranslateForeground(System.Drawing.ColorTranslator.FromHtml("#d1d1d1")) } }) { PatternType = PatternValues.Solid }), // dark Gray
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = TranslateForeground(System.Drawing.ColorTranslator.FromHtml("#D9E1F2")) } }) { PatternType = PatternValues.Solid }), // light blue
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = TranslateForeground(System.Drawing.ColorTranslator.FromHtml("#FFFF00")) } }) { PatternType = PatternValues.Solid }) // Yellow
            );

            Borders borders = new Borders(
                new Border() // index 0 default
            );

            CellFormats cellFormats = new CellFormats(
                new CellFormat(), // default                    
                new CellFormat() { FontId = 1, FillId = 2, BorderId = 0, ApplyBorder = true, ApplyFill = true }, // body index 1
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 1, FillId = 2, BorderId = 0, ApplyBorder = true, ApplyFill = true }, // body index 2
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 1, FillId = 3, BorderId = 0, ApplyBorder = true, ApplyFill = true }, // body index 3
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 0, BorderId = 0, ApplyBorder = true, ApplyFill = true }, // body index 4
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 1, FillId = 4, BorderId = 0, ApplyBorder = true, ApplyFill = true } // body index 5
            );

            styleSheet = new Stylesheet(fonts, fills, borders, cellFormats);

            return styleSheet;
        }

        private static HexBinaryValue TranslateForeground(System.Drawing.Color fillColor)
        {
            return new HexBinaryValue()
            {
                Value = System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(fillColor.A, fillColor.R, fillColor.G, fillColor.B)).Replace("#", "")
            };
        }

        public static Cell DynamicCellStyleIndex(string cellName, string columnCalledFrom)
        {
            Cell cell = new Cell();
            if (columnCalledFrom == "Import Face")
            {
                if (cellName == "SiteID" || cellName == "VendorSiteID" || cellName == "FaceID" || cellName == "VendorFaceID")
                    cell.StyleIndex = 5U;
                else
                    cell.StyleIndex = 1U;
            }

            if (columnCalledFrom == "Site Data List")
            {
                cell.StyleIndex = 4U;
                if (cellName == "State" || cellName == "Country" || cellName == "MainRoadType" || cellName == "MediaType" || cellName == "Installer"
                        || cellName == "CrossRoadType" || cellName == "SpeedLimit" || cellName == "LocationCategory" || cellName == "StructureType"
                        || cellName == "PanelType" || cellName == "StructureConfiguration")
                    cell.StyleIndex = 2U;
                if (cellName == "Code" || cellName == "Name" || cellName == "CountryCode" || cellName == "CountryName" || cellName == "Id" || cellName == "Description")
                    cell.StyleIndex = 3U;
            }

            if (columnCalledFrom == "Face Data List")
            {
                cell.StyleIndex = 4U;
                if (cellName == "Illuminated" || cellName == "IlluminationType" || cellName == "SignCategory" || cellName == "InstallationCategory"
                    || cellName == "SignFaces" || cellName == "MapFaces" || cellName == "ReaderSide" || cellName == "IlluminationSwitch"
                    || cellName == "IlluminationSchedule" || cellName == "IlluminationCustomTimes" || cellName == "PrintMedium" || cellName == "PrintSpecification"
                    || cellName == "InstallationIncludedInPrice" || cellName == "Digital" || cellName == "FaceType" || cellName == "FaceConstruction"
                    || cellName == "FaceLayout" || cellName == "MinimumContractDuration" || cellName == "BoardCanTakeExtension" || cellName == "VirtualFace"
                    || cellName == "Area" || cellName == "Visibility" || cellName == "SkirtLogo" || cellName == "Trim" || cellName == "TrimColor" || cellName == "ManagedBy"
                    || cellName == "TrafficFlow" || cellName == "PremiumFace")
                    cell.StyleIndex = 2U;
                if (cellName == "Id" || cellName == "Description")
                    cell.StyleIndex = 3U;
            }

            return cell;
        }

        System.Data.DataTable IExcelHelper.ExcelToDataTable(string fileName, string sheetName)
        {
            throw new NotImplementedException();
        }
    }
}
