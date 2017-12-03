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

namespace cn.hanbell.mcloud.HKGL004
{
    public class HKGL004
    {

        #region 頁面初使化、请假單身分頁、搜尋
        public string GetBasicSetting(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, "请假单"); //參數:要顯示的訊息、結果、Title//建構p2m

            #region 設定參數                        
            Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
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
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系

                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //请假單暫存資料
                #endregion

                #region 處裡畫面資料
                //檢查是否已有單據資料
                if (string.IsNullOrEmpty(entityJSONStr))//沒單據資料
                {
                    #region 取得預設資料
                    //設定參數
                    tparam.Clear();
                    tparam.Add("UserID", tStrUserID);       //查詢對象

                    //叫用服務
                    string tResponse = Utility.CallAPI(pM2Pxml, "Users", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    //回傳的JSON轉Class
                    Users EmployDetail = Utility.JSONDeserialize<Users>(tResponse);  //員工明細

                    //取值
                    string tCompany = string.Format("{0}-{1}", EmployDetail.company, ""/*EmployDetail.companyName*/);   //公司別代號-公司別名稱
                    string tUser = string.Format("{0}-{1}", EmployDetail.id, EmployDetail.userName);    //申請人代號-申請人名稱
                    string tDetp = string.Format("{0}-{1}", EmployDetail.deptno, EmployDetail.deptname);    //部門代號-部門名稱
                    string tDate = DateTime.Now.ToString("yyyy/MM/dd"); //申請時間
                    string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");    //请假類別code
                    string formTypeDesc = "1".Equals(formType) ? "平日请假" : "国庆或春节前后";   //请假類別name

                    #region 取得公司別名稱(如果GetEmployeeDetail有回傳名稱的話可以省掉)
                    tparam.Clear();

                    //取公司別值
                    tResponse = Utility.CallAPI(pM2Pxml, "Company", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    tResponse = "{\"data\" : " + tResponse + "}";                                           //為符合class格式自己加上去的
                    Company CompanyClass = Utility.JSONDeserialize<Company>(tResponse);                        //轉class方便取用
                    DataTable tCompanyTable = CompanyClass.GetTable(EmployDetail.company);                   //取公司別table
                    DataRow[] tCompanyRow = tCompanyTable.Select("company = '" + EmployDetail.company + "'");       //查詢指定company
                    if (tCompanyRow.Count() > 0) tCompany = tCompanyRow[0]["company"] + "-" + tCompanyRow[0]["name"];
                    #endregion

                    #endregion

                    #region 給值
                    //設定请假單資料
                    Entity entityClass = new Entity(tCompany, tDate, tUser, tDetp, formType, formTypeDesc, "1", "年休假");
                    entityClass.workType = "1";
                    entityClass.workTypeDesc = "常日班：80：00";
                    entityJSONStr = Utility.JSONSerialize(entityClass);//Class轉成JSON

                    //給定畫面欄位值
                    tRCPcompany.Value = tCompany;       //公司別
                    tRCPapplyDate.Value = tDate;        //申請日
                    tRCPuserid.Value = tUser;           //申請人
                    tRCPdeptno.Value = tDetp;           //申請部門
                    tRCPformKind.Value = "1-年休假";
                    tRCPworkType.Value = "1-常日班：08：00-20：20";
                    tRCPapplyDay.Value = "0";
                    tRCPapplyHour.Value = "0";
                    tRCPdocData.Value = entityJSONStr;     //请假單暫存資料

                    #endregion
                }
                else//有單據資料
                {
                    #region 顯示请假單資料
                    //先將暫存資料轉成Class方便取用
                    Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//请假單暫存資料

                    //設定畫面資料
                    tRCPcompany.Value = entityClass.company;       //公司別
                    tRCPapplyDate.Value = entityClass.applyDate;   //申請日
                    tRCPuserid.Value = entityClass.applyUser;      //申請人
                    tRCPdeptno.Value = entityClass.applyDept;      //申請部門
                    tRCPdocData.Value = entityJSONStr;                //请假單暫存資料
                    #endregion
                }
                #endregion
                //處理回傳
                tP2MObject.AddRCPControls(tRCPcompany, tRCPapplyDate, tRCPuserid, tRCPdeptno, tRCPformType, tRCPformKind, tRCPworkType,
                    tRCPstartDate, tRCPstartTime, tRCPendDate, tRCPendTime, tRCPapplyDay, tRCPapplyHour, tRCPreason, tRCPdocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBasicSetting Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 公司別開窗
        public string GetCompany_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
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
                tparam.Clear();

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "Company", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                Company CompanyClass = Utility.JSONDeserialize<Company>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得公司別Table
                DataTable tCompanyTable = CompanyClass.GetTable(tSearch);
                tCompanyTable.Columns.Add("companyC");//    Control applyUser + C = 開窗控件要顯示的值
                for (int i = 0; i < tCompanyTable.Rows.Count; i++) { tCompanyTable.Rows[i]["companyC"] = tCompanyTable.Rows[i]["company"] + "-" + tCompanyTable.Rows[i]["name"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, tCompanyTable, ref tRDcompany);
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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetCompany_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 单据类别更新
        public string GetFormType_OnBuler(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");                      //请假單據暫存
                string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");                    //请假類別code
                string formTypeDesc = "1".Equals(formType) ? "平日请假" : "节假日前后请假";
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//请假單暫存資料

                //修改公司別資料
                entityClass.formType = formType;
                entityClass.formTypeDesc = formTypeDesc;

                //请假單Class轉成JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //給值
                tRCPDocData.Value = entityJSONStr;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetFormType_OnBuler Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 申请部门开窗
        public string GetApplyDept_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RDControl tRDdeptno = new RDControl(new DataTable());  //部門清單
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處理畫面資料
                #region 取得公司別清單資料
                //設定參數
                tparam.Clear();
                tparam.Add("userId", tStrUserID);           //查詢對象

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "EmployeeDepartment", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                                    //為符合class格式自己加上去的
                EmployeeDepartment DeparmentClass = Utility.JSONDeserialize<EmployeeDepartment>(tResponse);    //轉class方便取用
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
                Utility.SetRDControlPage(pM2Pxml, tDeparmentTable, ref tRDdeptno);
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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetApplyDept_OpenQuery Error : " + err.Message.ToString());
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
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //请假單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string applyDeptC = DataTransferManager.GetControlsValue(pM2Pxml, "applyDeptC");    //申請部門(C是取外顯值)
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //请假單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//请假單暫存資料

                //修改部門資料
                entityClass.applyDept = applyDeptC;

                //请假單Class轉成JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //給值
                tRCPDocData.Value = entityJSONStr;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetApplyDept_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 请假类别開窗
        public string GetLeaveKind_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
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
                tparam.Clear();
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("functionName : " + tfunctionName);

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "LeaveKind", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                LeaveKind leaveKind = Utility.JSONDeserialize<LeaveKind>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得Table
                DataTable table = leaveKind.GetDataTable(tSearch, new string[] { "formKind", "formKindDesc" });
                table.Columns.Add("formKindC");//    Control ID + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["formKindC"] = table.Rows[i]["formKind"] + "-" + table.Rows[i]["formKindDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, table, ref tRDleaveKind);
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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetLeaveKind_OpenQuery Error : " + err.Message.ToString());
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
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //请假單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string formKind = DataTransferManager.GetControlsValue(pM2Pxml, "formKind");
                string formKindDesc = DataTransferManager.GetControlsValue(pM2Pxml, "formKindC");
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //请假單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//请假單暫存資料

                //修改公司別資料
                entityClass.formKind = formKind;
                entityClass.formKindDesc = formKindDesc;

                //请假單Class轉成JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //給值
                tRCPDocData.Value = entityJSONStr;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetLeaveKind_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 工作班别开窗
        public string GetWorkType_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m
            try
            {

                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤

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
                tparam.Clear();

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "WorkType", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                WorkType workType = Utility.JSONDeserialize<WorkType>(tResponse);   //轉class方便取用
                #endregion

                #region 給值
                //取得公司別Table
                DataTable table = workType.GetDataTable(tSearch, new string[] { "workType", "workTypeDesc" });
                table.Columns.Add("workTypeC");//    Control ID + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["workTypeC"] = table.Rows[i]["workType"] + "-" + table.Rows[i]["workTypeDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, table, ref tRDworkType);
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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetWorkType_OpenQuery Error : " + err.Message.ToString());
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
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //请假單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string workType = DataTransferManager.GetControlsValue(pM2Pxml, "workType");
                string workTypeDesc = DataTransferManager.GetControlsValue(pM2Pxml, "workTypeC");
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //请假單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//请假單暫存資料

                //修改公司別資料
                entityClass.workType = workType;
                entityClass.workTypeDesc = workTypeDesc;

                //请假單Class轉成JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //給值
                tRCPDocData.Value = entityJSONStr;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetWorkType_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 发起流程
        public string InvokeProcess(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
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
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
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

                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //请假單據暫存

                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);                //请假單暫存資料
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
                if (string.IsNullOrEmpty(entityJSONStr))
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

                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                #endregion

                #region 格式
                /*
                response
                {"code":"200","msg":"PKG_HK_GL03400000066"}
                */
                #endregion

                //單頭
                entityClass.company = entityClass.company.Split('-')[0];    //公司別只取id
                entityClass.applyUser = entityClass.applyUser.Split('-')[0];              //申請人只取id
                entityClass.applyDept = entityClass.applyDept.Split('-')[0];
                entityClass.startDate = startDate.Insert(4, "-").Insert(7, "-"); ;
                entityClass.startTime = startTime.Insert(2, ":");
                entityClass.endDate = endDate.Insert(4, "-").Insert(7, "-"); ;
                entityClass.endTime = endTime.Insert(2, ":");
                entityClass.leaveDay = Double.Parse(leaveDay);
                entityClass.leaveHour = Double.Parse(leaveHour);
                entityClass.leaveMinute = 0d;
                entityClass.reason = reason;

                #region 取得Response
                //叫用API
                string uri = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), "efgp/hkgl004/create", LoadConfig.GetWebConfig("APIKey"));
                entityJSONStr = Utility.JSONSerialize(entityClass);

                string tResponse = Utility.InvokeProcess(uri, entityJSONStr, out tErrorMsg);
                #endregion

                #region 處理畫面資料
                if (!string.IsNullOrEmpty(tErrorMsg))
                {
                    Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                }
                else
                {
                    //轉成class 方便取用
                    ResponseState rs = Utility.JSONDeserialize<ResponseState>(tResponse);

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
                        string DocDataStr = Utility.JSONSerialize(newEntity);//Class轉成JSON

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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "InvokeProcess Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
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

    class LeaveKind
    {
        public List<KV> data;

        public LeaveKind()
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