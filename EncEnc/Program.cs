using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EncEnc
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var viewModel = new MainFormViewModel();
            Application.Run(new MainForm(viewModel));
        }
    }
}
