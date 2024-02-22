using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ems.dbconn;
using ems.dbconn.Models;
using WebApp.Models;
using WebApp.Authorization;
using System.Data;
using System.Data.Odbc;
using Newtonsoft.Json;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Owin;
using System.Net.Mail;
using System.Web.Mail;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Buffers.Text;
using System.Reflection.Emit;
using System.Web.Http.Results;
using System.Collections;

namespace WebApp.Controllers
{
    [RoutePrefix("api/Login")]
    [AllowAnonymous]
    public class LoginController : ApiController
    {
        dbconn objdbconn = new dbconn();
        // MySqlDataReader objMySqlDataReader;

        OdbcDataReader objMySqlDataReader;
        cmnfunctions objcmnfunctions = new cmnfunctions();
        string dashboard_flag = string.Empty;
        string msSQL = string.Empty;
        int mnResult;
        string user_status;
        string vendoruser_status;
        string tokenvalue = string.Empty;
        string user_gid = string.Empty;
        string employee_gid = string.Empty;
        string department_gid = string.Empty;
        string password = string.Empty;
        string username = string.Empty;
        string departmentname = string.Empty;
        string lscompany_code;
        string lscompany_dbname;
        string domain = string.Empty;
        string lsexpiry_time;
        DataTable dt_datatable;
        string lsuser_password, lsuser_code, lsemployee_mobileno, lsuser_gid, lscompanyid, lscontact_id, lsusercode, msGetGid, msGetGid1;

        [HttpPost]
        [ActionName("UserLogin")]
        public HttpResponseMessage PostUserLogin(PostUserLogin values)
        {
            loginresponse objloginresponse = new loginresponse();
            try
            {
                if (!String.IsNullOrEmpty(values.company_code))
                {
                    var ObjToken = Token(values.user_code, objcmnfunctions.ConvertToAscii(values.user_password), values.company_code.ToLower());
                    dynamic newobj = JsonConvert.DeserializeObject(ObjToken);
                    if (newobj.access_token != null)
                    {
                        tokenvalue = "Bearer " + newobj.access_token;
                        msSQL = "call commondb.adm_mst_spstoretoken('" + tokenvalue + "','" + values.user_code + "','" + objcmnfunctions.ConvertToAscii(values.user_password) + "','" + values.company_code + "')";
                        objMySqlDataReader = objdbconn.GetDataReader(msSQL);

                        if (objMySqlDataReader.HasRows)
                        {
                            objloginresponse.token = tokenvalue;
                            objloginresponse.user_gid = objMySqlDataReader["user_gid"].ToString();
                            objloginresponse.dashboard_flag = objMySqlDataReader["dashboard_flag"].ToString();
                            objloginresponse.c_code = values.company_code;
                            objloginresponse.message = "Login Successful!";
                            objloginresponse.status = true;
                        }
                        else
                        {
                            objloginresponse.message = "Invalid Credentials!";
                        }
                    }
                    else
                    {
                        objloginresponse.message = "Invalid Credentials!";
                    }
                }
                else
                {
                    objloginresponse.message = "Company Code cannot be empty!";
                }
            }
            catch (Exception ex)
            {
                objloginresponse.message = "Exception occured while loggin in!";
            }
            finally
            {
                if (objMySqlDataReader != null)
                    objMySqlDataReader.Close();
            }
            return Request.CreateResponse(HttpStatusCode.OK, objloginresponse);
        }
        [HttpPost]
        [ActionName("UserForgot")]
        public HttpResponseMessage PostUserForgot(PostUserForgot values)
        {
            PostUserForgot GetForgotResponse = new PostUserForgot();
            domain = Request.RequestUri.Host.ToLower();
            string jsonFilePath = @" " + ConfigurationManager.AppSettings["CmnConfigfile_path"].ToString();
            string jsonString = File.ReadAllText(jsonFilePath);
            var jsonDataArray = JsonConvert.DeserializeObject<MdlCmnConn[]>(jsonString);
            string lscompany_dbname = (from a in jsonDataArray
                                       where a.company_code == values.companyid
                                       select a.company_dbname).FirstOrDefault();
            string lscompany_code = (from a in jsonDataArray
                                     where a.company_code == values.companyid
                                     select a.company_code).FirstOrDefault();
            if (lscompany_code != null && lscompany_code != " ")
            {
                msSQL = " SELECT  user_code,user_password,user_gid from adm_mst_tuser    where user_code = '" + values.usercode + "' ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lsuser_code = objMySqlDataReader["user_code"].ToString();
                    lsuser_password = objMySqlDataReader["user_password"].ToString();
                    lsuser_gid = objMySqlDataReader["user_gid"].ToString();
                }

                if (lsuser_code != null && lsuser_code != "")
                {
                    lsuser_code = lsuser_code.ToUpper();
                }
                else
                {
                    lsuser_code = null;

                }
                msSQL = " select   employee_mobileno FROM hrm_mst_temployee     where user_gid = '" + lsuser_gid + "' ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lsemployee_mobileno = objMySqlDataReader["employee_mobileno"].ToString();

                }
                msSQL = " SELECT  company_code FROM adm_mst_tcompany ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lscompany_code = objMySqlDataReader["company_code"].ToString();

                }
                if (lscompany_code != null && lscompany_code != "")
                {
                    lscompany_code = lscompany_code.ToUpper();
                }
                else
                {
                    lscompany_code = null;

                }
                if (values.companyid != null && values.companyid != "")
                {
                    lscompanyid = values.companyid.ToUpper();
                }
                else
                {
                    lscompanyid = null;

                }
                if (values.usercode != null && values.usercode != "")
                {
                    lsusercode = values.usercode.ToUpper();
                }
                else
                {
                    lsusercode = null;

                }

