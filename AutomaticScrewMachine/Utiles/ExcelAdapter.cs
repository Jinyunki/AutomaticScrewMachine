using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace AutomaticScrewMachine.Utiles {
    public static class ExcelAdapter {
        /// <summary>
        /// 해당 경로에, 파일이 존재하는지 체크 결과 입니다 True존재 False미존재
        /// </summary>
        public static bool IsConnectCheck { get; set; }
        public static List<string> WorkSheetNameList;
        public static List<string> ColumnData;
        static ExcelAdapter () {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
        public static void Connect (string IsFolderName, string IsFileName, int sheetIndex) {
            if (IsFolderName != null && IsFileName != null) {
                GetReadData(IsFolderName, IsFileName, sheetIndex);
            } else {
                Console.WriteLine("IsFolderName 를 입력해 주세요");
                Console.WriteLine("IsFileName 를 입력해 주세요 ");
            }
        }
        public static ObservableCollection<List<string>> GetReadData (string IsFolderName, string IsFileName, int sheetIndex) {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");

            try {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string folderName = Path.Combine(appPath, IsFolderName);
                string FilePath = Path.Combine(folderName, IsFileName);

                using (var package = new ExcelPackage(new FileInfo(FilePath))) {
                    IsConnectCheck = true;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetIndex]; // 시트 선택

                    ExcelWorksheets excelWorksheets = package.Workbook.Worksheets;
                    WorkSheetNameList = excelWorksheets.Select(x => x.Name).ToList();


                    int IsColCount = worksheet.Dimension.Columns; // 가로줄의 개수 ( 좌에서 우 )
                    int IsRowCount = worksheet.Dimension.Rows; // 세로줄의 개수 ( 위에서 아래 )

                    ObservableCollection<List<string>> TotalDataList = new ObservableCollection<List<string>>();

                    for (int row = 1; row <= IsRowCount; row++) {
                        ColumnData = new List<string>();

                        for (int col = 1; col <= IsColCount; col++) {
                            string cellValue = worksheet.Cells[row, col].Text;
                            ColumnData.Add(cellValue);
                        }

                        TotalDataList.Add(ColumnData);
                    }

                    package.Dispose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    return TotalDataList;
                }

            } catch (Exception ex) {
                IsConnectCheck = false;
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        public static void Save (string IsFolderName, string IsFileName) {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string folderName = Path.Combine(appPath, IsFolderName);
                string FilePath = Path.Combine(folderName, IsFileName);

                if (File.Exists(FilePath)) {
                    IsConnectCheck = true;
                    using (var package = new ExcelPackage(new FileInfo(FilePath))) {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        ExcelWorksheets excelWorksheets = package.Workbook.Worksheets;
                        excelWorksheets[0].Name = "Sequence TEST"; // SheetName
                                                                   //worksheet.Cells[1, 1].Value = "TEST"; // In Index를 어떻게 참조 할 것인가 


                        package.Save();
                        package.Dispose();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                } else {
                    IsConnectCheck = false;
                    Console.WriteLine("CheckFilePath Fail");
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        public static void Remove (string IsFolderName, string IsFileName, int worksheetIndex) {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {

                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string folderName = Path.Combine(appPath, IsFolderName);
                string FilePath = Path.Combine(folderName, IsFileName);

                using (var package = new ExcelPackage(new FileInfo(FilePath))) {
                    ExcelWorksheets excelWorksheets = package.Workbook.Worksheets;

                    if (worksheetIndex >= 0 && worksheetIndex < excelWorksheets.Count) {
                        // 워크시트가 존재하는 경우에만 제거
                        excelWorksheets.Delete(worksheetIndex); // 인덱스는 1부터 시작하므로 +1
                        package.Save();
                    } else {
                        // 유효하지 않은 인덱스 처리
                        Console.WriteLine("Invalid worksheet index");
                    }
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }

        public static void Add (string IsFolderName, string IsFileName, string newWorksheetName, List<List<string>> dataList) {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string folderName = Path.Combine(appPath, IsFolderName);
                string filePath = Path.Combine(folderName, IsFileName);

                // 열 헤더를 정의합니다.
                var item = new List<string>() { "Name", "X Value", "Y Value", "Z Value", "Torq", "Depth" };

                using (var package = new ExcelPackage(new FileInfo(filePath))) {
                    // 새로운 워크시트 생성
                    var newWorksheet = package.Workbook.Worksheets.Add(newWorksheetName);

                    // 첫 행에 정보 저장
                    for (int colIndex = 0; colIndex < item.Count; colIndex++) {
                        newWorksheet.Cells[1, colIndex + 1].Value = item[colIndex];
                    }

                    // 나머지 행에 데이터 저장
                    for (int rowIndex = 2; rowIndex <= dataList.Count + 1; rowIndex++) {
                        var rowData = dataList[rowIndex - 2]; // 각 행의 전체 데이터 가져오기
                        for (int colIndex = 0; colIndex < rowData.Count; colIndex++) {
                            newWorksheet.Cells[rowIndex, colIndex + 1].Value = rowData[colIndex];
                        }
                    }

                    package.Save();
                }
            } catch (Exception ex) {
                // 동일한 이름의 워크시트가 이미 존재하는 경우에 대한 예외 처리
                if (ex is InvalidOperationException && ex.Message.Contains("already exists")) {
                    // 원하는 처리를 여기에 추가하세요.
                    // 예를 들어, 새로운 이름을 생성하거나 기존 워크시트를 덮어쓰거나 다른 방식으로 처리할 수 있습니다.
                    newWorksheetName = GenerateNewWorksheetName(newWorksheetName);
                    Add(IsFolderName, IsFileName, newWorksheetName, dataList);
                } else {
                    Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                    throw;
                }
            }
        }
        private static string GenerateNewWorksheetName (string originalName) {
            // 숫자를 찾기 위한 정규표현식 패턴
            string pattern = @"\d+$";

            // 정규표현식을 사용하여 숫자를 찾음
            Match match = Regex.Match(originalName, pattern);

            if (match.Success) {
                // 찾은 숫자를 정수로 변환하고 1을 더함
                int number = int.Parse(match.Value) + 1;

                // 숫자를 새로운 이름에 추가하여 반환
                return Regex.Replace(originalName, pattern, number.ToString());
            } else {
                // 숫자가 없는 경우, "_1"을 추가하여 반환
                return originalName + "_1";
            }
        }

    }
}
