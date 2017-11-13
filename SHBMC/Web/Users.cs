using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{

    #region 員工明細
    /*JSON樣式
    {
	    "company": "C",
	    "cost": 0,
	    "deptname": "维护组",
	    "applyDept": "13120",
	    "enableSubstitute": 1,
	    "applyUser": "C0160",
	    "identificationType": "DEFAULT",
	    "ldapid": "DongTY",
	    "localeString": "zh_CN",
	    "mailAddress": "dtianyu@hanbell.cn",
	    "mailingFrequencyType": 0,
	    "objectVersion": 15,
	    "oid": "3a6b1714dbbb10048c65af58217aa75e",
	    "password": "s//RxFNKLw5LqwI3D34/DWUdH1w=",
	    "phoneNumber": "65299",
	    "title": {
		    "objectVersion": 1,
		    "occupantOID": "3a6b1714dbbb10048c65af58217aa75e",
		    "oid": "6dfa8be2dd29100482e520de6fdbb7f2",
		    "organizationUnitOID": "3aab5782dbbb10048c65af58217aa75e",
		    "titleDefinition": {
			    "description": "C",
			    "objectVersion": 1,
			    "oid": "6d88eb92dd29100482e520de6fdbb7f2",
			    "organizationOID": "3a8a4bb8dbbb10048c65af58217aa75e",
			    "titleDefinitionName": "C"
		    }
	    },
	    "userName": "董天雨",
	    "workflowServerOID": "00000000000000WorkflowServer0001"
    }
    */
    #endregion

    public class Users
    {
        public string company { get; set; }             //默认公司company
        public string cost { get; set; }
        public string deptname { get; set; }            //部门名称deptname
        public string deptno { get; set; }              //主要部门deptno
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
        public string userName { get; set; }            //员工姓名userName
        public string workflowServerOID { get; set; }
        public List<Title> title { get; set; }

        public Users()
        {
            this.title = new List<Title>();
        }
    }




}