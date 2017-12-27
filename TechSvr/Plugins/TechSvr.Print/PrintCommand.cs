using FastReport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using TechSvr.Utils;

namespace TechSvr.Print
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

        public string Excute(string request)
        {
            Thread importThread = new Thread(() =>
            {
                var jobject = request.ToObject<dynamic>();

                var template = jobject.TEMPLATE;
                var printMode = jobject.PRINTMODE;

                var maindataStr = jobject.DATA.MAINDATA.ToString();
                var mainDataTable = JsonConvert.DeserializeObject<DataTable>(maindataStr);
                mainDataTable.TableName = "MAINDATA";

                var FDataSet = new DataSet();
                FDataSet.Tables.Add(mainDataTable);
                if (jobject.DATA.DETAILDATA != null)
                {
                    var subDataTable = JsonConvert.DeserializeObject<DataTable>(jobject.DATA.DETAILDATA.ToString());
                    subDataTable.TableName = "subDataTable";
                    FDataSet.Tables.Add(subDataTable);
                }
                using (Report report = new Report())
                {
                    report.Load(@"Plugins\FastPrint\" + template);
                    report.RegisterData(FDataSet);
                    report.PrintSettings.ShowDialog = true;
                    switch (jobject.PRINTMODE.ToString())
                    {
                        //预览
                        case "0":
                            report.Show(TechSvrApplication.Instance.PrintFrm);
                            TechSvrApplication.Instance.PrintFrm.ShowDialog();
                            break;
                        //设计
                        case "1":
                            report.Design(TechSvrApplication.Instance.PrintFrm);
                            TechSvrApplication.Instance.PrintFrm.ShowDialog();
                            break;
                        //打印
                        case "2":
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
    }
}