                if (lscompany_code == lscompanyid)
                {
                    if (lsuser_code == lsusercode)
                    {

                        if (lsemployee_mobileno == values.mobile)
                        {
                            //msSQL = " update  adm_mst_tuser set " +
                            //    " user_password = '" + objcmnfunctions.ConvertToAscii(values.password) + "'," +
                            //    " updated_by = '" + lsuser_gid + "'," +
                            //    " updated_date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' where user_gid='" + lsuser_gid + "'  ";

                            //mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                            msSQL = " update  adm_mst_tuser set " +
                   " user_password = '" + objcmnfunctions.ConvertToAscii(values.password) + "'," +
                   " updated_by = '" + lsuser_gid + "'," +
                   " updated_date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' where user_gid='" + lsuser_gid + "'  ";

                            mnResult = objdbconn.ExecuteNonQuerySQLForgot(msSQL, lscompany_dbname);
                            if (mnResult == 1)
                            {
                                GetForgotResponse.status = true;
                                GetForgotResponse.message = "Password Update Successfully !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                            }
                            else
                            {
                                GetForgotResponse.status = false;
                                GetForgotResponse.message = "Error Occur While Updating Password !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                            }

                        }
                        else
                        {

                            GetForgotResponse.status = false;
                            GetForgotResponse.message = "Mobile Number is Invaild  !";
                            return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                        }
                    }
                    else
                    {
                        GetForgotResponse.status = false;
                        GetForgotResponse.message = "User code is Invaild !";
                        return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);

                    }
                }
                else
                {

                    GetForgotResponse.status = false;
                    GetForgotResponse.message = "Company code is Invaild !";
                    return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
                }

            }
            else
            {

                GetForgotResponse.status = false;
                GetForgotResponse.message = "Company code is Invaild !";
                return Request.CreateResponse(HttpStatusCode.OK, GetForgotResponse);
            }


        }

        [HttpPost]
        [ActionName("UserReset")]
        public HttpResponseMessage PostUserReset(PostUserReset values)
        {
            PostUserReset GetRestResponse = new PostUserReset();
            domain = Request.RequestUri.Host.ToLower();
            string jsonFilePath = @" " + ConfigurationManager.AppSettings["CmnConfigfile_path"].ToString();
            string jsonString = File.ReadAllText(jsonFilePath);
            var jsonDataArray = JsonConvert.DeserializeObject<MdlCmnConn[]>(jsonString);
            string lscompany_dbname = (from a in jsonDataArray
                                       where a.company_code == values.companyid_reset
                                       select a.company_dbname).FirstOrDefault();
            string lscompany_code = (from a in jsonDataArray
                                     where a.company_code == values.companyid_reset
                                     select a.company_code).FirstOrDefault();
            if (lscompany_code != null && lscompany_code != " ")
            {
                msSQL = " SELECT  user_code,user_password,user_gid from adm_mst_tuser    where user_code = '" + values.usercode_reset + "' ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lsuser_code = objMySqlDataReader["user_code"].ToString();
                    lsuser_password = objMySqlDataReader["user_password"].ToString();
                    lsuser_gid = objMySqlDataReader["user_gid"].ToString();
                }

                if (lsuser_code != null && lsuser_code != "")
                {
                    lsuser_code = lsuser_code.ToUpper();
                }
                else
                {
                    lsuser_code = null;

                }

                msSQL = " SELECT  company_code FROM adm_mst_tcompany ";
                objMySqlDataReader = objdbconn.GetDataReader(msSQL, lscompany_dbname);
                if (objMySqlDataReader.HasRows)
                {
                    lscompany_code = objMySqlDataReader["company_code"].ToString();

                }
                if (lscompany_code != null && lscompany_code != "")
                {
                    lscompany_code = lscompany_code.ToUpper();
                }
                else
                {
                    lscompany_code = null;

                }
                if (values.companyid_reset != null && values.companyid_reset != "")
                {
                    lscompanyid = values.companyid_reset.ToUpper();
                }
                else
                {
                    lscompanyid = null;

                }
                if (values.usercode_reset != null && values.usercode_reset != "")
                {
                    lsusercode = values.usercode_reset.ToUpper();
                }
                else
                {
                    lsusercode = null;

                }

                if (lscompany_code == lscompanyid)
                {
                    if (lsuser_code == lsusercode)
                    {

                        if (lsuser_password == objcmnfunctions.ConvertToAscii(values.old_password))
                        {
                            msSQL = " update  adm_mst_tuser set " +
                                " user_password = '" + objcmnfunctions.ConvertToAscii(values.password) + "'," +
                                " updated_by = '" + lsuser_gid + "'," +
                                " updated_date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' where user_gid='" + lsuser_gid + "'  ";

                            mnResult = objdbconn.ExecuteNonQuerySQLForgot(msSQL, lscompany_dbname);
                            if (mnResult == 1)
                            {
                                GetRestResponse.status = true;
                                GetRestResponse.message = "Password Reset Successfully !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                            }
                            else
                            {
                                GetRestResponse.status = false;
                                GetRestResponse.message = "Error Occur While Password Reset !";
                                return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                            }

                        }
                        else
                        {

                            GetRestResponse.status = false;
                            GetRestResponse.message = "Old Paaword is Invaild !";
                            return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                        }
                    }
                    else
                    {
                        GetRestResponse.status = false;
                        GetRestResponse.message = "User code is Invaild !";
                        return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);

                    }
                }
                else
                {

                    GetRestResponse.status = false;
                    GetRestResponse.message = "Company code is Invaild !";
                    return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
                }

            }
            else
            {

                GetRestResponse.status = false;
                GetRestResponse.message = "Company code is Invaild !";
                return Request.CreateResponse(HttpStatusCode.OK, GetRestResponse);
            }


        }
        public class MdlCmnConn
        {
            public string connection_string { get; set; }
            public string company_code { get; set; }
            public string company_dbname { get; set; }
        }
        public string Token(string userName, string password, string company_code = null)
        {

            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "grant_type", "password" ),
                            new KeyValuePair<string, string>( "username", userName ),
                            new KeyValuePair<string, string> ( "Password", password ),
                            new KeyValuePair<string, string>("Scope",company_code)
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                domain = Request.RequestUri.Authority.ToLower();
                var host = HttpContext.Current.Request.Url.Host;
                if (host == "localhost")
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    var response = client.PostAsync(ConfigurationManager.AppSettings["protocol"].ToString() + domain +
                               "/WebApp/token", new FormUrlEncodedContent(pairs)).Result;
                    return response.Content.ReadAsStringAsync().Result;


                }
                else
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    var response = client.PostAsync(ConfigurationManager.AppSettings["protocol"].ToString() + domain +
                               "/token", new FormUrlEncodedContent(pairs)).Result;
                    return response.Content.ReadAsStringAsync().Result;

                }

            }
        }

        [ActionName("incomingMail")]
        [HttpPost]
        public HttpResponseMessage incomingMail()
        {
            string c_code = Request.Headers.GetValues("c_code").FirstOrDefault();
            msSQL = " SELECT " + c_code + ".company_code FROM adm_mst_tcompany";
            string lscompany_code = objdbconn.GetExecuteScalar(msSQL);
            result objresult = new result();
            try
            {

                string jsonString = Request.Content.ReadAsStringAsync().Result;
                List<incomingMail> relayMessage1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<incomingMail>>(jsonString);
                incomingMail relayMessage = relayMessage1[0];
                msSQL = "select " + c_code + ".fn_getgid('MILC', '');";
                string mailmanagement_gid = objdbconn.GetExecuteScalar(msSQL);

                msSQL = "INSERT INTO  " + c_code + ".crm_smm_mailmanagement(" +
                        "mailmanagement_gid," +
                        "to_mail," +
                        "reply_to, " +
                        "sub," +
                        "body," +
                        "direction," +
                        "created_date)" +
                        "VALUES(" +
                        " '" + mailmanagement_gid + "'," +
                        " '" + relayMessage.msys.relay_message.friendly_from + "'," +
                        " '" + relayMessage.msys.relay_message.rcpt_to + "'," +
                        " '" + relayMessage.msys.relay_message.content.subject.Replace("'", "\\'") + "'," +
                        " '" + relayMessage.msys.relay_message.content.html.Replace("'", "\\'") + "'," +
                        "'incoming'," +
                        "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                if (mnResult == 0)
                {
                   
                    objresult.message = "Error occured while inserting";
                }
                else
                {
          
                    objresult.message = "success";
                    objresult.status = true;
                }
                msSQL = "select leadbank_gid from " + c_code + ".crm_smm_mailmanagement where to_mail='" + relayMessage.msys.relay_message.friendly_from + "';";
                string leadbank_gid = objdbconn.GetExecuteScalar(msSQL);
                if (leadbank_gid != null)
                {
                    msSQL = "update " + c_code + ".crm_smm_mailmanagement set leadbank_gid ='" + leadbank_gid + "' where mailmanagement_gid='" + mailmanagement_gid + " '; ";
                    mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                }
            }
            catch (Exception ex)
            {
                objresult.message = "Exception occured:" + ex.ToString();
              

            }
            return Request.CreateResponse(HttpStatusCode.OK, objresult);
        }
    
        //public void LoginErrorLog(string strVal)
        //{
        //    try
        //    {
        //        string lspath = ConfigurationManager.AppSettings["file_path"].ToString() + "/erpdocument/LOGIN_ERRLOG/" + DateTime.Now.Year + @"\" + DateTime.Now.Month;
        //        if ((!System.IO.Directory.Exists(lspath)))
        //            System.IO.Directory.CreateDirectory(lspath);

        //        lspath = lspath + @"\" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";
        //        System.IO.StreamWriter sw = new System.IO.StreamWriter(lspath, true);
        //        sw.WriteLine(strVal);
        //        sw.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

    }
}
