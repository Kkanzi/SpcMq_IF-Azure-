using DsiMq_IF.UTIL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DsiMq_IF
{
    public partial class FrmSetting : Form
    {
        // sqlite 전송항목을 불러오기위한 변수선언 임의수정금지XXX
        SQLiteConnection SQLiteconn = null;
        private const string SQLiteDbFile = "ScheInfo.dat";
        string ConnectionString = string.Format("Data Source={0};Version=3;", System.IO.Path.Combine(Application.StartupPath, SQLiteDbFile));

        // 리소스에 저장된 ConnectString, 업체코드를 불러온다.
        string mssqlConn = Properties.Settings.Default.connectionString.ToString();
        string customerCode = Properties.Settings.Default.CustomerCode.ToString();

        private config.dsJobList dtSetting = new config.dsJobList();

        public FrmSetting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 폼로드 메서드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmSetting_Load(object sender, EventArgs e)
        {
            try
            {
                dtSetting.dtSetting.Clear();
                LoadSQLite();

                txtDB.Text = mssqlConn;
                txtCustomerCode.Text = customerCode;

                // 그리드의 cellvaluechage이벤트 핸들러 추가
                dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            }
            catch (Exception eee)
            {
                MessageBox.Show(eee.Message);
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

                        string sql = "SELECT IDX, GUBUN, NAME, JOB, SEC, USEYN, EXTFILE FROM scheinfo";

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
                    dataGridView1.Columns["IDX"].Width = 80;
                    dataGridView1.Columns["NAME"].Width = 220;
                    dataGridView1.Columns["JOB"].Width = 250;
                    dataGridView1.Columns["SEC"].Width = 120;
                    dataGridView1.Columns["USEYN"].Width = 120;
                    dataGridView1.Columns["EXTFILE"].Width = 200;
                    dataGridView1.Columns["EXTFILE"].Visible = false;
                    
                    dataGridView1["IDX", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1["NAME", dataGridView1.Rows.Count - 1].ReadOnly = false;
                    dataGridView1["JOB", dataGridView1.Rows.Count - 1].ReadOnly = true;
                    dataGridView1["SEC", dataGridView1.Rows.Count - 1].ReadOnly = false;
                    dataGridView1["EXTFILE", dataGridView1.Rows.Count - 1].ReadOnly = false;

                    // 여기서에러남 다시봐야됨
                    int aa = dataGridView1.Columns.Count;
                    string bb = dataGridView1.Columns["GUBUN"].HeaderText;

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
        /// 수정된 셀을 저장하기위해 cellvalue가 변경된 row는 GUBUN의 상태를 바꿈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1["GUBUN", e.RowIndex].Value.ToString().Equals(""))
            {
                dataGridView1["GUBUN", e.RowIndex].Value = "U";
            }
        }
        
        /// <summary>
        /// GUBUN의 상태가 바뀐항목에 대해서 sqliteDB데이터를 UPDATE하고, 업체정보, MSSQL ConnectionString 정보를 저장한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count <= 0)
                {
                    return;
                }

                using (SQLiteConnection SQLiteconn = new SQLiteConnection(ConnectionString))
                {
                    SQLiteconn.Open();

                    using (SQLiteTransaction trans = SQLiteconn.BeginTransaction())
                    { 
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            if (!dataGridView1["GUBUN", i].Value.ToString().Equals(""))
                            {
                                using (SQLiteCommand command = new SQLiteCommand(SQLiteconn))
                                {
                                    command.CommandText = "update scheinfo set NAME = :name, JOB = :job, SEC = :sec, USEYN = :useyn where IDX=:idx";
                                    command.Parameters.Add("name", DbType.String).Value = dataGridView1["NAME", i].Value.ToString();
                                    command.Parameters.Add("job", DbType.String).Value = dataGridView1["JOB", i].Value.ToString();
                                    command.Parameters.Add("sec", DbType.Int16).Value = Convert.ToInt16(dataGridView1["SEC", i].Value.ToString());
                                    command.Parameters.Add("useyn", DbType.String).Value = dataGridView1["USEYN", i].Value.ToString();
                                    command.Parameters.Add("idx", DbType.Int16).Value = Convert.ToInt16(dataGridView1["IDX", i].Value.ToString());
                                    command.ExecuteNonQuery();
                                }

                            }
                        }

                        trans.Commit();
                    }
                }

                Properties.Settings.Default.connectionString = txtDB.Text.Trim();
                Properties.Settings.Default.CustomerCode = txtCustomerCode.Text.Trim();
                Properties.Settings.Default.Save();
            }
            catch (Exception eee)
            {
                MessageBox.Show(eee.Message);
            }
            finally
            {
                LoadSQLite();
            }
        }

        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_search_Click(object sender, EventArgs e)
        {
            LoadSQLite();
        }

        /// <summary>
        /// 폼종료시 다이얼로그 상태를 받아 부모창에서 컨트롤하기위한 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
