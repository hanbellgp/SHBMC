using com.digiwin.Mobile.MCloud.TransferLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace cn.hanbell.mcloud.HZCW017
{
    public class HZCW017
    {
        #region 頁面初使化、借支單身分頁、搜尋
        public string GetBasicSetting(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, "借支申请单"); //參數:要顯示的訊息、結果、Title//建構p2m

            #region 設定參數                        
            Dictionary<string, string> tparam = new Dictionary<string, string>();//叫用API的參數
            string tErrorMsg = string.Empty;//檢查是否錯誤
            #endregion
            try
            {
                #region 設定控件
                RCPControl tRCPbtnSave = new RCPControl("btnSave", null, null, null);       //送單
                RCPControl tRCPcompany = new RCPControl("company", "false", null, null);     //公司別
                RCPControl tRCPapplyDate = new RCPControl("loanDate", "false", null, null);//借支日期
                RCPControl tRCPuserid = new RCPControl("loanUser", "false", null, null);      //借款人
                RCPControl tRCPdeptno = new RCPControl("loanDept", "false", null, null);    //借款部门
                RCPControl tRCPloanproperty = new RCPControl("loanproperty", "true", null, null);    //借款性质
                RCPControl tRCPtrafficfee = new RCPControl("trafficfee", "true", null, null);    //交通费
                RCPControl tRCPaccommodation = new RCPControl("accommodation", "true", null, null);    //住宿费
                RCPControl tRCPallowance = new RCPControl("allowance", "true", null, null);    //出差补贴
                RCPControl tRCPentertain = new RCPControl("entertain", "true", null, null);    //招待费
                RCPControl tRCPother = new RCPControl("other", "true", null, null);    //其他
                RCPControl tRCPcoin = new RCPControl("coin", "true", null, null);    //币别
                RCPControl tRCPloanTotal = new RCPControl("loanTotal", "false", null, null);    //借款总额
                RCPControl tRCPpreAccno = new RCPControl("preAccno", "true", null, null);    //预算科目
                RCPControl tRCPprePayDate = new RCPControl("prePayDate", "true", null, null);    //预计付款日期
                RCPControl tRCPcenterid = new RCPControl("centerid", "true", null, null);    //预计付款日期
                RCPControl tRCPreason = new RCPControl("reason", "true", null, null);    //预计付款日期
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);     //表单暂存资料
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系

                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //借支單暫存資料
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
                    //string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");    //请假類別code
                    //string formTypeDesc = "1".Equals(formType) ? "平日请假" : "国庆或春节前后";   //请假類別name

                    #region 取得公司別名稱(如果GetEmployeeDetail有回傳名稱的話可以省掉)
                    tparam.Clear();

                    //取公司別值
                    tResponse = Utility.CallAPI(pM2Pxml, "Company", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    //tResponse = "{\"data\" : " + tResponse + "}";                                           //為符合class格式自己加上去的
                    Company CompanyClass = Utility.JSONDeserialize<Company>(tResponse);                        //轉class方便取用
                    DataTable tCompanyTable = CompanyClass.GetTable(EmployDetail.company);                   //取公司別table
                    DataRow[] tCompanyRow = tCompanyTable.Select("company = '" + EmployDetail.company + "'");       //查詢指定company
                    if (tCompanyRow.Count() > 0) tCompany = tCompanyRow[0]["company"] + "-" + tCompanyRow[0]["name"];
                    #endregion

                    //叫用服务获得预算中心
                    tparam.Clear();
                    tparam.Add("company", tCompany.Split('-')[0]);       //查詢参数1
                    tparam.Add("deptId", tDetp.Split('-')[0]);       //查詢参数2
                    string tReBudgetCenter = Utility.CallAPI(pM2Pxml, "BudgetCenter", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                    //回傳的JSON轉Class
                    tReBudgetCenter = "{\"data\" : " + tReBudgetCenter + "}";
                    BudgetCenter budgetCenter = Utility.JSONDeserialize<BudgetCenter>(tReBudgetCenter);  //预算中心对象
                    string tcenterid = "";
                    if (budgetCenter != null && budgetCenter.data.Count > 0)
                    {
                        tcenterid = budgetCenter.getBudgetCenterItem().budgetCenterPK.centerid;
                    }
                    #endregion

                    #region 給值    
                    //給定畫面欄位值
                    tRCPcompany.Value = tCompany;       //公司別
                    tRCPapplyDate.Value = tDate;        //申請日
                    tRCPuserid.Value = tUser;           //申請人
                    tRCPdeptno.Value = tDetp;           //申請部門
                    tRCPloanproperty.Value = "0-销售营业";
                    tRCPtrafficfee.Value = "0.00";
                    tRCPaccommodation.Value = "0.00";
                    tRCPallowance.Value = "0.00";
                    tRCPentertain.Value = "0.00";
                    tRCPother.Value = "0.00";
                    tRCPcoin.Value = "RMB-人民币";
                    tRCPpreAccno.Value = "";
                    tRCPloanTotal.Value = "";
                    tRCPprePayDate.Value = "";
                    tRCPreason.Value = "";
                    tRCPcenterid.Value = tcenterid;
                    //tRCPformKind.Value = "1-年休假";
                    //tRCPworkType.Value = "1-常日班：08：00-20：20";
                    //tRCPapplyDay.Value = "0";

                    //設定借支單資料
                    Entity entityClass = new Entity(tCompany, tDate, tUser, tDetp, "0", "销售营业", 0.00, 0.00, 0.00, 0.00, 0.00, "RMB-人民币", tcenterid);
                    //entityClass.workType = "1";
                    //entityClass.workTypeDesc = "常日班：80：00";
                    entityJSONStr = Utility.JSONSerialize(entityClass);//Class轉成JSON
                    tRCPdocData.Value = entityJSONStr;     //借支單暫存資料

                    #endregion
                }
                else//有單據資料
                {
                    #region 显示借支單資料
                    //先將暫存資料轉成Class方便取用
                    Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//借支單暫存資料

                    //設定畫面資料
                    tRCPcompany.Value = entityClass.company;       //公司別
                    tRCPapplyDate.Value = entityClass.loanDate;   //申請日
                    tRCPuserid.Value = entityClass.loanUser;      //申請人
                    tRCPdeptno.Value = entityClass.loanDept;      //申請部門
                    tRCPdocData.Value = entityJSONStr;                //借支單暫存資料
                    #endregion
                }

                //處理回傳
                tP2MObject.AddRCPControls(tRCPcompany, tRCPapplyDate, tRCPuserid, tRCPdeptno, tRCPloanproperty, tRCPtrafficfee,
                    tRCPaccommodation, tRCPallowance, tRCPentertain, tRCPother, tRCPcoin, tRCPloanTotal, tRCPpreAccno, tRCPprePayDate, tRCPreason, tRCPcenterid, tRCPdocData);
                #endregion
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

                //tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
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

        #region 借支性质開窗
        public string GetLoanProperty_OpenQuery(XDocument pM2Pxml)
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
                RDControl tRDloanProperty = new RDControl(new DataTable());  //借支性质清單
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
                string tResponse = Utility.CallAPI(pM2Pxml, "LoanProperty", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                LoanProperty loanproperty = Utility.JSONDeserialize<LoanProperty>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得Table
                DataTable table = loanproperty.GetDataTable(tSearch, new string[] { "loanproperty", "loanpropertyDesc" });
                table.Columns.Add("loanpropertyC");//    Control ID + C = 開窗控件要顯示的值
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["loanpropertyC"] = table.Rows[i]["loanproperty"] + "-" + table.Rows[i]["loanpropertyDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, table, ref tRDloanProperty);
                #endregion
                #endregion

                #region 設定物件屬性
                //借支性质清單屬性
                tRDloanProperty.AddCustomTag("DisplayColumn", "loanproperty§loanpropertyDesc§loanpropertyC");
                tRDloanProperty.AddCustomTag("StructureStyle", "loanproperty:R1:C1:1§loanpropertyDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDloanProperty.AddCustomTag("ColumnsName", "代號§名稱§显示");
                }
                else
                {
                    //簡體
                    tRDloanProperty.AddCustomTag("ColumnsName", "代号§名称§显示");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDloanProperty);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetLoanProperty_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetLoanProperty_OnBlur(XDocument pM2Pxml)
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
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //借支单據隱藏欄位
                #endregion

                #region 取得畫面資料
                string loanproperty = DataTransferManager.GetControlsValue(pM2Pxml, "loanproperty");
                string loanpropertyDesc = DataTransferManager.GetControlsValue(pM2Pxml, "loanpropertyC");
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //借支單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//借支單暫存資料

                //修改借支性质資料
                entityClass.loanproperty = loanproperty;
                entityClass.loanpropertyDesc = loanpropertyDesc;

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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetLoanProperty_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 币別開窗
        public string GetCoin_OpenQuery(XDocument pM2Pxml)
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
                RDControl tRDcoin = new RDControl(new DataTable());  //币別清單
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
                string tResponse = Utility.CallAPI(pM2Pxml, "Coin", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                Coin CoinClass = Utility.JSONDeserialize<Coin>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得币別Table
                //DataTable tCoinTable = CoinClass.GetDataTable(tSearch);
                DataTable tCoinTable = CoinClass.GetDataTable(tSearch, new string[] { "coin", "coinDesc" });
                tCoinTable.Columns.Add("coinC");//    Control applyUser + C = 開窗控件要顯示的值
                for (int i = 0; i < tCoinTable.Rows.Count; i++) { tCoinTable.Rows[i]["coinC"] = tCoinTable.Rows[i]["coin"] + "-" + tCoinTable.Rows[i]["coinDesc"]; }//設定顯示的值

                //設定公司別清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, tCoinTable, ref tRDcoin);
                #endregion
                #endregion

                #region 設定物件屬性
                //币別清單屬性
                tRDcoin.AddCustomTag("DisplayColumn", "coin§coinDesc§coinC");
                tRDcoin.AddCustomTag("StructureStyle", "coin:R1:C1:1§coinDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDcoin.AddCustomTag("ColumnsName", "代號§名稱§币別");
                }
                else
                {
                    //簡體
                    tRDcoin.AddCustomTag("ColumnsName", "代号§名称§币别");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDcoin);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetCoin_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetCoin_OnBlur(XDocument pM2Pxml)
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
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //借支單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrcoin = DataTransferManager.GetControlsValue(pM2Pxml, "coinC");  //币别
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //借支單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                //修改公司別資料
                DocDataClass.coin = tStrcoin;

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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetCoin_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        #region 预算科目開窗
        public string GetPreAccno_OpenQuery(XDocument pM2Pxml)
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
                RDControl tRDpreAccno = new RDControl(new DataTable());  //预算科目清單
                #endregion

                #region 取得畫面資料
                string tCompany = DataTransferManager.GetControlsValue(pM2Pxml, "company");  //取公司别
                string tCenterid = DataTransferManager.GetControlsValue(pM2Pxml, "centerid");  //取预算中心
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //單身搜尋條件
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                #endregion

                #region 處理畫面資料
                #region 取得预算科目清單資料
                //設定參數
                tparam.Clear();
                tparam.Add("company", tCompany.Split('-')[0]);       // 传入公司别参数
                tparam.Add("centerid", tCenterid);       // 传入预算中心参数

                //叫用服務
                string tResponse = Utility.CallAPI(pM2Pxml, "PreAccno", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //為符合class格式自己加上去的
                BudgetAccno preAccnoClass = Utility.JSONDeserialize<BudgetAccno>(tResponse);    //轉class方便取用
                #endregion

                #region 給值
                //取得预算科目的table
                DataTable tpreAccnoTable = preAccnoClass.GetDataTable(tSearch, new string[] { "preAccno", "preAccnoDesc" });
                tpreAccnoTable.Columns.Add("preAccnoC");//    Control applyUser + C = 開窗控件要顯示的值
                for (int i = 0; i < tpreAccnoTable.Rows.Count; i++) { tpreAccnoTable.Rows[i]["preAccnoC"] = tpreAccnoTable.Rows[i]["preAccno"] + "-" + tpreAccnoTable.Rows[i]["preAccnoDesc"]; }//設定顯示的值

                //設定预算科目清單資料(含分頁)
                Utility.SetRDControlPage(pM2Pxml, tpreAccnoTable, ref tRDpreAccno);
                #endregion
                #endregion

                #region 設定物件屬性
                //预算科目清單屬性
                tRDpreAccno.AddCustomTag("DisplayColumn", "preAccno§preAccnoDesc§preAccnoC");
                tRDpreAccno.AddCustomTag("StructureStyle", "preAccno:R1:C1:1§preAccnoDesc:R1:C2:1");

                //設定多語系
                if ("Lang01".Equals(tStrLanguage))
                {
                    //繁體
                    tRDpreAccno.AddCustomTag("ColumnsName", "代號§名稱§预算科目");
                }
                else
                {
                    //簡體
                    tRDpreAccno.AddCustomTag("ColumnsName", "代号§名称§预算科目");
                }
                #endregion

                //處理回傳
                tP2MObject.AddRDControl(tRDpreAccno);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetPreAccno_OpenQuery Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }

        public string GetPreAccno_OnBlur(XDocument pM2Pxml)
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
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //借支單據隱藏欄位
                #endregion

                #region 取得畫面資料
                string tStrpreAccnoC = DataTransferManager.GetControlsValue(pM2Pxml, "preAccnoC");  // 预算科目栏位
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //借支單據暫存
                #endregion

                #region 處理畫面資料
                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//加班單暫存資料

                //修改预算科目資料
                DocDataClass.preAccno = tStrpreAccnoC;

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
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetPreAccnoC_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;
        }
        #endregion

        //费用变更 核算金额
        public string GetLoanTotal_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//回傳值
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //參數:要顯示的訊息、結果、Title//建構p2m
            #region 設定控件

            RCPControl tRCPloanTotal = new RCPControl("loanTotal", "false", "true", null);
            RCPControl tRCPtrafficfee = new RCPControl("trafficfee", "true", null, null);    //交通费
            RCPControl tRCPaccommodation = new RCPControl("accommodation", "true", null, null);    //住宿费
            RCPControl tRCPallowance = new RCPControl("allowance", "true", null, null);    //出差补贴
            RCPControl tRCPentertain = new RCPControl("entertain", "true", null, null);    //招待费
            RCPControl tRCPother = new RCPControl("other", "true", null, null);    //其他
            RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);
            #endregion

            #region 取得畫面資料
            string tTrafficfee = DataTransferManager.GetControlsValue(pM2Pxml, "trafficfee");  //交通费   
            string tAccommodation = DataTransferManager.GetControlsValue(pM2Pxml, "accommodation");  //住宿费   
            string tAllowance = DataTransferManager.GetControlsValue(pM2Pxml, "allowance");  //出差补贴
            string tEntertain = DataTransferManager.GetControlsValue(pM2Pxml, "entertain");        //招待费
            string tOther = DataTransferManager.GetControlsValue(pM2Pxml, "other");  //其他费用
            string tLoanTotal = DataTransferManager.GetDataValue(pM2Pxml, "loanTotal");        //合计
            string tDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");
            #endregion
            try
            {
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tDocData);

                tLoanTotal = (double.Parse(tTrafficfee) + double.Parse(tAccommodation) + double.Parse(tAllowance) + double.Parse(tOther) + double.Parse(tEntertain)).ToString();
                tRCPloanTotal.Value = tLoanTotal;
                //Class轉成JSON
                DocDataClass.loanTotal = double.Parse(tLoanTotal);
                DocDataClass.trafficfee = double.Parse(tTrafficfee);
                DocDataClass.accommodation = double.Parse(tAccommodation);
                DocDataClass.allowance = double.Parse(tAllowance);
                DocDataClass.entertain = double.Parse(tEntertain);
                DocDataClass.other = double.Parse(tOther);

                string JSONStr = Utility.JSONSerialize(DocDataClass);
                //給值
                tRCPDocData.Value = JSONStr;
                //處理回傳
                tP2MObject.AddRCPControls(tRCPloanTotal, tRCPtrafficfee, tRCPaccommodation, tRCPallowance, tRCPentertain, tRCPother,
                    tRCPDocData);
                //tP2MObject.AddRCPControls(tRCPloanTotal, tRCPDocData);
                //tP2MObject.AddRCPControls(tRCPtrafficfee, tRCPDocData);
                //tP2MObject.AddRCPControls(tRCPaccommodation, tRCPDocData);
                //tP2MObject.AddRCPControls(tRCPallowance, tRCPDocData);
                //tP2MObject.AddRCPControls(tRCPentertain, tRCPDocData);
                //tP2MObject.AddRCPControls(tRCPother, tRCPDocData);

            }
            catch (Exception err)
            {
                Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, "GetLoanTotal_OnBlur Error : " + err.Message.ToString());
            }

            tP2Mxml = tP2MObject.ToDucument().ToString();
            return tP2Mxml;

        }

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
                RCPControl tRCPcompany = new RCPControl("company", "false", null, null);     //公司別
                RCPControl tRCPapplyDate = new RCPControl("loanDate", "false", null, null);//借支日期
                RCPControl tRCPuserid = new RCPControl("loanUser", "false", null, null);      //借款人
                RCPControl tRCPdeptno = new RCPControl("loanDept", "false", null, null);    //借款部门
                RCPControl tRCPloanproperty = new RCPControl("loanproperty", "true", null, null);    //借款性质
                RCPControl tRCPtrafficfee = new RCPControl("trafficfee", "true", null, null);    //交通费
                RCPControl tRCPaccommodation = new RCPControl("accommodation", "true", null, null);    //住宿费
                RCPControl tRCPallowance = new RCPControl("allowance", "true", null, null);    //出差补贴
                RCPControl tRCPentertain = new RCPControl("entertain", "true", null, null);    //招待费
                RCPControl tRCPother = new RCPControl("other", "true", null, null);    //其他
                RCPControl tRCPcoin = new RCPControl("coin", "true", null, null);    //币别
                RCPControl tRCPloanTotal = new RCPControl("loanTotal", "false", null, null);    //借款总额
                RCPControl tRCPpreAccno = new RCPControl("preAccno", "true", null, null);    //预算科目
                RCPControl tRCPprePayDate = new RCPControl("prePayDate", "true", null, null);    //预计付款日期
                RCPControl tRCPcenterid = new RCPControl("centerid", "true", null, null);    //预算中心
                RCPControl tRCPreason = new RCPControl("reason", "true", null, null);    //理由
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);     //表单暂存资料
                #endregion

                #region 取得畫面資料
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //登入者帳號
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //語系
                string company = DataTransferManager.GetControlsValue(pM2Pxml, "company");      //公司別
                string loanDate = DataTransferManager.GetControlsValue(pM2Pxml, "loanDate");        //借支日期
                string loanUser = DataTransferManager.GetControlsValue(pM2Pxml, "loanUser");
                string loanDept = DataTransferManager.GetControlsValue(pM2Pxml, "loanDept");
                string loanproperty = DataTransferManager.GetControlsValue(pM2Pxml, "loanproperty");
                string trafficfee = DataTransferManager.GetControlsValue(pM2Pxml, "trafficfee");
                string accommodation = DataTransferManager.GetControlsValue(pM2Pxml, "accommodation");
                string allowance = DataTransferManager.GetControlsValue(pM2Pxml, "allowance");
                string entertain = DataTransferManager.GetControlsValue(pM2Pxml, "entertain");
                string other = DataTransferManager.GetControlsValue(pM2Pxml, "other");
                string coin = DataTransferManager.GetControlsValue(pM2Pxml, "coin");
                string loanTotal = DataTransferManager.GetControlsValue(pM2Pxml, "loanTotal");
                string preAccno = DataTransferManager.GetControlsValue(pM2Pxml, "preAccnoC");
                string prePayDate = DataTransferManager.GetControlsValue(pM2Pxml, "prePayDate");
                string centerid = DataTransferManager.GetControlsValue(pM2Pxml, "centerid");
                string reason = DataTransferManager.GetControlsValue(pM2Pxml, "reason");
                string docData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //借支單據暫存

                //先將暫存資料轉成Class方便取用
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(docData);                //借支單暫存資料
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
                if (string.IsNullOrEmpty(loanDept))
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
                if (string.IsNullOrEmpty(loanproperty))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "借款性质必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "借款性质必填\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(coin))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "币别必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "币别必填\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(preAccno))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "预算科目必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "预算科目必填\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(prePayDate))
                {
                    //設定多語系
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //繁體
                        tErrorMsg += "预计付款日期必填\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "预计付款日期必填\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(centerid))
                {
                    //設定多語系
                    if ("Lang01".Equals(centerid))
                    {
                        //繁體
                        tErrorMsg += "未找到部门对应预算中心\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "未找到部门对应预算中心\r\n";
                    }
                }

                //檢查必填
                if (string.IsNullOrEmpty(reason))
                {
                    //設定多語系
                    if ("Lang01".Equals(reason))
                    {
                        //繁體
                        tErrorMsg += "请填写借支理由\r\n";
                    }
                    else
                    {
                        //簡體
                        tErrorMsg += "请填写借支理由\r\n";
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

                    if (Double.Parse(loanTotal) <= 0)
                    {
                        tErrorMsg += "借款总金额必须大于0";
                    }

                }
                catch (Exception ex)
                {
                    tErrorMsg += "借款总金额格式错误";
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
                //DocDataClass.company = DocDataClass.company.Split('-')[0];    //公司別只取id
                DocDataClass.company = DocDataClass.company;
                DocDataClass.loanUser = DocDataClass.loanUser.Split('-')[0];              //申請人只取id
                DocDataClass.loanDept = DocDataClass.loanDept.Split('-')[0];
                DocDataClass.loanDate = loanDate.Insert(4, "-").Insert(7, "-");
                DocDataClass.loanproperty = DocDataClass.loanproperty;
                DocDataClass.loanpropertyDesc = DocDataClass.loanpropertyDesc;
                DocDataClass.trafficfee = Double.Parse(trafficfee);
                DocDataClass.accommodation = Double.Parse(accommodation);
                DocDataClass.allowance = Double.Parse(allowance);
                DocDataClass.entertain = Double.Parse(entertain);
                DocDataClass.other = Double.Parse(other);
                DocDataClass.coin = DocDataClass.coin.Split('-')[0];
                DocDataClass.loanTotal = Double.Parse(loanTotal);
                DocDataClass.preAccno = preAccno;
                DocDataClass.prePayDate = prePayDate.Insert(4, "/").Insert(7, "/");
                DocDataClass.centerid = centerid;
                DocDataClass.reason = reason;

                #region 取得Response
                //叫用API
                string uri = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), "efgp/hzcw017/create", LoadConfig.GetWebConfig("APIKey"));
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
                    ResponseState rs = Utility.JSONDeserialize<ResponseState>(tResponse);

                    //判斷回傳
                    if ("200".Equals(rs.code))
                    {
                        string tMsg = string.Empty;

                        //設定多語系
                        if ("Lang01".Equals(tStrLanguage))
                        {
                            //繁體
                            tMsg = "借支单建立成功，單號 :\r\n";
                        }
                        else
                        {
                            //簡體
                            tMsg = "借支单建立成功，单号 :\r\n";
                        }

                        tP2MObject.Message = tMsg + rs.msg;
                        tP2MObject.Result = "false";

                        #region 給值
                        //設定借支單資料
                        Entity newEntity = new Entity(company, loanDate, tStrUserID, loanDept, DocDataClass.loanproperty, DocDataClass.loanpropertyDesc, DocDataClass.trafficfee, DocDataClass.accommodation, DocDataClass.allowance, DocDataClass.entertain, DocDataClass.other,
                                                        DocDataClass.coin, DocDataClass.centerid);
                        string DocDataStr = Utility.JSONSerialize(newEntity);//Class轉成JSON

                        //給定畫面欄位值
                        tRCPtrafficfee.Value = "0.00";
                        tRCPaccommodation.Value = "0.00";
                        tRCPallowance.Value = "0.00";
                        tRCPentertain.Value = "0.00";
                        tRCPother.Value = "0.00";
                        tRCPcoin.Value = "RMB-人民币";
                        tRCPpreAccno.Value = "";
                        tRCPloanTotal.Value = "";
                        tRCPprePayDate.Value = "";
                        tRCPcenterid.Value = centerid;
                        tRCPreason.Value = "";
                        tRCPdocData.Value = DocDataStr;     //借支單暫存資料

                        #endregion

                        //處理回傳
                        tP2MObject.AddRCPControls(tRCPtrafficfee, tRCPaccommodation, tRCPallowance, tRCPentertain, tRCPother, tRCPcoin,
                         tRCPpreAccno, tRCPloanTotal, tRCPprePayDate, tRCPcenterid, tRCPreason, tRCPdocData);
                    }
                    else
                    {
                        tP2MObject.Message = "借支单建立失败，原因 :\r\n" + rs.msg;
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


    //借支單Class
    class Entity
    {

        public string company { get; set; }     //公司別
        public string loanDate { get; set; }        //借款日期
        public string loanUser { get; set; }          //申請人
        public string loanDept { get; set; }      //申請部門
        public string loanproperty { get; set; }    //借支性质code
        public string loanpropertyDesc { get; set; }//借支性质name
        public double trafficfee { get; set; }    //交通费 
        public double accommodation { get; set; }//住宿费
        public double allowance { get; set; }    //出差补贴
        public double entertain { get; set; }   //招待费   
        public double other { get; set; }      //其他费用
        public string coin { get; set; }      //币别
        public double loanTotal { get; set; }      //借款总额
        public string preAccno { get; set; }    //预算科目
        public string prePayDate { get; set; }  //预计付款日
        public string centerid { get; set; }  //预算中心

        //public double leaveDay { get; set; }
        //public double leaveHour { get; set; }
        //public double leaveMinute { get; set; }

        public string reason { get; set; }

        public Entity()
        {
            this.company = "C-上海汉钟";
            this.loanDate = "";
            this.loanUser = "";
            this.loanDept = "";
            this.loanproperty = "0";
            this.loanpropertyDesc = "销售营业";
            this.trafficfee = 0.00;
            this.accommodation = 0.00;
            this.allowance = 0.00;
            this.entertain = 0.00;
            this.other = 0d;
            this.coin = "RMB-人民币";
            this.loanTotal = 0;
            this.preAccno = "";
            this.prePayDate = "";
            //this.formKind = "1";
            //this.formTypeDesc = "年休假";
        }

        public Entity(string company, string date, string id, string deptno, string loanproperty, string loanpropertyDesc, double trafficfee, double accommodation, double allowance, double entertain, double other, string coin, string centerid)
        {
            this.company = company;
            this.loanDate = date;
            this.loanUser = id;
            this.loanDept = deptno;
            this.loanproperty = loanproperty;
            this.loanpropertyDesc = loanpropertyDesc;
            this.trafficfee = trafficfee;
            this.accommodation = accommodation;
            this.allowance = allowance;
            this.entertain = entertain;
            this.other = other;
            this.coin = coin;
            this.centerid = centerid;
        }

    }

    //借支性质
    class LoanProperty
    {
        public List<KV> data;

        public LoanProperty()
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
                item = this.data[i].GetItem(); //取得借支性质資料

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