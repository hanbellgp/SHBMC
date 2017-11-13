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
    public class HKGL004
    {

        #region 頁面初使化、请假單身分頁、搜尋
        public string GetHKGL004_BasicSetting(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, "请假单"); //參數:要顯示的訊息、結果、Title//建構p2m

            #region 設定參數                        
            JavaScriptSerializer tSerObject = new JavaScriptSerializer();//設定JSON轉換物件
            Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
            string tfunctionName = string.Empty;//叫用API的方法名稱(自定義)
            string tErrorMsg = string.Empty;//檢查是否錯誤
            #endregion
            try
            {
                #region 設定控件
                RCPControl tRCPbtnSave = new RCPControl("btnSave", null, null, null);       //送單
                RCPControl tRCPcompany = new RCPControl("company", "true", null, null);     //公司別
                RCPControl tRCPapplyDate = new RCPControl("applyDate", "false", null, null);//申請日
                RCPControl tRCPuserid = new RCPControl("applyUser", "false", null, null);      //申請人
                RCPControl tRCPdeptno = new RCPControl("applyDept", "true", null, null);
                RCPControl tRCPformType = new RCPControl("formType", "true", null, null);
                RCPControl tRCPformKind = new RCPControl("formKind", "true", null, null);
                RCPControl tRCPworkType = new RCPControl("workType", "true", null, null);
                RCPControl tRCPstartDate = new RCPControl("startDate", "true", null, null);
                RCPControl tRCPstartTime = new RCPControl("startTime", "true", null, null);
                RCPControl tRCPendDate = new RCPControl("endDate", "true", null, null);
                RCPControl tRCPendTime = new RCPControl("endTime", "true", null, null);
                RCPControl tRCPapplyDay = new RCPControl("applyDay", "true", null, null);
                RCPControl tRCPapplyHour = new RCPControl("applyHour", "true", null, null);
                RCPControl tRCPreason = new RCPControl("reason", "true", null, null);
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "EFGPTEST");    //登入者帳號
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //请假單暫存資料
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處裡畫面資料
                //檢查是否已有單據資料
                if (string.IsNullOrEmpty(tStrDocData))//沒單據資料
                {
                    #region 取得預設資料
                    //設定參數
                    tfunctionName = "Users";    //取得員工明細資料
                    tparam.Clear();
                    tparam.Add("UserID", tStrUserID);       //查詢對象

                    //叫用服務
                    string tResponse = CallAPI(pM2Pxml, tfunctionName, tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    //回傳的JSON轉Class
                    Users EmployDetail = tSerObject.Deserialize<Users>(tResponse);  //員工明細

                    //取值
                    string tCompany = string.Format("{0}-{1}", EmployDetail.company, ""/*EmployDetail.companyName*/);   //公司別代號-公司別名稱
                    string tUser = string.Format("{0}-{1}", EmployDetail.id, EmployDetail.userName);    //申請人代號-申請人名稱
                    string tDetp = string.Format("{0}-{1}", EmployDetail.deptno, EmployDetail.deptname);    //部門代號-部門名稱
                    string tDate = DateTime.Now.ToString("yyyy/MM/dd"); //申請時間
                    string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");    //请假類別code
                    string formTypeDesc = "1".Equals(formType) ? "平日请假" : "国庆或春节前后";   //请假類別name

                    #region 取得公司別名稱(如果GetEmployeeDetail有回傳名稱的話可以省掉)
                    tfunctionName = "Company";    //取得員工明細資料
                    tparam.Clear();
                    //new CustomLogger.Logger(pM2Pxml).WriteInfo("functionName : " + tfunctionName);
                    //new CustomLogger.Logger(pM2Pxml).WriteInfo("Search UserID : " + tStrUserID);

                    //取公司別值
                    tResponse = CallAPI(pM2Pxml, tfunctionName, tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    tResponse = "{\"CompanyItems\" : " + tResponse + "}";                                           //為符合class格式自己加上去的
                    Company CompanyClass = tSerObject.Deserialize<Company>(tResponse);                        //轉class方便取用
                    DataTable tCompanyTable = CompanyClass.GetCompanyTable(EmployDetail.company);                   //取公司別table
                    DataRow[] tCompanyRow = tCompanyTable.Select("company = '" + EmployDetail.company + "'");       //查詢指定company
                    if (tCompanyRow.Count() > 0) tCompany = tCompanyRow[0]["company"] + "-" + tCompanyRow[0]["name"];
                    #endregion

                    #endregion

                    #region 給值
                    //設定请假單資料
                    Entity DocDataClass = new Entity(tCompany, tDate, tUser, tDetp, formType, formTypeDesc, "1", "年休假");
                    DocDataClass.workType = "1";
                    DocDataClass.workTypeDesc = "常日班：80：00";
                    string DocDataStr = tSerObject.Serialize(DocDataClass);//Class轉成JSON

                    //給定畫面欄位值
                    tRCPcompany.Value = tCompany;       //公司別
                    tRCPapplyDate.Value = tDate;        //申請日
                    tRCPuserid.Value = tUser;           //申請人
                    tRCPdeptno.Value = tDetp;           //申請部門
                    tRCPformKind.Value = "1-年休假";
                    tRCPworkType.Value = "1-常日班：08：00-20：20";
                    tRCPapplyDay.Value = "0";
                    tRCPapplyHour.Value = "0";
                    tRCPdocData.Value = DocDataStr;     //请假單暫存資料

                    #endregion
                }
                else//有單據資料
                {
                    #region 顯示请假單資料
                    //先將暫存資料轉成Class方便取用
                    Entity DocDataClass = tSerObject.Deserialize<Entity>(tStrDocData);//请假單暫存資料

                    //設定畫面資料
                    tRCPcompany.Value = DocDataClass.company;       //公司別
                    tRCPapplyDate.Value = DocDataClass.applyDate;   //申請日
                    tRCPuserid.Value = DocDataClass.applyUser;      //申請人
                    tRCPdeptno.Value = DocDataClass.applyDept;      //申請部門
                    tRCPdocData.Value = tStrDocData;                //请假單暫存資料
                    #endregion
                }
                //處理回傳
                tP2MObject.AddRCPControls(tRCPcompany, tRCPapplyDate, tRCPuserid, tRCPdeptno, tRCPformType, tRCPformKind, tRCPworkType, tRCPstartDate, tRCPstartTime, tRCPendDate, tRCPendTime, tRCPapplyDay, tRCPapplyHour, tRCPreason, tRCPdocData);
            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetHKGL004_BasicSetting Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 公司別開窗
        public string GetOpenQueryCompany(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RDControl tRDcompany = new RDControl(new DataTable());  //公司別清單
                #endregion

                #region 取得畫面資料
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處理畫面資料
                #region 取得公司別清單資料
                //設定參數
                tfunctionName = "Company";    //取得公司別
                tparam.Clear();
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("functionName : " + tfunctionName);

                //叫用服務
                string tResponse = CallAPI(pM2Pxml, tfunctionName, tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"CompanyItems\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                Company CompanyClass = tSerObject.Deserialize<Company>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得公司別Table
                DataTable tCompanyTable = CompanyClass.GetCompanyTable(tSearch);
                tCompanyTable.Columns.Add("companyC");//    Control applyUser + C = 開窗控件要顯示的值
                for (int i = 0; i < tCompanyTable.Rows.Count; i++) { tCompanyTable.Rows[i]["companyC"] = tCompanyTable.Rows[i]["company"] + "-" + tCompanyTable.Rows[i]["name"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                SetRDControlPaged(pM2Pxml, tCompanyTable, ref tRDcompany);
                #endregion
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDcompany.AddCustomTag("DisplayColumn", "company§name§companyC");
                tRDcompany.AddCustomTag("StructureStyle", "company:R1:C1:1§name:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDcompany.AddCustomTag("ColumnsName", "代號§名稱§公司別");
                }
                else
                {
                    //簡體
                    tRDcompany.AddCustomTag("ColumnsName", "代号§名称§公司别");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDcompany);

            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetOpenQueryCompany Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        public string GetFormType_OnBuler(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");                      //请假單據暫存
                string tStrformType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");                    //请假類別code
                string fStrormTypeDesc = "1".Equals(tStrformType) ? "平日请假" : "节假日前后请假";
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = tSerObject.Deserialize<Entity>(tStrDocData);//请假單暫存資料

                //修改公司別資料
                DocDataClass.formType = tStrformType;
                DocDataClass.formTypeDesc = fStrormTypeDesc;

                //请假單Class轉成JSON
                string tDocdataJSON = tSerObject.Serialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetFormType_OnBuler Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetOpenQueryApplyDept(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RDControl tRDdeptno = new RDControl(new DataTable());  //部門清單
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "EFGPTEST");    //登入者帳號
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處理畫面資料
                #region 取得公司別清單資料
                //設定參數
                tfunctionName = "GetEmployeeDepartment";    //查詢員工部門
                tparam.Clear();
                tparam.Add("userId", tStrUserID);           //查詢對象

                //叫用服務
                string tResponse = CallAPI(pM2Pxml, tfunctionName, tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                                    //為符合class格式自己加上去的
                EmployeeDepartment DeparmentClass = tSerObject.Deserialize<EmployeeDepartment>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得部門Table
                DataTable tDeparmentTable = DeparmentClass.GetTable(tSearch);

                //轉換成符合格式的table
                if (tDeparmentTable.Columns.Contains("id")) { tDeparmentTable.Columns["id"].ColumnName = "applyDept"; }
                if (tDeparmentTable.Columns.Contains("organizationUnitName")) { tDeparmentTable.Columns["organizationUnitName"].ColumnName = "applyDeptDesc"; }
                tDeparmentTable.Columns.Add("applyDeptC");
                for (int i = 0; i < tDeparmentTable.Rows.Count; i++) { tDeparmentTable.Rows[i]["applyDeptC"] = tDeparmentTable.Rows[i]["applyDept"] + "-" + tDeparmentTable.Rows[i]["applyDeptDesc"]; }

                //設定公司別清單資料(含分頁)
                SetRDControlPaged(pM2Pxml, tDeparmentTable, ref tRDdeptno);
                #endregion
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDdeptno.AddCustomTag("DisplayColumn", "applyDept§applyDeptDesc§applyDeptC");
                tRDdeptno.AddCustomTag("ColumnsName", "代號§名稱§部門");
                tRDdeptno.AddCustomTag("StructureStyle", "applyDept:R1:C1:1§applyDeptDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDdeptno.AddCustomTag("ColumnsName", "代號§名稱§部門");
                }
                else
                {
                    //簡體
                    tRDdeptno.AddCustomTag("ColumnsName", "代号§名称§部门");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDdeptno);

            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetOpenQueryApplyDept Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        #region 请假类别開窗
        public string GetOpenQueryLeaveKind(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RDControl tRDleaveKind = new RDControl(new DataTable());  //公司別清單
                #endregion

                #region 取得畫面資料
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處理畫面資料
                #region 取得清單資料
                //設定參數
                tfunctionName = "getLeaveKind";    //取得公司別
                tparam.Clear();
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("functionName : " + tfunctionName);

                //叫用服務
                string tResponse = CallAPI(pM2Pxml, tfunctionName, tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                LeaveKind leaveKind = tSerObject.Deserialize<LeaveKind>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得Table
                DataTable table = leaveKind.GetDataTable(tSearch, new string[] { "formKind", "formKindDesc" });
                table.Columns.Add("formKindC");//    Control applyUser + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["formKindC"] = table.Rows[i]["formKind"] + "-" + table.Rows[i]["formKindDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                SetRDControlPaged(pM2Pxml, table, ref tRDleaveKind);
                #endregion
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDleaveKind.AddCustomTag("DisplayColumn", "formKind§formKindDesc§formKindC");
                tRDleaveKind.AddCustomTag("StructureStyle", "formKind:R1:C1:1§formKindDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDleaveKind.AddCustomTag("ColumnsName", "代號§名稱§显示");
                }
                else
                {
                    //簡體
                    tRDleaveKind.AddCustomTag("ColumnsName", "代号§名称§显示");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDleaveKind);

            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetOpenQueryLeaveKind Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        public string GetOpenQueryWorkType(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m
            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RDControl tRDworkType = new RDControl(new DataTable());  //公司別清單
                #endregion

                #region 取得畫面資料
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處理畫面資料
                #region 取得公司別清單資料
                //設定參數
                tfunctionName = "getWorkType";    //取得公司別
                tparam.Clear();
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("functionName : " + tfunctionName);

                //叫用服務
                string tResponse = CallAPI(pM2Pxml, tfunctionName, tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                WorkType workType = tSerObject.Deserialize<WorkType>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得公司別Table
                DataTable table = workType.GetDataTable(tSearch, new string[] { "workType", "workTypeDesc" });
                table.Columns.Add("workTypeC");//    Control applyUser + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["workTypeC"] = table.Rows[i]["workType"] + "-" + table.Rows[i]["workTypeDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                SetRDControlPaged(pM2Pxml, table, ref tRDworkType);
                #endregion
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDworkType.AddCustomTag("DisplayColumn", "workType§workTypeDesc§workTypeC");
                tRDworkType.AddCustomTag("StructureStyle", "workType:R1:C1:1§workTypeDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDworkType.AddCustomTag("ColumnsName", "代號§名稱§显示");
                }
                else
                {
                    //簡體
                    tRDworkType.AddCustomTag("ColumnsName", "代号§名称§显示");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDworkType);

            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetOpenQueryWorkType Error : " + err.Message.ToString());
            }
            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetApplyDept_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //加班單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrdeptno = DataTransferManager.GetControlsValue(pM2Pxml, "applyDeptC");    //申請部門(C是取外顯值)
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //加班單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = tSerObject.Deserialize<Entity>(tStrDocData);//加班單暫存資料

                //修改部門資料
                DocDataClass.applyDept = tStrdeptno;

                //加班單Class轉成JSON
                string tDocdataJSON = tSerObject.Serialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetApplyDept_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetLeaveKind_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //请假單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tFormKind = DataTransferManager.GetControlsValue(pM2Pxml, "formKind");
                string tFormKindDesc = DataTransferManager.GetControlsValue(pM2Pxml, "formKindC");
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //请假單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = tSerObject.Deserialize<Entity>(tStrDocData);//请假單暫存資料

                //修改公司別資料
                DocDataClass.formKind = tFormKind;
                DocDataClass.formKindDesc = tFormKindDesc;

                //请假單Class轉成JSON
                string tDocdataJSON = tSerObject.Serialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetLeaveKind_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetWorkType_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //请假單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tWorkType = DataTransferManager.GetControlsValue(pM2Pxml, "workType");
                string tWorkTypeDesc = DataTransferManager.GetControlsValue(pM2Pxml, "workTypeC");
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //请假單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = tSerObject.Deserialize<Entity>(tStrDocData);//请假單暫存資料

                //修改公司別資料
                DocDataClass.workType = tWorkType;
                DocDataClass.workTypeDesc = tWorkTypeDesc;

                //请假單Class轉成JSON
                string tDocdataJSON = tSerObject.Serialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetWorkType_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string CallAPI(XDocument pM2Pxml, string pFunctionName, Dictionary<string, string> pParam, out string pErrorMsg)
        {
            pErrorMsg = string.Empty;

            string tRequest = GetRequest(pFunctionName, pParam, out pErrorMsg);
            if (!string.IsNullOrEmpty(pErrorMsg))
            {
                pErrorMsg = string.Format("function : {0}\r\nGet Request Error : {1}", pFunctionName, pErrorMsg);
                return "";
            }
            else
            {
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("Request : " + tRequest);
            }

            string tResponse = CallAPI(tRequest, out pErrorMsg);
            if (!string.IsNullOrEmpty(pErrorMsg))
            {
                pErrorMsg = string.Format("function : {0}\r\nGet Response Error : {1}", pFunctionName, pErrorMsg);
                return "";
            }
            else
            {
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("Response : " + tResponse);
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
        public void SetRCPControlPaged(XDocument pM2PXml, DataTable pDT, ref RCPControl pRCP)
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
        public void SetRDControlPaged(XDocument pM2PXml, DataTable pDT, ref RDControl pRCP)
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
            pRCP.AddCustomTag("TotalPage", tTotalPage.ToString());      //總頁數
            pRCP.AddCustomTag("PageNo", tPageNO.ToString());            //目前頁數
            pRCP.Table = DataTransferManager.MakeDataTablePaged(pDT, tPageNO, Convert.ToInt32(tQueryCount));    //把結果給GridView
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
        public string ReturnErrorMsg(XDocument pM2Pxml, ref ProductToMCloudBuilder pP2MObject, string pErrorMsg)
        {
            pP2MObject.Message = pErrorMsg;
            pP2MObject.Result = "false";

            return pP2MObject.ToDucument().ToString();
        }
        #endregion

        #region 取得Request
        /// <summary>
        /// 取得Request
        /// </summary>
        /// <param name="functionName">方法名稱</param>
        /// <param name="param">參數</param>
        /// <returns></returns>
        public string GetRequest(string functionName, Dictionary<string, string> param, out string pErrorMsg)
        {
            /*使用範例
                Dictionary<string, string> tparam = new Dictionary<string, string>();
                new CustomLogger.Logger(pM2Pxml).WriteInfo("functionName : Company");
                string trequest = GetRequest("Company", tparam);
                new CustomLogger.Logger(pM2Pxml).WriteInfo("Request : " + trequest);
                string tresponse = CallAPI(trequest);
                new CustomLogger.Logger(pM2Pxml).WriteInfo("Response : " + tresponse);
             */
            pErrorMsg = string.Empty;       //錯誤訊息
            string api = string.Empty;   //服務位置
            try
            {
                switch (functionName)
                {
                    #region 取得公司別
                    case "Company":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/eap/company?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //取回:公司代号company、公司简称name
                        api = "eap/company";
                        break;
                    #endregion

                    #region 取得員工清單資料
                    case "GetEmployee":
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
                    case "GetEmployeeDepartment":
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
                    case "GetDepartment":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/organizationunit/f;applyUser=13120/s;applyUser=ASC/0/20?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //返回EFGP中的部门信息，/f;applyUser=13120/s;applyUser=ASC，筛选和排序条件，没有条件时用/f/s
                        //篩選條件:按代号（applyUser=XXXX）,名称（organizationUnitName=XXXX）
                        //取回:部门编号id、部门名称organizationUnitName

                        param.Add("condition",
                            (param.ContainsKey("deptId") ? string.Format(";applyUser={0}", param["deptId"]) : "") +                        //篩選條件:按代号(applyUser=XXXX)
                            (param.ContainsKey("deptName") ? string.Format(";organizationUnitName={0}", param["deptName"]) : "")    //篩選條件:按名称（organizationUnitName=XXXX）
                        );

                        api = string.Format("efgp/organizationunit/f{0}/s;applyUser=ASC/{1}/{2}",
                                    param.ContainsKey("condition") ? param["condition"] : "",   //搜尋條件 (;applyUser=xx;organizationUnitName=xx)
                                    param.ContainsKey("StartRow") ? param["StartRow"] : "0",    //搜尋起始筆數
                                    param.ContainsKey("EndRow") ? param["EndRow"] : "1000"      //搜尋結尾筆數
                                );
                        break;
                    #endregion

                    #region 部門人員查詢
                    case "GetDepartmentEmployee":
                        //ex:http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/users/functions/organizationunit/f;organizationUnit.applyUser=13120/s/0/20?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177
                        //根据部门代号返回EFGP中此部门相关人员信息
                        //篩選條件:部门代号(organizationUnit.applyUser=XXXX)
                        //取回:员工代号id、员工姓名userName

                        param.Add("condition",
                            (param.ContainsKey("deptId") ? string.Format(";organizationUnit.applyUser={0}", param["deptId"]) : "")  //篩選條件:部门代号(organizationUnit.applyUser=XXXX)
                        );

                        api = string.Format("efgp/users/functions/organizationunit/f{0}/s/{1}/{2}",
                                    param.ContainsKey("condition") ? param["condition"] : "",   //搜尋條件 (;users.applyUser=xx)
                                    param.ContainsKey("StartRow") ? param["StartRow"] : "0",    //搜尋起始筆數
                                    param.ContainsKey("EndRow") ? param["EndRow"] : "1000"      //搜尋結尾筆數
                                );
                        break;
                    #endregion
                    case "getLeaveKind":
                        api = "efgp/leavekind";
                        break;
                    case "getWorkType":
                        api = "efgp/worktype";
                        break;

                }

                if (!string.IsNullOrEmpty(api))
                {
                    //組合request
                    api = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), api, LoadConfig.GetWebConfig("APIKey"));
                }
                else
                {
                    pErrorMsg = "找不到functionName";
                }
            }
            catch (Exception err)
            {
                pErrorMsg = err.Message.ToString();
            }

            return api;
        }
        #endregion

        #region 取得Response
        /// <summary>
        /// 叫用API
        /// </summary>
        /// <param name="pRequest">Request</param>
        /// <param name="pErrorMsg">錯誤訊息</param>
        /// <param name="pParam">錯誤訊息</param>
        /// <returns>Rsponse</returns>
        public string CallAPI(string pRequest, out string pErrorMsg, params Dictionary<string, string>[] pParam)
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


        #region 立單
        public string GetHKGL004_CreateDoc(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                JavaScriptSerializer tSerObject = new JavaScriptSerializer();  //設定JSON轉換物件
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPbtnSave = new RCPControl("btnSave", null, null, null);       //送單
                RCPControl tRCPcompany = new RCPControl("company", "true", null, null);     //公司別
                RCPControl tRCPapplyDate = new RCPControl("applyDate", "false", null, null);//申請日
                RCPControl tRCPuserid = new RCPControl("applyUser", "false", null, null);      //申請人
                RCPControl tRCPdeptno = new RCPControl("applyDept", "true", null, null);
                RCPControl tRCPformType = new RCPControl("formType", "true", null, null);
                RCPControl tRCPformKind = new RCPControl("formKind", "true", null, null);
                RCPControl tRCPstartDate = new RCPControl("startDate", "true", null, null);
                RCPControl tRCPstartTime = new RCPControl("startTime", "true", null, null);
                RCPControl tRCPendDate = new RCPControl("endDate", "true", null, null);
                RCPControl tRCPendTime = new RCPControl("endTime", "true", null, null);
                RCPControl tRCPapplyDay = new RCPControl("applyDay", "true", null, null);
                RCPControl tRCPapplyHour = new RCPControl("applyHour", "true", null, null);
                RCPControl tRCPreason = new RCPControl("reason", "true", null, null);
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "EFGPTEST");    //登入者帳號
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                string company = DataTransferManager.GetControlsValue(pM2Pxml, "company");      //公司別
                string applyDept = DataTransferManager.GetControlsValue(pM2Pxml, "applyDept");        //申請部門
                string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");    //请假類別code
                string formTypeDesc = "1".Equals(formType) ? "平日请假" : "国庆或春节前后";   //请假類別name
                string startDate = DataTransferManager.GetControlsValue(pM2Pxml, "startDate");
                string startTime = DataTransferManager.GetControlsValue(pM2Pxml, "startTime");
                string endDate = DataTransferManager.GetControlsValue(pM2Pxml, "endDate");
                string endTime = DataTransferManager.GetControlsValue(pM2Pxml, "endTime");
                string leaveDay = DataTransferManager.GetControlsValue(pM2Pxml, "applyDay");
                string leaveHour = DataTransferManager.GetControlsValue(pM2Pxml, "applyHour");
                //string leaveMinute = DataTransferManager.GetControlsValue(pM2Pxml, "applyMinute");
                string reason = DataTransferManager.GetControlsValue(pM2Pxml, "reason");
                string docData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //请假單據暫存

                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = tSerObject.Deserialize<Entity>(docData);                //请假單暫存資料
                #endregion

                #region 檢查錯誤
                //檢查公司別必填
                if (string.IsNullOrEmpty(company))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "公司別必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "公司别必填\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(applyDept))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "申請部門必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "申请部门必填\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(startDate))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "请假日期必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "请假日期必填\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(endDate))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "截止日期必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "截止日期必填\r\n";
                    }
                }

                //檢查單身是否存在
                if (string.IsNullOrEmpty(docData))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "請確定單身資料是否存在\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "请确定单身资料是否存在\r\n";
                    }
                }
                try
                {
                    Double.Parse(leaveDay);
                    Double.Parse(leaveHour);
                }
                catch (Exception ex)
                {
                    tErrorMsg += "请假天数或时间格式错误";
                }

                if (!string.IsNullOrEmpty(tErrorMsg)) return ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                #endregion

                #region 格式
                /*
                response
                {"code":"200","msg":"PKG_HK_GL03400000066"}
                */
                #endregion

                //單頭
                DocDataClass.company = DocDataClass.company.Split('-')[0];    //公司別只取id
                DocDataClass.applyUser = DocDataClass.applyUser.Split('-')[0];              //申請人只取id
                DocDataClass.applyDept = DocDataClass.applyDept.Split('-')[0];
                DocDataClass.startDate = startDate.Insert(4, "-").Insert(7, "-"); ;
                DocDataClass.startTime = startTime.Insert(2, ":");
                DocDataClass.endDate = endDate.Insert(4, "-").Insert(7, "-"); ;
                DocDataClass.endTime = endTime.Insert(2, ":");
                DocDataClass.leaveDay = Double.Parse(leaveDay);
                DocDataClass.leaveHour = Double.Parse(leaveHour);
                DocDataClass.leaveMinute = 0d;
                DocDataClass.reason = reason;

                #region 取得Response
                //叫用API
                string uri = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), "efgp/hkgl004/create", LoadConfig.GetWebConfig("APIKey"));
                string tBodyContext = tSerObject.Serialize(DocDataClass);

                ////測試用
                //string uri = "http://i2.hanbell.com.cn:8080/Hanbell-JRS/api/efgp/hkgl034/create?appid=1505278334853&token=0ec858293fccfad55575e26b0ce31177";
                //string param = "{\"head\":{\"company\":\"C\",\"date\":\"2017/09/23\",\"id\":\"C0160\",\"deptno\":\"13120\",\"formType\":\"1\",\"formTypeDesc\":\"平日请假\"},\"body\":[{\"lunch\":\"N\",\"dinner\":\"N\",\"deptno\":\"13120\",\"id\":\"C0160\",\"date\":\"2017/9/23\",\"starttime\":\"17:10\",\"endtime\":\"18:10\",\"worktime\":\"1\",\"note\":\"備註\"},{\"lunch\":\"N\",\"dinner\":\"N\",\"deptno\":\"13120\",\"id\":\"C0160\",\"date\":\"2017/9/24\",\"starttime\":\"17:10\",\"endtime\":\"18:10\",\"worktime\":\"1\",\"note\":\"備註\"}],\"appid\":\"1505278334853\",\"token\":\"0ec858293fccfad55575e26b0ce31177\"}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);   //建立連線
                request.Method = "POST";                                                //設定叫用模型
                request.ContentType = "application/json";
                request.Timeout = 300000;                                               //設定timeout時間
                string tResponse = string.Empty;

                //Get Response
                try
                {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(tBodyContext);
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
                            tErrorMsg = "叫用服務失敗:" + response.StatusCode.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    tErrorMsg = "叫用服務失敗:" + ex.Message.ToString();
                }

                #endregion

                #region 處理畫面資料
                if (!string.IsNullOrEmpty(tErrorMsg))
                {
                    ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                }
                else
                {
                    //轉成class 方便取用
                    ResponseState rs = tSerObject.Deserialize<ResponseState>(tResponse);

                    //判斷回傳
                    if ("200".Equals(rs.code))
                    {
                        string tMsg = string.Empty;

                        //設定多語系
                        if ("Lang01".Equals(tStrLanguage))
                        {
                            //繁體
                            tMsg = "請假單建立成功，單號 :\r\n";
                        }
                        else
                        {
                            //簡體
                            tMsg = "请假单建立成功，单号 :\r\n";
                        }

                        tP2MObject.Message = tMsg + rs.msg;
                        tP2MObject.Result = "false";


                        //設定请假單資料
                        Entity newEntity = new Entity(company, "", tStrUserID, applyDept, formType, formTypeDesc, "1", "年休假");
                        newEntity.workType = "1";
                        newEntity.workTypeDesc = "常日班：80：00";
                        string DocDataStr = tSerObject.Serialize(newEntity);//Class轉成JSON

                        //給定畫面欄位值
                        tRCPstartDate.Value = "";
                        tRCPendDate.Value = "";
                        tRCPapplyDay.Value = "0";
                        tRCPapplyHour.Value = "0";
                        tRCPreason.Value = "";
                        tRCPdocData.Value = DocDataStr;

                        //處理回傳
                        tP2MObject.AddRCPControls(tRCPstartDate, tRCPendDate, tRCPapplyDay, tRCPapplyHour, tRCPreason, tRCPdocData);
                    }
                    else
                    {
                        tP2MObject.Message = "请假单建立失败，原因 :\r\n" + rs.msg;
                        tP2MObject.Result = "false";
                    }

                }
                #endregion

            }
            catch (Exception err)
            {
                ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetHKGL004_CreateDoc Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #endregion
    }

    //请假單Class
    class Entity
    {

        public string company { get; set; }     //公司別
        public string applyDate { get; set; }        //申請日
        public string applyUser { get; set; }          //申請人
        public string applyDept { get; set; }      //申請部門
        public string formType { get; set; }    //请假類別code
        public string formTypeDesc { get; set; }//请假類別name
        public string formKind { get; set; }    //请假類別code
        public string formKindDesc { get; set; }//请假類別name
        public string workType { get; set; }    //请假類別code
        public string workTypeDesc { get; set; }//请假類別name
        public string startDate { get; set; }
        public string startTime { get; set; }
        public string endDate { get; set; }
        public string endTime { get; set; }

        public double leaveDay { get; set; }
        public double leaveHour { get; set; }
        public double leaveMinute { get; set; }

        public string reason { get; set; }

        public Entity()
        {
            this.company = "";
            this.applyDate = "";
            this.applyUser = "";
            this.applyDept = "";
            this.formType = "1";
            this.formTypeDesc = "平日请假";
            this.formKind = "1";
            this.formTypeDesc = "年休假";
            this.leaveDay = 0d;
            this.leaveHour = 0d;
            this.leaveMinute = 0d;
        }

        public Entity(string company, string date, string id, string deptno, string formType, string formTypeDesc, string formKind, string formKindDesc)
        {
            this.company = company;
            this.applyDate = date;
            this.applyUser = id;
            this.applyDept = deptno;
            this.formType = formType;
            this.formTypeDesc = formTypeDesc;
            this.formKind = formKind;
            this.formKindDesc = formKindDesc;
            this.leaveDay = 0d;
            this.leaveHour = 0d;
            this.leaveMinute = 0d;
        }

    }

    class WorkType
    {
        public List<KV> data;

        public WorkType()
        {
            data = new List<KV>();
        }

        public DataTable GetDataTable(string pSearch, string[] columns)
        {
            Dictionary<string, string[]> item = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable table = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                item = this.data[i].GetItem(); //取得公司別資料

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in columns) table.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in item["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (tInsertData) table.Rows.Add(item["Values"]);
            }

            return table;
        }
    }

}