using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cn.hanbell.mcloud
{
    public class KV
    {
        public string k { get; set; }
        public string v { get; set; }

        public KV()
        {

        }

        public KV(string k, string v)
        {
            this.k = k;
            this.v = v;
        }
        //取得资料
        public Dictionary<string, string[]> GetItem()
        {
            Dictionary<string, string[]> Items = new Dictionary<string, string[]>();

            //需要的
            Items.Add("Columns", new string[] { "k", "v" });
            Items.Add("Values", new string[] { this.k, this.v });

            return Items;
        }
    }
}