using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{

    public class EmployeeDepartment
    {

        public List<EmployeeDepartmentItem> data { get; set; }

        public EmployeeDepartment()
        {
            data = new List<EmployeeDepartmentItem>();
        }

        //回傳部門table
        public DataTable GetTable(string pSearch)
        {
            Dictionary<string, string[]> DepartmentItems = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable DepartmentTable = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                DepartmentItems = this.data[i].organizationUnit.GetItem(); //取得organizationUnit資料

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in DepartmentItems["Columns"]) DepartmentTable.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in DepartmentItems["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//找到就跳出
                            break;
                        }
                    }
                }
                //新增資料
                if (tInsertData) DepartmentTable.Rows.Add(DepartmentItems["Values"]);
            }

            return DepartmentTable;
        }

    }

    public class EmployeeDepartmentItem
    {
        public string approvalLevelOID { get; set; }
        public string definitionOID { get; set; }
        public string isMain { get; set; }
        public string objectVersion { get; set; }
        public string occupantOID { get; set; }
        public string oid { get; set; }
        public string specifiedManagerOID { get; set; }
        public OrgUnit organizationUnit { get; set; }
        public Users users { get; set; }

        public EmployeeDepartmentItem()
        {
            this.organizationUnit = new OrgUnit();
            this.users = new Users();
        }
    }
}