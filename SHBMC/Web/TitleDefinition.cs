using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{
    //titleDefinitionName
    public class TitleDefinition
    {
        public string description { get; set; }
        public string objectVersion { get; set; }
        public string oid { get; set; }
        public string organizationOID { get; set; }
        public string titleDefinitionName { get; set; }

        public TitleDefinition() { }
    }
}