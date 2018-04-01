using FastReport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TechSvr.Utils;
using TechSvr.Utils.DTO;

namespace TechSvr.Plugin.Print
{
    public class PrintCommand : ICommand
    {
        private const string img = "图片";
        private const string imgdata = "Bytes";
        private const string TEMPLATE_STR = "TEMPLATE";
        private const string SYSTYPE_STR = "Systype";
        private const string APPLYNO_STR = "APPLYNO";

        public string Name
        {
            get
            {
                return "Print";
            }
        }

        public ResposeMessage Excute(InputArgs request)
        {
            var data = request.Data.ToString();

            var jobject = Json.ToJObject(data);
            var template = jobject.TEMPLATE.ToString();
            var printMode = (FPrintMode)int.Parse(jobject.PRINTMODE.ToString());

            //指定打印模板目录
            var sysType = request.SysType.ToString();


            using (Report report = new Report())
            {
                string reportfileName = DirectoryManage.GetFrxFullPath(template, sysType);

                if (File.Exists(reportfileName))
                {
                    report.Load(reportfileName);
                }
                else
                {
                    //模板不存在时，自动设置为设计模式
                    printMode = FPrintMode.Design;
                    //模板文件不存在，则重新设置文件名
                    report.FileName = reportfileName;
                }

                var ds = BuildDS(jobject);

                if (printMode != FPrintMode.BatchPrint)
                {
                    report.RegisterData(ds);
                    //设置报表显示数据源选项
                    foreach (DataTable dt in ds.Tables)
                    {
                        report.GetDataSource(dt.TableName).Enabled = true;
                    }
                }


                switch (printMode)
                {
                    case FPrintMode.Preview:
                    case FPrintMode.Design:
                    case FPrintMode.ExportPDF:
                        var mainform = TechSvrApplication.Instance.MainFrm;
                        mainform.Invoke(new MethodInvoker(() =>
                                                {
                                                    mainform.TopMost = true;
                                                    if (mainform.WindowState == FormWindowState.Minimized)
                                                    {
                                                        mainform.WindowState = FormWindowState.Normal;
                                                    }
                                                    var originSize = mainform.Size;
                                                    mainform.Width = 2;
                                                    mainform.Height = 2;
                                                    mainform.BringToFront();
                                                    mainform.TopMost = false;
                                                    if (printMode == FPrintMode.Preview)
                                                    {
                                                        report.Show();
                                                    }
                                                    else if (printMode == FPrintMode.Design)
                                                    {
                                                        report.Design();
                                                    }
                                                    else if (printMode == FPrintMode.ExportPDF)
                                                    {
                                                        try
                                                        {
                                                            //导出PDF文件
                                                            report.Prepare();
                                                            using (SaveFileDialog sfd = new SaveFileDialog())
                                                            {
                                                                sfd.Filter = @"PDF文件|*.pdf";
                                                                sfd.ShowDialog();
                                                                var pdfFileName = sfd.FileName;

                                                                report.Export(new FastReport.Export.Pdf.PDFExport(), pdfFileName);
                                                                //System.Diagnostics.Process.Start("Explorer.exe", pdfFileName);
                                                            }
                                                        }
                                                        catch (Exception)
                                                        {
                                                            mainform.Size = originSize;
                                                            mainform.WindowState = FormWindowState.Minimized;
                                                            throw;
                                                        }
                                                    }
                                                    mainform.Size = originSize;
                                                    mainform.WindowState = FormWindowState.Minimized;
                                                }));
                        break;
                    case FPrintMode.Print:
                        //不弹打印窗口 直接打印
                        report.PrintSettings.ShowDialog = false;
                        report.Print();
                        break;
                    case FPrintMode.BatchPrint:
                        //批量打印
                        //查找打印模板
                        var ReportFileArr = ((DataTable)ds.Tables[0]).AsEnumerable()
                            .Select(p => new
                            {
                                TEMPLATE = p.Field<string>(TEMPLATE_STR),
                                SYSTYPE = p.Field<string>(SYSTYPE_STR),
                            }).Distinct().ToList();

                        //遍历打印模板
                        foreach (var ReportFile in ReportFileArr)
                        {
                            DataSet dsDataSource = new DataSet();

                            var dt1 = (DataTable)ds.Tables[0];
                            //获取打印模板对应的数据
                            var mainDataRows = dt1.AsEnumerable().Where(p => p.Field<string>(TEMPLATE_STR) == ReportFile.TEMPLATE && p.Field<string>(SYSTYPE_STR) == ReportFile.SYSTYPE);
                            var mainDataTable = dt1.Clone();
                            mainDataTable.TableName = dt1.TableName;

                            var dt2 = (DataTable)ds.Tables[1];
                            var detailDataTable = dt2.Clone();
                            detailDataTable.TableName = dt2.TableName;

                            foreach (var dataRow in mainDataRows)
                            {
                                mainDataTable.Rows.Add(dataRow.ItemArray);
                                //获取主表对应的子表数据行
                                var detailDataRows = dt2.AsEnumerable().Where(p => p.Field<Int64>(APPLYNO_STR) == Int64.Parse(dataRow[APPLYNO_STR].ToString())).ToList();
                                foreach (var detailDataRow in detailDataRows)
                                {
                                    detailDataTable.Rows.Add(detailDataRow.ItemArray);
                                }
                            }
                            dsDataSource.Tables.Add(mainDataTable);
                            dsDataSource.Tables.Add(detailDataTable);

                            report.RegisterData(dsDataSource);

                            string templatefileName = DirectoryManage.GetFrxFullPath(ReportFile.TEMPLATE, ReportFile.SYSTYPE);
                            report.Load(templatefileName);
                            report.PrintSettings.ShowDialog = false;
                            report.Print();
                        }
                        break;
                    default:
                        break;
                }
            }
            return new ResposeMessage
            {
                type = ResultType.SUCCESS.ToString(),
                messageCode = MessageCode.information.ToString(),
                message = "打印成功",
                data = ""
            };
        }


        /// <summary>
        /// 构建数据源
        /// </summary>
        /// <param name="jobject"></param>
        /// <returns></returns>
        private DataSet BuildDS(dynamic jobject)
        {
            var FDataSet = new DataSet();
            FDataSet = JsonConvert.DeserializeObject<DataSet>(jobject.DATA.ToString());
            foreach (DataTable dt in FDataSet.Tables)
            {
                if (dt.Columns.Cast<DataColumn>().Any(o => o.ColumnName.Contains(img)))
                {
                    List<string> newAddedColumns = new List<string>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ColumnName.Contains(img))
                        {
                            newAddedColumns.Add(column.ColumnName);
                        }
                    }
                    var canAddColumn = true;
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (var newAddedColumn in newAddedColumns)
                        {
                            if (canAddColumn)
                                dt.Columns.Add(newAddedColumn + imgdata, typeof(Byte[]));

                            row[newAddedColumn + imgdata] = Convert.FromBase64String(row[newAddedColumn].ToString());
                        }
                        canAddColumn = false;
                    }
                }
            }
            return FDataSet;
        }
    }
}
