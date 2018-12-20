using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ipsi.Models;
using Ipsi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Ipsi.Controllers
{
    [Authorize]
    public class ExcelController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private ExcelService excelService;
        public ExcelController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            excelService = new ExcelService();
        }


        public async Task<IActionResult> Export()
        {
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string sFileName = @"demo.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);

            TestModel model1 = new TestModel();
            model1.Id = 1;
            model1.Emp_No = "011";
            model1.Name = "홍길동";
            model1.Age = 28;
            model1.Dept = "정보전산과";

            TestModel model2 = new TestModel();
            model2.Id = 2;
            model2.Emp_No = "012";
            model2.Name = "이순신";
            model2.Age = 33;
            model2.Dept = "입학관리과";

            TestModel model3 = new TestModel();
            model3.Id = 3;
            model3.Emp_No = "013";
            model3.Name = "권율";
            model3.Age = 47;
            model3.Dept = "경리과";

            List<TestModel> list = new List<TestModel>();
            list.Add(model1);
            list.Add(model2);
            list.Add(model3);

            return File(await excelService.Export(sWebRootFolder, sFileName, list), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        }
    }
}