using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{

    //Title
    public class Title
    {
        public string objectVersion { get; set; }
        public string occupantOID { get; set; }
        public string oid { get; set; }
        public string organizationUnitOID { get; set; }
        public List<TitleDefinition> titleDefinition { get; set; }

        public Title()
        {
            this.titleDefinition = new List<TitleDefinition>();
        }
    }
}