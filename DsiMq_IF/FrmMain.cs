using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DsiSpcMq;
using System.Net;
using System.Data.SqlClient;
using System.Data.SQLite;
using log4net.Config;
using System.Runtime.InteropServices;
using log4net;
using Quartz;
using Quartz.Impl;
using System.Threading;
using System.Reflection;
using System.Resources;
using System.Configuration;
namespace DsiMq_IF
{
    public partial class FrmMain : Form
    {
        #region LOG호출을 위한 시스템 내부 선언 (수정X)
        const int WM_COPYDATA = 0x4a;

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        ILog log = LogManager.GetLogger(typeof(FrmMain));

        // construct a scheduler factory
        private ISchedulerFactory schedFact = new StdSchedulerFactory();

        // get a scheduler
        private IScheduler sched = null;

        private bool isExit = false;

        // 디버깅 유무
        private bool debug = false;
        private bool[] trstart;
        Thread[] t;
        #endregion

        //sqlite DB파일이름 선언
        private const string SQLiteDbFile = "ScheInfo.dat";
        //Sqlite 파일위치 및 ConnectionString선언
        string ConnectionString = string.Format("Data Source={0};Version=3;", System.IO.Path.Combine(Application.StartupPath, SQLiteDbFile));

        //sqlite select결과를 가상테이블형태로 저장하기위한 테이블선언
        private config.dsJobList dtSetting = new config.dsJobList();
       
        SQLiteConnection SQLiteconn = null;
        

        public FrmMain()
        {
            InitializeComponent();

            System.Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(assemblyVersion.Build).AddSeconds(assemblyVersion.Revision * 2);
            
            this.Text = Assembly.GetExecutingAssembly().GetName().Name + " Ver." 
                + Assembly.GetExecutingAssembly().GetName().Version + " Build."
                + Properties.Resources.BuildDate;

            // 로그파일 내부저장을 위한 log4net설정xml 선언
            string applicationBasePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string configPath = applicationBasePath + "log4net.config";
            XmlConfigurator.Configure(new System.IO.FileInfo(configPath));


            log.Info("인터페이스 프로그램 스타트");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SQLiteInit();
            LoadSQLite();

            pBox.Image = Properties.Resources.OFF;
        }
        
