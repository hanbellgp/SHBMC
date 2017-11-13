using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{
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
}