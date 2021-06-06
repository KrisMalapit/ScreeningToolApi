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
            bool levelidisnull = false;
            
            try
            {
                if (rptVM.LevelId == null)
                {
                    levelidisnull = true;
                }

               

                DataSet ds = new DataSet();
                LocalReport LocalReport = new LocalReport
                {
                    ReportPath = baseDir + "\\Reports\\" + report + ".rdlc"
                };


                DateTime def = new DateTime(1, 1, 1);

                
                if (!levelidisnull &&  rptVM.LevelId[0] == 6)
                {

                    LocalReport.ReportPath = baseDir + "\\Reports\\rptNonCompliant.rdlc";


                    var _dts = NonCompliant(rptVM.fromDate, rptVM.toDate);
                    
                    ReportDataSource _datasources = new ReportDataSource("NonCompliant", _dts);
                    LocalReport.DataSources.Clear();
                    LocalReport.DataSources.Add(_datasources);
                    return LocalReport.Render(rptVM.rptType);
                }
               



                    var v = db.ScreenLogs.Select(a => new
                    {

                        a.EmployeeId
                    ,
                        EmployeeName = a.LastName + ", " + a.FirstName
                   ,
                        Company = db.Employees.Where(b => b.EmployeeId == a.EmployeeId).FirstOrDefault().Department.Company.Name
                   ,
                        Department = db.Employees.Where(b => b.EmployeeId == a.EmployeeId).FirstOrDefault().Department.Code
                   ,
                        db.Employees.Where(b => b.EmployeeId == a.EmployeeId).FirstOrDefault().Barangay
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
                        db.Employees.Where(b => b.EmployeeId == a.EmployeeId).FirstOrDefault().PickUpPoint
                    ,
                        DateTransaction = a.TransactionDate
                    ,
                        a.WorkPlace

                    });






                    if (rptVM.fromDate != def)
                    {
                        v = v.Where(a => a.DateTransaction >= rptVM.fromDate && a.DateTransaction <= rptVM.toDate);
                    }

                    if (rptVM.LevelId != null)
                    {

                            if (rptVM.LevelId[0] == 5)
                            {

                            }
                            else
                            {
                        v = v.Where(a => rptVM.LevelId.Contains(a.LevelId));
                    }

                       


                    }


                    var lsts = v.OrderBy(a => a.EmployeeName).ToList();
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
       
        public DataTable NonCompliant (DateTime fromDate,DateTime toDate)
        {

            string status = "";
            string message = "";
            string fullfilename = "";
            string filename = "";
            string constr = @"Server=192.168.70.102; Database=ScreeningTool; Uid=ict; Pwd=ict@ictdept;";
            SqlConnection conn = new SqlConnection(constr);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            var fdate  = fromDate.Date.ToString("MM/dd/yyyy");
            var tdate = toDate.Date.ToString("MM/dd/yyyy");

            try
            {
                string sqlCmd = "SELECT A.TransactionDate,A.EmployeeId,A.EmployeeName,'" + fdate  + "' as FromDate,'"+ tdate + "' as ToDate from " +
                                 "(SELECT distinct A.TransactionDate,B.EmployeeId,B.LastName + ', ' + B.FirstName AS EmployeeName " +
                                 "FROM [ScreeningTool].[dbo].[ScreenLogs] A " +
                                 "CROSS JOIN (SELECT DISTINCT EmployeeId,[FirstName],[LastName] " +
                                 "FROM [ScreeningTool].[dbo].[Employees] where Status = 'Active') B " +
                                 "WHERE A.TransactionDate between '" + fromDate + "' " + "and '" + toDate + "'" +
                                 ") A " +
                                 "LEFT JOIN(SELECT distinct " +
                                 "TransactionDate,EmployeeId,LastName " +
                                 "FROM [ScreeningTool].[dbo].[ScreenLogs]) B ON A.EmployeeId = B.EmployeeId AND A.TransactionDate = B.TransactionDate " +
                                 "WHERE LastName IS NULL " +
                                 "ORDER BY EmployeeId";




                conn.Open();
                
                ds.Tables.Add(dt);
                using (var da = new SqlDataAdapter(sqlCmd, conn))
                {
                    da.Fill(dt);

                }
                conn.Close();
                conn.Dispose();

               


            }
            catch (Exception e)
            {
             

                status = "failed";
                message = e.InnerException.Message.ToString(); 

            }


            var model = new
            {
                status,
                message
            };


            return dt;
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

