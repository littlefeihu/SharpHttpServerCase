using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TechSvr.Utils;

namespace TechSvr.ModuleForm
{
    public partial class OutputRecordCountForm : Form
    {
        public OutputRecordCountForm(int maxRecordcount)
        {
            InitializeComponent();
            this.txtMaxRecordcount.Text = maxRecordcount.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            INIHelper.WriteInteger(Constants.INI_MAXRecordCount, int.Parse(txtMaxRecordcount.Text.Trim()), TechSvrApplication.Instance.GetINIFullPath());
            MessageBox.Show("保存成功");
        }
    }
}
