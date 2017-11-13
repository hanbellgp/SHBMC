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
                    #region
                    case "HKGL004"://

                        switch (tServiceName)
                        {
                            case "BasicSetting":
                                tP2Mxml = new HKGL004().GetHKGL004_BasicSetting(tM2PXDoc);
                                break;
                            case "openQueryCompany":          //公司別開窗
                                tP2Mxml = new HKGL004().GetOpenQueryCompany(tM2PXDoc);
                                break;
                            case "formType_OnBlur":    //加班類別異動
                                tP2Mxml = new HKGL004().GetFormType_OnBuler(tM2PXDoc);
                                break;
                            case "openQueryApplyDept":          //公司別開窗
                                tP2Mxml = new HKGL004().GetOpenQueryApplyDept(tM2PXDoc);
                                break;
                            case "applyDept_OnBlur":
                                tP2Mxml = new HKGL004().GetApplyDept_OnBlur(tM2PXDoc);
                                break;
                            case "openQueryLeaveKind":          //公司別開窗
                                tP2Mxml = new HKGL004().GetOpenQueryLeaveKind(tM2PXDoc);
                                break;
                            case "formKind_OnBlur":
                                tP2Mxml = new HKGL004().GetLeaveKind_OnBlur(tM2PXDoc);
                                break;
                            case "openQueryWorkType":          //公司別開窗
                                tP2Mxml = new HKGL004().GetOpenQueryWorkType(tM2PXDoc);
                                break;
                            case "workType_OnBlur":
                                tP2Mxml = new HKGL004().GetWorkType_OnBlur(tM2PXDoc);
                                break;
                            case "post":
                                tP2Mxml = new HKGL004().GetHKGL004_CreateDoc(tM2PXDoc);
                                break;
                        }
                        ProductToMCloudBuilder tProductToMCloudBuilder = new ProductToMCloudBuilder(null, "TRUE", "请假申请");
                        //將Status載入至tProductToMCloudBuilder
                        tProductToMCloudBuilder.AddStatus("enableall");
                        //將tProductToMCloudBuilder轉成XML
                        tResultXDoc = tProductToMCloudBuilder.ToDucument();
                        #endregion
                        break;
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