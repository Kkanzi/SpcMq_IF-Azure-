using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DsiMq_IF
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new FrmMain());

            bool bNew;

            Mutex mutex = new Mutex(true, "DsiMq", out bNew);

            if (bNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmMain());

                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("이미 실행중인 프로세스가 있습니다.");
                Application.Exit();
            }
        }
    }
}
