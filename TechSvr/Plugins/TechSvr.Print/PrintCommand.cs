using FastReport;
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

        public string Excute(string input)
        {
            Thread importThread = new Thread(() =>
            {
                var FDataSet = new DataSet();

                DataTable table = new DataTable();
                table.TableName = "Employees";
                FDataSet.Tables.Add(table);
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("Name", typeof(string));

                var emps = input.ToObject<List<Employee>>();
                foreach (var emp in emps)
                {
                    table.Rows.Add(emp.ID, emp.Name);
                }

                using (Report report = new Report())
                {
                    report.Load(@"Plugins\FastPrint\report.frx");
                    report.RegisterData(FDataSet);

                    report.PrintSettings.ShowDialog = true;
                    report.Show(TechSvrApplication.Instance.PrintFrm);
                    TechSvrApplication.Instance.PrintFrm.ShowDialog();

                    //System.Drawing.Printing.PrinterSettings setting = new System.Drawing.Printing.PrinterSettings();
                    //report.ShowPrintDialog(out setting);
                }
            });

            importThread.SetApartmentState(ApartmentState.STA); //Fast Report 要求线程是STA模式的
            importThread.Start();

            importThread.Join();

            return "Print Test";
        }
    }
}
