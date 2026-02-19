using System;
using System.IO;
using System.Web;
using System.Text;
using System.Data;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ClosedXML.Excel;

using DTPortal.Core.Utilities;

namespace DTPortal.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        private readonly ILogClient _logClient;

        protected BaseController(ILogClient logClient)
        {
            _logClient = logClient;
        }

        [NonAction]
        protected virtual void SendAdminLog(string moduleName, string serviceName, string activityName, 
            string logMessageType, string logMessage, string dataTransformation = null, string userName = null)
        {
            userName = (!string.IsNullOrEmpty(userName) ? userName : FullName);
            AdminLogMessage adminLogMessage
                = new AdminLogMessage(moduleName, serviceName, activityName, logMessage, logMessageType, userName, dataTransformation);

            string logWithChecksum = PKIMethods.Instance.AddChecksum(JsonConvert.SerializeObject(adminLogMessage,
               new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
            _logClient.SendAdminLogMessage(logWithChecksum);
        }

        [NonAction]
        public byte[] ExportToPDF(DataTable dataTable)
        {           
            byte[] result = new byte[] { };
            return result;
        }

        [NonAction]
        public byte[] ExportToExcel(DataTable dataTable)
        {
            MemoryStream stream;
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dataTable);
                ws.Columns().AdjustToContents();
                using (stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                }
            }
            return stream.ToArray();
        }

        [NonAction]
        public byte[] ExportToCSV(DataTable dataTable)
        {
            StringBuilder data = new StringBuilder();

            //Taking the column names.
            for (int column = 0; column < dataTable.Columns.Count; column++)
            {
                //Making sure that end of the line, shoould not have comma delimiter.
                if (column == dataTable.Columns.Count - 1)
                    data.Append(dataTable.Columns[column].ColumnName.ToString().Replace(",", ";"));
                else
                    data.Append(dataTable.Columns[column].ColumnName.ToString().Replace(",", ";") + ',');
            }

            data.Append(Environment.NewLine);//New line after appending columns.

            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                for (int column = 0; column < dataTable.Columns.Count; column++)
                {
                    ////Making sure that end of the line, shoould not have comma delimiter.
                    if (column == dataTable.Columns.Count - 1)
                        data.Append(dataTable.Rows[row][column].ToString().Replace(",", ";"));
                    else
                        data.Append(dataTable.Rows[row][column].ToString().Replace(",", ";") + ',');
                }

                //Making sure that end of the file, should not have a new line.
                if (row != dataTable.Rows.Count - 1)
                    data.Append(Environment.NewLine);
            }

            return ASCIIEncoding.ASCII.GetBytes(data.ToString());
        }

        public string FullName
        {
            get { return User.Identity.Name; }
        }

        public string UUID
        {
            get { return HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value; }
        }

        public string Email
        {
            get { return HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value; }
        }

        public string AccessToken
        {
            get { return HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Access_Token").Value; }
        }
    }
}
