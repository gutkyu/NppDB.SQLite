using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using NppDB.Comm;

namespace NppDB.SQLite
{
    public class SQLiteExecutor : ISQLExecutor 
    {
        protected Func<SQLiteConnection> _connector = null;
        public SQLiteExecutor(Func<SQLiteConnection> connector)
        {
            _connector = connector;
            //_cmd = connector connection.CreateCommand();
        }

        private System.Threading.Thread _execTh = null;
        public virtual void Execute(string sqlQuery, Action<Exception,DataTable> callback)
        {
            _execTh = new System.Threading.Thread(new System.Threading.ThreadStart(
                delegate
                {
                    var dt = new DataTable();
                    try
                    {
                        using (var conn = _connector())
                        {
                            conn.Open();
                            var cmd = new SQLiteCommand(sqlQuery,conn);
                            var rd = cmd.ExecuteReader();
                            dt.Load(rd);
                        }
                    }
                    catch (Exception ex)
                    {
                        callback(ex,null);
                        return;
                    }
                    callback(null,dt);
                    _execTh = null;
                }));
            _execTh.IsBackground = true;
            _execTh.Start();
        }

        public virtual Boolean CanExecute()
        {
            return !CanStop();
        }

        public virtual void Stop()
        {
            if (!CanStop()) return;
            if(_execTh != null ) _execTh.Abort();
            _execTh = null;
        }

        public virtual bool CanStop()
        {
            return _execTh != null && (_execTh.ThreadState & System.Threading.ThreadState.Running) != 0;
        }

    }
}
