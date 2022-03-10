using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Data;

namespace DsiMq_IF.jobs
{
    public class syncProcInfo : IJob
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
        ILog log = LogManager.GetLogger(typeof(syncDeptInfo));



        // 디버깅할 경우에서 메인함수에서 직접 호출할 수 있음(rtCode가 0이면 Nodata, 1이면 전송완료, 2이면 전송실패)
        private void Msg(string msg, int ReturnCode)
        {
            string prc = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Process[] pro = Process.GetProcessesByName(prc);
            if (pro.Length > 0)
            {
                msg += ("|" + this.GetType().Name);
                msg += ("|" + ReturnCode);

                byte[] buff = System.Text.Encoding.Default.GetBytes(msg);

                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                cds.cbData = buff.Length + 1;
                cds.lpData = msg;

                SendMessage(pro[0].MainWindowHandle, WM_COPYDATA, 0, ref cds);
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            Run();
        }

        /// <summary>
        /// 내부DB Select하여 MQ전송하고 내부DB SENT_YN을 Y로 변경
        /// 사용자 DB특성에 맞게 수정
        /// </summary>
        public void Run()
        {
            string DBConn = Properties.Settings.Default.connectionString;
            string CustomerCode = Properties.Settings.Default.CustomerCode;

            UTIL.MssqlDB dbconn = new UTIL.MssqlDB(DBConn);

            try
            {
                using (DsiSpcMq.DsiSpcMq SpcCore = new DsiSpcMq.DsiSpcMq(
                                                                Properties.Settings.Default.ServerUrl.ToString(),
                                                                Properties.Settings.Default.NameSpace.ToString(),
                                                                Properties.Settings.Default.KeyName.ToString(),
                                                                Properties.Settings.Default.Key.ToString(),
                                                                Properties.Settings.Default.topicName.ToString(),
                                                                Properties.Settings.Default.standardKey.ToString(),
                                                                Properties.Settings.Default.measureKey.ToString(),
                                                                Properties.Settings.Default.StatKey.ToString(),
                                                                Properties.Settings.Default.BadKey.ToString()))
                {

                    bool isok = false;

                    dbconn.sqlConnection();
                    // 프로시저 수정(동봉된 프로시저 스키마 참조)★★★
                    dbconn.CmdText = UTIL.DBQuery.ProccessInfoProc;
                    dbconn.cmdType = System.Data.CommandType.StoredProcedure;
                    dbconn.Store_Param("@IN_SENT", SqlDbType.Char, "N");

                    DataSet ds = dbconn.sqlCommandQuery();

                    if (ds.Tables[0].Rows.Count == 0 || ds == null)
                    {
                        Msg("Nodata", 0);
                        return;
                    }

                    isok = SpcCore.syncProcInfo(CustomerCode, ds.Tables[0]);

                    if (isok)
                    {
                        dbconn.Clear_Param();
                        dbconn.Store_Param("@IN_SENT", SqlDbType.Char, "S");

                        dbconn.sqlExcuteNonQuery();

                        // 로그에 전송한 IDXkey남기기(전송은 됐으나 두산 db에 저장되지않은 경우의 디버깅 용도로 추가함
                        List<string> s = ds.Tables[0].AsEnumerable().Select(x => x["idxKey"].ToString()).ToList();

                        string idxkey = string.Empty;

                        for (int i = 0; i < s.Count; i++)
                        {
                            if (i == 0)
                            {
                                idxkey = s[0];
                            }
                            else
                            {
                                idxkey += "," + s[i];
                            }
                        }

                        log.Debug("공정정보 " + ds.Tables[0].Rows.Count + "건 전송완료 IDXKEY : (" + idxkey + ")");
                        Msg("공정정보 " + ds.Tables[0].Rows.Count + "건 전송완료", 1);
                    }
                }
            }
            catch (Exception ie)
            {
                log.Error(ie.Message);
                Msg(ie.Message, 2);
            }
            finally
            {
                dbconn.sqlConnClose(0);
            }
        }
    }
}
