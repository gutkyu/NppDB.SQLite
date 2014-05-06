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
    public class SQLiteTableGroup : TreeNode, IRefreshable, IMenuProvider
    {
        public SQLiteTableGroup()
        {
            this.Text = "Tables";
            this.SelectedImageKey = this.ImageKey = "Group";
        }

        public string Title { set { this.Text = value; } get { return this.Text; } }

        protected virtual string QueryString
        {
            get
            {
                return "select name, tbl_name from "+this.Parent.Text+".sqlite_master where type='table' ";
            }
        }

        protected virtual TreeNode CreateTreeNode(System.Data.DataRow dataRow)
        {
            var tbl = new SQLiteTable();
            tbl.Title = dataRow["name"].ToString();
            return tbl;
        }

        public void Refresh()
        {
            var conn = (SQLiteConnect)this.Parent.Parent;
            using (var cnn = conn.GetConnection())
            {
                this.TreeView.Cursor = Cursors.WaitCursor;
                this.TreeView.Enabled = false;
                try
                {
                    cnn.Open();
                    var cmd = new SQLiteCommand(QueryString, cnn);
                    var reader = cmd.ExecuteReader();
                    var dt = new System.Data.DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        this.Nodes.Clear();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            this.Nodes.Add(CreateTreeNode(row));
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception");
                    return;
                }
                finally
                {
                    this.TreeView.Enabled = true;
                    this.TreeView.Cursor = null;
                }
            }
        }

        public virtual ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }
    }
}
