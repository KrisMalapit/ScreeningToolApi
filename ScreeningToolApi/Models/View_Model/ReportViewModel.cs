using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScreeningToolApi.Models.View_Model
{
    public class ReportViewModel
    {
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public int[] LevelId { get; set; }
        public int[] EmployeeName { get; set; }
        public int EmployeeType { get; set; }
        public string Report { get; set; }
        public string rptType { get; set; }
    }
    public class SMSArray
    {
        public string[] PhoneNumbers { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }
    public class Response
    {
        bool IsSuccess = false;
        string Message;
        object ResponseData;
        public Response(bool status, string message, object data)
        {
            IsSuccess = status;
            Message = message;
            ResponseData = data;
        }
    }
}