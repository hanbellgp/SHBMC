using com.digiwin.Mobile.MCloud.TransferLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace cn.hanbell.mcloud.HZGL004
{
    public class HZGL004
    {

        #region 頁面初使化
        public string GetBasicSetting(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, "出差单"); //參數:要顯示的訊息、結果、Title//建構p2m

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
                RCPControl tRCPapplyUser = new RCPControl("applyUser", "false", null, null);      //申請人
                RCPControl tRCPapplyDept = new RCPControl("applyDept", "true", null, null);
                RCPControl tRCPformType = new RCPControl("formType", "true", null, null);
                RCPControl tRCPotherType = new RCPControl("otherType", "false", null, null);
                RCPControl tRCPvehicle = new RCPControl("vehicle", "true", null, null);
                RCPControl tRCPotherVehicle = new RCPControl("otherVehicle", "false", null, null);
                RCPControl tRCPdestination = new RCPControl("destination", "true", null, null);
                RCPControl tRCPstartDate = new RCPControl("startDate", "true", null, null);
                RCPControl tRCPendDate = new RCPControl("endDate", "true", null, null);
                RCPControl tRCPdays = new RCPControl("days", "true", null, null);
                RCPControl tRCPbizDetail = new RCPControl("bizDetail", "true", null, null);
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系

                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //暫存資料
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
                    string tDept = string.Format("{0}-{1}", EmployDetail.deptno, EmployDetail.deptname);    //部門代號-部門名稱
                    string tDate = DateTime.Now.ToString("yyyy/MM/dd"); //申請時間
                    string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");
                    string vehicle = DataTransferManager.GetControlsValue(pM2Pxml, "vehicle");

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

                    #region 給值
                    //設定请假單資料
                    Entity entityClass = new Entity(tCompany, tDate, tUser, tDept, "1", "业务推展", "1", "公务车");
                    entityClass.destination = "1";
                    entityClass.destinationDesc = "中国大陆";
                    entityJSONStr = Utility.JSONSerialize(entityClass);//Class轉成JSON

                    //給定畫面欄位值
                    tRCPcompany.Value = tCompany;       //公司別
                    tRCPapplyDate.Value = tDate;        //申請日
                    tRCPapplyUser.Value = tUser;           //申請人
                    tRCPapplyDept.Value = tDept;           //申請部門
                    tRCPformType.Value = "1-业务推展";
                    tRCPvehicle.Value = "1-公务车";
                    tRCPdestination.Value = "1-中国大陆";
                    tRCPdays.Value = "0";
                    tRCPdocData.Value = entityJSONStr;     //请假單暫存資料

                    tRCPotherType.Enable = "false";
                    tRCPotherType.Value = "";
                    tRCPotherVehicle.Enable = "false";
                    tRCPotherVehicle.Value = "";
                    #endregion
                }
                else//有單據資料
                {
                    #region 显示出差资料
                    //先將暫存資料轉成Class方便取用
                    Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//出差单暫存資料

                    //設定畫面資料
                    tRCPcompany.Value = entityClass.company;           //公司別
                    tRCPapplyDate.Value = entityClass.applyDate;       //申請日
                    tRCPapplyUser.Value = entityClass.applyUser;       //申請人
                    tRCPapplyDept.Value = entityClass.applyDept;     //申請部門
                    tRCPformType.Value = entityClass.formType + "-" + entityClass.formTypeDesc;
                    tRCPvehicle.Value = entityClass.vehicle + "-" + entityClass.vehicleDesc;
                    tRCPdestination.Value = entityClass.destination + "-" + entityClass.destinationDesc;
                    tRCPstartDate.Value = entityClass.startDate;
                    tRCPendDate.Value = entityClass.endDate;
                    tRCPdays.Value = entityClass.days.ToString();
                    tRCPdocData.Value = entityJSONStr;                    //暫存資料
                    //設定單身資料(含分頁)
                    DataTable bizDetailTable = entityClass.GetDetailTable("");
                    Utility.SetRCPControlPage(pM2Pxml, bizDetailTable, ref tRCPbizDetail);

                    if ("7".Equals(entityClass.formType))
                    {
                        tRCPotherType.Enable = "true";
                    }
                    if ("6".Equals(entityClass.vehicle))
                    {
                        tRCPotherVehicle.Enable = "true";
                    }
                    #endregion
                }

                tRCPbizDetail.AddCustomTag("ColumnsName", "出差人员§出差日期§起§至§出差对象§出差地址");
                #endregion
                //處理回傳
                tP2MObject.AddRCPControls(tRCPcompany, tRCPapplyDate, tRCPapplyUser, tRCPapplyDept, tRCPformType, tRCPotherType, tRCPvehicle, tRCPotherVehicle, tRCPdestination, tRCPstartDate, tRCPendDate, tRCPdays, tRCPbizDetail, tRCPdocData);
            }
            catch (Exception ex)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBasicSetting Error : " + ex.Message.ToString());
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
                tCompanyTable.Columns.Add("companyC");//    Control ID + C = 開窗控件要顯示的值
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
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //出差单據隱藏欄位
                #endregion

                #region 取得畫面資料
                string companyC = DataTransferManager.GetControlsValue(pM2Pxml, "companyC");
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//出差单暫存資料

                entityClass.company = companyC;

                //出差单Class轉成JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //給值
                tRCPDocData.Value = entityJSONStr;
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

        #region 部门開窗
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
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //出差单據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tApplyDeptno = DataTransferManager.GetControlsValue(pM2Pxml, "applyDeptC");    //申請部門(C是取外顯值)
                string tDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //出差单據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tDocData);//出差单暫存資料

                //修改部門資料
                DocDataClass.applyDept = tApplyDeptno;

                //出差单Class轉成JSON
                string tDocdataJSON = Utility.JSONSerialize(DocDataClass);

                //給值
                tRCPDocData.Value = tDocdataJSON;
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

        #region 出差性质開窗
        public string GetFormType_OpenQuery(XDocument pM2Pxml)
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
                RDControl tRDbizKind = new RDControl(new DataTable());  //出差性质清單
                #endregion

                #region 取得畫面資料
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 取得清單資料
                //設定參數
                tparam.Clear();

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "BizKind", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                BizKind bizKind = Utility.JSONDeserialize<BizKind>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得Table
                DataTable table = bizKind.GetDataTable(tSearch, new string[] { "formType", "formTypeDesc" });
                table.Columns.Add("formTypeC");//    Control applyUser + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["formTypeC"] = table.Rows[i]["formType"] + "-" + table.Rows[i]["formTypeDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, table, ref tRDbizKind);
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDbizKind.AddCustomTag("DisplayColumn", "formType§formTypeDesc§formTypeC");
                tRDbizKind.AddCustomTag("StructureStyle", "formType:R1:C1:1§formTypeDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDbizKind.AddCustomTag("ColumnsName", "代號§名稱§显示");
                }
                else
                {
                    //簡體
                    tRDbizKind.AddCustomTag("ColumnsName", "代号§名称§显示");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDbizKind);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetLeaveKind_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

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
                RCPControl tRCPotherType = new RCPControl("otherType", "false", null, null);
                #endregion

                #region 取得畫面資料
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");                      //请假單據暫存
                string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");                    //请假類別code
                string formTypeDesc;
                switch (formType)
                {
                    case "1":
                        formTypeDesc = "业务推展";
                        break;
                    case "2":
                        formTypeDesc = "售后服务";
                        break;
                    case "3":
                        formTypeDesc = "厂商洽谈";
                        break;
                    case "4":
                        formTypeDesc = "培训";
                        break;
                    case "7":
                        formTypeDesc = "其他";
                        break;
                    default:
                        formTypeDesc = "";
                        break;
                }
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
                if ("7".Equals(formType))
                {
                    tRCPotherType.Enable = "true";
                }
                else
                {
                    tRCPotherType.Enable = "false";
                    tRCPotherType.Value = "";
                }
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPotherType, tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetFormType_OnBuler Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 交通工具開窗
        public string GetBizVehicle_OpenQuery(XDocument pM2Pxml)
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
                RDControl tRDbizVehicle = new RDControl(new DataTable());  //出差性质清單
                #endregion

                #region 取得畫面資料
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 取得清單資料
                //設定參數
                tparam.Clear();

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "BizVehicle", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";   //為符合class格式自己加上去的
                VehicleKind vehicleKind = Utility.JSONDeserialize<VehicleKind>(tResponse);  //轉class方便取用
                #endregion

                #region 給值
                //取得Table
                DataTable table = vehicleKind.GetDataTable(tSearch, new string[] { "vehicle", "vehicleDesc" });
                table.Columns.Add("vehicleC");//    Control id + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["vehicleC"] = table.Rows[i]["vehicle"] + "-" + table.Rows[i]["vehicleDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, table, ref tRDbizVehicle);
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDbizVehicle.AddCustomTag("DisplayColumn", "vehicle§vehicleDesc§vehicleC");
                tRDbizVehicle.AddCustomTag("StructureStyle", "vehicle:R1:C1:1§vehicleDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDbizVehicle.AddCustomTag("ColumnsName", "代號§名稱§显示");
                }
                else
                {
                    //簡體
                    tRDbizVehicle.AddCustomTag("ColumnsName", "代号§名称§显示");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDbizVehicle);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBizVehicle_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetBizVehicle_OnBlur(XDocument pM2Pxml)
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
                RCPControl tRCPotherVehicle = new RCPControl("otherVehicle", "false", null, null);
                #endregion

                #region 取得畫面資料
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");                      //请假單據暫存
                string vehicle = DataTransferManager.GetControlsValue(pM2Pxml, "vehicle");                    //请假類別code
                string vehicleDesc;
                switch (vehicle)
                {
                    case "1":
                        vehicleDesc = "公务车";
                        break;
                    case "2":
                        vehicleDesc = "长途巴士";
                        break;
                    case "3":
                        vehicleDesc = "高铁";
                        break;
                    case "4":
                        vehicleDesc = "火车";
                        break;
                    case "5":
                        vehicleDesc = "飞机";
                        break;
                    case "6":
                        vehicleDesc = "其他";
                        break;
                    default:
                        vehicleDesc = "";
                        break;
                }
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//请假單暫存資料

                //修改公司別資料
                entityClass.vehicle = vehicle;
                entityClass.vehicleDesc = vehicleDesc;

                //请假單Class轉成JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //給值
                tRCPDocData.Value = entityJSONStr;
                if ("6".Equals(vehicle))
                {
                    tRCPotherVehicle.Enable = "true";
                }
                else
                {
                    tRCPotherVehicle.Enable = "false";
                    tRCPotherVehicle.Value = "";
                }

                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPotherVehicle, tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBizVehicle_OnBuler Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 目的地開窗
        public string GetBizDestination_OpenQuery(XDocument pM2Pxml)
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
                RDControl tRDbizDestiantion = new RDControl(new DataTable());  //出差性质清單
                #endregion

                #region 取得畫面資料
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 取得清單資料
                //設定參數
                tparam.Clear();

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "BizDestination", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";   //為符合class格式自己加上去的
                DestinationKind destinationKind = Utility.JSONDeserialize<DestinationKind>(tResponse);  //轉class方便取用
                #endregion

                #region 給值
                //取得Table
                DataTable table = destinationKind.GetDataTable(tSearch, new string[] { "destination", "destinationDesc" });
                table.Columns.Add("destinationC");//    Control id + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["destinationC"] = table.Rows[i]["destination"] + "-" + table.Rows[i]["destinationDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, table, ref tRDbizDestiantion);
                #endregion

                #region 設定物件屬性
                //公司別清單屬性
                tRDbizDestiantion.AddCustomTag("DisplayColumn", "destination§destinationDesc§destinationC");
                tRDbizDestiantion.AddCustomTag("StructureStyle", "destination:R1:C1:1§destinationDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDbizDestiantion.AddCustomTag("ColumnsName", "代號§名稱§显示");
                }
                else
                {
                    //簡體
                    tRDbizDestiantion.AddCustomTag("ColumnsName", "代号§名称§显示");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDbizDestiantion);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBizDestination_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetBizDestination_OnBlur(XDocument pM2Pxml)
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
                string destination = DataTransferManager.GetControlsValue(pM2Pxml, "destination");                    //请假類別code
                string destinationDesc;
                switch (destination)
                {
                    case "1":
                        destinationDesc = "中国大陆";
                        break;
                    case "2":
                        destinationDesc = "港澳台";
                        break;
                    case "3":
                        destinationDesc = "国外";
                        break;
                    default:
                        destinationDesc = "";
                        break;
                }
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//请假單暫存資料

                //修改公司別資料
                entityClass.destination = destination;
                entityClass.destinationDesc = destinationDesc;

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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBizDestination_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 出差时间更新
        public string GetStartDate_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //出差单據隱藏欄位
                #endregion

                #region 取得畫面資料
                string startDate = DataTransferManager.GetControlsValue(pM2Pxml, "startDate");    //申請部門(C是取外顯值)
                string tDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //出差单據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tDocData);//出差单暫存資料

                //修改部門資料
                DocDataClass.startDate = startDate.Insert(4, "/").Insert(7, "/");

                //出差单Class轉成JSON
                string JSONStr = Utility.JSONSerialize(DocDataClass);

                //給值
                tRCPDocData.Value = JSONStr;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetStartDate_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetEndDate_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPendDate = new RCPControl("endDate", "true", null, null);
                RCPControl tRCPdays = new RCPControl("days", "true", null, null);
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);
                #endregion

                #region 取得畫面資料
                string startDate = DataTransferManager.GetControlsValue(pM2Pxml, "startDate");
                string endDate = DataTransferManager.GetControlsValue(pM2Pxml, "endDate");    //申請部門(C是取外顯值)
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //出差单據暫存
                #endregion

                #region 處理畫面資料
                if (string.IsNullOrEmpty(startDate))
                {
                    tErrorMsg += "先填写开始日期\r\n";
                }
                else if (startDate.CompareTo(endDate) > 0)
                {
                    tErrorMsg += "结束日期应大于开始日期\r\n";
                }
                if (!string.IsNullOrEmpty(tErrorMsg))
                {
                    tRCPendDate.Value = "";
                    tP2MObject.AddRCPControls(tRCPendDate);
                    return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                }
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//出差单暫存資料

                //修改出差日期
                entityClass.endDate = endDate.Insert(4, "/").Insert(7, "/");
                DateTime day1 = Convert.ToDateTime(entityClass.startDate);
                DateTime day2 = Convert.ToDateTime(entityClass.endDate);
                TimeSpan ts = day2 - day1;                                                  //差異天數
                entityClass.days = ts.TotalDays + 1;

                //出差单Class轉成JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //給值
                tRCPdays.Value = entityClass.days.ToString();
                tRCPDocData.Value = entityJSONStr;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPdays, tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetApplyDept_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetDays_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m

            try
            {
                #region 設定參數                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
                string tfunctionName = string.Empty;    //叫用API的方法名稱(自定義)
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //出差单據隱藏欄位
                #endregion

                #region 取得畫面資料
                string days = DataTransferManager.GetControlsValue(pM2Pxml, "days");    //申請部門(C是取外顯值)
                string tDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //出差单據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tDocData);//出差单暫存資料

                //修改出差日期
                DocDataClass.days = double.Parse(days);

                //出差单Class轉成JSON
                string JSONStr = Utility.JSONSerialize(DocDataClass);

                //給值
                tRCPDocData.Value = JSONStr;
                #endregion

                //處理回傳
                tP2MObject.AddRCPControls(tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetDays_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 出差明细页面初始
        public string GetBizDetail_BasicSetting(XDocument pM2Pxml)
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
                RCPControl tRCPbtnAdd = new RCPControl("btnAdd", "true", "true", null);          //新增按鈕
                RCPControl tRCPbtnEdit = new RCPControl("btnEdit", "true", "false", null);       //修改            
                RCPControl tRCPbizEmployee = new RCPControl("bizEmployee", "false", null, null);      //(多人/新增)
                RCPControl tRCPbizDate = new RCPControl("bizDate", "true", null, null);           //
                RCPControl tRCPbizTime1 = new RCPControl("bizTime1", "true", null, null); //
                RCPControl tRCPbizTime2 = new RCPControl("bizTime2", "true", null, null);//
                RCPControl tRCPbizObject = new RCPControl("bizObject", "true", null, null);//
                RCPControl tRCPbizAddress = new RCPControl("bizAddress", "true", null, null);//
                RCPControl tRCPbizContent = new RCPControl("bizContent", "true", null, null);//
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //申請單資料(隱)
                RCPControl tRCPitem = new RCPControl("item", null, null, null);             //判斷是新增或是修改(隱)
                #endregion

                #region 取得畫面資料
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");//語系
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");//登入者帳號

                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");//暫存資料
                string tStritem = DataTransferManager.GetControlsValue(pM2Pxml, "item");//判斷是新增或是修改
                #endregion

                #region 處裡畫面資料
                //判斷新增或是修改
                if (string.IsNullOrEmpty(tStritem))//新增
                {
                    #region 取得預設資料
                    //設定參數
                    //取得員工明細資料
                    //tparam.Clear();
                    //tparam.Add("UserID", tStrUserID);       //查詢對象

                    ////叫用服務
                    //string tResponse = Utility.CallAPI(pM2Pxml, "Users", tparam, out tErrorMsg);
                    //if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    ////回傳的JSON轉Class
                    //Users EmployDetail = Utility.JSONDeserialize<Users>(tResponse);  //員工明細

                    //取值

                    string tDate = DateTime.Now.ToString("yyyy/MM/dd"); //加班日期
                    string tTime = DateTime.Now.ToString("HH:mm");//加班起訖
                    #endregion

                    #region 給預設值
                    //tRCPbizEmployee.Value = EmployDetail.id + "-" + EmployDetail.userName;
                    tRCPbizDate.Value = tDate;
                    tRCPbizTime1.Value = tTime;
                    tRCPbizTime2.Value = tTime;
                    tRCPDocData.Value = tStrDocData;
                    tRCPitem.Value = tStritem;//判斷是新增或是修改(隱)                    
                    #endregion

                    #region 設定物件屬性
                    tRCPbtnAdd.Visible = "true";           //新增按鈕
                    tRCPbtnEdit.Visible = "false";         //修改按鈕
                    tRCPbizEmployee.Visible = "true";           //加班人員(多人/新增)
                    #endregion                    
                }
                else //修改
                {
                    #region 取得單身資料
                    //先將暫存資料轉成Class方便取用
                    Entity entityClass = Utility.JSONDeserialize<Entity>(tStrDocData);//出差单暫存資料

                    //取值
                    string bizEmployee = string.Empty;  //出差人员
                    string bizDate = string.Empty;      //出差日期
                    string bizTime1 = string.Empty;     //开始时间
                    string bizTime2 = string.Empty;     //结束时间
                    string bizObject = string.Empty;    //出差对象
                    string bizAddress = string.Empty;   //出差地址    
                    string bizContent = string.Empty;
                    foreach (var d in entityClass.detailList)
                    {
                        if (tStritem.Equals(d.item))
                        {
                            bizEmployee = d.bizEmployee;
                            bizDate = d.bizDate;
                            bizTime1 = d.bizTime1;
                            bizTime2 = d.bizTime2;
                            bizObject = d.bizObject;
                            bizAddress = d.bizAddress;
                            bizContent = d.bizContent;
                            break;
                        }
                    }
                    #endregion

                    #region 給值
                    tRCPbizDate.Value = bizDate;
                    tRCPbizTime1.Value = bizTime1;
                    tRCPbizTime2.Value = bizTime2;
                    tRCPbizObject.Value = bizObject;
                    tRCPbizAddress.Value = bizAddress;
                    tRCPbizContent.Value = bizContent;
                    tRCPDocData.Value = tStrDocData;
                    tRCPitem.Value = tStritem;
                    #endregion

                    #region 設定物件屬性
                    tRCPbtnAdd.Visible = "false";          //新增按鈕
                    tRCPbtnEdit.Visible = "true";          //修改按鈕
                    #endregion
                }
                #endregion

                //處理回傳
                tP2MObject.AddTimeout(300);
                tP2MObject.AddRCPControls(tRCPbtnAdd, tRCPbtnEdit, tRCPbizEmployee, tRCPbizDate,
                                          tRCPbizTime1, tRCPbizTime2, tRCPbizObject, tRCPbizAddress, tRCPbizContent, tRCPDocData, tRCPitem);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBizDetail_BasicSetting Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 新增/修改明细
        public string GetBizDetail_Operate(XDocument pM2Pxml)
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
                RCPControl tRCPbizEmployee = new RCPControl("bizEmployee", "false", null, null);
                RCPControl tRCPbizDate = new RCPControl("bizDate", "true", null, null);
                RCPControl tRCPbizTime1 = new RCPControl("bizTime1", "true", null, null);
                RCPControl tRCPbizTime2 = new RCPControl("bizTime2", "true", null, null);
                RCPControl tRCPbizObject = new RCPControl("bizObject", "true", null, null);
                RCPControl tRCPbizAddress = new RCPControl("bizAddress", "true", null, null);
                RCPControl tRCPbizContent = new RCPControl("bizContent", "true", null, null);
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");        //登入者帳號
                string tStrService = DataTransferManager.GetDataValue(pM2Pxml, "ServiceName");         //service名稱
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");

                string bizEmployee = DataTransferManager.GetControlsValue(pM2Pxml, "bizEmployee");
                string bizDate = DataTransferManager.GetControlsValue(pM2Pxml, "bizDate");
                string bizTime1 = DataTransferManager.GetControlsValue(pM2Pxml, "bizTime1");
                string bizTime2 = DataTransferManager.GetControlsValue(pM2Pxml, "bizTime2");
                string bizObject = DataTransferManager.GetControlsValue(pM2Pxml, "bizObject");
                string bizAddress = DataTransferManager.GetControlsValue(pM2Pxml, "bizAddress");
                string bizContent = DataTransferManager.GetControlsValue(pM2Pxml, "bizContent");
                string item = DataTransferManager.GetControlsValue(pM2Pxml, "item");

                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//出差单暫存資料
                #endregion

                #region 驗證輸入資料是否有問題
                //必填
                if (string.IsNullOrEmpty(bizEmployee))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "出差人员必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "出差人员必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(bizDate))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "出差日期必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "出差日期必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(bizTime1))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "开始时间必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "开始时间必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(bizTime2))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "结束时间必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "结束时间必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(bizObject))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "出差对象必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "出差对象必填\r\n";
                    }
                }

                if (string.IsNullOrEmpty(bizAddress))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "出差地址必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "出差地址必填\r\n";
                    }
                }
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                //資料驗證
                //出差日期
                DateTime day1 = Convert.ToDateTime(DocDataClass.startDate);
                DateTime day2 = Convert.ToDateTime(DocDataClass.endDate); //申請日期
                DateTime tbody_Date = Convert.ToDateTime(bizDate.Insert(4, "/").Insert(7, "/"));     //加班日期
                TimeSpan ts1 = tbody_Date - day1;
                TimeSpan ts2 = tbody_Date - day2;                                                  //差異天數
                if (ts1.TotalDays < 0 || ts2.TotalDays > 0)
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "「明细日期」不在「出差日期」范围内\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "「明细日期」不在「出差日期」范围内\r\n";
                    }
                }

                //出差起迄
                if (!(int.Parse(bizTime2) >= int.Parse(bizTime1)))
                {
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "「开始時間」需於「结束時間」之前\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "「开始日期」需于「结束日期」之前\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                #endregion

                switch (tStrService)
                {
                    case "addBizDetail":             //新增單身
                        #region 新增單身資料
                        string tKeyField = bizEmployee + bizDate + bizTime1; //key值
                        //檢查是否存在
                        foreach (var bodyitem in DocDataClass.detailList)
                        {
                            if (tKeyField.Equals(bodyitem.item))
                            {
                                tErrorMsg += bizDate + "_" + bizTime1 + "\r\n";
                            }
                        }

                        //檢查不存在則新增
                        if (string.IsNullOrEmpty(tErrorMsg))
                        {
                            BizDetail d = new BizDetail();
                            d.item = tKeyField;//key
                            d.bizEmployee = bizEmployee;
                            d.bizEmployeeName = bizEmployee.Split('-')[1];
                            d.bizDate = bizDate;
                            d.bizTime1 = bizTime1;
                            d.bizTime2 = bizTime2;
                            d.bizObject = bizObject;
                            d.bizAddress = bizAddress;
                            d.bizContent = bizContent;

                            DocDataClass.detailList.Add(d);
                        }

                        //有重的資料就回傳錯誤
                        if (!string.IsNullOrEmpty(tErrorMsg))
                        {
                            tErrorMsg += tKeyField + "明细记录已存在";
                            return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                        }
                        #endregion
                        break;
                    case "editBizDetail":            //修改明細
                        #region 修改單身資料
                        foreach (var d in DocDataClass.detailList)
                        {
                            if (item.Equals(d.item))
                            {
                                d.bizDate = bizDate;
                                d.bizTime1 = bizTime1;
                                d.bizTime2 = bizTime2;
                                d.bizObject = bizObject;
                                d.bizAddress = bizAddress;
                                d.bizContent = bizContent;
                                break;
                            }
                        }
                        #endregion
                        break;
                }

                #region 更新畫面欄位資料
                tRCPDocData.Value = Utility.JSONSerialize(DocDataClass);//暫存
                #endregion

                //處理回傳
                tP2MObject.AddStatus("callwork&HZGL004_01-HZGL004+出差明细返回&None");//返回出差单
                tP2MObject.AddRCPControls(tRCPbizEmployee, tRCPbizDate, tRCPbizTime1, tRCPbizTime2,
                    tRCPbizObject, tRCPbizAddress, tRCPbizContent, tRCPDocData);
            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetBizDetail_Operate Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 刪除明细
        public string GetBizDetail_Del(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null);

            try
            {
                #region 設定參數
                string tErrorMsg = string.Empty;        //檢查是否錯誤
                #endregion

                #region 設定控件
                RCPControl tRCPcompany = new RCPControl("company", "true", null, null);     //公司別
                RCPControl tRCPbizDetail = new RCPControl("bizDetail", "true", null, null); //單身
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);       //隱藏欄位
                #endregion

                #region 取得畫面資料
                string selectedDetail = DataTransferManager.GetControlsValue(pM2Pxml, "bizDetail");    //出差单單身勾選的資料
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //出差单據暫存
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                #endregion

                if (!string.IsNullOrEmpty(selectedDetail))
                {
                    #region 處理畫面資料
                    //先將暫存資料轉成Class方便取用
                    Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//出差单暫存資料

                    //刪除單身資料
                    foreach (var item in selectedDetail.Split('§'))
                    {
                        foreach (var d in DocDataClass.detailList)
                        {
                            if (item.Equals(d.item))
                            {
                                DocDataClass.detailList.Remove(d);
                                break;
                            }
                        }
                    }
                    #endregion

                    #region 更新出差单隐藏资料                    
                    tRCPdocData.Value = Utility.JSONSerialize(DocDataClass);

                    //檢查是否有單身資料
                    if (!(DocDataClass.detailList.Count > 0))//沒有單身資料
                    {
                        //設定單頭欄位屬性
                        tRCPcompany.Enable = "true";   //公司別
                    }
                    else//有單身資料
                    {
                        //設定單頭屬性
                        tRCPcompany.Enable = "false";   //公司別
                    }

                    //整理Table轉成符合顯示的格式
                    DataTable bizDetailTable = DocDataClass.GetDetailTable("");
                    for (int i = 0; i < bizDetailTable.Rows.Count; i++)
                    {
                        bizDetailTable.Rows[i]["bizTime1"] = bizDetailTable.Rows[i]["bizTime1"].ToString().Insert(2, ":");
                        bizDetailTable.Rows[i]["bizTime2"] = bizDetailTable.Rows[i]["bizTime2"].ToString().Insert(2, ":");
                    }
                    //設定單身資料(含分頁)
                    Utility.SetRCPControlPage(pM2Pxml, bizDetailTable, ref tRCPbizDetail);
                    #endregion

                    //處理回傳
                    tP2MObject.AddRCPControls(tRCPcompany, tRCPbizDetail, tRCPdocData);
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
                RCPControl tRCPapplyUser = new RCPControl("applyUser", "false", null, null);      //申請人
                RCPControl tRCPapplyDeptno = new RCPControl("applyDept", "true", null, null);
                RCPControl tRCPformType = new RCPControl("formType", "true", null, null);
                RCPControl tRCPotherType = new RCPControl("otherType", "false", null, null);
                RCPControl tRCPvehicle = new RCPControl("vehicle", "true", null, null);
                RCPControl tRCPotherVehicle = new RCPControl("otherVehicle", "false", null, null);
                RCPControl tRCPdestination = new RCPControl("destination", "true", null, null);
                RCPControl tRCPstartDate = new RCPControl("startDate", "true", null, null);
                RCPControl tRCPendDate = new RCPControl("endDate", "true", null, null);
                RCPControl tRCPdays = new RCPControl("days", "true", null, null);
                RCPControl tRCPbizDetail = new RCPControl("bizDetail", "true", null, null);
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系

                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //加班單據暫存

                string company = DataTransferManager.GetControlsValue(pM2Pxml, "company");          //公司別
                string applyDate = DataTransferManager.GetControlsValue(pM2Pxml, "applyDate");
                string applyUser = DataTransferManager.GetControlsValue(pM2Pxml, "applyUser");
                string applyDept = DataTransferManager.GetControlsValue(pM2Pxml, "applyDept");
                string otherType = DataTransferManager.GetControlsValue(pM2Pxml, "otherType");
                string otherVehicle = DataTransferManager.GetControlsValue(pM2Pxml, "otherVehicle");
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);                //加班單暫存資料
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

                //檢查申請部門必填
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

                if (entityClass.days < 1)
                {
                    tErrorMsg += "出差天数错误\r\n";
                }

                if("7".Equals(entityClass.formType) && string.IsNullOrEmpty(otherType))
                {
                    tErrorMsg += "其他性质必填\r\n";
                }
                if ("6".Equals(entityClass.vehicle) && string.IsNullOrEmpty(otherVehicle))
                {
                    tErrorMsg += "其他工具必填\r\n";
                }

                //檢查單身是否存在
                if (string.IsNullOrEmpty(entityJSONStr) || entityClass.detailList.Count <= 0)
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
                entityClass.company = entityClass.company.Split('-')[0];
                entityClass.applyUser = entityClass.applyUser.Split('-')[0];
                entityClass.applyDept = entityClass.applyDept.Split('-')[0];
                entityClass.otherType = otherType;
                entityClass.otherVehicle = otherVehicle;
                //單身
                foreach (var d in entityClass.detailList)
                {
                    d.bizEmployee = d.bizEmployee.Split('-')[0];
                    d.bizDate = d.bizDate.Insert(4, "/").Insert(7, "/");    //日期加/
                    d.bizTime1 = d.bizTime1.Insert(2, ":");                 //時間加:
                    d.bizTime2 = d.bizTime2.Insert(2, ":");                 //時間加:
                }
                #endregion

                #region 取得Response
                //叫用API
                string uri = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), "efgp/hzgl004/create", LoadConfig.GetWebConfig("APIKey"));
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
                    ResponseState responseState = Utility.JSONDeserialize<ResponseState>(tResponse);

                    //判斷回傳
                    if ("200".Equals(responseState.code))
                    {
                        string tMsg = string.Empty;

                        //設定多語系
                        if ("Lang01".Equals(tStrLanguage))
                        {
                            //繁體
                            tMsg = "出差单建立成功，單號 :\r\n";
                        }
                        else
                        {
                            //簡體
                            tMsg = "出差单建立成功，单号 :\r\n";
                        }

                        tP2MObject.Message = tMsg + responseState.msg;
                        tP2MObject.Result = "false";


                        #region 給值
                        //設定请假單資料
                        entityClass = new Entity(company, applyDate, applyUser, applyDept, "1", "业务推展", "1", "公务车");
                        entityClass.destination = "1";
                        entityClass.destinationDesc = "中国大陆";
                        entityJSONStr = Utility.JSONSerialize(entityClass);//Class轉成JSON

                        //給定畫面欄位值
                        tRCPformType.Value = "1-业务推展";
                        tRCPvehicle.Value = "1-公务车";
                        tRCPdestination.Value = "1-中国大陆";
                        tRCPstartDate.Value = "";
                        tRCPendDate.Value = "";
                        tRCPdays.Value = "0";
                        tRCPbizDetail.Table = new DataTable();
                        tRCPdocData.Value = entityJSONStr;     //请假單暫存資料

                        tRCPotherType.Value = "";
                        tRCPotherType.Enable = "false";
                        tRCPotherVehicle.Value = "";
                        tRCPotherVehicle.Enable = "false";
                        #endregion

                        //處理回傳
                        tP2MObject.AddRCPControls(tRCPcompany, tRCPapplyDate, tRCPapplyUser, tRCPapplyDeptno, tRCPformType, tRCPotherType,
                            tRCPvehicle, tRCPotherVehicle, tRCPdestination, tRCPstartDate, tRCPendDate, tRCPdays, tRCPbizDetail, tRCPdocData);
                    }
                    else
                    {
                        tP2MObject.Message = "出差单建立失敗，說明 :\r\n" + responseState.msg;
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

    class Entity
    {

        public string company { get; set; } //公司別
        public string applyDate { get; set; }   //申請日
        public string applyUser { get; set; }   //申請人
        public string applyDept { get; set; }   //申請部門
        public string formType { get; set; }    //
        public string formTypeDesc { get; set; }//
        public string otherType { get; set; }
        public string vehicle { get; set; }
        public string vehicleDesc { get; set; }
        public string otherVehicle { get; set; }
        public string destination { get; set; }
        public string destinationDesc { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public double days { get; set; }

        public List<BizDetail> detailList { get; set; }

        public Entity()
        {
            this.company = "";
            this.applyDate = "";
            this.applyUser = "";
            this.applyDept = "";
            this.formType = "1";
            this.formTypeDesc = "业务推展";
            this.vehicle = "1";
            this.vehicleDesc = "公务车";
            this.destination = "1";
            this.destinationDesc = "中国大陆";
            this.days = 0d;
            this.detailList = new List<BizDetail>();
        }

        public Entity(string company, string date, string userid, string deptno, string formType, string formTypeDesc, string vehicle, string vehicleDesc)
        {
            this.company = company;
            this.applyDate = date;
            this.applyUser = userid;
            this.applyDept = deptno;
            this.formType = formType;
            this.formTypeDesc = formTypeDesc;
            this.vehicle = vehicle;
            this.vehicleDesc = vehicleDesc;
            this.days = 0d;
            this.detailList = new List<BizDetail>();
        }

        //回傳單身table
        public DataTable GetDetailTable(string pSearch)
        {
            Dictionary<string, string[]> items = new Dictionary<string, string[]>();

            DataTable tbl = new DataTable();
            for (int i = 0; i < this.detailList.Count; i++)
            {
                bool flag = false;               //是否insert
                items = this.detailList[i].GetItem();

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in items["Columns"]) tbl.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    flag = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in items["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            flag = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (flag) tbl.Rows.Add(items["Values"]);
            }

            return tbl;
        }

    }

    class BizDetail
    {
        public string item { get; set; }
        public string seq { get; set; }
        public string bizEmployee { get; set; }
        public string bizEmployeeName { get; set; }
        public string bizDate { get; set; }
        public string bizTime1 { get; set; }
        public string bizTime2 { get; set; }
        public string bizObject { get; set; }
        public string bizAddress { get; set; }
        public string bizContent { get; set; }
        public string userTitle { get; set; }

        //取得單身欄位
        public Dictionary<string, string[]> GetItem()
        {
            Dictionary<string, string[]> item = new Dictionary<string, string[]>();
            item.Add("Columns", new string[] { "item", "seq", "bizEmployee", "bizEmployeeName", "bizDate", "bizTime1", "bizTime2", "bizObject", "bizAddress", "bizContent", "userTitle" });
            item.Add("Values", new string[] { this.item, this.seq, this.bizEmployee, this.bizEmployeeName, this.bizDate, this.bizTime1, this.bizTime2, this.bizObject, this.bizAddress, this.bizContent, userTitle });

            return item;
        }
    }

    class BizKind
    {
        public List<KV> data;

        public BizKind()
        {
            data = new List<KV>();
        }

        public DataTable GetDataTable(string pSearch, string[] columns)
        {
            Dictionary<string, string[]> item = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable table = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool flag = false;             //是否insert
                item = this.data[i].GetItem(); //取得資料

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in columns) table.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    flag = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in item["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            flag = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (flag) table.Rows.Add(item["Values"]);
            }

            return table;
        }
    }

    class VehicleKind
    {
        public List<KV> data;

        public VehicleKind()
        {
            data = new List<KV>();
        }

        public DataTable GetDataTable(string pSearch, string[] columns)
        {
            Dictionary<string, string[]> item = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable table = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool flag = false;               //是否insert
                item = this.data[i].GetItem();

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in columns) table.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    flag = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in item["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            flag = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (flag) table.Rows.Add(item["Values"]);
            }

            return table;
        }
    }

    class DestinationKind
    {
        public List<KV> data;

        public DestinationKind()
        {
            data = new List<KV>();
        }

        public DataTable GetDataTable(string pSearch, string[] columns)
        {
            Dictionary<string, string[]> item = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable table = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool flag = false;               //是否insert
                item = this.data[i].GetItem();

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in columns) table.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    flag = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in item["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            flag = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (flag) table.Rows.Add(item["Values"]);
            }

            return table;
        }
    }

}