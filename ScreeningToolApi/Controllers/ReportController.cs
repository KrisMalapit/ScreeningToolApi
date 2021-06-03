using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using ScreeningToolApi.Models.View_Model;
using ScreeningToolApi.Models;
using System.Globalization;


namespace ScreeningToolApi.Controllers
{
    public class ReportController : ApiController
    {
        private ScreeningToolEntities db = new ScreeningToolEntities();
      


        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/printreport")]

        public byte[] PrintReports(string rvm)
        {
            
           
            var o = JsonConvert.DeserializeObject(rvm);
            ReportViewModel rptVM = JsonConvert.DeserializeObject<ReportViewModel>(rvm);

            string report = rptVM.Report;

            
            try
            {
                //var xxx = db.ScreenLogs.ToList();



                DataSet ds = new DataSet();
                LocalReport LocalReport = new LocalReport
                {
                    ReportPath = baseDir + "\\Reports\\" + report + ".rdlc"
                };


                DateTime def = new DateTime(1, 1, 1);




                var v = db.ScreenLogs.Select(a => new {

                    a.EmployeeId
                    ,
                    EmployeeName = a.LastName + ", " + a.FirstName
                   , Company = db.Employees.Where(b => b.EmployeeId == a.EmployeeId).FirstOrDefault().Department.Company.Name
                   , Department = db.Employees.Where(b => b.EmployeeId == a.EmployeeId).FirstOrDefault().Department.Code
                   , db.Employees.Where(b => b.EmployeeId == a.EmployeeId).FirstOrDefault().Barangay
                    ,
                    a.Age
                    ,
                    a.City
                    ,
                    a.ContactNo
                    ,
                    a.Remarks
                    ,
                    a.Result
                    ,
                    Category = a.Result == 0 ? "No Risk" : (a.Result == 1 ? "Low Risk" : (a.Result <= 3 ? "Moderate Risk" : "High Risk"))
                    ,
                    FromDate = rptVM.fromDate
                    ,
                    ToDate = rptVM.toDate
                    ,
                    CreatedDate = a.CreatedAt
                    ,
                    LevelId = a.Result == 0 ? 1 : (a.Result == 1 ? 2 : (a.Result <= 3 ? 3 : 4))
                    ,
                    db.Employees.Where(b=>b.EmployeeId == a.EmployeeId).FirstOrDefault().PickUpPoint
                    ,DateTransaction = a.TransactionDate
                    ,a.WorkPlace

                });




                

                    if (rptVM.fromDate != def)
                    {
                        v = v.Where(a => a.DateTransaction >= rptVM.fromDate && a.DateTransaction <= rptVM.toDate);
                    }

                    if (rptVM.LevelId != null)
                    {
                        
                          v = v.Where(a => rptVM.LevelId.Contains(a.LevelId));
                       
                       
                    }
                   

                    var lsts = v.OrderBy(a=>a.EmployeeName).ToList();
                    DataTable dts = new DataTable();
                    dts = ToDataTable(lsts);
                    ReportDataSource datasources = new ReportDataSource("ScreenLogs", dts);
                    LocalReport.DataSources.Clear();
                    LocalReport.DataSources.Add(datasources);
                    return LocalReport.Render(rptVM.rptType);

                   


            }
            catch (Exception e)
            {

                throw;
            }


        }
        
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

       
    }
       

    }

