using com.digiwin.Mobile.MCloud.TransferLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace cn.hanbell.mcloud
{
    public class Utility
    {

        #region 取得Response
        /// <summary>
        /// 叫用API
        /// </summary>
        /// <param name="pRequest">Request</param>
        /// <param name="pErrorMsg">錯誤訊息</param>
        /// <param name="pParam">錯誤訊息</param>
        /// <returns>Rsponse</returns>
        public static string CallAPI(string pRequest, out string pErrorMsg, params Dictionary<string, string>[] pParam)
        {
            pErrorMsg = string.Empty;   //錯誤訊息
            string tResponse = string.Empty;

            //Get Response
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pRequest);   //建立連線
                request.Timeout = 300000;                                               //設定timeout時間

                if (pParam.Count() == 0)
                {
                    //一般取response
                    request.Method = "GET";
                }
                else
                {
                    //生單才有額外參數
                    request.Method = "POST";
                    request.ContentType = "application/json";

                    if (pParam[0].ContainsKey("BodyContent"))
                    {
                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(pParam[0]["BodyContent"]);
                        }
                    }
                    else
                    {
                        pErrorMsg = "叫用服務失敗: 找不到 BodyContent";
                    }
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)//送出參數取得回傳值
                {
                    if (response.StatusCode == HttpStatusCode.OK)//判斷是否成功
                    {
                        using (Stream reponseStream = response.GetResponseStream())//取得回傳結果
                        {
                            using (StreamReader rd = new StreamReader(reponseStream, Encoding.UTF8))//讀取
                            {
                                string responseString = rd.ReadToEnd();

                                tResponse = responseString;
                            }
                        }
                    }
                    else
                    {
                        pErrorMsg = "叫用服務失敗:" + response.StatusCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                pErrorMsg = "叫用服務失敗:" + ex.Message.ToString();
            }

            return tResponse;
        }
        #endregion

        public static string CallAPI(XDocument pM2Pxml, string pObjectName, Dictionary<string, string> pParam, out string pErrorMsg)
        {
            pErrorMsg = string.Empty;

            string tRequest = GetRequest(pObjectName, pParam, out pErrorMsg);
            if (!string.IsNullOrEmpty(pErrorMsg))
            {
                pErrorMsg = string.Format("function : {0}\r\nGet Request Error : {1}", pObjectName, pErrorMsg);
                return "";
            }
            else
            {
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("Request : " + tRequest);
            }

            string tResponse = CallAPI(tRequest, out pErrorMsg);
            if (!string.IsNullOrEmpty(pErrorMsg))
            {
                pErrorMsg = string.Format("function : {0}\r\nGet Response Error : {1}", pObjectName, pErrorMsg);
                return "";
            }
            else
            {
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("Response : " + tResponse);
            }

            return tResponse;
        }

        public static string EncodingUTF8(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(value));
        }

        #region 取得RequestAPI
        /// <summary>
        /// 取得RequestAPI
        /// </summary>
        /// <param name="objectName">对象名稱</param>
        /// <param name="param">參數</param>
        /// <returns></returns>
        public static string GetRequest(string objectName, Dictionary<string, string> param, out string errorMsg)
        {

            errorMsg = string.Empty;    //錯誤訊息
            string api = string.Empty;   //服務位置
            try
            {
                switch (objectName)
                {
                    #region 取得公司別
                    case "Company":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/eap/company?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //取回:公司代号company、公司简称name
                        api = "eap/company";
                        break;
                    #endregion

                    #region 取得員工清單資料
                    case "Employee":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/users/f/s/0/20?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //返回EFGP中从0开始的20个用户(/0/20)，没有筛选和排序条件（/f/s）
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/users/f;userName=天雨/s/0/20?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //返回EFGP Users表中,姓名包含“天雨”的20条记录，筛选条件/f;userName=天雨,“f”是个固定值，“;”后面是条件，多个条件用“;”隔开
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/users/f;userName=王/s;applyUser=DESC/0/20/size?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //返回EFGP Users表中,姓名包含“王”的20条记录，并按id字段倒排序，排序条件/s;applyUser=DESC，“s”是个固定值,“;”后面是条件，多个条件用“;”隔开，如果是顺序就是/s;applyUser=ASC
                        //篩選條件:按代号（applyUser=XXXX）,名称（userName=XXXX）
                        //取回:员工代号id、员工姓名userName

                        param.Add("condition",
                            (param.ContainsKey("userId") ? string.Format(";applyUser={0}", param["userId"]) : "") +            //篩選條件:按代号（applyUser=XXXX）
                            (param.ContainsKey("userName") ? string.Format(";userName={0}", param["userName"]) : "")    //篩選條件:按名称（userName=XXXX）
                        );

                        api = string.Format("efgp/users/f{0}/s;applyUser=ASC/{1}/{2}",
                                    param.ContainsKey("condition") ? param["condition"] : "",   //搜尋條件 (;applyUser=XX;userName=xx)
                                    param.ContainsKey("StartRow") ? param["StartRow"] : "0",    //搜尋起始筆數
                                    param.ContainsKey("EndRow") ? param["EndRow"] : "1000"      //搜尋結尾筆數
                                 );
                        break;
                    #endregion

                    #region 取得員工明細資料
                    case "Users":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/users/C0160?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //取回:员工代号id、员工姓名userName、主要部门deptno、部门名称deptname、默认公司company
                        api = string.Format("efgp/users/{0}",
                                    param.ContainsKey("UserID") ? param["UserID"] : "0001"  //使用者待號
                                );
                        break;
                    #endregion

                    #region 查詢員工部門
                    case "EmployeeDepartment":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/functions/f;users.applyUser=C0160/s/0/20?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //根据Users.id返回EFGP中此账户所在部门
                        //篩選條件:按代号(users.applyUser=xx)
                        //取回:部门代号organizationUnit.applyUser、部门名称organizationUnit.organizationUnitName

                        param.Add("condition",
                            (param.ContainsKey("userId") ? string.Format(";users.id={0}", param["userId"]) : "")  //篩選條件:按代号(users.applyUser=xx)
                        );

                        api = string.Format("efgp/functions/f{0}/s/{1}/{2}",
                                    param.ContainsKey("condition") ? param["condition"] : "",   //搜尋條件 (;users.applyUser=xx)
                                    param.ContainsKey("StartRow") ? param["StartRow"] : "0",    //搜尋起始筆數
                                    param.ContainsKey("EndRow") ? param["EndRow"] : "1000"      //搜尋結尾筆數
                                );
                        break;
                    #endregion

                    #region 部門查詢
                    case "Department":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/organizationunit/f;applyUser=13120/s;applyUser=ASC/0/20?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //返回EFGP中的部门信息，/f;applyUser=13120/s;applyUser=ASC，筛选和排序条件，没有条件时用/f/s
                        //篩選條件:按代号（applyUser=XXXX）,名称（organizationUnitName=XXXX）
                        //取回:部门编号id、部门名称organizationUnitName

                        param.Add("condition",
                            (param.ContainsKey("deptId") ? string.Format(";id={0}", param["deptId"]) : "") +                        //篩選條件:按代号(applyUser=XXXX)
                            (param.ContainsKey("deptName") ? string.Format(";organizationUnitName={0}", param["deptName"]) : "")    //篩選條件:按名称（organizationUnitName=XXXX）
                        );

                        api = string.Format("efgp/organizationunit/f{0}/s;id=ASC/{1}/{2}",
                                    param.ContainsKey("condition") ? param["condition"] : "",   //搜尋條件 (;applyUser=xx;organizationUnitName=xx)
                                    param.ContainsKey("StartRow") ? param["StartRow"] : "0",    //搜尋起始筆數
                                    param.ContainsKey("EndRow") ? param["EndRow"] : "1000"      //搜尋結尾筆數
                                );
                        break;
                    #endregion

                    #region 部門人員查詢
                    case "DepartmentEmployee":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/users/functions/organizationunit/f;organizationUnit.applyUser=13120/s/0/20?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //根据部门代号返回EFGP中此部门相关人员信息
                        //篩選條件:部门代号(organizationUnit.applyUser=XXXX)
                        //取回:员工代号id、员工姓名userName

                        param.Add("condition",
                            (param.ContainsKey("deptId") ? string.Format(";organizationUnit.id={0}", param["deptId"]) : "")  //篩選條件:部门代号(organizationUnit.applyUser=XXXX)
                        );

                        api = string.Format("efgp/users/functions/organizationunit/f{0}/s/{1}/{2}",
                                    param.ContainsKey("condition") ? param["condition"] : "",   //搜尋條件 (;users.applyUser=xx)
                                    param.ContainsKey("StartRow") ? param["StartRow"] : "0",    //搜尋起始筆數
                                    param.ContainsKey("EndRow") ? param["EndRow"] : "1000"      //搜尋結尾筆數
                                );
                        break;
                    #endregion

                    #region 请假类别查詢
                    case "LeaveKind":
                        api = "efgp/leavekind";
                        break;
                    #endregion

                    case "BizKind":
                        api = "efgp/bizkind";
                        break;

                    case "BizVehicle":
                        api = "efgp/bizvehicle";
                        break;

                    case "BizDestination":
                        api = "efgp/bizdestination";
                        break;

                }

                if (!string.IsNullOrEmpty(api))
                {
                    //組合request
                    api = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), api, LoadConfig.GetWebConfig("APIKey"));
                }
                else
                {
                    errorMsg = "找不到WebAPI URI";
                }
            }
            catch (Exception err)
            {
                errorMsg = err.Message.ToString();
            }

            return api;
        }
        #endregion

        public static T JSONDeserialize<T>(string jsonString)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();  //設定JSON轉換物件
            try
            {
                return jss.Deserialize<T>(jsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string JSONSerialize(Object obj)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                return jss.Serialize(obj);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string InvokeProcess(string uri, string content, out string errorMsg)
        {
            errorMsg = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);   //建立連線
            request.Method = "POST";                                                //設定叫用模型
            request.ContentType = "application/json;charset=utf8";
            request.Timeout = 300000;                                               //設定timeout時間
            string tResponse = string.Empty;

            //Get Response
            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(content);
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)//送出參數取得回傳值
                {
                    if (response.StatusCode == HttpStatusCode.OK)//判斷是否成功
                    {
                        using (Stream reponseStream = response.GetResponseStream())//取得回傳結果
                        {
                            using (StreamReader rd = new StreamReader(reponseStream, Encoding.UTF8))//讀取
                            {
                                string responseString = rd.ReadToEnd();

                                tResponse = responseString;
                            }
                        }
                    }
                    else
                    {
                        errorMsg = "叫用服務失敗:" + response.StatusCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "叫用服務失敗:" + ex.Message.ToString();
            }
            return tResponse;
        }

        #region 分頁設定 (RCPControl)
        /// <summary>
        /// 分頁設定
        /// </summary>
        /// <param name="pM2PXml">M2P</param>
        /// <param name="pDT">GridView內容</param>
        /// <param name="pRCP">GridView控件</param>
        public static void SetRCPControlPage(XDocument pM2PXml, DataTable pDT, ref RCPControl pRCP)
        {
            #region 處理分頁
            int tPageNO = 1; //目前頁數(預設1)
            if (com.digiwin.Mobile.LibTool.DataTransferManager.GetControlsValue(pM2PXml, "PageNo") != string.Empty)
            {
                tPageNO = Convert.ToInt16(DataTransferManager.GetControlsValue(pM2PXml, "PageNo"));  //檢查頁數
            }
            double tQueryCount = 20;//顯示筆數

            double tDdouble = pDT.Rows.Count / tQueryCount;
            int tTotalPage = Convert.ToInt32(Math.Ceiling(tDdouble));//總頁數
            #endregion
            //設定屬性
            pRCP.AddCustomTag("TotalPage", tTotalPage.ToString());      //總頁數
            pRCP.AddCustomTag("PageNo", tPageNO.ToString());            //目前頁數
            pRCP.Table = DataTransferManager.MakeDataTablePaged(pDT, tPageNO, Convert.ToInt32(tQueryCount));    //把結果給GridView
        }
        #endregion

        #region 分頁設定 (RDControl)
        /// <summary>
        /// 分頁設定
        /// </summary>
        /// <param name="pM2PXml">M2P</param>
        /// <param name="pDT">GridView內容</param>
        /// <param name="pRCP">GridView控件</param>
        public static void SetRDControlPage(XDocument pM2PXml, DataTable pDT, ref RDControl pRD)
        {
            #region 處理分頁
            int tPageNO = 1; //目前頁數(預設1)
            if (DataTransferManager.GetControlsValue(pM2PXml, "PageNo") != string.Empty)
            {
                tPageNO = Convert.ToInt16(DataTransferManager.GetControlsValue(pM2PXml, "PageNo"));  //檢查頁數
            }
            double tQueryCount = 20;//顯示筆數

            double tDdouble = pDT.Rows.Count / tQueryCount;
            int tTotalPage = Convert.ToInt32(Math.Ceiling(tDdouble));//總頁數
            #endregion
            //設定屬性
            pRD.AddCustomTag("TotalPage", tTotalPage.ToString());      //總頁數
            pRD.AddCustomTag("PageNo", tPageNO.ToString());            //目前頁數
            pRD.Table = DataTransferManager.MakeDataTablePaged(pDT, tPageNO, Convert.ToInt32(tQueryCount));    //把結果給GridView
        }
        #endregion

        #region 處理報錯
        /// <summary>
        /// 處理報錯
        /// </summary>
        /// <param name="pM2Pxml">M2P</param>
        /// <param name="pP2MObject">P2M</param>
        /// <param name="pErrorMsg">錯誤訊息</param>
        /// <returns></returns>
        public static string ReturnErrorMsg(XDocument pM2Pxml, ref ProductToMCloudBuilder pP2MObject, string pErrorMsg)
        {
            pP2MObject.Message = pErrorMsg;
            pP2MObject.Result = "false";
            return pP2MObject.ToDucument().ToString();
        }
        #endregion

    }

    public class Company
    {
        public List<CompanyItem> data { get; set; }

        public Company()
        {
            this.data = new List<CompanyItem>();
        }

        //回傳公司別table
        public DataTable GetTable(string pSearch)
        {
            Dictionary<string, string[]> companyItems = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable CompanyTable = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                companyItems = this.data[i].GetCompanyItem(); //取得公司別資料

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in companyItems["Columns"]) CompanyTable.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in companyItems["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (tInsertData) CompanyTable.Rows.Add(companyItems["Values"]);
            }

            return CompanyTable;
        }
    }

    //公司別資料
    public class CompanyItem
    {
        public string type { get; set; }
        public string id { get; set; }
        public string cfmdate { get; set; }
        public string cfmuser { get; set; }
        public string creator { get; set; }
        public string credate { get; set; }
        public string optdate { get; set; }
        public string optuser { get; set; }
        public string status { get; set; }
        public string address { get; set; }
        public string assetcode { get; set; }
        public string boss { get; set; }
        public string company { get; set; }     //公司代号company、
        public string contacter { get; set; }
        public string fax { get; set; }
        public string fullname { get; set; }
        public string name { get; set; }        //公司简称name
        public string remark { get; set; }
        public string tel { get; set; }

        public CompanyItem() { }

        //取得單身欄位
        public Dictionary<string, string[]> GetCompanyItem()
        {
            Dictionary<string, string[]> Items = new Dictionary<string, string[]>();
            //全部的
            //Items.Add("Columns", new string[] { "type", "applyUser", "cfmdate", "cfmuser", "creator", "credate", "optdate", "optuser", "status", "address",
            //                                    "assetcode", "boss", "company", "contacter", "fax", "fullname", "name", "remark", "tel"});
            //Items.Add("Values", new string[] { this.type, this.applyUser, this.cfmdate, this.cfmuser, this.creator, this.credate, this.optdate, this.optuser, this.status, this.address,
            //                                    this.assetcode, this.boss, this.company, this.contacter, this.fax, this.fullname, this.name, this.remark, this.tel});

            //需要的
            Items.Add("Columns", new string[] { "company", "name" });
            Items.Add("Values", new string[] { this.company, this.name });

            return Items;
        }
    }

    #region 部門集合查詢
    /*
      {//為符合class格式自己加上去的
         "data" : //為符合class格式自己加上去的
          [{
                 "id" : "10000",
                 "manager" : {
                     "cost" : 0,
                     "enableSubstitute" : 1,
                     "id" : "C0002",
                     "identificationType" : "DEFAULT",
                     "ldapid" : "YuZZ",
                     "localeString" : "zh_CN",
                     "mailAddress" : "philipyu@hanbell.cn",
                     "mailingFrequencyType" : 0,
                     "objectVersion" : 15,
                     "oid" : "3a6a95c3dbbb10048c65af58217aa75e",
                     "password" : "wi4Hym6sACoEcRMbn9cMPhcpyBI=",
                     "phoneNumber" : "6118",
                     "userName" : "余昱暄",
                     "workflowServerOID" : "00000000000000WorkflowServer0001"
                 },
                 "objectVersion" : 5,
                 "oid" : "3aab026adbbb10048c65af58217aa75e",
                 "organizationOID" : "3a8a4bb8dbbb10048c65af58217aa75e",
                 "organizationUnitName" : "汉钟",
                 "organizationUnitType" : 0,
                 "validType" : 1
             }, {
                 "id" : "11000",
                 "manager" : {
                     "cost" : 0,
                     "enableSubstitute" : 1,
                     "id" : "C0002",
                     "identificationType" : "DEFAULT",
                     "ldapid" : "YuZZ",
                     "localeString" : "zh_CN",
                     "mailAddress" : "philipyu@hanbell.cn",
                     "mailingFrequencyType" : 0,
                     "objectVersion" : 15,
                     "oid" : "3a6a95c3dbbb10048c65af58217aa75e",
                     "password" : "wi4Hym6sACoEcRMbn9cMPhcpyBI=",
                     "phoneNumber" : "6118",
                     "userName" : "余昱暄",
                     "workflowServerOID" : "00000000000000WorkflowServer0001"
                 },
                 "objectVersion" : 10,
                 "oid" : "3aabacbedbbb10048c65af58217aa75e",
                 "organizationOID" : "3a8a4bb8dbbb10048c65af58217aa75e",
                 "organizationUnitName" : "总经理室",
                 "organizationUnitType" : 0,
                 "superUnitOID" : "3aab026adbbb10048c65af58217aa75e",
                 "validType" : 1
             }
         ] 
      } //為符合class格式自己加上去的
      */
    public class Department
    {
        public List<OrgUnit> data { get; set; }

        public Department()
        {
            this.data = new List<OrgUnit>();
        }

        //回傳部門table
        public DataTable GetTable(string pSearch)
        {
            Dictionary<string, string[]> DeptItems = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable DepartmentTable = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                //bool tInsertData = false;               //是否insert//改成一定要下搜尋
                DeptItems = this.data[i].GetItem();

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in DeptItems["Columns"]) DepartmentTable.Columns.Add(col);

                //改成一定要下搜尋
                ////檢查搜尋
                //if (string.IsNullOrEmpty(pSearch))
                //{
                //    tInsertData = true;
                //}
                //else
                //{
                //    //有搜尋字段才需處理搜尋
                //    foreach (var value in DeptItems["Values"])
                //    {
                //        if (value.Contains(pSearch))
                //        {
                //            tInsertData = true;//找到就跳出
                //            break;
                //        }
                //    }
                //}

                //新增資料
                //if (tInsertData)//改成一定要下搜尋
                DepartmentTable.Rows.Add(DeptItems["Values"]);
            }

            return DepartmentTable;
        }

    }
    #endregion

    #region 部門人員查詢
    public class DepartmentEmployee
    {
        //有用到的:员工代号id、员工姓userName
        public List<Manager> data { get; set; }   //與員工部門查詢的MANAGER相同, 這裡不再新增

        public DepartmentEmployee()
        {
            this.data = new List<Manager>();
        }

        //回傳部門table
        public DataTable GetTable(string pSearch)
        {
            Dictionary<string, string[]> EmployeeItems = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable EmployeeTable = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                EmployeeItems = this.data[i].GetItem();

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in EmployeeItems["Columns"]) EmployeeTable.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in EmployeeItems["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (tInsertData) EmployeeTable.Rows.Add(EmployeeItems["Values"]);
            }

            return EmployeeTable;
        }

    }
    #endregion

    #region 人員部門查詢
    public class EmployeeDepartment
    {

        public List<EmployeeDepartmentItem> data { get; set; }

        public EmployeeDepartment()
        {
            data = new List<EmployeeDepartmentItem>();
        }

        //回傳部門table
        public DataTable GetTable(string pSearch)
        {
            Dictionary<string, string[]> DepartmentItems = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable DepartmentTable = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                DepartmentItems = this.data[i].organizationUnit.GetItem(); //取得organizationUnit資料

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in DepartmentItems["Columns"]) DepartmentTable.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in DepartmentItems["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (tInsertData) DepartmentTable.Rows.Add(DepartmentItems["Values"]);
            }

            return DepartmentTable;
        }

    }

    public class EmployeeDepartmentItem
    {
        public string approvalLevelOID { get; set; }
        public string definitionOID { get; set; }
        public string isMain { get; set; }
        public string objectVersion { get; set; }
        public string occupantOID { get; set; }
        public string oid { get; set; }
        public string specifiedManagerOID { get; set; }
        public OrgUnit organizationUnit { get; set; }
        public Users users { get; set; }

        public EmployeeDepartmentItem()
        {
            this.organizationUnit = new OrgUnit();
            this.users = new Users();
        }
    }
    #endregion

    public class KV
    {
        public string k { get; set; }
        public string v { get; set; }

        public KV()
        {

        }

        public KV(string k, string v)
        {
            this.k = k;
            this.v = v;
        }
        //取得资料
        public Dictionary<string, string[]> GetItem()
        {
            Dictionary<string, string[]> Items = new Dictionary<string, string[]>();

            //需要的
            Items.Add("Columns", new string[] { "k", "v" });
            Items.Add("Values", new string[] { this.k, this.v });

            return Items;
        }
    }

    public class Manager
    {
        public string cost { get; set; }
        public string enableSubstitute { get; set; }
        public string id { get; set; }                  //员工代号id
        public string identificationType { get; set; }
        public string ldapid { get; set; }
        public string localeString { get; set; }
        public string mailAddress { get; set; }
        public string mailingFrequencyType { get; set; }
        public string objectVersion { get; set; }
        public string oid { get; set; }
        public string password { get; set; }
        public string phoneNumber { get; set; }
        public string userName { get; set; }            //员工姓userName
        public string workflowServerOID { get; set; }

        public Manager() { }

        //取得單身欄位
        public Dictionary<string, string[]> GetItem()
        {
            Dictionary<string, string[]> Items = new Dictionary<string, string[]>();
            //需要的
            Items.Add("Columns", new string[] { "id", "userName" });
            Items.Add("Values", new string[] { this.id, this.userName });

            return Items;
        }
    }

    public class OrgUnit
    {
        public string id { get; set; }                      //部门代号organizationUnit.id
        public string objectVersion { get; set; }
        public string oid { get; set; }
        public string organizationOID { get; set; }
        public string organizationUnitName { get; set; }    //部门名称organizationUnit.organizationUnitName
        public string organizationUnitType { get; set; }
        public string superUnitOID { get; set; }
        public string validType { get; set; }
        public Manager manager { get; set; }

        public OrgUnit()
        {
            this.manager = new Manager();
        }

        public Dictionary<string, string[]> GetItem()
        {
            Dictionary<string, string[]> Items = new Dictionary<string, string[]>();
            //需要的
            Items.Add("Columns", new string[] { "id", "organizationUnitName" });
            Items.Add("Values", new string[] { this.id, this.organizationUnitName });

            return Items;
        }
    }

    public class ResponseState
    {
        public string code { get; set; }    //回傳代碼
        public string msg { get; set; }     //回傳訊息

        public ResponseState()
        {

        }
    }

    public class Title
    {
        public string objectVersion { get; set; }
        public string occupantOID { get; set; }
        public string oid { get; set; }
        public string organizationUnitOID { get; set; }
        public List<TitleDefinition> titleDefinition { get; set; }

        public Title()
        {
            this.titleDefinition = new List<TitleDefinition>();
        }
    }

    public class TitleDefinition
    {
        public string description { get; set; }
        public string objectVersion { get; set; }
        public string oid { get; set; }
        public string organizationOID { get; set; }
        public string titleDefinitionName { get; set; }

        public TitleDefinition() { }
    }

    #region 員工明細
    /*JSON樣式
    {
	    "company": "C",
	    "cost": 0,
	    "deptname": "维护组",
	    "applyDept": "13120",
	    "enableSubstitute": 1,
	    "applyUser": "C0160",
	    "identificationType": "DEFAULT",
	    "ldapid": "DongTY",
	    "localeString": "zh_CN",
	    "mailAddress": "dtianyu@hanbell.cn",
	    "mailingFrequencyType": 0,
	    "objectVersion": 15,
	    "oid": "3a6b1714dbbb10048c65af58217aa75e",
	    "password": "s//RxFNKLw5LqwI3D34/DWUdH1w=",
	    "phoneNumber": "65299",
	    "title": {
		    "objectVersion": 1,
		    "occupantOID": "3a6b1714dbbb10048c65af58217aa75e",
		    "oid": "6dfa8be2dd29100482e520de6fdbb7f2",
		    "organizationUnitOID": "3aab5782dbbb10048c65af58217aa75e",
		    "titleDefinition": {
			    "description": "C",
			    "objectVersion": 1,
			    "oid": "6d88eb92dd29100482e520de6fdbb7f2",
			    "organizationOID": "3a8a4bb8dbbb10048c65af58217aa75e",
			    "titleDefinitionName": "C"
		    }
	    },
	    "userName": "董天雨",
	    "workflowServerOID": "00000000000000WorkflowServer0001"
    }
    */
    #endregion

    public class Users
    {
        public string company { get; set; }             //默认公司company
        public string cost { get; set; }
        public string deptname { get; set; }            //部门名称deptname
        public string deptno { get; set; }              //主要部门deptno
        public string enableSubstitute { get; set; }
        public string id { get; set; }                  //员工代号id
        public string identificationType { get; set; }
        public string ldapid { get; set; }
        public string localeString { get; set; }
        public string mailAddress { get; set; }
        public string mailingFrequencyType { get; set; }
        public string objectVersion { get; set; }
        public string oid { get; set; }
        public string password { get; set; }
        public string phoneNumber { get; set; }
        public string userName { get; set; }            //员工姓名userName
        public string workflowServerOID { get; set; }
        public List<Title> title { get; set; }

        public Users()
        {
            this.title = new List<Title>();
        }
    }

}