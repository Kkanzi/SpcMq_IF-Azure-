using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DsiMq_IF.UTIL
{
    public class MssqlDB
    {
        const int WM_COPYDATA = 0x4A;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, ref COPYDATASTRUCT lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, ref COPYDATASTRUCT lParam);

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        ILog log = LogManager.GetLogger(typeof(MssqlDB));

        public SqlDataAdapter sqlData = new SqlDataAdapter();
        public SqlCommand sqlCmd = new SqlCommand();
        private string db_info = string.Empty;
        public MssqlDB(string Conn)
        {
            db_info = Conn;

        }

        private void Msg(string msg, int ReturnCode)
        {
            log.Info(msg);


            string prc = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Process[] pro = Process.GetProcessesByName(prc);
            if (pro.Length > 0)
            {
                byte[] buff = System.Text.Encoding.Default.GetBytes(msg);

                msg += ("|" + this.GetType().Name);
                msg += ("|" + ReturnCode);

                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                cds.cbData = buff.Length + 1;
                cds.lpData = msg;

                SendMessage(pro[0].MainWindowHandle, WM_COPYDATA, 0, ref cds);
            }
        }

        /// <summary>
        /// Sql Script / Stored Procedure Name
        /// </summary>
        public string CmdText
        {
            set
            {
                sqlCmd.CommandText = value;
            }
        }

        /// <summary>
        /// CommandType = CommandType....
        /// </summary>
        public CommandType cmdType
        {

            set { sqlCmd.CommandType = value; }
        }

        /// <summary>
        /// DataSet 요청
        /// </summary>
        /// <returns>DataSet</returns>
        public DataSet sqlCommandQuery()
        {
            //
            DataSet ds = new DataSet();
            try
            {
                sqlData.SelectCommand = sqlCmd;
                sqlCmd.Connection = new SqlConnection(db_info);
                sqlCmd.Connection.Open();

                sqlData.Fill(ds);

                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {

            }
            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int sqlExcuteNonQuery()
        {
            return sqlExcuteNonQuery(0);
        }

        public int sqlConnOpen(int Trns)
        {

            sqlData.SelectCommand = sqlCmd;
            sqlCmd.Connection = new SqlConnection(db_info);
            sqlCmd.Connection.Open();
            if (Trns > 0)
                sqlCmd.Transaction = sqlCmd.Connection.BeginTransaction(IsolationLevel.ReadCommitted);

            return 0;
        }

        public int sqlConnExecute()
        {
            return sqlCmd.ExecuteNonQuery();
        }

        public int sqlConnClose(int Trns)
        {
            if (Trns > 0)
                sqlCmd.Transaction.Commit();
            sqlCmd.Connection.Close();

            return 0;
        }

        public int sqlConnRollback()
        {
            sqlCmd.Transaction.Rollback();

            return 0;
        }


        public int sqlConnection()
        {
            sqlData.SelectCommand = sqlCmd;
            sqlCmd.Connection = new SqlConnection(db_info);
            sqlCmd.Connection.Open();
            return 0;
        }

        public int sqlConnection(string dbinfo)
        {
            sqlData.SelectCommand = sqlCmd;
            sqlCmd.Connection = new SqlConnection(dbinfo);
            sqlCmd.Connection.Open();
            return 0;
        }

        public void sqlClose()
        {
            sqlCmd.Connection.Close();
        }

        public void sqlBeginTrans()
        {
            sqlCmd.Transaction = sqlCmd.Connection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void sqlCommitTrans()
        {
            if (sqlCmd.Transaction != null)
                sqlCmd.Transaction.Commit();
        }

        public void sqlRollbackTrans()
        {
            if (sqlCmd.Transaction != null)
                sqlCmd.Transaction.Rollback();
        }

        public int sqlOpenedExcuteNonQuery()
        {
            return sqlCmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Sql Excute Command
        /// Transaction: 1 begin transaction
        /// Transaction: 0 No transaction
        /// </summary>
        /// <param name="Transaction">Transaction= 1 Or 0</param>
        /// <returns>Return Effective Rows</returns>
        public int sqlExcuteNonQuery(int Transaction)
        {
            try
            {
                sqlData.SelectCommand = sqlCmd;
                sqlCmd.Connection = new SqlConnection(db_info);
                sqlCmd.Connection.Open();
                if (Transaction > 0)
                    sqlCmd.Transaction = sqlCmd.Connection.BeginTransaction(IsolationLevel.ReadCommitted);

                int rtn = sqlCmd.ExecuteNonQuery();

                if (Transaction > 0)
                    sqlCmd.Transaction.Commit();
                sqlCmd.Connection.Close();

                return rtn;
            }
            catch (Exception ex)
            {

                if (Transaction > 0)
                {
                    sqlCmd.Transaction.Rollback();
                    ///    MessageBox.Show(ex.Message, "Sql StoredProcedure(sqlProcedure) Error");

                    //throw new Exception("Source : " + ex.Source + "\nMethod : " + ex.TargetSite + "\nMessage : " + ex.Message + "\n관리자에게 문의 하세요");
                }
                Msg(sqlCmd.CommandText + "\r\n" + ex.Message, 2);
                return -1;

            }
            finally
            {
            }
        }


        /// <summary>
        /// 쿼리명령어(Update/Insert/Delete)등 실행
        /// </summary>
        /// <param name="sQuery"></param>
        /// <returns>Return 1(Success) / 0(Fail)</returns>
        public int StrNonQuery(string sQuery)
        {
            int rtn = _StringNonQuery(sQuery, 0);

            return rtn;
        }

        /// <summary>
        /// 직접 쿼리결과를 가져오는 함수
        /// </summary>
        /// <param name="sQuery"></param>
        /// <returns></returns>
        public static DataSet StringQuery(string Conn, string sQuery)
        {
            return new MssqlDB(Conn)._StringQuery(sQuery);
        }
        /// <summary>
        /// 내부사용 쿼리 명령 실행기
        /// </summary>
        /// <param name="sQuery"></param>
        /// <returns></returns>
        private DataSet _StringQuery(string sQuery)
        {
            //
            DataSet ds = new DataSet();

            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.CommandText = sQuery;
            sqlCmd.Connection = new SqlConnection(db_info);
            try
            {
                sqlData.SelectCommand = sqlCmd;
                sqlCmd.Connection.Open();

                sqlData.Fill(ds);

                sqlCmd.Connection.Close();
                sqlCmd.Dispose();

                return ds;
            }
            catch (Exception ex)
            {
            }
            finally
            {

            }
            return null;
        }

        public static Boolean GetPopupDataTable(DataSet ds, out DataTable dt)
        {
            if (ds != null && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].GetChanges() != null && ds.Tables[0].GetChanges().Rows.Count > 0)
            {
                dt = ds.Tables[0].GetChanges();
                return true;
            }
            dt = null;
            return false;
        }

        /// <summary>
        /// 쿼리명령어(Update/Insert/Delete)등 실행
        /// </summary>
        /// <param name="sQuery"></param>
        /// <returns>Return 1(Success) / 0(Fail)</returns>
        public static int StringNonQuery(string Conn, string sQuery)
        {
            return new MssqlDB(Conn)._StringNonQuery(sQuery, 0);
        }
        /// <summary>
        /// 쿼리명령어(Update/Insert/Delete)등 실행
        /// </summary>
        /// <param name="sQuery"></param>
        /// <param name="Transaction">Transaction= 1 Or 0</param>
        /// <returns>Return 1(Success) / 0(Fail)</returns>
        public static int StringNonQuery(string Conn, string sQuery, int Transaction)
        {
            return new MssqlDB(Conn)._StringNonQuery(sQuery, Transaction);
        }
        /// <summary>
        /// Transaction: 1 begin transaction
        /// Transaction: 0 No transaction
        /// </summary>
        /// </summary>
        /// <param name="sQuery"></param>
        /// <param name="Transaction"></param>
        /// <returns>Return : 1 Success   -1 Fail</returns>
        private int _StringNonQuery(string sQuery, int Transaction)
        {
            //
            DataSet ds = new DataSet();

            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.CommandText = sQuery;
            sqlCmd.Connection = new SqlConnection(db_info);
            try
            {
                sqlData.SelectCommand = sqlCmd;
                sqlCmd.Connection.Open();
                if (Transaction > 0)
                    sqlCmd.Transaction = sqlCmd.Transaction = sqlCmd.Connection.BeginTransaction(IsolationLevel.ReadCommitted);

                int rtn = sqlCmd.ExecuteNonQuery();

                if (Transaction > 0)
                    sqlCmd.Transaction.Commit();
                sqlCmd.Connection.Close();
                sqlCmd.Dispose();

                return rtn;
            }
            catch (Exception ex)
            {
                if (Transaction > 0)
                    sqlCmd.Transaction.Rollback();
                return -1;
                //throw new Exception("Source : " + ex.Source + "\nMethod : " + ex.TargetSite + "\nMessage : " + ex.Message + "\n관리자에게 문의 하세요");

            }
            finally
            {
                //
            }
        }
        /// <summary>
        /// Parameter Clear
        /// </summary>
        public void Clear_Param()
        {
            sqlCmd.Parameters.Clear();
        }

        /// <summary>
        /// Store_Param(iSql, "@prc", SqlDbType.VarChar,"C");
        /// </summary>
        /// <param name="Param"></param>
        /// <param name="Db_Type"></param>
        /// <param name="values"></param>
        public void Store_Param(string Param, SqlDbType Db_Type, object values)
        {
            SqlParameter param1 = new SqlParameter(Param, Db_Type);
            param1.Value = values;
            sqlCmd.Parameters.Add(param1);
        }

        /// <summary>
        /// Store_Param(iSql, "@MSG_CD", SqlDbType.VarChar, "", ParameterDirection.Output);
        /// </summary>
        /// <param name="Param"></param>
        /// <param name="Db_Type"></param>
        /// <param name="values"></param>
        /// <param name="Param_io"></param>
        public void Store_Param(string Param, SqlDbType Db_Type, object values, ParameterDirection Param_io)
        {
            SqlParameter param1 = new SqlParameter(Param, Db_Type);
            if (Param_io == ParameterDirection.Input || Param_io == ParameterDirection.InputOutput)
                param1.Value = values;
            param1.Direction = Param_io;
            sqlCmd.Parameters.Add(param1);
        }

        /// <summary>
        /// Store_Param(iSql, "return_value", SqlDbType.Int , "", ParameterDirection.ReturnValue , 5); ---- return문
        /// Store_Param(iSql, "@MSG_TEXT", SqlDbType.VarChar, "", ParameterDirection.Output, 100);
        /// </summary>
        /// <param name="Param"></param>
        /// <param name="Db_Type"></param>
        /// <param name="values"></param>
        /// <param name="Param_io"></param>
        /// <param name="nSize"></param>
        public void Store_Param(string Param, SqlDbType Db_Type, object values, ParameterDirection Param_io, int nSize)
        {
            SqlParameter param1 = new SqlParameter(Param, Db_Type);
            if (Param_io == ParameterDirection.Input || Param_io == ParameterDirection.InputOutput)
                param1.Value = values;
            param1.Direction = Param_io;
            param1.Size = nSize;
            sqlCmd.Parameters.Add(param1);
        }

        /// <summary>
        /// sp의 OUTPUT 파라미터 값 읽기
        /// int(string) return = (int/string)Return_Param("@return");
        /// </summary>
        /// <param name="param">"@param"</param>
        /// <returns>object</returns>
        public object Return_Param(string param)
        {
            return sqlCmd.Parameters[param].Value;
        }
    }
}
