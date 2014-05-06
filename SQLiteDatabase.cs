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
    public class SQLiteDatabase : TreeNode, IRefreshable, IMenuProvider
    {
        public SQLiteDatabase()
        {
            this.SelectedImageKey = this.ImageKey = "Database";
            Refresh();
        }

        public string Title { set { this.Text = value; } get { return this.Text; } }
        public string File { set; get; }

        public virtual void Refresh()
        {
            this.Nodes.Clear();
            this.Nodes.Add(new SQLiteTableGroup());
            this.Nodes.Add(new SQLiteViewGroup());
            //this.Nodes.Add(new SQLiteIndexGroup());
            //this.Nodes.Add(new SQLiteTriggerGroup());
        }

        public virtual ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }
    }
}
