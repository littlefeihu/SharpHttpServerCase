using FastReport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TechSvr.Utils;

namespace TechSvr.Plugin.Print
{
    public class PrintCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "Print";
            }
        }

        public string Excute(InputArgs request)
        {
            Thread importThread = new Thread(() =>
            {
                var jobject = request.Data.ToObject<dynamic>();
                var template = jobject.TEMPLATE.ToString();
                var printMode = (FPrintMode)int.Parse(jobject.PRINTMODE.ToString());

                using (Report report = new Report())
                {
                    string reportfileName = DirectoryManage.GetFrxFullPath(template, request.SysType);

                    if (File.Exists(reportfileName))
                    {
                        report.Load(reportfileName);
                    }
                    else
                    {    //模板文件不存在，则重新设置文件名
                        report.FileName = reportfileName;
                    }
                    report.RegisterData(BuildDS(jobject));
                    //设置报表显示数据源选项
                    report.GetDataSource(Constants.MainDataTable).Enabled = true;
                    report.GetDataSource(Constants.SubDataTable).Enabled = true;

                    switch (printMode)
                    {
                        case FPrintMode.Preview:
                        case FPrintMode.Design:
                            var mainform = TechSvrApplication.Instance.MainFrm;
                            mainform.Invoke(new MethodInvoker(() =>
                            {
                                mainform.TopMost = true;
                                if (mainform.WindowState == FormWindowState.Minimized)
                                {
                                    mainform.WindowState = FormWindowState.Normal;
                                }
                                mainform.BringToFront();
                                mainform.TopMost = false;
                                if (printMode == FPrintMode.Preview)
                                {
                                    report.Show();
                                }
                                else
                                {
                                    report.Design();
                                }
                                mainform.WindowState = FormWindowState.Minimized;
                            }));
                            break;
                        case FPrintMode.Print:
                            //不弹打印窗口 直接打印
                            report.PrintSettings.ShowDialog = false;
                            report.Print();
                            break;
                        default:
                            break;
                    }
                }
            });

            importThread.SetApartmentState(ApartmentState.STA); //Fast Report 要求线程是STA模式的
            importThread.Start();

            importThread.Join();

            return "Print Test";
        }

        /// <summary>
        /// 构建数据源
        /// </summary>
        /// <param name="jobject"></param>
        /// <returns></returns>
        private DataSet BuildDS(dynamic jobject)
        {
            var FDataSet = new DataSet();

            if (jobject.DATA.MAINDATA != null)
            {
                var mainDataTable = JsonConvert.DeserializeObject<DataTable>(jobject.DATA.MAINDATA.ToString());
                mainDataTable.TableName = Constants.MainDataTable;
                FDataSet.Tables.Add(mainDataTable);
            }
            if (jobject.DATA.DETAILDATA != null)
            {
                var subDataTable = JsonConvert.DeserializeObject<DataTable>(jobject.DATA.DETAILDATA.ToString());
                subDataTable.TableName = Constants.SubDataTable;
                FDataSet.Tables.Add(subDataTable);
            }
            return FDataSet;
        }
    }
}
