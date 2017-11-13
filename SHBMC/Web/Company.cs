using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{
    public class Company
    {
        public List<CompanyItem> CompanyItems { get; set; }

        public Company()
        {
            this.CompanyItems = new List<CompanyItem>();
        }

        //回傳公司別table
        public DataTable GetCompanyTable(string pSearch)
        {
            Dictionary<string, string[]> companyItems = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable CompanyTable = new DataTable();
            for (int i = 0; i < this.CompanyItems.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                companyItems = this.CompanyItems[i].GetCompanyItem(); //取得公司別資料

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
}