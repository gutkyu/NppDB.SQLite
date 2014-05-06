using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Windows.Forms;

namespace NppDB.SQLite
{
    public class SQLiteTempTableGroup : SQLiteTableGroup
    {
        public SQLiteTempTableGroup()
        {
            this.Text = "Temp Tables";
        }
        protected override string QueryString
        {
            get
            {
                return "select name, tbl_name from sqlite_temp_master where type='table' ";
            }
        }

        protected override TreeNode CreateTreeNode(System.Data.DataRow dataRow)
        {
            var vw = new SQLiteTempTable();
            vw.Title = dataRow["name"].ToString();
            return vw;
        }
    }
}
