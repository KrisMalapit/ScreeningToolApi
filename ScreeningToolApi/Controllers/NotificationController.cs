using Newtonsoft.Json;
using ScreeningToolApi.Models;
using ScreeningToolApi.Models.View_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ScreeningToolApi.Controllers
{
    public class NotificationController : ApiController
    {
        private readonly ScreeningToolEntities db = new ScreeningToolEntities();
        readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private string[] phonenumber;
        private string[] emailaddress;


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/greet")]
        public string Greet()
        {

            return "Hi";
        }
  
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("api/savedata")]
        public IHttpActionResult SaveData(Models.ScreenLog sclogs)
        {


            sclogs.Id = 0;
            string status = "";
            string message = "";
            string empname = "";
            int totalScore = 0;
            string remarks = "";
            string messageToUser = "";
            int deptId = 0;
   

            try
            {

                var emp = db.Employees.Where(a => a.EmployeeId == sclogs.EmployeeId).FirstOrDefault();
                deptId = emp.DepartmentId;
                totalScore = GetTotalScore(sclogs);
                remarks = ScoreRemarks(sclogs);
                sclogs.Id = 0;
                empname = emp.LastName + ", " + emp.FirstName;
                sclogs.FirstName = emp.FirstName;
                sclogs.LastName = emp.LastName;
                sclogs.Barangay = emp.Barangay;
                sclogs.City = emp.City;
                sclogs.Age = emp.Age;
                sclogs.ContactNo = emp.ContactNo;
                sclogs.ContactPerson = emp.ContactPerson;
                sclogs.CreatedAt = DateTime.Now;
                sclogs.TransactionDate = DateTime.Now.Date;
                sclogs.Result = totalScore;
                db.ScreenLogs.Add(sclogs);
                db.SaveChanges();

                Log log = new Log();
                log.Action = "Add - SMS";
                log.Description = "Add new Screen Log. ID : " + sclogs.Id;
                log.Status = "Success";
                log.CreatedDate = DateTime.Now;
                db.Logs.Add(log);
                db.SaveChanges();
                status = "success";
                if (totalScore > 2)
                {
                    status = SendNotification(empname, remarks, totalScore, deptId, sclogs.Id);
                }
              

                
                message = "";



                
                switch (totalScore)
                {
                    case 0:
                        messageToUser = "The results show that your FIT to report to work. Please continue observing proper hand hygiene and always wear your face mask. For questions, please get in touch with our HR Officer Nico Ching, 09178860524 or through email: hrod@semirarampc.com.";
                        break;
                    case 1:
                        messageToUser = "Please be informed that your results show that your condition/exposure is LOW and you are allowed to report to work. We would like you to keep monitoring your condition and it is also recommended that you consult with a Medicard Affiliated Doctor. This information is also sent to your immediate supervisor. For questions, please get in touch with our HR Officer Nico Ching, 09178860524 or through email: hrod@semirarampc.com.";
                        break;
                    case 2:
                    case 3:
                        messageToUser = "Please be informed that your results show that your condition/exposure is MODERATE. You are not allowed to report to work at this time. Stay home and undergo the mandatory quarantine period and please consult a medical doctor through an accredited MEDICARD facility. Please secure a BHERT Certification from your Barangay. This information is also sent to your immediate supervisor. For questions, please get in touch with our HR Officer Nico Ching, 09178860524 or through email: hrod@semirarampc.com.";
                        break;
                    default:
                        messageToUser = "Please be informed that your results show that your condition/ exposure is HIGH.You are not allowed to report to work at this time.Stay home and undergo the mandatory quarantine period and please consult a medical doctor through an accredited MEDICARD facility.Please secure a BHERT Certification from your Barangay. This information is also sent to your immediate supervisor. For questions, please get in touch with our HR Officer Nico Ching, 09178860524 or through email: hrod @semirarampc.com.";
                        break;
                }


            }
            catch (Exception e)
            {
                status = "fail";
                message = e.Message.ToString();

            }

            

            var xxx = new
            {
                empname,
                sclogs.ContactNo,
                totalScore,
                sclogs.Id,
                status,
                message,
                messageToUser = messageToUser + " Your RefId is " + sclogs.Id

            };

            return Ok(xxx);
            //return totalScore;


        }
        private int GetTotalScore(Models.ScreenLog ts)
        {


            int q1 = ts.Q1 == 1 ? 1 : 0;
            int q2 = ts.Q2 == 1 ? 1 : 0;
            int q3 = ts.Q3 == 1 ? 1 : 0;
            int q4 = ts.Q4 == 1 ? 1 : 0;
            int q5 = ts.Q5 == 1 ? 1 : 0;
            int q6 = ts.Q6 == 1 ? 1 : 0;
            int q7 = ts.Q7 == 1 ? 1 : 0;
            int q8 = ts.Q8 == 1 ? 2 : 0;
            int q9 = ts.Q9 == 1 ? 2 : 0;
            int q10 = ts.Q10 == 1 ? 3 : 0;
            int q11 = ts.Q11 == 1 ? 4 : 0;
            int q12 = ts.Q12 == 1 ? 2 : 0;

            int res = q1 + q2 + q3 + q4 + q5 + q6 + q7 + q8 + q9 + q10 + q11 + q12;
            return res;
        }
        private string ScoreRemarks(Models.ScreenLog ts)
        {

            string rem = "";

            rem = ts.Q1 == 1 ? "SC-P-HW-MC," : "";
            rem += ts.Q2 == 1 ? "COUGH," : "";
            rem += ts.Q3 == 1 ? "COLDS," : "";
            rem += ts.Q4 == 1 ? "DIARRHEA," : "";
            rem += ts.Q5 == 1 ? "SORE THROAT," : "";
            rem += ts.Q6 == 1 ? "BODY WEAKNESS," : "";
            rem += ts.Q7 == 1 ? "HEADACHE," : "";
            rem += ts.Q8 == 1 ? "DIFFICULTY OF BREATHING," : "";
            rem += ts.Q9 == 1 ? "EASY FATIGUABILITY," : "";
            rem += ts.Q10 == 1 ? "LIVING IN A Household WITH A PUI or CONFIRMED COVID-19 Case," : "";
            rem += ts.Q11 == 1 ? "DIRECT-CONTACT," : "";
            rem += ts.Q12 == 1 ? "PUI or CONFIRMED COVID-19 Case 100 meters away," : "";

            if (rem.Trim() != "")
            {
                rem = rem.Remove(rem.Length - 1);
            }

            
            return rem;
        }
        private string SendNotification(string empName, string remarks, int totalScore, int deptId, int screenlog_id)
        {

            string status = "";
            Task<string> task = SetDepartmentHeadDetails(deptId, screenlog_id);
            task.Wait();
      


            try
            {

                string smsResult = SendSMS("COVID-19 SCREENING TOOL Infoblast" + Environment.NewLine + Environment.NewLine + empName + " has a total score of " + totalScore
                                                              + Environment.NewLine + "Remarks:" + Environment.NewLine + remarks, screenlog_id);


                status = SendEmail("COVID-19 SCREENING TOOL Infoblast <br /><br />" + empName + " has a total score of " + totalScore
                               + "<br /> Remarks: <br />" + remarks + ". <br /><br />", screenlog_id, "Employee");


                status = "success";
            }
            catch (Exception e)
            {
                status = "fail " + e.Message;

            }

            return status;

        }
        private async Task<string> SetDepartmentHeadDetails(int deptId, int screenlog_id)
        {
            string status = "";
            try
            {
                var dept = await db.Departments.FindAsync(deptId);
                string deptHeads = dept.DepartmentHeads;
                var collection = deptHeads.Split(',');
                phonenumber = new string[collection.Length + 6];
                emailaddress = new string[collection.Length];
                int cnt = 0;
                foreach (var item in collection)
                {
                    int id = Convert.ToInt32(item);
                    var deptHeadDetail = db.DepartmentHeads.Find(id);
                    phonenumber[cnt] = deptHeadDetail.ContactNo;
                    emailaddress[cnt] = deptHeadDetail.Email;
                    cnt++;
                }

                phonenumber[cnt] = "09178860524"; // ramonico ching
                phonenumber[cnt + 1] = "09197989212"; // charizza
                phonenumber[cnt + 2] = "09985974855"; // jonathan
                phonenumber[cnt + 3] = "09257061809"; // fides quintos
                phonenumber[cnt + 4] = "09173121719"; // joyce lagarde
                phonenumber[cnt + 5] = "09953668620"; // raph

                status = "success";
            }
            catch (Exception e)
            {
                status = "fail :" + e.Message ;

            }


            Log log = new Log();
            log.Action = "Get Dept Head Details";
            log.Description = "Get DeptHead Details. Details phonenos " + phonenumber.ToString() + " emails " + emailaddress.ToString() + " SceenLog Id : " + screenlog_id;
            log.Status = status;
            db.Logs.Add(log);
            db.SaveChanges();



            return "Done Set Dept Head";

        }
        public string SendSMS(string msg, int screenid)
        {
            //msg = msg.Remove(msg.Length - 1);
            SMSArray res = new SMSArray();
            try
            {

                var sms = new
                {
                    message = msg,
                    PhoneNumbers = phonenumber,
                };

                string xstring = JsonConvert.SerializeObject(sms);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = new HttpResponseMessage();
                response = client.GetAsync("http://192.168.70.74/smsapi/api/smsweb?cmd=" + xstring).Result; //live

                if (response.IsSuccessStatusCode)
                {
                    res.message = "200";
                }
                else
                {
                    res.message = response.StatusCode.ToString();
                }




            }
            catch (Exception e)
            {

                res.message = e.Message.ToString();
            }

            Log log = new Log();
            log.Action = "Send SMS";
            log.Description = "Send SMS by SceenLog Id : " + screenid;
            log.Status = res.message;
            log.CreatedDate = DateTime.Now;
            db.Logs.Add(log);
            db.SaveChanges();

            return res.message;

        }
        private string SendEmail(string msg, int screenid, string sendertype)
        {
            SMSArray res = new SMSArray();

            try
            {
                //msg = msg.Remove(msg.Length - 1);
                var body = msg;
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("webhelpdeskadmin@semirarampc.com", "Screening Tool Notification");



                //get sup email
                if (sendertype == "Employee")
                {
                    mail.To.Add(new MailAddress("hrod@semirarampc.com"));
                    foreach (var item in emailaddress)
                    {
                        mail.CC.Add(new MailAddress(item));
                    }

                }
                else
                {
                    mail.To.Add(new MailAddress("hrod@semirarampc.com"));
                }



                //get sup email


                mail.Bcc.Add(new MailAddress("rpgustilo@semirarampc.com"));
                //mail.Bcc.Add(new MailAddress("kcmalapit@semirarampc.com"));

                mail.Subject = "Screening Tool";
                mail.Body = string.Format(body + " Click on this link to view details. https://ictsystems.semirarampc.com:8443/ScreeningTool");
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient()) //mail server
                {
                    try
                    {
                        smtp.Host = "mail.hoaccess.com";
                        smtp.Credentials = new System.Net.NetworkCredential(@"smcdacon\webhelpdeskadmin", "Str@wb3rry##");
                        smtp.Port = 587;
                        smtp.EnableSsl = false;
                        smtp.Send(mail);

                    }
                    catch (Exception e)
                    {


                        Environment.Exit(0);
                    }

                }
                res.message = "Done Email";

            }
            catch (Exception e)
            {

                res.message = e.Message.ToString();
            }

            Log log = new Log();
            log.Action = "Send Email";
            log.Description = "Send Email by SceenLog Id : " + screenid;
            log.Status = res.message;
            log.CreatedDate = DateTime.Now;
            db.Logs.Add(log);
            db.SaveChanges();
            return res.message;
        }
    }
}