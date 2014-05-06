using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Windows.Forms;

namespace NppDB.SQLite
{
    public class SQLiteTempViewGroup : SQLiteTempTableGroup
    {
        public SQLiteTempViewGroup()
        {
            this.Text = "Temp Views";
        }
        protected override string QueryString
        {
            get
            {
                return "select name, tbl_name from sqlite_temp_master where type='view' ";
            }
        }

    }
}
