using com.digiwin.Mobile.MCloud.TransferLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace cn.hanbell.mcloud.HKGL034
{
    public class HKGL034
    {
        #region 頁面初使化、加班單身分頁、搜尋
        public string GetBasicSetting(XDocument pM2Pxml)
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
                RCPControl tRCPAdd = new RCPControl("Add", null, null, null);               //新增
                RCPControl tRCPDel = new RCPControl("Del", null, null, null);               //刪除
                RCPControl tRCPformType = new RCPControl("formType", "true", null, null);   //加班類別
                RCPControl tRCPcompany = new RCPControl("company", "true", null, null);     //公司別
                RCPControl tRCPdate = new RCPControl("date", "false", null, null);          //申請日
                RCPControl tRCPuserid = new RCPControl("userid", "false", null, null);      //申請人
                RCPControl tRCPdeptno = new RCPControl("deptno", "true", null, null);       //申請部門
                RCPControl tRCPBodyInfo = new RCPControl("BodyInfo", "true", null, null);   //申請單單身
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //加班單暫存資料
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //加班單暫存資料
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處裡畫面資料
                //檢查是否已有單據資料
                if (string.IsNullOrEmpty(tStrDocData))//沒單據資料
                {
                    #region 取得預設資料
                    //設定參數
                    tparam.Clear();
                    tparam.Add("UserID", tStrUserID);

                    //叫用服務
                    string tResponse = Utility.CallAPI(pM2Pxml, "Users", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    //回傳的JSON轉Class
                    Users EmployDetail = Utility.JSONDeserialize<Users>(tResponse);  //員工明細

                    //取值
                    string tCompany = string.Format("{0}-{1}", EmployDetail.company, ""/*EmployDetail.companyName*/);   //公司別代號-公司別名稱
                    string tUser = string.Format("{0}-{1}", EmployDetail.id, EmployDetail.userName);                    //申請人代號-申請人名稱
                    string tDetp = string.Format("{0}-{1}", EmployDetail.deptno, EmployDetail.deptname);                //部門代號-部門名稱
                    string tDate = DateTime.Now.ToString("yyyy/MM/dd");                                                 //申請時間
                    string tStrformType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");                    //加班類別code
                    string fStrormTypeDesc = "1".Equals(tStrformType) ? "平日加班" : "2".Equals(tStrformType) ? "双休加班" : "节日加班";    //加班類別name

                    #region 取得公司別名稱(如果有回傳名稱的話可以省掉)
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
                    //設定加班單資料
                    Entity DocDataClass = new Entity();
                    DocDataClass.head = new HeadData(tCompany, tDate, tUser, tDetp, tStrformType, fStrormTypeDesc);
                    string DocDataStr = Utility.JSONSerialize(DocDataClass);//Class轉成JSON

                    //給定畫面欄位值
                    tRCPcompany.Value = tCompany;       //公司別
                    tRCPdate.Value = tDate;             //申請日
                    tRCPuserid.Value = tUser;           //申請人
                    tRCPdeptno.Value = tDetp;           //申請部門
                    tRCPDocData.Value = DocDataStr;     //加班單暫存資料
                    #endregion
                }
                else//有單據資料
                {
                    #region 顯示加班單資料
                    //先將暫存資料轉成Class方便取用
                    Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                    //設定畫面資料
                    tRCPcompany.Value = DocDataClass.head.company;      //公司別
                    tRCPdate.Value = DocDataClass.head.date;            //申請日
                    tRCPuserid.Value = DocDataClass.head.id;            //申請人
                    tRCPdeptno.Value = DocDataClass.head.deptno;        //申請部門
                    tRCPDocData.Value = tStrDocData;                    //加班單暫存資料

                    //因實際不影響單身新增資料, 故取消唯讀控制
                    ////檢查是否有單身資料
                    //if (!(DocDataClass.body.Count > 0))//沒有單身資料
                    //{
                    //    //設定單頭欄位屬性
                    //    tRCPcompany.Enable = "true";   //公司別
                    //    tRCPdeptno.Enable = "true";    //申請部門
                    //}
                    //else//有單身資料
                    //{
                    //    //設定單頭屬性
                    //    tRCPcompany.Enable = "false";   //公司別
                    //    tRCPdeptno.Enable = "false";    //申請部門
                    //}

                    //整理Table轉成符合顯示的格式
                    DataTable tBodyTable = DocDataClass.GetBodyDataTable(tSearch);
                    tBodyTable.Columns.Add("dateC");//日期,有加/
                    tBodyTable.Columns.Add("time"); //加班時間
                    tBodyTable.Columns.Add("food"); //供餐
                    for (int i = 0; i < tBodyTable.Rows.Count; i++)
                    {
                        string tstarttime = tBodyTable.Rows[i]["starttime"].ToString().Insert(2, ":");
                        string tdate = tBodyTable.Rows[i]["date"].ToString().Insert(4, "/").Insert(7, "/");
                        string tendtime = tBodyTable.Rows[i]["endtime"].ToString().Insert(2, ":");
                        string tlunch = tBodyTable.Rows[i]["lunch"].ToString();
                        string tdinner = tBodyTable.Rows[i]["dinner"].ToString();

                        tBodyTable.Rows[i]["dateC"] = tdate;
                        tBodyTable.Rows[i]["time"] = tstarttime + "-" + tendtime;
                        tBodyTable.Rows[i]["food"] = ("N".Equals(tlunch) && "N".Equals(tdinner)) ? "無" :
                                                     ("Y".Equals(tlunch) && "Y".Equals(tdinner)) ? "午、晚餐" :
                                                     ("Y".Equals(tlunch) && "N".Equals(tdinner)) ? "午餐" : "晚餐";

                    }

                    //設定單身資料(含分頁)
                    Utility.SetRCPControlPage(pM2Pxml, tBodyTable, ref tRCPBodyInfo);
                    #endregion
                }
                #endregion

                #region 設定物件屬性
                //清除GridView勾選
                tRCPBodyInfo.Value = "";

                //單身清單屬性
                tRCPBodyInfo.AddCustomTag("DisplayColumn", "id§dateC§time§worktime§food§note");
                tRCPBodyInfo.AddCustomTag("StructureStyle", "id:R1:C1:3:::§dateC:R1:C4:3:::§time:R2:C1:3:::§worktime:R2:C4:1:::§food:R2:C5:2:::§note:R3:C1:6:::");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRCPbtnSave.Title = "送單";
                    tRCPAdd.Title = "新增";
                    tRCPDel.Title = "刪除";
                    tRCPformType.Title = "加班類別";
                    tRCPcompany.Title = "公司別";
                    tRCPdate.Title = "申請日";
                    tRCPuserid.Title = "申請人";
                    tRCPdeptno.Title = "申請部門";
                    tRCPBodyInfo.AddCustomTag("ColumnsName", "加班人員§加班日期§加班時間§時數§供餐§加班內容");
                }
                else
                {
                    //簡體
                    tRCPbtnSave.Text = "送单";
                    tRCPAdd.Text = "新增";
                    tRCPDel.Text = "删除";
                    tRCPformType.Title = "加班类别";
                    tRCPcompany.Title = "公司别";
                    tRCPdate.Title = "申请日";
                    tRCPuserid.Title = "申请人";
                    tRCPdeptno.Title = "申请部门";
                    tRCPBodyInfo.AddCustomTag("ColumnsName", "加班人员§加班日期§加班时间§时数§供餐§加班内容");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPformType, tRCPcompany, tRCPdate, tRCPuserid, tRCPdeptno, tRCPBodyInfo, tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBasicSetting Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 加班類別異動
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
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //加班單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");                      //加班單據暫存
                string tStrformType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");                    //加班類別code
                string fStrormTypeDesc = "1".Equals(tStrformType) ? "平日加班" : "2".Equals(tStrformType) ? "双休加班" : "节日加班";//加班類別name
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                //修改公司別資料
                DocDataClass.head.formType = tStrformType;
                DocDataClass.head.formTypeDesc = fStrormTypeDesc;

                //加班單Class轉成JSON
                string tDocdataJSON = Utility.JSONSerialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
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
                Company CompanyClass = Utility.JSONDeserialize<Company>(tResponse); //轉class方便取用
                #endregion

                #region 給值
                //取得公司別Table
                DataTable tCompanyTable = CompanyClass.GetTable(tSearch);
                tCompanyTable.Columns.Add("companyC");//    Control id + C = 開窗控件要顯示的值
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

        #region 公司別異動
        public string GetCompany_OnBlur(XDocument pM2Pxml)
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
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //加班單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrcompany = DataTransferManager.GetControlsValue(pM2Pxml, "companyC");  //公司別(C是取外顯值)
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //加班單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                //修改公司別資料
                DocDataClass.head.company = tStrcompany;

                //加班單Class轉成JSON
                string tDocdataJSON = Utility.JSONSerialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetCompany_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 部門開窗
        public string GetDeptno_OpenQuery(XDocument pM2Pxml)
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
                //查詢員工部門
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
                if (tDeparmentTable.Columns.Contains("id")) { tDeparmentTable.Columns["id"].ColumnName = "deptno"; }
                if (tDeparmentTable.Columns.Contains("organizationUnitName")) { tDeparmentTable.Columns["organizationUnitName"].ColumnName = "deptnoName"; }
                tDeparmentTable.Columns.Add("deptnoC");
                for (int i = 0; i < tDeparmentTable.Rows.Count; i++) { tDeparmentTable.Rows[i]["deptnoC"] = tDeparmentTable.Rows[i]["deptno"] + "-" + tDeparmentTable.Rows[i]["deptnoName"]; }

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, tDeparmentTable, ref tRDdeptno);
                #endregion
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDdeptno.AddCustomTag("DisplayColumn", "deptno§deptnoName§deptnoC");
                tRDdeptno.AddCustomTag("ColumnsName", "代號§名稱§部門");
                tRDdeptno.AddCustomTag("StructureStyle", "deptno:R1:C1:1§deptnoName:R1:C2:1");

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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDeptno_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 部門異動
        public string GetDeptno_OnBlur(XDocument pM2Pxml)
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
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //加班單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrdeptno = DataTransferManager.GetControlsValue(pM2Pxml, "deptnoC");    //申請部門(C是取外顯值)
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //加班單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                //修改部門資料
                DocDataClass.head.deptno = tStrdeptno;

                //加班單Class轉成JSON
                string tDocdataJSON = Utility.JSONSerialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDeptno_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 刪除單身
        public string GetDetail01_Del(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPcompany = new RCPControl("company", "true", null, null);     //公司別
                RCPControl tRCPdeptno = new RCPControl("deptno", "true", null, null);       //申請部門
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //加班單據隱藏欄位
                RCPControl tRCPBodyInfo = new RCPControl("BodyInfo", "true", null, null);   //申請單單身
                #endregion

                #region 取得畫面資料
                string tStrBodyInfo = DataTransferManager.GetControlsValue(pM2Pxml, "BodyInfo");    //加班單單身勾選的資料
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //加班單據暫存
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                #endregion

                if (!string.IsNullOrEmpty(tStrBodyInfo))
                {
                    #region 處理畫面資料
                    //先將暫存資料轉成Class方便取用
                    Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                    //刪除單身資料
                    foreach (var item in tStrBodyInfo.Split('§'))
                    {
                        foreach (var bodyitem in DocDataClass.body)
                        {
                            if (item.Equals(bodyitem.item))
                            {
                                DocDataClass.body.Remove(bodyitem);
                                break;
                            }
                        }
                    }

                    #region 顯示加班單資料                    
                    tRCPDocData.Value = Utility.JSONSerialize(DocDataClass);                    //加班單暫存資料//加班單Class轉成JSON

                    //檢查是否有單身資料
                    if (!(DocDataClass.body.Count > 0))//沒有單身資料
                    {
                        //設定單頭欄位屬性
                        tRCPcompany.Enable = "true";   //公司別
                        tRCPdeptno.Enable = "true";    //申請部門
                    }
                    else//有單身資料
                    {
                        //設定單頭屬性
                        tRCPcompany.Enable = "false";   //公司別
                        tRCPdeptno.Enable = "false";    //申請部門
                    }

                    //整理Table轉成符合顯示的格式
                    DataTable tBodyTable = DocDataClass.GetBodyDataTable(tSearch);
                    tBodyTable.Columns.Add("dateC");//日期,有加/
                    tBodyTable.Columns.Add("time"); //加班時間
                    tBodyTable.Columns.Add("food"); //供餐
                    for (int i = 0; i < tBodyTable.Rows.Count; i++)
                    {
                        string tstarttime = tBodyTable.Rows[i]["starttime"].ToString().Insert(2, ":");
                        string tdate = tBodyTable.Rows[i]["date"].ToString().Insert(4, "/").Insert(7, "/");
                        string tendtime = tBodyTable.Rows[i]["endtime"].ToString().Insert(2, ":");
                        string tlunch = tBodyTable.Rows[i]["lunch"].ToString();
                        string tdinner = tBodyTable.Rows[i]["dinner"].ToString();

                        tBodyTable.Rows[i]["dateC"] = tdate;
                        tBodyTable.Rows[i]["time"] = tstarttime + "-" + tendtime;
                        tBodyTable.Rows[i]["food"] = ("N".Equals(tlunch) && "N".Equals(tdinner)) ? "無" :
                                                     ("Y".Equals(tlunch) && "Y".Equals(tdinner)) ? "午、晚餐" :
                                                     ("Y".Equals(tlunch) && "N".Equals(tdinner)) ? "午餐" : "晚餐";

                    }

                    //設定單身資料(含分頁)
                    Utility.SetRCPControlPage(pM2Pxml, tBodyTable, ref tRCPBodyInfo);
                    #endregion

                    #endregion

                    //處理回傳
                    tP2MObject.AddRCPControls(tRCPcompany, tRCPdeptno, tRCPDocData, tRCPBodyInfo);
                }
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDetail01_Del Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 加班明細 頁面初使化
        public string GetDetail01_BasicSetting(XDocument pM2Pxml)
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
                RCPControl tRCPAdd = new RCPControl("Add", "true", "true", null);          //新增按鈕
                RCPControl tRCPEdit = new RCPControl("Edit", "true", "false", null);       //修改
                RCPControl tRCfood = new RCPControl("food", "true", null, null);            //供餐
                RCPControl tRCPdeptno = new RCPControl("deptno", "true", null, null);       //加班部門
                RCPControl tRCPids = new RCPControl("ids", "true", "true", null);           //加班人員(多人/新增)
                RCPControl tRCPid = new RCPControl("id", "false", "false", null);           //加班人員(單人/修改)
                RCPControl tRCPdate = new RCPControl("date", "true", null, null);           //加班日期
                RCPControl tRCPstarttime = new RCPControl("starttime", "true", null, null); //加班起
                RCPControl tRCPendtime = new RCPControl("endtime", "true", null, null);     //加班迄
                RCPControl tRCPhour = new RCPControl("hour", "true", null, null);           //加班時數
                RCPControl tRCPnote = new RCPControl("note", "true", null, null);           //加班說明
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //申請單資料(隱)
                RCPControl tRCPitem = new RCPControl("item", null, null, null);             //判斷是新增或是修改(隱)
                #endregion

                #region 取得畫面資料
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");        //登入者帳號
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");          //加班單暫存資料
                string tStritem = DataTransferManager.GetControlsValue(pM2Pxml, "item");                //判斷是新增或是修改
                #endregion

                #region 處裡畫面資料
                //判斷新增或是修改
                if (string.IsNullOrEmpty(tStritem))//新增
                {
                    #region 取得預設資料
                    //設定參數
                    //取得員工明細資料
                    tparam.Clear();
                    tparam.Add("UserID", tStrUserID);       //查詢對象

                    //叫用服務
                    string tResponse = Utility.CallAPI(pM2Pxml, "Users", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    //回傳的JSON轉Class
                    Users EmployDetail = Utility.JSONDeserialize<Users>(tResponse);  //員工明細

                    //取值
                    string tDetp = string.Format("{0}-{1}", EmployDetail.deptno, EmployDetail.deptname);                //部門代號-部門名稱
                    string tDate = DateTime.Now.ToString("yyyy/MM/dd"); //加班日期
                    string tTime = DateTime.Now.ToString("HH:mm");//加班起訖
                    #endregion

                    #region 給預設值
                    tRCfood.Value = "0";                //供餐
                    tRCPdeptno.Value = tDetp;           //加班部門
                    tRCPids.Value = "";                 //加班人員(多人/新增)
                    tRCPid.Value = "";                  //加班人員(單人/修改)
                    tRCPdate.Value = tDate;             //加班日期
                    tRCPstarttime.Value = tTime;        //加班起
                    tRCPendtime.Value = tTime;          //加班迄
                    tRCPhour.Value = "";                //加班時數
                    tRCPnote.Value = "";                //加班說明
                    tRCPDocData.Value = tStrDocData;    //申請單資料(隱)
                    tRCPitem.Value = tStritem;          //判斷是新增或是修改(隱)                    
                    #endregion

                    #region 設定物件屬性
                    tRCPAdd.Visible = "true";           //新增按鈕
                    tRCPEdit.Visible = "false";         //修改按鈕
                    tRCPdeptno.Enable = "true";         //加班部門
                    tRCPids.Visible = "true";           //加班人員(多人/新增)
                    tRCPid.Visible = "false";           //加班人員(單人/修改)
                    #endregion                    
                }
                else //修改
                {
                    #region 取得單身資料
                    //先將暫存資料轉成Class方便取用
                    Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                    //取值
                    string tdinner = string.Empty;      //晚餐
                    string tlunch = string.Empty;       //午餐
                    string tdeptno = string.Empty;      //加班部門
                    string tid = string.Empty;          //加班人員
                    string tdate = string.Empty;        //加班日期
                    string tstarttime = string.Empty;   //加班起
                    string tendtime = string.Empty;     //加班迄
                    string tworktime = string.Empty;    //加班時數
                    string tnote = string.Empty;        //加班說明    

                    foreach (var bodyitem in DocDataClass.body)
                    {
                        if (tStritem.Equals(bodyitem.item))
                        {
                            tdinner = bodyitem.dinner;
                            tlunch = bodyitem.lunch;
                            tdeptno = bodyitem.deptno;
                            tid = bodyitem.id;
                            tdate = bodyitem.date;
                            tstarttime = bodyitem.starttime;
                            tendtime = bodyitem.endtime;
                            tworktime = bodyitem.worktime;
                            tnote = bodyitem.note;
                            break;
                        }
                    }
                    #endregion

                    #region 給值
                    tRCfood.Value = //供餐
                        ("N".Equals(tlunch) && "N".Equals(tdinner)) ? "0" :         //不供餐
                        ("Y".Equals(tlunch) && "Y".Equals(tdinner)) ? "3" :         //供午、晚餐
                        ("Y".Equals(tlunch) && "N".Equals(tdinner)) ? "1" : "2";    //午餐或晚餐
                    tRCPdeptno.Value = tdeptno;         //加班部門
                    tRCPids.Value = "";                 //加班人員(多人/新增)
                    tRCPid.Value = tid;                 //加班人員(單人/修改)
                    tRCPdate.Value = tdate;             //加班日期
                    tRCPstarttime.Value = tstarttime;   //加班起
                    tRCPendtime.Value = tendtime;       //加班迄
                    tRCPhour.Value = tworktime;         //加班時數
                    tRCPnote.Value = tnote;             //加班說明
                    tRCPDocData.Value = tStrDocData;    //申請單資料(隱)
                    tRCPitem.Value = tStritem;          //判斷是新增或是修改(隱)                    
                    #endregion

                    #region 設定物件屬性
                    tRCPAdd.Visible = "false";          //新增按鈕
                    tRCPEdit.Visible = "true";          //修改按鈕
                    tRCPdeptno.Enable = "false";        //加班部門
                    tRCPids.Visible = "false";          //加班人員(多人/新增)
                    tRCPid.Visible = "true";            //加班人員(單人/修改)
                    #endregion
                }
                #endregion

                #region 設定物件屬性
                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRCPAdd.Title = "新增";
                    tRCPEdit.Title = "修改";
                    tRCfood.Title = "供餐";
                    tRCPdeptno.Title = "加班部門";
                    tRCPids.Title = "加班人員";
                    tRCPid.Title = "加班人員";
                    tRCPdate.Title = "加班日期";
                    tRCPstarttime.Title = "加班起";
                    tRCPendtime.Title = "加班迄";
                    tRCPhour.Title = "加班時數";
                    tRCPnote.Title = "加班說明";
                }
                else
                {
                    //簡體
                    tRCPAdd.Text = "新增";
                    tRCPEdit.Text = "修改";
                    tRCfood.Title = "供餐";
                    tRCPdeptno.Title = "加班部门";
                    tRCPids.Title = "加班人员";
                    tRCPid.Title = "加班人员";
                    tRCPdate.Title = "加班日期";
                    tRCPstarttime.Title = "加班起";
                    tRCPendtime.Title = "加班迄";
                    tRCPhour.Title = "加班时数";
                    tRCPnote.Title = "加班说明";
                }
                #endregion

                //處理回傳
                tP2MObject.AddTimeout(300);
                tP2MObject.AddRCPControls(tRCPAdd, tRCPEdit, tRCfood, tRCPdeptno, tRCPids, tRCPid, tRCPdate,
                                          tRCPstarttime, tRCPendtime, tRCPhour, tRCPnote, tRCPDocData, tRCPitem);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDetail01_BasicSetting Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 部門開窗
        public string GetDetail01_Deptno_OpenQuery(XDocument pM2Pxml)
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
                //因部門資料太多, 改成一定要下搜尋條件才顯示資料
                if (!string.IsNullOrEmpty(tSearch))
                {
                    #region 取得部門清單資料
                    //設定參數
                    //查詢部門
                    tparam.Clear();
                    //tparam.Add("deptId", tSearch);              //部門id
                    tparam.Add("deptName", tSearch);            //部門名稱

                    //叫用服務
                    string tResponse = Utility.CallAPI(pM2Pxml, "Department", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                    tResponse = "{\"data\" : " + tResponse + "}";                            //為符合class格式自己加上去的
                    Department DeparmentClass = Utility.JSONDeserialize<Department>(tResponse);    //轉class方便取用
                    #endregion

                    #region 給值
                    //取得部門Table
                    DataTable tDeparmentTable = DeparmentClass.GetTable(tSearch);

                    //轉換成符合格式的table
                    if (tDeparmentTable.Columns.Contains("organizationUnitName")) { tDeparmentTable.Columns["organizationUnitName"].ColumnName = "deptnoName"; }
                    tDeparmentTable.Columns.Add("deptno");
                    tDeparmentTable.Columns.Add("deptnoC");
                    for (int i = 0; i < tDeparmentTable.Rows.Count; i++)
                    {
                        tDeparmentTable.Rows[i]["deptno"] = tDeparmentTable.Rows[i]["id"] + "-" + tDeparmentTable.Rows[i]["deptnoName"];    //內存
                        tDeparmentTable.Rows[i]["deptnoC"] = tDeparmentTable.Rows[i]["id"] + "-" + tDeparmentTable.Rows[i]["deptnoName"];   //外顯
                    }

                    //設定公司別清單資料(含分頁)
                    Utility.SetRDControlPage(pM2Pxml, tDeparmentTable, ref tRDdeptno);
                    #endregion
                }
                else
                {
                    tP2MObject.Message = "請先輸入部門名稱條件";
                    tP2MObject.Result = "true";
                }
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDdeptno.AddCustomTag("DisplayColumn", "id§deptnoName§deptno");
                tRDdeptno.AddCustomTag("ColumnsName", "代號§名稱§部門");
                tRDdeptno.AddCustomTag("StructureStyle", "id:R1:C1:1§deptnoName:R1:C2:1");

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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDetail01_Deptno_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 部門異動
        public string GetDetail01_Deptno_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定控件
                RCPControl tRCPids = new RCPControl("ids", null, null, ""); //加班人員 //部門影響人員所以要清空
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPids);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDetail01_Deptno_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 人員開窗
        public string GetDetail01_Employee_OpenQuery(XDocument pM2Pxml)
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
                RDControl tRDids = new RDControl(new DataTable());  //人員清單
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrdeptno = DataTransferManager.GetControlsValue(pM2Pxml, "deptno");        //查詢的部門
                string tStrids = DataTransferManager.GetControlsValue(pM2Pxml, "ids");              //已勾選人員
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處裡畫面資料
                #region 取得資料
                //設定參數
                //取得員工明細資料
                tparam.Clear();
                tparam.Add("deptId", tStrdeptno.Split('-')[0]);       //查詢部門

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "DepartmentEmployee", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                                    //為符合class格式自己加上去的
                DepartmentEmployee EmployeeClass = Utility.JSONDeserialize<DepartmentEmployee>(tResponse);     //轉class方便取用
                #endregion

                #region 給值
                //取得部門人員Table
                DataTable tEmployeeTable = EmployeeClass.GetTable(tSearch);
                tEmployeeTable.Columns.Add("ids");
                for (int i = 0; i < tEmployeeTable.Rows.Count; i++)
                {
                    tEmployeeTable.Rows[i]["ids"] = tEmployeeTable.Rows[i]["id"] + "-" + tEmployeeTable.Rows[i]["userName"];    //KEYValue, 主要讓UIFLOW可以傳到前一頁
                }

                //設定部門人員清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, tEmployeeTable, ref tRDids);

                #endregion
                #endregion

                #region 設定物件屬性
                //單身清單屬性
                tRDids.AddCustomTag("DisplayColumn", "id§userName");
                tRDids.AddCustomTag("ColumnsName", "工號§姓名");
                tRDids.AddCustomTag("StructureStyle", "id:R1:C1:1§userName:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDids.AddCustomTag("ColumnsName", "工號§姓名");
                }
                else
                {
                    //簡體
                    tRDids.AddCustomTag("ColumnsName", "工号§姓名");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDids);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDetail01_Employee_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 新增/修改單身
        public string GetDetail01_Operate(XDocument pM2Pxml)
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
                RCPControl tRCfood = new RCPControl("food", "true", null, null);            //供餐
                RCPControl tRCPdeptno = new RCPControl("deptno", "true", null, null);       //加班部門
                RCPControl tRCPids = new RCPControl("ids", "true", null, null);             //加班人員(多人/新增)
                RCPControl tRCPdate = new RCPControl("date", "true", null, null);           //加班日期
                RCPControl tRCPstarttime = new RCPControl("starttime", "true", null, null); //加班起
                RCPControl tRCPendtime = new RCPControl("endtime", "true", null, null);     //加班迄
                RCPControl tRCPhour = new RCPControl("hour", "true", null, null);           //加班時數
                RCPControl tRCPnote = new RCPControl("note", "true", null, null);           //加班說明
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //申請單資料(隱)
                #endregion

                #region 取得畫面資料
                string tServiceName = DataTransferManager.GetDataValue(pM2Pxml, "ServiceName");         //service名稱

                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");        //登入者帳號
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");          //加班單暫存資料
                string tStrfood = DataTransferManager.GetControlsValue(pM2Pxml, "food");                //供餐
                string tStrdeptno = DataTransferManager.GetControlsValue(pM2Pxml, "deptno");            //加班部門
                string tStrdate = DataTransferManager.GetControlsValue(pM2Pxml, "date");                //加班日期
                string tStrids = DataTransferManager.GetControlsValue(pM2Pxml, "ids");                  //新增人員畫面回傳的勾選資料
                string tStrstarttime = DataTransferManager.GetControlsValue(pM2Pxml, "starttime");      //加班起
                string tStrendtime = DataTransferManager.GetControlsValue(pM2Pxml, "endtime");          //加班迄
                string tStrhour = DataTransferManager.GetControlsValue(pM2Pxml, "hour");                //加班時數
                string tStrnote = DataTransferManager.GetControlsValue(pM2Pxml, "note");                //加班說明
                string tStritem = DataTransferManager.GetControlsValue(pM2Pxml, "item");                //判斷是新增或是修改
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");            //語系

                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料
                #endregion

                #region 處裡畫面資料
                #region 驗證輸入資料是否有問題
                //必填
                if (string.IsNullOrEmpty(tStrdeptno))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "加班部門必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "加班部门必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(tStrdate))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "加班日期必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "加班日期必填\r\n";
                    }
                }

                if ("Add".Equals(tServiceName) && string.IsNullOrEmpty(tStrids))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "加班人員必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "加班人员必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(tStrhour))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "加班時數必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "加班时数必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(tStrnote))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "加班說明必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "加班说明必填\r\n";
                    }
                }
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                //資料驗證
                //日期
                DateTime thead_Date = Convert.ToDateTime(DocDataClass.head.date);                   //申請日期
                DateTime tbody_Date = Convert.ToDateTime(tStrdate.Insert(4, "/").Insert(7, "/"));     //加班日期
                TimeSpan ts = tbody_Date - thead_Date;                                              //差異天數
                if (ts.TotalDays < -5)
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "「加班日期」需於「申請日」前五天之後\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "「加班日期」需于「申请日」前五天之后\r\n";
                    }
                }

                //加班起迄
                if (!(int.Parse(tStrendtime) >= int.Parse(tStrstarttime)))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "「加班起」需於「加班迄」之前\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "「加班起」需于「加班迄」之前\r\n";
                    }
                }

                //加班時數
                if (!(24 >= double.Parse(tStrhour) && double.Parse(tStrhour) > 0))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "「加班時數」需「小於等於24且大於等於0」\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "「加班时数」需「小於等於24且大於等於0」\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                #endregion

                switch (tServiceName)
                {
                    case "Add":             //新增單身
                        #region 新增單身資料
                        //依人員逐一新增
                        foreach (var employee in tStrids.Split('§'))
                        {
                            string tKeyField = employee + tStrdate; //key值

                            //檢查是否存在
                            foreach (var bodyitem in DocDataClass.body)
                            {
                                if (tKeyField.Equals(bodyitem.item))
                                {
                                    tErrorMsg += employee + "\r\n";
                                }
                            }

                            //檢查不存在則新增
                            if (string.IsNullOrEmpty(tErrorMsg))
                            {
                                BodyData tbody = new BodyData();
                                tbody.item = tKeyField;   //key
                                tbody.lunch = "1".Equals(tStrfood) || "3".Equals(tStrfood) ? "Y" : "N";     //午餐
                                tbody.dinner = "2".Equals(tStrfood) || "3".Equals(tStrfood) ? "Y" : "N";    //晚餐
                                tbody.deptno = tStrdeptno;          //加班部門
                                tbody.date = tStrdate;              //加班日期
                                tbody.id = employee;                //加班人員
                                tbody.starttime = tStrstarttime;    //加班起
                                tbody.endtime = tStrendtime;        //加班迄
                                tbody.worktime = tStrhour;          //加班時數
                                tbody.note = tStrnote;              //加班說明

                                DocDataClass.body.Add(tbody);
                            }
                        }

                        //有重的資料就回傳錯誤
                        if (!string.IsNullOrEmpty(tErrorMsg))
                        {
                            tErrorMsg += tStrdate.Insert(4, "/").Insert(7, "/") + "加班資料已存在";
                            return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                        }
                        #endregion
                        break;
                    case "Edit":            //修改明細
                        #region 修改單身資料
                        foreach (var bodyitem in DocDataClass.body)
                        {
                            if (tStritem.Equals(bodyitem.item))
                            {
                                bodyitem.lunch = "1".Equals(tStrfood) || "3".Equals(tStrfood) ? "Y" : "N";     //午餐
                                bodyitem.dinner = "2".Equals(tStrfood) || "3".Equals(tStrfood) ? "Y" : "N";    //晚餐
                                bodyitem.date = tStrdate;              //加班日期
                                bodyitem.starttime = tStrstarttime;    //加班起
                                bodyitem.endtime = tStrendtime;        //加班迄
                                bodyitem.worktime = tStrhour;          //加班時數
                                bodyitem.note = tStrnote;              //加班說明
                                break;
                            }
                        }
                        #endregion
                        break;
                }

                #region 更新畫面欄位資料
                tRCPDocData.Value = Utility.JSONSerialize(DocDataClass);     //加班單暫存
                #endregion

                #endregion

                //處理回傳
                tP2MObject.AddStatus("callwork&HANBELL01_01-HANBELL01+加班明細返回&None"); //返回申請單
                tP2MObject.AddRCPControls(tRCfood, tRCPdeptno, tRCPids, tRCPdate, tRCPstarttime,
                                          tRCPendtime, tRCPhour, tRCPnote, tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDetail01_Operate Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 立單
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
                RCPControl tRCPformType = new RCPControl("formType", "true", null, null);   //加班類別
                RCPControl tRCPcompany = new RCPControl("company", "true", null, null);     //公司別
                RCPControl tRCPdate = new RCPControl("date", "false", null, null);          //申請日
                RCPControl tRCPuserid = new RCPControl("userid", "false", null, null);      //申請人
                RCPControl tRCPdeptno = new RCPControl("deptno", "true", null, null);       //申請部門
                RCPControl tRCPBodyInfo = new RCPControl("BodyInfo", "true", null, null);   //申請單單身
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //加班單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrcompany = DataTransferManager.GetControlsValue(pM2Pxml, "company");      //公司別
                string tStrdeptno = DataTransferManager.GetControlsValue(pM2Pxml, "deptno");        //申請部門
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //加班單據暫存
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系

                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);                //加班單暫存資料
                #endregion

                #region 檢查錯誤
                //檢查公司別必填
                if (string.IsNullOrEmpty(tStrcompany))
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

                //檢查申請部門必填
                if (string.IsNullOrEmpty(tStrdeptno))
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

                //檢查單身是否存在
                if (string.IsNullOrEmpty(tStrDocData) || DocDataClass.body.Count <= 0)
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "請確定單身資料是否存在";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "请确定单身资料是否存在";
                    }
                }

                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                #endregion

                #region 整理Request
                //單頭
                DocDataClass.head.company = DocDataClass.head.company.Split('-')[0];    //公司別只取id
                DocDataClass.head.id = DocDataClass.head.id.Split('-')[0];              //申請人只取id
                DocDataClass.head.deptno = DocDataClass.head.deptno.Split('-')[0];      //申請部門只取id

                //單身
                foreach (var bodyitem in DocDataClass.body)
                {
                    bodyitem.deptno = bodyitem.deptno.Split('-')[0];                //加班部門只取id
                    bodyitem.id = bodyitem.id.Split('-')[0];                        //加班人員只取id
                    bodyitem.date = bodyitem.date.Insert(4, "/").Insert(7, "/");    //日期加/
                    bodyitem.starttime = bodyitem.starttime.Insert(2, ":");         //時間加:
                    bodyitem.endtime = bodyitem.endtime.Insert(2, ":");             //時間加:
                }
                #endregion

                #region 取得Response
                //叫用API
                string uri = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), "efgp/hkgl034/create", LoadConfig.GetWebConfig("APIKey"));
                string tBodyContext = Utility.JSONSerialize(DocDataClass);

                string tResponse = Utility.InvokeProcess(uri, tBodyContext, out tErrorMsg);
                #endregion

                #region 處理畫面資料
                if (!string.IsNullOrEmpty(tErrorMsg))
                {
                    Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                }
                else
                {
                    //轉成class 方便取用
                    ResponseState tCreateDocResultClass = Utility.JSONDeserialize<ResponseState>(tResponse);

                    //判斷回傳
                    if ("200".Equals(tCreateDocResultClass.code))
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

                        tP2MObject.Message = tMsg + tCreateDocResultClass.msg;
                        tP2MObject.Result = "false";

                        #region 取得預設資料
                        //設定參數
                        //取得員工明細資料
                        tparam.Clear();
                        tparam.Add("UserID", tStrUserID);       //查詢對象

                        //叫用服務
                        tResponse = Utility.CallAPI(pM2Pxml, "Users", tparam, out tErrorMsg);
                        if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                        //回傳的JSON轉Class
                        Users EmployDetail = Utility.JSONDeserialize<Users>(tResponse);  //員工明細

                        //取值
                        string tCompany = string.Format("{0}-{1}", EmployDetail.company, ""/*EmployDetail.companyName*/);   //公司別代號-公司別名稱
                        string tUser = string.Format("{0}-{1}", EmployDetail.id, EmployDetail.userName);                    //申請人代號-申請人名稱
                        string tDetp = string.Format("{0}-{1}", EmployDetail.deptno, EmployDetail.deptname);                //部門代號-部門名稱
                        string tDate = DateTime.Now.ToString("yyyy/MM/dd");                                                 //申請時間
                        string tStrformType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");                    //加班類別code
                        string fStrormTypeDesc = "1".Equals(tStrformType) ? "平日加班" : "2".Equals(tStrformType) ? "雙休加班" : "節日加班";    //加班類別name

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
                        //設定加班單資料
                        DocDataClass = new Entity();
                        DocDataClass.head = new HeadData(tCompany, tDate, tUser, tDetp, tStrformType, fStrormTypeDesc);
                        string DocDataStr = Utility.JSONSerialize(DocDataClass);//Class轉成JSON

                        //給定畫面欄位值
                        tRCPcompany.Value = tCompany;       //公司別
                        tRCPdate.Value = tDate;             //申請日
                        tRCPuserid.Value = tUser;           //申請人
                        tRCPdeptno.Value = tDetp;           //申請部門
                        tRCPDocData.Value = DocDataStr;     //加班單暫存資料
                        tRCPBodyInfo.Table = new DataTable();//加班單單身
                        #endregion

                        //處理回傳
                        tP2MObject.AddRCPControls(tRCPformType, tRCPcompany, tRCPdate, tRCPuserid, tRCPdeptno, tRCPBodyInfo, tRCPDocData);
                    }
                    else
                    {
                        tP2MObject.Message = "請假單建立失敗，說明 :\r\n" + tCreateDocResultClass.msg;
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

    //加班單Class
    class Entity
    {
        public HeadData head { get; set; }
        public List<BodyData> body { get; set; }

        public Entity()
        {
            this.head = new HeadData();
            this.body = new List<BodyData>();
        }

        public Entity(HeadData thead, List<BodyData> body)
        {
            this.head = thead;
            this.body = body;
        }

        //回傳單身table
        public DataTable GetBodyDataTable(string pSearch)
        {
            Dictionary<string, string[]> BodyItems = new Dictionary<string, string[]>();  //取得body有的<欄位/值, 陣列值>

            DataTable BodyTable = new DataTable();
            for (int i = 0; i < this.body.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                BodyItems = this.body[i].GetItem(); //取得body資料

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in BodyItems["Columns"]) BodyTable.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in BodyItems["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (tInsertData) BodyTable.Rows.Add(BodyItems["Values"]);
            }

            return BodyTable;
        }
    }
    //單頭欄位
    class HeadData
    {
        public string company { get; set; }     //公司別
        public string date { get; set; }        //申請日
        public string id { get; set; }          //申請人
        public string deptno { get; set; }      //申請部門
        public string formType { get; set; }    //加班類別code
        public string formTypeDesc { get; set; }//加班類別name

        public HeadData()
        {
            this.company = "";
            this.date = "";
            this.id = "";
            this.deptno = "";
            this.formType = "0";
            this.formTypeDesc = "平日加班";
        }

        public HeadData(string company, string date, string id, string deptno, string formType, string formTypeDesc)
        {
            this.company = company;
            this.date = date;
            this.id = id;
            this.deptno = deptno;
            this.formType = formType;
            this.formTypeDesc = formTypeDesc;
        }
    }
    //單身欄位
    class BodyData
    {
        public string item { get; set; }        //Key值
        public string lunch { get; set; }       //午餐
        public string dinner { get; set; }      //晚餐
        public string deptno { get; set; }      //加班部門
        public string id { get; set; }          //加班人員
        public string date { get; set; }        //加班日期
        public string starttime { get; set; }   //加班起
        public string endtime { get; set; }     //加班迄
        public string worktime { get; set; }    //加班時數
        public string note { get; set; }        //備註

        public BodyData() { }

        public BodyData(string item, string lunch, string dinner, string deptno, string id, string date,
                        string starttime, string endtime, string worktime, string note)
        {
            this.item = item;
            this.lunch = lunch;
            this.dinner = dinner;
            this.deptno = deptno;
            this.id = id;
            this.date = date;
            this.starttime = starttime;
            this.endtime = endtime;
            this.worktime = worktime;
            this.note = note;
        }

        //取得單身欄位
        public Dictionary<string, string[]> GetItem()
        {
            Dictionary<string, string[]> Items = new Dictionary<string, string[]>();
            Items.Add("Columns", new string[] { "item", "lunch", "dinner", "deptno", "id", "date", "starttime", "endtime", "worktime", "note" });
            Items.Add("Values", new string[] { this.item, this.lunch, this.dinner, this.deptno, this.id, this.date, this.starttime, this.endtime, this.worktime, this.note });

            return Items;
        }
    }

}