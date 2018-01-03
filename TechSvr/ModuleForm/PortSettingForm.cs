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
    public partial class PortSettingForm : Form
    {
        public PortSettingForm(string portStr)
        {
            InitializeComponent();
            this.txtPort.Text = portStr;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            INIHelper.WriteString(Constants.INI_ServicePort, txtPort.Text.Trim(), DirectoryManage.GetINIFullPath());
            MessageBox.Show("端口已保存成功");
        }
    }
}
