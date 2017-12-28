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

        public string Excute(string request)
        {
            Thread importThread = new Thread(() =>
            {
                var jobject = request.ToObject<dynamic>();
                var template = jobject.TEMPLATE;
                var printMode = jobject.PRINTMODE.ToString();

                using (Report report = new Report())
                {
                    report.Load(@"Plugins\FastPrint\" + template);
                    report.RegisterData(BuildDS(jobject));
                    switch (printMode)
                    {
                        //预览
                        case "0":
                            report.Show();
                            break;
                        //设计
                        case "1":
                            report.Design();
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
                mainDataTable.TableName = "mainDataTable";
                FDataSet.Tables.Add(mainDataTable);
            }
            if (jobject.DATA.DETAILDATA != null)
            {
                var subDataTable = JsonConvert.DeserializeObject<DataTable>(jobject.DATA.DETAILDATA.ToString());
                subDataTable.TableName = "subDataTable";
                FDataSet.Tables.Add(subDataTable);
            }
            return FDataSet;
        }
    }
}
