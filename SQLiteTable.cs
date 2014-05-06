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
    public class SQLiteTable : TreeNode, IRefreshable, IMenuProvider, ISQLiteObject
    {
        public SQLiteTable()
        {
            this.SelectedImageKey = this.ImageKey = "Table";
        }
        public string Title { set { this.Text = value; } get { return this.Text; } }
        public string FullName { get { return this.Title; } }

        protected virtual string QueryString
        {
            get
            {
                return "pragma " + this.Parent.Parent.Text + ".table_info('" + this.FullName + "')";
            }
        }

        public virtual void Refresh()
        {
            var conn = (SQLiteConnect)this.Parent.Parent.Parent;
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
                    int maxLen = -1;
                    if (dt.Rows.Count > 0)
                    {
                        this.Nodes.Clear();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            var col = new SQLiteColumnInfo(row["name"].ToString(), row["type"].ToString());
                            maxLen = Math.Max(col.ColumnName.Length, maxLen);
                            this.Nodes.Add(col);
                        }
                        if (maxLen > -1)
                            foreach (var colInfo in this.Nodes)
                                ((SQLiteColumnInfo)colInfo).AdjustColumnNameFixedWidth(maxLen);
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
            var menuList = new ContextMenuStrip(){ShowImageMargin= false};
            var connect = GetDBConnect();
            if (connect != null && connect.CommandHost !=null )
            {
                var host = connect.CommandHost;
                menuList.Items.Insert(0, new ToolStripButton("Select ... Limit 100", null, (s, e) =>
                {
                    host.Execute(NppDBCommandType.NewFile, null);
                    var id = host.Execute(NppDBCommandType.GetActivatedBufferID, null);
                    var query = "Select * From " + this.FullName + " Limit 100 \n";
                    host.Execute(NppDBCommandType.AppendToCurrentView, new object[]{query});
                    host.Execute(NppDBCommandType.CreateResultView, new object[] { id, connect, connect.CreateSQLExecutor()  });
                    host.Execute(NppDBCommandType.ExecuteSQL, new object[] { id, query });
                }));
                menuList.Items.Insert(1, new ToolStripSeparator());
            }
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));
            return menuList;
        }

        private SQLiteConnect GetDBConnect()
        {
            var connect = this.Parent.Parent.Parent as SQLiteConnect;
            return connect;
        }
    }
}