        /// <summary>
        /// SQlite 초기선언(내부에 SQLite DB가 없으면 DB, TABLE schema를 자동으로 생성)
        /// 초기에 한번실행되고나면 해당메서드는 더이상 실행되지않음
        /// </summary>
        private void SQLiteInit()
        {
            try
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(Application.StartupPath, SQLiteDbFile)))
                {
                    SQLiteConnection.CreateFile(System.IO.Path.Combine(Application.StartupPath, SQLiteDbFile));  // SQLite DB 생성

                    using (SQLiteconn = new SQLiteConnection(ConnectionString))
                    {
                        SQLiteconn.Open();
                        // 임의수정 금지XXXXXXXXXXX
                        string CreateTablesql = UTIL.DBQuery.SQLiteInfoCreateTable;
                        using (SQLiteCommand command = new SQLiteCommand(CreateTablesql, SQLiteconn))
                        {
                            command.ExecuteNonQuery();
                        }
                        // 임의수정 금지XXXXXXXXXXX
                        string InsertTablesql = UTIL.DBQuery.SQLiteInfoInsertRecord;

                        using (SQLiteCommand command = new SQLiteCommand(InsertTablesql, SQLiteconn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 생성된SQlite DB파일을 읽어와서 전송항목에 대한 기준정보를 세팅하는 메서드
        /// </summary>
        private void LoadSQLite()
        {
            try
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(Application.StartupPath, SQLiteDbFile)))
                {
                    dtSetting.dtSetting.Clear();

                    using (SQLiteconn = new SQLiteConnection(ConnectionString))
                    {
                        SQLiteconn.Open();
                        // 임의수정 금지XXXXXXXXXXX
                        string sql = UTIL.DBQuery.SQLiteInfoSelectQuery;

                        using (SQLiteDataAdapter adpt = new SQLiteDataAdapter(sql, SQLiteconn))
                        {
                            DataSet LiteDs = new DataSet();
                            adpt.Fill(LiteDs);

                            //dsJobLIST.xsd의 가상테이블형태로 sqlite조회결과를 merge하며 일치하지않는항목은 무시된다. 
                            dtSetting.dtSetting.Merge(LiteDs.Tables[0], false, MissingSchemaAction.Ignore);
                        }
                    }


                    

                    dataGridView1.DataSource = dtSetting.dtSetting;

                    dataGridView1.Columns["GUBUN"].Width = 100;
                    dataGridView1.Columns["GUBUN"].ReadOnly = true;
                    dataGridView1.Columns["GUBUN"].Visible = false;
                    dataGridView1["IDX", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1.Columns["IDX"].Width = 80;
                    dataGridView1["NAME", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1.Columns["NAME"].Width = 220;
                    dataGridView1["JOB", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1.Columns["JOB"].Width = 250;
                    dataGridView1["SEC", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1.Columns["SEC"].Width = 120;
                    dataGridView1["USEYN", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1.Columns["USEYN"].Width = 120;
                    dataGridView1["EXTFILE", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1.Columns["EXTFILE"].Visible = false;
                    dataGridView1.Columns["EXTFILE"].Width = 200;

                    dataGridView1.Columns["GUBUN"].HeaderText = "구분";
                    dataGridView1.Columns["IDX"].HeaderText = "순번";
                    dataGridView1.Columns["NAME"].HeaderText = "Schedule 명";
                    dataGridView1.Columns["JOB"].HeaderText = "Class 명";
                    dataGridView1.Columns["SEC"].HeaderText = "전송주기(초)";
                    dataGridView1.Columns["USEYN"].HeaderText = "전송여부(Y,N)";
                    dataGridView1.Columns["EXTFILE"].HeaderText = "외부파일";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 폼 종료시 isExit가 true이면 완전종료, isExit가 false이면 종료되지않고 최소화하여 트레이창에 들어간다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isExit == true)
            {
                if (sched != null && sched.IsStarted)
                    sched.Shutdown();
                e.Cancel = false;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Setting 폼을 호출하고 Setting폼을 종료하면 Sqlite기준정보를 다시 세팅한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Setting_Click(object sender, EventArgs e)
        {
            FrmSetting frm = new FrmSetting();

            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadSQLite();
            }
        }

        /// <summary>
        /// 스케줄러 클래스에서 발생되는 메세지를 바탕으로 폼을 제어 액세스하기위한 메서드(크로스쓰레드 방지)
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            UInt16 WM_QUERYENDSESSION = 0x11;   // Logoff or shutdown

            /*----------------------------------------------------*/
            /* 윈도우 종료시에 종료시켜 버린다.                   */
            /*----------------------------------------------------*/
            if (m.Msg == WM_QUERYENDSESSION)
            {
                isExit = true;
            }
            else if (m.Msg == WM_COPYDATA)
            {

                COPYDATASTRUCT cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
                // 메세지가 (메세지|클래스이름|리턴코드) 형태로 들어와서 split처리하여 구분값 받음
                string[] splitText = cds.lpData.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                string strMessage = splitText[0];
                string strClassName = splitText[1];
                string strReturnCode = splitText[2];

                //Msg(cds.lpData);
                //CellColorChange(cds.clName, cds.rtCode);
                
                CellColorChange(strMessage, strClassName, strReturnCode);
            }

            base.WndProc(ref m);
        }


        /// <summary>
        /// 실시간 로깅으로 로그를 list형태로 보여주며 100건이넘으면 가장 나중에 생성된 로그부터 삭제한다.
        /// </summary>
        /// <param name="msg"></param>
        private void Msg(string msg)
        {
            lblList.Items.Add(msg);
            if (lblList.Items.Count > 100)
                lblList.Items.RemoveAt(0);
            lblList.SelectedIndex = lblList.Items.Count - 1;
        }

        /// <summary>
        /// 각 전송항목의 전송완료 플래그(0이면 nodata, 1이면 전송완료, 2이면 전송실패)로 list row 색상구분
        /// </summary>
        /// <param name="ClassName"></param>
        /// <param name="ErrorCode"></param>
        private void CellColorChange(string Message, string ClassName, string ErrorCode)
        {
            int rownum = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1["JOB", i].Value.ToString().Equals(ClassName))
                    rownum = i;
            }

            // 전송완료
            if (ErrorCode == "0")
            {
                dataGridView1.Rows[rownum].DefaultCellStyle.BackColor = SystemColors.Window;
            }
            else if (ErrorCode == "1")   // 전송완료
            {
                dataGridView1.Rows[rownum].DefaultCellStyle.BackColor = Color.LightGreen;
                Msg(Message);
            }
            else if (ErrorCode == "2")   // 전송에러
            {
                dataGridView1.Rows[rownum].DefaultCellStyle.BackColor = Color.OrangeRed;
                Msg(Message);
            }
        }

        /// <summary>
        /// sqlite에 저장된 각 JOB의 명칭의 JOB파일을 어셈블리형태로 저장하여 Scheduler에서 실행하기 위한 메서드
        /// </summary>
        /// <param name="pa_dir"></param>
        /// <returns></returns>
        private object GetAssembly(string pa_dir)
        {
            System.Reflection.Assembly Asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.Module[] mdls = Asm.GetModules();

            System.Type t = null;

            if (!string.IsNullOrEmpty(pa_dir))
            {
                // sqlite에 저장된 각 JOB의 명칭의 접두사에 DsiMq_IF.jobs.을 붙여 각 클래스명칭을 호출한다. 
                string Ppa_dir = string.Format("DsiMq_IF.jobs.{0}", pa_dir);
                t = mdls[0].GetType(Ppa_dir, true);
            }
            else
            {
                t = mdls[0].GetType(pa_dir, true);
            }

            

            if (t != null)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 작업시작 버튼을 눌러 각 JOB의 어셈블리를 호출하고 Scheduler의 트리거를 발생시킨다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Start_Click(object sender, EventArgs e)
        {
            try
            {
                if (sched != null && sched.IsStarted)
                {
                    MessageBox.Show("먼저 종료를 하신 후 다시 시작하십시오.");
                    return;
                }
                sched = schedFact.GetScheduler();
                sched.Start();

                int cntUSEY = 0;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    // sqlite에 저장된 USEYN의 값이 Y인 전송항목만 실행한다.
                    if (dataGridView1["USEYN", i].Value.ToString().ToUpper().Equals("Y"))
                    {

                        string job = dataGridView1["JOB", i].Value.ToString();
                        string sec = dataGridView1["SEC", i].Value.ToString();
                        string nm = dataGridView1["NAME", i].Value.ToString();

                        string ext = dataGridView1["EXTFILE", i].Value.ToString();
                        int iSec = 0;
                        int.TryParse(sec, out iSec);

                        Object obj = GetAssembly(job);

                        if (obj == null)
                        {
                            Assembly dll = Assembly.LoadFile(Application.StartupPath + @"\" + ext);
                            if (dll == null)
                                continue;
                            Module[] mdl = dll.GetModules();
                            Type tjob = mdl[0].GetType(job);
                            obj = Activator.CreateInstance(tjob);
                        }

                        if (obj == null || iSec == 0)
                            continue;



                        IJobDetail quartzJob = JobBuilder.Create(obj.GetType())
                       .WithIdentity(job, "DSI")
                       .Build();

                        ITrigger quartzTrigger = TriggerBuilder.Create()
                                .WithIdentity("trg_" + job, "DSI")
                                .StartNow()
                                .WithSimpleSchedule(x => x.WithIntervalInSeconds(iSec).RepeatForever())
                                .Build();

                        sched.ScheduleJob(quartzJob, quartzTrigger);
                        Msg(nm + " ( " + sec + " 간격) 시작");
                        log.Info(nm + " ( " + sec + " 간격) 시작");
                        
                        cntUSEY++;
                    }
                }

                if (cntUSEY == 0)
                {
                    MessageBox.Show("전송여부가 'Y'인 항목이 하나도 없습니다.");
                    return;
                }
                else
                {
                    pBox.Image = Properties.Resources.ON;
                    btn_Setting.Enabled = false;

                    if (btn_Start.Enabled)
                    { 
                        btn_Start.Enabled = false;
                        btn_End.Enabled = true;
                    }
                    else
                    { 
                        btn_Start.Enabled = true;
                        btn_End.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 작업종료 버튼을 눌러 프로세스를 종료한다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_End_Click(object sender, EventArgs e)
        {
            if (sched != null && sched.IsStarted)
            {
                sched.Shutdown();
                sched = null;

                pBox.Image = Properties.Resources.OFF;
                btn_Setting.Enabled = true;

                Msg(" 프로세스 종료 ");
                log.Info(" 프로세스 종료 ");

                if (btn_End.Enabled)
                {
                    btn_Start.Enabled = true;
                    btn_End.Enabled = false;
                }
                else
                {
                    btn_Start.Enabled = false;
                    btn_End.Enabled = true;
                }
                
            }
            else
            { 
                MessageBox.Show("실행중이지 않습니다.");
            }
        }

        /// <summary>
        /// 최소화버튼을 누르면 트레이창에 숨김
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.Hide();
        }

        /// <summary>
        /// 트레이창의 아이콘을 더블클릭하면 활성화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon2_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        /// <summary>
        /// 트레이창의 아이콘을 클릭하여 종료했을때 비로소 최종 프로그램이 종료되도록 하는 메서드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void eXITToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isExit = true;
            Application.Exit();
        }
    }
}
