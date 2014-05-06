using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Windows.Forms;

namespace NppDB.SQLite
{
    public class SQLiteViewGroup : SQLiteTableGroup
    {
        public SQLiteViewGroup()
        {
            this.Text = "Views";
        }
        protected override string QueryString
        {
            get
            {
                return "select name, tbl_name from " + this.Parent.Text + ".sqlite_master where type='view' ";
            }
        }

    }
}
