
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ipsi.Services
{
    public class ExcelService
    {
        public async Task<MemoryStream> Export<T>(string path, string fileName, List<T> listModel)
        {
            // string sWebRootFolder = _hostingEnvironment.WebRootPath;
            // string sFileName = @"demo.xlsx";
            // string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            FileInfo file = new FileInfo(Path.Combine(path, fileName));
            var memory = new MemoryStream();
            using (var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Demo");

                var props = listModel[0].GetType().GetProperties();

                IRow row = excelSheet.CreateRow(0);

                for (int i = 0; i <= listModel.Count; i++)
                {
                    int j = i - 1;
                    row = excelSheet.CreateRow(i);



                    int k = 0;
                    foreach (var p in props)
                    {
                        if (i == 0)
                        {
                            row.CreateCell(k).SetCellValue(p.Name);
                        }
                        else
                        {

                            if (p.GetValue(listModel[j]) != null)
                            {
                                if (p.PropertyType == typeof(System.Int32))
                                {
                                    row.CreateCell(k).SetCellValue(int.Parse(p.GetValue(listModel[j]).ToString()));
                                }
                                else if (p.PropertyType == typeof(System.Decimal) || p.PropertyType == typeof(System.Double))
                                {
                                    row.CreateCell(k).SetCellValue(double.Parse(p.GetValue(listModel[j]).ToString()));
                                }
                                else
                                {
                                    row.CreateCell(k).SetCellValue(p.GetValue(listModel[j]).ToString());
                                }

                            }
                        }
                        k++;

                    }
                }

                workbook.Write(fs);
            }
            using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return memory;//File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}