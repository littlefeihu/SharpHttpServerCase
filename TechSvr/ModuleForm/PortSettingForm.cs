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
        public PortSettingForm(int port)
        {
            InitializeComponent();
            this.txtPort.Text = port.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            INIHelper.WriteInteger(Constants.INI_ServicePort, int.Parse(txtPort.Text.Trim()), TechSvrApplication.Instance.GetINIFullPath());
            MessageBox.Show("端口已保存成功");
        }
    }
}
