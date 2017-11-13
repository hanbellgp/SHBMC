using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{
    public class LeaveKind
    {
        public List<KV> data;

        public LeaveKind()
        {
            data = new List<KV>();
        }

        public DataTable GetDataTable(string pSearch, string[] columns)
        {
            Dictionary<string, string[]> item = new Dictionary<string, string[]>();  //取得<欄位/值, 陣列值>

            DataTable table = new DataTable();
            for (int i = 0; i < this.data.Count; i++)
            {
                bool tInsertData = false;               //是否insert
                item = this.data[i].GetItem(); //取得公司別資料

                //第一次進來要新增欄位
                if (i == 0) foreach (var col in columns) table.Columns.Add(col);

                //檢查搜尋
                if (string.IsNullOrEmpty(pSearch))
                {
                    tInsertData = true;
                }
                else
                {
                    //有搜尋字段才需處理搜尋
                    foreach (var value in item["Values"])
                    {
                        if (value.Contains(pSearch))
                        {
                            tInsertData = true;//找到就跳出
                            break;
                        }
                    }
                }

                //新增資料
                if (tInsertData) table.Rows.Add(item["Values"]);
            }

            return table;
        }
    }


}