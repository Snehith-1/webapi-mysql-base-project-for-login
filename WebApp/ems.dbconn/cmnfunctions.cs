using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.Odbc;

namespace ems.dbconn
{
    public class cmnfunctions
    {
        ems.dbconn.dbconn objdbconn = new ems.dbconn.dbconn();
        OdbcCommand cmdQuery = new OdbcCommand();
        OdbcDataReader objreader;
        DataSet objdataset = new DataSet();
        string lsTempGid = string.Empty;
        int mnResult, ls_port;
        string msSQL, ls_username, ls_password, ls_server;
        string scalar = string.Empty;
        DataTable objTblRQ = new DataTable("objTblRQ");
        DataTable table = new DataTable("table");
        DataTable dt_table;
        DataColumn myCol0;
        string lblemployeereporting_to, lsemployeeGID;
        int lscount;
        String[] lsCCReceipients;
        string return_path, upload_gid, path, company_code, file_path, file_name, lsfile_name;
        HttpRequest httpRequest;
        HttpPostedFile httpPostedFile;
        MemoryStream ms = new MemoryStream();
        MemoryStream ms_stream = new MemoryStream();
        Stream ls_readStream;
        string lsconverted_date;

        public string[] Split(string input, string pattern)
        {
            string[] elements = Regex.Split(input, pattern);
            return elements;
        }

        public string ConvertToAscii(string str)
        {
            int iIndex;
            int lenOfUserString;
            string newUserPass = string.Empty;
            string tmp;
            lenOfUserString = str.Length;
            for (iIndex = 0; iIndex < lenOfUserString; iIndex++)
            {
                tmp = str.Substring(iIndex, 1);
                tmp = (((int)Convert.ToChar(tmp)) - lenOfUserString).ToString();
                newUserPass = newUserPass + (tmp.Length < 3 ? "0" : "") + tmp;
            }
            return newUserPass;
        }


        public string GetMasterGID(string pModule_Code)
        {
            lsTempGid = null;

            msSQL = " select year(fyear_start) as finyear from adm_mst_tyearendactivities order by finyear_gid desc limit 0,1";
            string lsfinyear = objdbconn.GetExecuteScalar(msSQL);

            msSQL = " select sequence_flag from adm_mst_tsequence where sequence_code='" + pModule_Code + "' and finyear='" + lsfinyear + "' ";
            string lssequence_flag = objdbconn.GetExecuteScalar(msSQL);
            if (lssequence_flag == "N")
            {
                msSQL = " select  sequence_curval + 1 AS sequence_curval from adm_mst_tsequence where sequence_code = '" + pModule_Code + "' and finyear='" + lsfinyear + "'";
                string sequencecurval = objdbconn.GetExecuteScalar(msSQL);

                DateTime currentDate = DateTime.Now;
                msSQL = " update  adm_mst_tsequence set " +
            " sequence_curval = '" + sequencecurval + "'" +
            "  where sequence_code='" + pModule_Code + "' and finyear='" + lsfinyear + "'  ";
                mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                lsTempGid = pModule_Code + currentDate.ToString("yyMMdd") + sequencecurval;


            }
            else
            {
                msSQL = " select  sequence_curval + 1 AS sequence_curval from adm_mst_tsequence where sequence_code = '" + pModule_Code + "' and finyear='" + lsfinyear + "'";
                string sequencecurval = objdbconn.GetExecuteScalar(msSQL);

                DateTime currentDate = DateTime.Now;
                msSQL = " update  adm_mst_tsequence set " +
            " sequence_curval = '" + sequencecurval + "'" +
            "  where sequence_code='" + pModule_Code + "' and finyear='" + lsfinyear + "'  ";
                mnResult = objdbconn.ExecuteNonQuerySQL(msSQL);
                lsTempGid = pModule_Code + currentDate.ToString("yyMMdd") + sequencecurval;


            }
            

            if (lsTempGid == null || lsTempGid == "")
                return "E";
            else
                return lsTempGid;
        }
    
        public void LogForAudit(string strVal)
        {

            try
            {
                string lspath = HttpContext.Current.Server.MapPath("../../documents/") + ConfigurationManager.AppSettings["company_code"] + GetMasterGID("LOGF") + "_" + System.IO.Path.GetFileName(HttpContext.Current.Request.Url.ToString()).Replace(".aspx", string.Empty).Replace("?ls=", string.Empty) + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                if ((!System.IO.File.Exists(lspath)))
                    System.IO.File.Create(lspath).Dispose();
                System.IO.StreamWriter sw = new System.IO.StreamWriter(lspath);
                sw.WriteLine(strVal);
                sw.Close();
            }
            catch
            {
            }
        }





    }

}
