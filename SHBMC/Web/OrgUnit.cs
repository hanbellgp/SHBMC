using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{
    public class OrgUnit
    {
        public string id { get; set; }                      //部门代号organizationUnit.id
        public string objectVersion { get; set; }
        public string oid { get; set; }
        public string organizationOID { get; set; }
        public string organizationUnitName { get; set; }    //部门名称organizationUnit.organizationUnitName
        public string organizationUnitType { get; set; }
        public string superUnitOID { get; set; }
        public string validType { get; set; }
        public Manager manager { get; set; }

        public OrgUnit()
        {
            this.manager = new Manager();
        }

        public Dictionary<string, string[]> GetItem()
        {
            Dictionary<string, string[]> Items = new Dictionary<string, string[]>();
            //需要的
            Items.Add("Columns", new string[] { "id", "organizationUnitName" });
            Items.Add("Values", new string[] { this.id, this.organizationUnitName });

            return Items;
        }
    }
}