using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using com.digiwin.Mobile.MCloud.TransferLibrary;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using System.Data;
using System.Net;
using System.IO;
using System.Text;

namespace cn.hanbell.mcloud
{
    /// <summary>
    /// MCloudEFGP 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class MCloudEFGPAdapter : System.Web.Services.WebService
    {

        [WebMethod]
        public string ServiceEntryPoint(string pM2Pxml)
        {
            String tP2Mxml = String.Empty;
            XDocument tM2PXDoc = XDocument.Parse(pM2Pxml);

            //回傳P to M的XML
            XDocument tResultXDoc = new XDocument();

            try
            {
                //取得ProgramID
                string tProgramID = DataTransferManager.GetDataValue(tM2PXDoc, "ProgramID");
                //取得ServiceName
                string tServiceName = DataTransferManager.GetDataValue(tM2PXDoc, "ServiceName");
                //取得ProductUserID
                string tProductUserID = DataTransferManager.GetDataValue(tM2PXDoc, "ProductUserID");

                switch (tProgramID)
                {
                    #region 请假单
                    case "HKGL004":

                        switch (tServiceName)
                        {
                            case "BasicSetting":
                                tP2Mxml = new HKGL004.HKGL004().GetBasicSetting(tM2PXDoc);
                                break;
                            case "openQueryCompany":          //公司別開窗
                                tP2Mxml = new HKGL004.HKGL004().GetCompany_OpenQuery(tM2PXDoc);
                                break;
                            case "formType_OnBlur":    //加班類別異動
                                tP2Mxml = new HKGL004.HKGL004().GetFormType_OnBuler(tM2PXDoc);
                                break;
                            case "openQueryApplyDept":          //公司別開窗
                                tP2Mxml = new HKGL004.HKGL004().GetApplyDept_OpenQuery(tM2PXDoc);
                                break;
                            case "applyDept_OnBlur":
                                tP2Mxml = new HKGL004.HKGL004().GetApplyDept_OnBlur(tM2PXDoc);
                                break;
                            case "openQueryLeaveKind":          //公司別開窗
                                tP2Mxml = new HKGL004.HKGL004().GetLeaveKind_OpenQuery(tM2PXDoc);
                                break;
                            case "formKind_OnBlur":
                                tP2Mxml = new HKGL004.HKGL004().GetLeaveKind_OnBlur(tM2PXDoc);
                                break;
                            case "openQueryWorkType":          //公司別開窗
                                tP2Mxml = new HKGL004.HKGL004().GetWorkType_OpenQuery(tM2PXDoc);
                                break;
                            case "workType_OnBlur":
                                tP2Mxml = new HKGL004.HKGL004().GetWorkType_OnBlur(tM2PXDoc);
                                break;
                            case "post":
                                tP2Mxml = new HKGL004.HKGL004().InvokeProcess(tM2PXDoc);
                                break;
                        }
                        ProductToMCloudBuilder tProductToMCloudBuilder = new ProductToMCloudBuilder(null, "TRUE", "请假申请");
                        //將Status載入至tProductToMCloudBuilder
                        tProductToMCloudBuilder.AddStatus("enableall");
                        //將tProductToMCloudBuilder轉成XML
                        tResultXDoc = tProductToMCloudBuilder.ToDucument();
                        break;
                    #endregion

                    #region 加班申請單
                    case "HANBELL01":   //加班申請單
                        switch (tServiceName)
                        {
                            case "BasicSetting":        //頁面初始化
                            case "PageWebService":      //分頁
                            case "SearchWebService":    //搜尋
                                tP2Mxml = new HKGL034.HKGL034().GetBasicSetting(tM2PXDoc);
                                break;

                            case "formType_OnBlur":    //加班類別異動
                                tP2Mxml = new HKGL034.HKGL034().GetFormType_OnBuler(tM2PXDoc);
                                break;

                            case "Company_OP":          //公司別開窗
                                tP2Mxml = new HKGL034.HKGL034().GetCompany_OpenQuery(tM2PXDoc);
                                break;
                            case "company_OnBlur":   //公司別異動
                            case "company_OnClear":
                                tP2Mxml = new HKGL034.HKGL034().GetCompany_OnBlur(tM2PXDoc);
                                break;

                            case "deptno_OP":           //部門開窗
                                tP2Mxml = new HKGL034.HKGL034().GetDeptno_OpenQuery(tM2PXDoc);
                                break;
                            case "deptno_OnBlur":   //部門異動
                            case "deptno_OnClear":
                                tP2Mxml = new HKGL034.HKGL034().GetDeptno_OnBlur(tM2PXDoc);
                                break;

                            case "Del":     //刪除單身
                                tP2Mxml = new HKGL034.HKGL034().GetDetail01_Del(tM2PXDoc);
                                break;

                            case "CreateDoc": //立單
                                tP2Mxml = new HKGL034.HKGL034().InvokeProcess(tM2PXDoc);
                                break;
                        }
                        break;

                    case "HANBELL01_01": //加班申請單名細
                        switch (tServiceName)
                        {
                            case "BasicSetting":        //頁面初始化
                                tP2Mxml = new HKGL034.HKGL034().GetDetail01_BasicSetting(tM2PXDoc);
                                break;

                            case "deptno_OP":        //部門開窗
                                tP2Mxml = new HKGL034.HKGL034().GetDetail01_Deptno_OpenQuery(tM2PXDoc);
                                break;
                            case "deptno_OnBlur":   //部門異動
                            case "deptno_OnClear":
                                tP2Mxml = new HKGL034.HKGL034().GetDetail01_Deptno_OnBlur(tM2PXDoc);
                                break;

                            case "ids_OP":          //人員開窗
                                tP2Mxml = new HKGL034.HKGL034().GetDetail01_Employee_OpenQuery(tM2PXDoc);
                                break;

                            case "Add":             //新增單身
                            case "Edit":            //修改明細
                                tP2Mxml = new HKGL034.HKGL034().GetDetail01_Operate(tM2PXDoc);
                                break;
                        }
                        break;
                        #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return tP2Mxml;
        }
    }





}