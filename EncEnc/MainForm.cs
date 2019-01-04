using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Windows.Forms;

namespace EncEnc
{
    public partial class MainForm : Form
    {
        private MainFormViewModel _viewModel;

        public MainForm(MainFormViewModel viewModel)
        {
            InitializeComponent();

            this._viewModel = viewModel;

            SetBind();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this._viewModel.SelectFile();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (this.cmbExecuteMode.SelectedValue.Equals(ExecuteMode.Encrypt))
            {
                this._viewModel.Encrypt();
            }
            else
            {
                this._viewModel.Decrypt();
            }

            MessageBox.Show("Done.");
        }

        private void SetBind()
        {
            this.cmbExecuteMode.DisplayMember = "Value";
            this.cmbExecuteMode.ValueMember = "Key";
            this.cmbExecuteMode.DataSource =
                Enum.GetValues(typeof(ExecuteMode))
                .OfType<ExecuteMode>()
                .Select(x => new { Key = x, Value = x.DisplayName() })
                .ToList();

            this.btnRun.DataBindings.Add(nameof(this.btnRun.Enabled), this._viewModel, nameof(this._viewModel.CanClickRunButton), false, DataSourceUpdateMode.OnPropertyChanged);
            this.txtFilePath.DataBindings.Add(nameof(this.txtFilePath.Text), this._viewModel, nameof(this._viewModel.FilePath), false, DataSourceUpdateMode.OnPropertyChanged);
            this.txtPassword.DataBindings.Add(nameof(this.txtPassword.Text), this._viewModel, nameof(this._viewModel.Password), false, DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}
