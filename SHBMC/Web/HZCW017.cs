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
        #region ����ʹ������֧�����퓡��ь�
        public string GetBasicSetting(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, "��֧���뵥"); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            #region �O������                        
            Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
            string tErrorMsg = string.Empty;//�z���Ƿ��e�`
            #endregion
            try
            {
                #region �O���ؼ�
                RCPControl tRCPbtnSave = new RCPControl("btnSave", null, null, null);       //�͆�
                RCPControl tRCPcompany = new RCPControl("company", "false", null, null);     //��˾�e
                RCPControl tRCPapplyDate = new RCPControl("loanDate", "false", null, null);//��֧����
                RCPControl tRCPuserid = new RCPControl("loanUser", "false", null, null);      //�����
                RCPControl tRCPdeptno = new RCPControl("loanDept", "false", null, null);    //����
                RCPControl tRCPloanproperty = new RCPControl("loanproperty", "true", null, null);    //�������
                RCPControl tRCPtrafficfee = new RCPControl("trafficfee", "true", null, null);    //��ͨ��
                RCPControl tRCPaccommodation = new RCPControl("accommodation", "true", null, null);    //ס�޷�
                RCPControl tRCPallowance = new RCPControl("allowance", "true", null, null);    //�����
                RCPControl tRCPentertain = new RCPControl("entertain", "true", null, null);    //�д���
                RCPControl tRCPother = new RCPControl("other", "true", null, null);    //����
                RCPControl tRCPcoin = new RCPControl("coin", "true", null, null);    //�ұ�
                RCPControl tRCPloanTotal = new RCPControl("loanTotal", "false", null, null);    //����ܶ�
                RCPControl tRCPpreAccno = new RCPControl("preAccno", "true", null, null);    //Ԥ���Ŀ
                RCPControl tRCPprePayDate = new RCPControl("prePayDate", "true", null, null);    //Ԥ�Ƹ�������
                RCPControl tRCPcenterid = new RCPControl("centerid", "true", null, null);    //Ԥ�Ƹ�������
                RCPControl tRCPreason = new RCPControl("reason", "true", null, null);    //Ԥ�Ƹ�������
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);     //����ݴ�����
                #endregion

                #region ȡ�î����Y��
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //�����ߎ�̖
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //�Zϵ

                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //��֧�Ε����Y��
                #endregion

                #region ̎�e�����Y��
                //�z���Ƿ����ІΓ��Y��
                if (string.IsNullOrEmpty(entityJSONStr))//�]�Γ��Y��
                {
                    #region ȡ���A�O�Y��
                    //�O������
                    tparam.Clear();
                    tparam.Add("UserID", tStrUserID);       //��ԃ����

                    //���÷���
                    string tResponse = Utility.CallAPI(pM2Pxml, "Users", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    //�؂���JSON�DClass
                    Users EmployDetail = Utility.JSONDeserialize<Users>(tResponse);  //�T������

                    //ȡֵ
                    string tCompany = string.Format("{0}-{1}", EmployDetail.company, ""/*EmployDetail.companyName*/);   //��˾�e��̖-��˾�e���Q
                    string tUser = string.Format("{0}-{1}", EmployDetail.id, EmployDetail.userName);    //��Ո�˴�̖-��Ո�����Q
                    string tDetp = string.Format("{0}-{1}", EmployDetail.deptno, EmployDetail.deptname);    //���T��̖-���T���Q
                    string tDate = DateTime.Now.ToString("yyyy/MM/dd"); //��Ո�r�g
                    //string formType = DataTransferManager.GetControlsValue(pM2Pxml, "formType");    //���ecode
                    //string formTypeDesc = "1".Equals(formType) ? "ƽ�����" : "����򴺽�ǰ��";   //���ename

                    #region ȡ�ù�˾�e���Q(���GetEmployeeDetail�л؂����Q��Ԓ����ʡ��)
                    tparam.Clear();

                    //ȡ��˾�eֵ
                    tResponse = Utility.CallAPI(pM2Pxml, "Company", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                    tResponse = "{\"data\" : " + tResponse + "}";                                           //�����class��ʽ�Լ�����ȥ��
                    Company CompanyClass = Utility.JSONDeserialize<Company>(tResponse);                        //�Dclass����ȡ��
                    DataTable tCompanyTable = CompanyClass.GetTable(EmployDetail.company);                   //ȡ��˾�etable
                    DataRow[] tCompanyRow = tCompanyTable.Select("company = '" + EmployDetail.company + "'");       //��ԃָ��company
                    if (tCompanyRow.Count() > 0) tCompany = tCompanyRow[0]["company"] + "-" + tCompanyRow[0]["name"];
                    #endregion

                    //���÷�����Ԥ������
                    tparam.Clear();
                    tparam.Add("company", tCompany.Split('-')[0]);       //��ԃ����1
                    tparam.Add("deptId", tDetp.Split('-')[0]);       //��ԃ����2
                    string tReBudgetCenter = Utility.CallAPI(pM2Pxml, "BudgetCenter", tparam, out tErrorMsg);
                    if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                    //�؂���JSON�DClass
                    tReBudgetCenter = "{\"data\" : " + tReBudgetCenter + "}";
                    BudgetCenter budgetCenter = Utility.JSONDeserialize<BudgetCenter>(tReBudgetCenter);  //Ԥ�����Ķ���
                    string tcenterid = "";
                    if (budgetCenter != null && budgetCenter.data.Count > 0)
                    {
                        tcenterid = budgetCenter.getBudgetCenterItem().budgetCenterPK.centerid;
                    }
                    #endregion

                    #region �oֵ    
                    //�o�������λֵ
                    tRCPcompany.Value = tCompany;       //��˾�e
                    tRCPapplyDate.Value = tDate;        //��Ո��
                    tRCPuserid.Value = tUser;           //��Ո��
                    tRCPdeptno.Value = tDetp;           //��Ո���T
                    tRCPloanproperty.Value = "0-����Ӫҵ";
                    tRCPtrafficfee.Value = "0.00";
                    tRCPaccommodation.Value = "0.00";
                    tRCPallowance.Value = "0.00";
                    tRCPentertain.Value = "0.00";
                    tRCPother.Value = "0.00";
                    tRCPcoin.Value = "RMB-�����";
                    tRCPpreAccno.Value = "";
                    tRCPloanTotal.Value = "";
                    tRCPprePayDate.Value = "";
                    tRCPreason.Value = "";
                    tRCPcenterid.Value = tcenterid;
                    //tRCPformKind.Value = "1-���ݼ�";
                    //tRCPworkType.Value = "1-���հࣺ08��00-20��20";
                    //tRCPapplyDay.Value = "0";

                    //�O����֧���Y��
                    Entity entityClass = new Entity(tCompany, tDate, tUser, tDetp, "0", "����Ӫҵ", 0.00, 0.00, 0.00, 0.00, 0.00, "RMB-�����", tcenterid);
                    //entityClass.workType = "1";
                    //entityClass.workTypeDesc = "���հࣺ80��00";
                    entityJSONStr = Utility.JSONSerialize(entityClass);//Class�D��JSON
                    tRCPdocData.Value = entityJSONStr;     //��֧�Ε����Y��

                    #endregion
                }
                else//�ІΓ��Y��
                {
                    #region ��ʾ��֧���Y��
                    //�Ȍ������Y���D��Class����ȡ��
                    Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//��֧�Ε����Y��

                    //�O�������Y��
                    tRCPcompany.Value = entityClass.company;       //��˾�e
                    tRCPapplyDate.Value = entityClass.loanDate;   //��Ո��
                    tRCPuserid.Value = entityClass.loanUser;      //��Ո��
                    tRCPdeptno.Value = entityClass.loanDept;      //��Ո���T
                    tRCPdocData.Value = entityJSONStr;                //��֧�Ε����Y��
                    #endregion
                }

                //̎��؂�
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

        #region ��˾�e�_��
        public string GetCompany_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RDControl tRDcompany = new RDControl(new DataTable());  //��˾�e���
                #endregion

                #region ȡ�î����Y��
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //�����ь��l��
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //�Zϵ
                #endregion

                #region ̎������Y��
                #region ȡ�ù�˾�e����Y��
                //�O������
                tparam.Clear();

                //���÷���
                string tResponse = Utility.CallAPI(pM2Pxml, "Company", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //�����class��ʽ�Լ�����ȥ��
                Company CompanyClass = Utility.JSONDeserialize<Company>(tResponse);    //�Dclass����ȡ��
                #endregion

                #region �oֵ
                //ȡ�ù�˾�eTable
                DataTable tCompanyTable = CompanyClass.GetTable(tSearch);
                tCompanyTable.Columns.Add("companyC");//    Control applyUser + C = �_���ؼ�Ҫ�@ʾ��ֵ
                for (int i = 0; i < tCompanyTable.Rows.Count; i++) { tCompanyTable.Rows[i]["companyC"] = tCompanyTable.Rows[i]["company"] + "-" + tCompanyTable.Rows[i]["name"]; }//�O���@ʾ��ֵ

                //�O����˾�e����Y��(�����)
                Utility.SetRDControlPage(pM2Pxml, tCompanyTable, ref tRDcompany);
                #endregion
                #endregion

                #region �O���������
                //��˾�e��Ό���
                tRDcompany.AddCustomTag("DisplayColumn", "company��name��companyC");
                tRDcompany.AddCustomTag("StructureStyle", "company:R1:C1:1��name:R1:C2:1");

                //�O�����Zϵ
                if ("Lang01".Equals(tStrLanguage))
                {
                    //���w
                    tRDcompany.AddCustomTag("ColumnsName", "��̖�����Q�칫˾�e");
                }
                else
                {
                    //���w
                    tRDcompany.AddCustomTag("ColumnsName", "���š����ơ칫˾��");
                }
                #endregion

                //̎��؂�
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

        #region ��֧�����_��
        public string GetLoanProperty_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RDControl tRDloanProperty = new RDControl(new DataTable());  //��֧�������
                #endregion

                #region ȡ�î����Y��
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //�����ь��l��
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //�Zϵ
                #endregion

                #region ̎������Y��
                #region ȡ������Y��
                //�O������
                tparam.Clear();
                //new CustomLogger.Logger(pM2Pxml).WriteInfo("functionName : " + tfunctionName);

                //���÷���
                string tResponse = Utility.CallAPI(pM2Pxml, "LoanProperty", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //�����class��ʽ�Լ�����ȥ��
                LoanProperty loanproperty = Utility.JSONDeserialize<LoanProperty>(tResponse);    //�Dclass����ȡ��
                #endregion

                #region �oֵ
                //ȡ��Table
                DataTable table = loanproperty.GetDataTable(tSearch, new string[] { "loanproperty", "loanpropertyDesc" });
                table.Columns.Add("loanpropertyC");//    Control ID + C = �_���ؼ�Ҫ�@ʾ��ֵ
                for (int i = 0; i < table.Rows.Count; i++) { table.Rows[i]["loanpropertyC"] = table.Rows[i]["loanproperty"] + "-" + table.Rows[i]["loanpropertyDesc"]; }//�O���@ʾ��ֵ

                //�O����˾�e����Y��(�����)
                Utility.SetRDControlPage(pM2Pxml, table, ref tRDloanProperty);
                #endregion
                #endregion

                #region �O���������
                //��֧������Ό���
                tRDloanProperty.AddCustomTag("DisplayColumn", "loanproperty��loanpropertyDesc��loanpropertyC");
                tRDloanProperty.AddCustomTag("StructureStyle", "loanproperty:R1:C1:1��loanpropertyDesc:R1:C2:1");

                //�O�����Zϵ
                if ("Lang01".Equals(tStrLanguage))
                {
                    //���w
                    tRDloanProperty.AddCustomTag("ColumnsName", "��̖�����Q����ʾ");
                }
                else
                {
                    //���w
                    tRDloanProperty.AddCustomTag("ColumnsName", "���š����ơ���ʾ");
                }
                #endregion

                //̎��؂�
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
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //��֧�����[�ؙ�λ
                #endregion

                #region ȡ�î����Y��
                string loanproperty = DataTransferManager.GetControlsValue(pM2Pxml, "loanproperty");
                string loanpropertyDesc = DataTransferManager.GetControlsValue(pM2Pxml, "loanpropertyC");
                string entityJSONStr = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //��֧�Γ�����
                #endregion

                #region ̎������Y��
                //�Ȍ������Y���D��Class����ȡ��
                Entity entityClass = Utility.JSONDeserialize<Entity>(entityJSONStr);//��֧�Ε����Y��

                //�޸Ľ�֧�����Y��
                entityClass.loanproperty = loanproperty;
                entityClass.loanpropertyDesc = loanpropertyDesc;

                //��ن�Class�D��JSON
                entityJSONStr = Utility.JSONSerialize(entityClass);

                //�oֵ
                tRCPDocData.Value = entityJSONStr;
                #endregion

                //̎��؂�
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

        #region �҄e�_��
        public string GetCoin_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RDControl tRDcoin = new RDControl(new DataTable());  //�҄e���
                #endregion

                #region ȡ�î����Y��
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //�����ь��l��
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //�Zϵ
                #endregion

                #region ̎������Y��
                #region ȡ�ù�˾�e����Y��
                //�O������
                tparam.Clear();

                //���÷���
                string tResponse = Utility.CallAPI(pM2Pxml, "Coin", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //�����class��ʽ�Լ�����ȥ��
                Coin CoinClass = Utility.JSONDeserialize<Coin>(tResponse);    //�Dclass����ȡ��
                #endregion

                #region �oֵ
                //ȡ�ñ҄eTable
                //DataTable tCoinTable = CoinClass.GetDataTable(tSearch);
                DataTable tCoinTable = CoinClass.GetDataTable(tSearch, new string[] { "coin", "coinDesc" });
                tCoinTable.Columns.Add("coinC");//    Control applyUser + C = �_���ؼ�Ҫ�@ʾ��ֵ
                for (int i = 0; i < tCoinTable.Rows.Count; i++) { tCoinTable.Rows[i]["coinC"] = tCoinTable.Rows[i]["coin"] + "-" + tCoinTable.Rows[i]["coinDesc"]; }//�O���@ʾ��ֵ

                //�O����˾�e����Y��(�����)
                Utility.SetRDControlPage(pM2Pxml, tCoinTable, ref tRDcoin);
                #endregion
                #endregion

                #region �O���������
                //�҄e��Ό���
                tRDcoin.AddCustomTag("DisplayColumn", "coin��coinDesc��coinC");
                tRDcoin.AddCustomTag("StructureStyle", "coin:R1:C1:1��coinDesc:R1:C2:1");

                //�O�����Zϵ
                if ("Lang01".Equals(tStrLanguage))
                {
                    //���w
                    tRDcoin.AddCustomTag("ColumnsName", "��̖�����Q��҄e");
                }
                else
                {
                    //���w
                    tRDcoin.AddCustomTag("ColumnsName", "���š����ơ�ұ�");
                }
                #endregion

                //̎��؂�
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
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //��֧�Γ��[�ؙ�λ
                #endregion

                #region ȡ�î����Y��
                string tStrcoin = DataTransferManager.GetControlsValue(pM2Pxml, "coinC");  //�ұ�
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //��֧�Γ�����
                #endregion

                #region ̎������Y��
                //�Ȍ������Y���D��Class����ȡ��
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//�Ӱ�Ε����Y��

                //�޸Ĺ�˾�e�Y��
                DocDataClass.coin = tStrcoin;

                //�Ӱ��Class�D��JSON
                string tDocdataJSON = Utility.JSONSerialize(DocDataClass);

                //�oֵ
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //̎��؂�
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

        #region Ԥ���Ŀ�_��
        public string GetPreAccno_OpenQuery(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������                        
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RDControl tRDpreAccno = new RDControl(new DataTable());  //Ԥ���Ŀ���
                #endregion

                #region ȡ�î����Y��
                string tCompany = DataTransferManager.GetControlsValue(pM2Pxml, "company");  //ȡ��˾��
                string tCenterid = DataTransferManager.GetControlsValue(pM2Pxml, "centerid");  //ȡԤ������
                string tSearch = DataTransferManager.GetControlsValue(pM2Pxml, "SearchCondition");  //�����ь��l��
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //�Zϵ
                #endregion

                #region ̎������Y��
                #region ȡ��Ԥ���Ŀ����Y��
                //�O������
                tparam.Clear();
                tparam.Add("company", tCompany.Split('-')[0]);       // ���빫˾�����
                tparam.Add("centerid", tCenterid);       // ����Ԥ�����Ĳ���

                //���÷���
                string tResponse = Utility.CallAPI(pM2Pxml, "PreAccno", tparam, out tErrorMsg);
                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);

                tResponse = "{\"data\" : " + tResponse + "}";                       //�����class��ʽ�Լ�����ȥ��
                BudgetAccno preAccnoClass = Utility.JSONDeserialize<BudgetAccno>(tResponse);    //�Dclass����ȡ��
                #endregion

                #region �oֵ
                //ȡ��Ԥ���Ŀ��table
                DataTable tpreAccnoTable = preAccnoClass.GetDataTable(tSearch, new string[] { "preAccno", "preAccnoDesc" });
                tpreAccnoTable.Columns.Add("preAccnoC");//    Control applyUser + C = �_���ؼ�Ҫ�@ʾ��ֵ
                for (int i = 0; i < tpreAccnoTable.Rows.Count; i++) { tpreAccnoTable.Rows[i]["preAccnoC"] = tpreAccnoTable.Rows[i]["preAccno"] + "-" + tpreAccnoTable.Rows[i]["preAccnoDesc"]; }//�O���@ʾ��ֵ

                //�O��Ԥ���Ŀ����Y��(�����)
                Utility.SetRDControlPage(pM2Pxml, tpreAccnoTable, ref tRDpreAccno);
                #endregion
                #endregion

                #region �O���������
                //Ԥ���Ŀ��Ό���
                tRDpreAccno.AddCustomTag("DisplayColumn", "preAccno��preAccnoDesc��preAccnoC");
                tRDpreAccno.AddCustomTag("StructureStyle", "preAccno:R1:C1:1��preAccnoDesc:R1:C2:1");

                //�O�����Zϵ
                if ("Lang01".Equals(tStrLanguage))
                {
                    //���w
                    tRDpreAccno.AddCustomTag("ColumnsName", "��̖�����Q��Ԥ���Ŀ");
                }
                else
                {
                    //���w
                    tRDpreAccno.AddCustomTag("ColumnsName", "���š����ơ�Ԥ���Ŀ");
                }
                #endregion

                //̎��؂�
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
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);       //��֧�Γ��[�ؙ�λ
                #endregion

                #region ȡ�î����Y��
                string tStrpreAccnoC = DataTransferManager.GetControlsValue(pM2Pxml, "preAccnoC");  // Ԥ���Ŀ��λ
                string tStrDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");  //��֧�Γ�����
                #endregion

                #region ̎������Y��
                //�Ȍ������Y���D��Class����ȡ��
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tStrDocData);//�Ӱ�Ε����Y��

                //�޸�Ԥ���Ŀ�Y��
                DocDataClass.preAccno = tStrpreAccnoC;

                //�Ӱ��Class�D��JSON
                string tDocdataJSON = Utility.JSONSerialize(DocDataClass);

                //�oֵ
                tRCPDocData.Value = tDocdataJSON;
                #endregion

                //̎��؂�
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

        //���ñ�� ������
        public string GetLoanTotal_OnBlur(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m
            #region �O���ؼ�

            RCPControl tRCPloanTotal = new RCPControl("loanTotal", "false", "true", null);
            RCPControl tRCPtrafficfee = new RCPControl("trafficfee", "true", null, null);    //��ͨ��
            RCPControl tRCPaccommodation = new RCPControl("accommodation", "true", null, null);    //ס�޷�
            RCPControl tRCPallowance = new RCPControl("allowance", "true", null, null);    //�����
            RCPControl tRCPentertain = new RCPControl("entertain", "true", null, null);    //�д���
            RCPControl tRCPother = new RCPControl("other", "true", null, null);    //����
            RCPControl tRCPDocData = new RCPControl("DocData", null, null, null);
            #endregion

            #region ȡ�î����Y��
            string tTrafficfee = DataTransferManager.GetControlsValue(pM2Pxml, "trafficfee");  //��ͨ��   
            string tAccommodation = DataTransferManager.GetControlsValue(pM2Pxml, "accommodation");  //ס�޷�   
            string tAllowance = DataTransferManager.GetControlsValue(pM2Pxml, "allowance");  //�����
            string tEntertain = DataTransferManager.GetControlsValue(pM2Pxml, "entertain");        //�д���
            string tOther = DataTransferManager.GetControlsValue(pM2Pxml, "other");  //��������
            string tLoanTotal = DataTransferManager.GetDataValue(pM2Pxml, "loanTotal");        //�ϼ�
            string tDocData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");
            #endregion
            try
            {
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(tDocData);

                tLoanTotal = (double.Parse(tTrafficfee) + double.Parse(tAccommodation) + double.Parse(tAllowance) + double.Parse(tOther) + double.Parse(tEntertain)).ToString();
                tRCPloanTotal.Value = tLoanTotal;
                //Class�D��JSON
                DocDataClass.loanTotal = double.Parse(tLoanTotal);
                DocDataClass.trafficfee = double.Parse(tTrafficfee);
                DocDataClass.accommodation = double.Parse(tAccommodation);
                DocDataClass.allowance = double.Parse(tAllowance);
                DocDataClass.entertain = double.Parse(tEntertain);
                DocDataClass.other = double.Parse(tOther);

                string JSONStr = Utility.JSONSerialize(DocDataClass);
                //�oֵ
                tRCPDocData.Value = JSONStr;
                //̎��؂�
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

        #region ��������
        public string InvokeProcess(XDocument pM2Pxml)
        {
            string tP2Mxml = string.Empty;//�؂�ֵ
            ProductToMCloudBuilder tP2MObject = new ProductToMCloudBuilder(null, null, null); //����:Ҫ�@ʾ��ӍϢ���Y����Title//����p2m

            try
            {
                #region �O������
                Dictionary<string, string> tparam = new Dictionary<string, string>();//����API�ą���
                string tErrorMsg = string.Empty;        //�z���Ƿ��e�`
                #endregion

                #region �O���ؼ�
                RCPControl tRCPbtnSave = new RCPControl("btnSave", null, null, null);       //�͆�
                RCPControl tRCPcompany = new RCPControl("company", "false", null, null);     //��˾�e
                RCPControl tRCPapplyDate = new RCPControl("loanDate", "false", null, null);//��֧����
                RCPControl tRCPuserid = new RCPControl("loanUser", "false", null, null);      //�����
                RCPControl tRCPdeptno = new RCPControl("loanDept", "false", null, null);    //����
                RCPControl tRCPloanproperty = new RCPControl("loanproperty", "true", null, null);    //�������
                RCPControl tRCPtrafficfee = new RCPControl("trafficfee", "true", null, null);    //��ͨ��
                RCPControl tRCPaccommodation = new RCPControl("accommodation", "true", null, null);    //ס�޷�
                RCPControl tRCPallowance = new RCPControl("allowance", "true", null, null);    //�����
                RCPControl tRCPentertain = new RCPControl("entertain", "true", null, null);    //�д���
                RCPControl tRCPother = new RCPControl("other", "true", null, null);    //����
                RCPControl tRCPcoin = new RCPControl("coin", "true", null, null);    //�ұ�
                RCPControl tRCPloanTotal = new RCPControl("loanTotal", "false", null, null);    //����ܶ�
                RCPControl tRCPpreAccno = new RCPControl("preAccno", "true", null, null);    //Ԥ���Ŀ
                RCPControl tRCPprePayDate = new RCPControl("prePayDate", "true", null, null);    //Ԥ�Ƹ�������
                RCPControl tRCPcenterid = new RCPControl("centerid", "true", null, null);    //Ԥ������
                RCPControl tRCPreason = new RCPControl("reason", "true", null, null);    //����
                RCPControl tRCPdocData = new RCPControl("DocData", null, null, null);     //����ݴ�����
                #endregion

                #region ȡ�î����Y��
                string tStrUserID = DataTransferManager.GetUserMappingValue(pM2Pxml, "HANBELL");    //�����ߎ�̖
                string tStrLanguage = DataTransferManager.GetDataValue(pM2Pxml, "Language");        //�Zϵ
                string company = DataTransferManager.GetControlsValue(pM2Pxml, "company");      //��˾�e
                string loanDate = DataTransferManager.GetControlsValue(pM2Pxml, "loanDate");        //��֧����
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
                string docData = DataTransferManager.GetControlsValue(pM2Pxml, "DocData");      //��֧�Γ�����

                //�Ȍ������Y���D��Class����ȡ��
                Entity DocDataClass = Utility.JSONDeserialize<Entity>(docData);                //��֧�Ε����Y��
                #endregion

                #region �z���e�`
                //�z�鹫˾�e����
                if (string.IsNullOrEmpty(company))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //���w
                        tErrorMsg += "��˾�e����\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "��˾�����\r\n";
                    }
                }

                //�z�����
                if (string.IsNullOrEmpty(loanDept))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //���w
                        tErrorMsg += "��Ո���T����\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "���벿�ű���\r\n";
                    }
                }

                //�z�����
                if (string.IsNullOrEmpty(loanproperty))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //���w
                        tErrorMsg += "������ʱ���\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "������ʱ���\r\n";
                    }
                }

                //�z�����
                if (string.IsNullOrEmpty(coin))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //���w
                        tErrorMsg += "�ұ����\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "�ұ����\r\n";
                    }
                }

                //�z�����
                if (string.IsNullOrEmpty(preAccno))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //���w
                        tErrorMsg += "Ԥ���Ŀ����\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "Ԥ���Ŀ����\r\n";
                    }
                }

                //�z�����
                if (string.IsNullOrEmpty(prePayDate))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //���w
                        tErrorMsg += "Ԥ�Ƹ������ڱ���\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "Ԥ�Ƹ������ڱ���\r\n";
                    }
                }

                //�z�����
                if (string.IsNullOrEmpty(centerid))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(centerid))
                    {
                        //���w
                        tErrorMsg += "δ�ҵ����Ŷ�ӦԤ������\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "δ�ҵ����Ŷ�ӦԤ������\r\n";
                    }
                }

                //�z�����
                if (string.IsNullOrEmpty(reason))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(reason))
                    {
                        //���w
                        tErrorMsg += "����д��֧����\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "����д��֧����\r\n";
                    }
                }

                //�z������Ƿ����
                if (string.IsNullOrEmpty(docData))
                {
                    //�O�����Zϵ
                    if ("Lang01".Equals(tStrLanguage))
                    {
                        //���w
                        tErrorMsg += "Ո�_�������Y���Ƿ����\r\n";
                    }
                    else
                    {
                        //���w
                        tErrorMsg += "��ȷ�����������Ƿ����\r\n";
                    }
                }
                try
                {

                    if (Double.Parse(loanTotal) <= 0)
                    {
                        tErrorMsg += "����ܽ��������0";
                    }

                }
                catch (Exception ex)
                {
                    tErrorMsg += "����ܽ���ʽ����";
                }

                if (!string.IsNullOrEmpty(tErrorMsg)) return Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                #endregion

                #region ��ʽ
                /*
                response
                {"code":"200","msg":"PKG_HK_GL03400000066"}
                */
                #endregion

                //���^
                //DocDataClass.company = DocDataClass.company.Split('-')[0];    //��˾�eֻȡid
                DocDataClass.company = DocDataClass.company;
                DocDataClass.loanUser = DocDataClass.loanUser.Split('-')[0];              //��Ո��ֻȡid
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

                #region ȡ��Response
                //����API
                string uri = string.Format("{0}{1}?{2}", LoadConfig.GetWebConfig("APIURI"), "efgp/hzcw017/create", LoadConfig.GetWebConfig("APIKey"));
                string tBodyContext = Utility.JSONSerialize(DocDataClass);

                string tResponse = Utility.InvokeProcess(uri, tBodyContext, out tErrorMsg);
                #endregion

                #region ̎������Y��
                if (!string.IsNullOrEmpty(tErrorMsg))
                {
                    Utility.ReturnErrorMsg(pM2Pxml, ref tP2MObject, tErrorMsg);
                }
                else
                {
                    //�D��class ����ȡ��
                    ResponseState rs = Utility.JSONDeserialize<ResponseState>(tResponse);

                    //�Д�؂�
                    if ("200".Equals(rs.code))
                    {
                        string tMsg = string.Empty;

                        //�O�����Zϵ
                        if ("Lang01".Equals(tStrLanguage))
                        {
                            //���w
                            tMsg = "��֧�������ɹ�����̖ :\r\n";
                        }
                        else
                        {
                            //���w
                            tMsg = "��֧�������ɹ������� :\r\n";
                        }

                        tP2MObject.Message = tMsg + rs.msg;
                        tP2MObject.Result = "false";

                        ////�O����ن��Y��
                        //Entity newEntity = new Entity(company, "", tStrUserID, applyDept, formType, formTypeDesc, "1", "���ݼ�");
                        //newEntity.workType = "1";
                        //newEntity.workTypeDesc = "���հࣺ80��00";
                        //string DocDataStr = Utility.JSONSerialize(newEntity);//Class�D��JSON

                        ////�o�������λֵ
                        //tRCPstartDate.Value = "";
                        //tRCPendDate.Value = "";
                        //tRCPapplyDay.Value = "0";
                        //tRCPapplyHour.Value = "0";
                        //tRCPreason.Value = "";
                        //tRCPdocData.Value = DocDataStr;


                        #region �oֵ
                        //�O����֧���Y��
                        Entity newEntity = new Entity(company, loanDate, tStrUserID, loanDept, DocDataClass.loanproperty, DocDataClass.loanpropertyDesc, DocDataClass.trafficfee, DocDataClass.accommodation, DocDataClass.allowance, DocDataClass.entertain, DocDataClass.other,
                                                        DocDataClass.coin, DocDataClass.centerid);
                        string DocDataStr = Utility.JSONSerialize(newEntity);//Class�D��JSON

                        //�o�������λֵ
                        tRCPtrafficfee.Value = "0.00";
                        tRCPaccommodation.Value = "0.00";
                        tRCPallowance.Value = "0.00";
                        tRCPentertain.Value = "0.00";
                        tRCPother.Value = "0.00";
                        tRCPcoin.Value = "RMB-�����";
                        tRCPpreAccno.Value = "";
                        tRCPloanTotal.Value = "";
                        tRCPprePayDate.Value = "";
                        tRCPcenterid.Value = centerid;
                        tRCPreason.Value = "";
                        tRCPdocData.Value = DocDataStr;     //��֧�Ε����Y��

                        #endregion

                        //̎��؂�
                        tP2MObject.AddRCPControls(tRCPtrafficfee, tRCPaccommodation, tRCPallowance, tRCPentertain, tRCPother, tRCPcoin,
                         tRCPpreAccno, tRCPloanTotal, tRCPprePayDate, tRCPcenterid, tRCPreason, tRCPdocData);
                    }
                    else
                    {
                        tP2MObject.Message = "��֧������ʧ�ܣ�ԭ�� :\r\n" + rs.msg;
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


    //��֧��Class
    class Entity
    {

        public string company { get; set; }     //��˾�e
        public string loanDate { get; set; }        //�������
        public string loanUser { get; set; }          //��Ո��
        public string loanDept { get; set; }      //��Ո���T
        public string loanproperty { get; set; }    //��֧����code
        public string loanpropertyDesc { get; set; }//��֧����name
        public double trafficfee { get; set; }    //��ͨ�� 
        public double accommodation { get; set; }//ס�޷�
        public double allowance { get; set; }    //�����
        public double entertain { get; set; }   //�д���   
        public double other { get; set; }      //��������
        public string coin { get; set; }      //�ұ�
        public double loanTotal { get; set; }      //����ܶ�
        public string preAccno { get; set; }    //Ԥ���Ŀ
        public string prePayDate { get; set; }  //Ԥ�Ƹ�����
        public string centerid { get; set; }  //Ԥ������

        //public double leaveDay { get; set; }
        //public double leaveHour { get; set; }
        //public double leaveMinute { get; set; }

        public string reason { get; set; }

        public Entity()
        {
            this.company = "C-�Ϻ�����";
            this.loanDate = "";
            this.loanUser = "";
            this.loanDept = "";
            this.loanproperty = "0";
            this.loanpropertyDesc = "����Ӫҵ";
            this.trafficfee = 0.00;
            this.accommodation = 0.00;
            this.allowance = 0.00;
            this.entertain = 0.00;
            this.other = 0d;
            this.coin = "RMB-�����";
            this.loanTotal = 0;
            this.preAccno = "";
            this.prePayDate = "";
            //this.formKind = "1";
            //this.formTypeDesc = "���ݼ�";
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

    //��֧����
    class LoanProperty
    {
        public List<KV> data;

        public LoanProperty()
        {
            data = new List<KV>();
        }

        public DataTable GetDataTable(string pSearch, string[] columns)
        {
            Dictionary<string, string[]> item = new Dictionary<string, string[]>();  //ȡ��<��λ/ֵ, ���ֵ>

            DataTable table = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool tInsertData = false;               //�Ƿ�insert
                item = this.data[i].GetItem(); //ȡ�ý�֧�����Y��

                //��һ���M��Ҫ������λ
                if (i == 0) foreach (var col in columns) table.Columns.Add(col);

                //�z���ь�
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //���ь��ֶβ���̎���ь�
                    foreach (var value in item["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//�ҵ�������
                            break;
                        }
                    }
                }

                //�����Y��
                if (tInsertData) table.Rows.Add(item["Values"]);
            }

            return table;
        }
    }

}