using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Xml.Serialization;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.SQLite
{
    [XmlRoot]
    [ConnectAttr(Id = "SQLiteConnect", Title = "SQLite")]
    public class SQLiteConnect : TreeNode, IDBConnect, IRefreshable, IMenuProvider, IIconProvider, INppDBCommandClient
    {

        //only one
        [XmlElement]
        public string Title { set { this.Text = value; } get { return this.Text;} }
        //file path
        [XmlElement]
        public string ServerAddress { set; get; }

        public string Account { get; set; }

        [XmlIgnore]
        public string Password { set; get; }

        private SQLiteConnection _conn = null;

        public string GetDefaultTitle()
        {
            return string.IsNullOrEmpty(ServerAddress) ? "" : System.IO.Path.GetFileName(ServerAddress);
        }

        public System.Drawing.Bitmap GetIcon()
        {
            //return Properties.Resources.Sqlite;
            return Properties.Resources.Sqlite16x16;
        }

        public bool CheckLogin()
        {
            this.ToolTipText = this.ServerAddress;
            bool shownPwd = false;
            if (string.IsNullOrEmpty(this.ServerAddress) || !System.IO.File.Exists(this.ServerAddress))
            {
                if (!string.IsNullOrEmpty(this.ServerAddress) &&
                    (MessageBox.Show("file(" + this.ServerAddress + ") don't existed.\ncreate a new database?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    )
                    return false;

                frmSQLiteConnect dlg = new frmSQLiteConnect();
                dlg.ServerAddress = this.ServerAddress;

                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return false;

                FileDialog fdlg = null;
                shownPwd = dlg.IsNew;
                if (dlg.IsNew)
                {
                    fdlg = new SaveFileDialog();
                    fdlg.Title = "New SQLite File";
                }
                else
                {
                    fdlg = new OpenFileDialog();
                    fdlg.Title = "Open SQLite File";
                }
                    if (!string.IsNullOrEmpty(this.ServerAddress))
                        fdlg.InitialDirectory = System.IO.Path.GetDirectoryName(this.ServerAddress);
                    fdlg.AddExtension = false;
                    fdlg.DefaultExt = ".*";
                    fdlg.Filter = "All Files(*.*)|*.*";
                    var result = fdlg.ShowDialog();
                    if (result != System.Windows.Forms.DialogResult.OK) return false;
                    this.ServerAddress = fdlg.FileName;
            }
            var pdlg = new frmPassword() { VisiblePassword = shownPwd };
            if (pdlg.ShowDialog() != DialogResult.OK) return false;
            this.Password = pdlg.Password;
            
            return true;
        }

        public void Connect()
        {
            if (_conn == null) _conn = new SQLiteConnection();
            string curConnStr = GetConnectionString();
            if (_conn.ConnectionString != curConnStr) _conn.ConnectionString = curConnStr;
            if (_conn.State != System.Data.ConnectionState.Open)
            {
                try { _conn.Open(); }
                catch (Exception ex)
                {
                    throw new ApplicationException("connect fail", ex);
                }
            }
        }

        public void Disconnect()
        {
            if (_conn == null) return;
            if (_conn.State != System.Data.ConnectionState.Closed) _conn.Close();
        }

        public bool IsOpened { get { return _conn != null && _conn.State == System.Data.ConnectionState.Open; } }

        internal SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(GetConnectionString());
        }

        internal ISQLExecutor CreateSQLExecutor() { return new SQLiteExecutor(GetConnection); }

        public void Refresh()
        {
            using (var conn = GetConnection())
            {
                this.TreeView.Cursor = Cursors.WaitCursor;
                this.TreeView.Enabled = false;
                try
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand("pragma database_list", _conn);
                    var reader = cmd.ExecuteReader();
                    var dt = new System.Data.DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        this.Nodes.Clear();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            string nm = row["name"].ToString();
                            string file = row["file"].ToString() ;
                            TreeNode db = null;
                            if (nm == "temp")
                            {
                                db = new SQLiteTempDatabase() { Title = nm, File = file, ToolTipText = file };
                            }
                            else
                            {
                                db = new SQLiteDatabase() { Title = nm, File = file, ToolTipText = file };
                            }
                            this.Nodes.Add(db);
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

        public ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            var connect = this as SQLiteConnect;
            if (connect != null && connect.CommandHost != null)
            {
                var host = connect.CommandHost;
                menuList.Items.Insert(0, new ToolStripButton("Open", null, (s, e) =>
                {
                    host.Execute(NppDBCommandType.NewFile, null);
                    var id = host.Execute(NppDBCommandType.GetActivatedBufferID, null);
                    host.Execute(NppDBCommandType.CreateResultView, new object[] { id, connect, CreateSQLExecutor() });
                }));
                menuList.Items.Insert(1, new ToolStripSeparator());
            }
            menuList.Items.Add(new ToolStripButton("Connect", null, (s, e) =>
            {
                if (this.IsOpened || this.Password != null) return;
                if (!CheckLogin()) return;
                try
                {
                    Connect();
                    Refresh();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + (ex.InnerException != null ? " : " + ex.InnerException.Message : ""));
                    return;
                }
            }) { Enabled = !this.IsOpened });
            menuList.Items.Add(new ToolStripButton("Disconnect", null, (s, e) => { Disconnect(); }) { Enabled = this.IsOpened });
            menuList.Items.Add(new ToolStripSeparator());
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }

        internal string GetConnectionString()
        {
            var cSBuilder = new SQLiteConnectionStringBuilder();
            cSBuilder.DataSource = ServerAddress;
            if(!string.IsNullOrEmpty(Password)) cSBuilder.Password = Password;

            return cSBuilder.ConnectionString;
        }

        public void Reset()
        {
            Title = ""; ServerAddress = ""; Password = "";;
            Disconnect();
            _conn = null;
        }


        internal INppDBCommandHost CommandHost { get; private set; }

        public void SetCommandHost(INppDBCommandHost host)
        {
            CommandHost = host;
        }
    }

}
