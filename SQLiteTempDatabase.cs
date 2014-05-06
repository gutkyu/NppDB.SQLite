using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.SQLite
{
    public class SQLiteTempDatabase : SQLiteDatabase
    {

        public SQLiteTempDatabase()
        {
            Refresh();
        }

        public override void Refresh()
        {
            this.Nodes.Clear();
            this.Nodes.Add(new SQLiteTempTableGroup());
            this.Nodes.Add(new SQLiteTempViewGroup());
            //this.Nodes.Add(new SQLiteIndexGroup());
            //this.Nodes.Add(new SQLiteTriggerGroup());
        }

        public override ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }
    }
}
