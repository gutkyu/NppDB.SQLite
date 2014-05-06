﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NppDB.SQLite
{
    public partial class frmSQLiteConnect : Form
    {
        public frmSQLiteConnect()
        {
            InitializeComponent();
        }

        public frmSQLiteConnect(SQLiteConnect connect): this()
        {
            ServerAddress = connect.ServerAddress;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string ServerAddress { get; set; }

        public bool IsNew { get { return rdoNew.Checked; } set { rdoNew.Checked = value; } }

        private void btnOK_Click(object sender, EventArgs e)
        {
            
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            
        }

    }
}
