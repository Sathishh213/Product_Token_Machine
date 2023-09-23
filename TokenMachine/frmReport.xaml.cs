using log4net;
using MaterialDesignThemes.Wpf;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VendingMachine.Helpers;
using Microsoft.Office.Interop.Excel;
using System.Net.Mail;
using System.Net.Http.Headers;
using System.Net;
using System.IO.Enumeration;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Excel = Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System.Reflection;
using System.ComponentModel;
using DataTable = System.Data.DataTable;
using System.Globalization;

namespace VendingMachine
{
    /// <summary>
    /// Interaction logic for frmReport.xaml
    /// </summary>
    public partial class frmReport : System.Windows.Window
    {
        public frmReport()
        {
            InitializeComponent();
        }

        Access acc = new Access();
        string cmd = string.Empty;string ToAddress = string.Empty;
        System.Data.DataTable dt = new System.Data.DataTable();
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                radioBtn_Stock.IsChecked = true;
                radioBtn_Sales.IsChecked = false;
                txtEmailId.Focus();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        private void btnSendReport_Click(object sender, RoutedEventArgs e)
        {
            string reportName = string.Empty;
            if (IsvalidEmailFormat())
            {
                ToAddress = txtEmailId.Text;
                if (radioBtn_Sales.IsChecked.HasValue ? radioBtn_Sales.IsChecked.Value : false)
                {
                    reportName = radioBtn_Sales.Content.ToString();
                }
                else if (radioBtn_Stock.IsChecked.HasValue ? radioBtn_Stock.IsChecked.Value : false)
                {
                    reportName = radioBtn_Stock.Content.ToString();
                }
                try
                {
                    string filePath = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "/Reports/" + reportName + "/" + string.Format("{0}.txt", reportName));
                    cmd = File.ReadAllText(filePath);
                    dt = acc.GetTable(cmd);
                    if (dt.Rows.Count > 0 && !string.IsNullOrEmpty(reportName))
                    {
                        string filename = ExportToExcel(dt, reportName);
                        SendThroughMail(filename, reportName);
                    }
                }
                catch (Exception ex)
                {
                    DisplayMsg(ex.Message);
                    log.Error(ex);
                }
            }
            else
            {
                DisplayMsg("Please Enter Valid Email ID!!");
            }
        }

        public async void DisplayMsg(string msg)
        {
            try
            {
                var sampleMessageDialog = new Dialog { Message = { Text = msg } };
                await DialogHost.Show(sampleMessageDialog, "frmReportDialog");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async void SendThroughMail(string fileAttachment, string ReportName)
        {
            try
            {
                string filePath = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "/Reports/MailContent.html");
                if (!string.IsNullOrEmpty(filePath))
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(Properties.Settings.Default.FromMailAddress);
                    mail.To.Add(ToAddress);
                    mail.Subject = string.Format("{0} - {1}", ReportName, DateTime.Now);
                    mail.IsBodyHtml = true;
                    mail.Body = File.ReadAllText(filePath);
                    SmtpClient SmtpServer = new SmtpClient(Properties.Settings.Default.SMTPClientAddress);
                    System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(fileAttachment);
                    mail.Attachments.Add(attachment);
                    SmtpServer.Port = 587;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new NetworkCredential(Properties.Settings.Default.FromMailAddress, Properties.Settings.Default.SMTPAppPassword);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                    DisplayMsg("Mail Sent Successfully");
                }
                else
                {
                    DisplayMsg("Mail Content Not Found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string ExportToExcel(System.Data.DataTable dt,string label)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object misValue = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;
            Microsoft.Office.Interop.Excel.Range rng = null;
            string filename = String.Empty;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            try
            {
                filename = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + "/Reports/"+label+"/"+string.Format("{0}.xlsx",label));
                excel = new Microsoft.Office.Interop.Excel.Application();
                wb = excel.Workbooks.Open(filename, 0, false, 5, "", "",
                            false, XlPlatform.xlWindows, "", true, false,
                            0, true, false, false);
                ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                for (int Idx = 0; Idx < dt.Columns.Count; Idx++)
                {
                    ws.Range["A1"].Offset[0, Idx].Value = dt.Columns[Idx].ColumnName;
                    ws.Range["A1"].Offset[0].EntireRow.Font.Bold = true;
                }

                if (label == "Stock Report")
                {
                    for (int Idx = 0; Idx < dt.Rows.Count; Idx++)
                    {
                        ws.Range["A2"].Offset[Idx].Resize[1, dt.Columns.Count].Value =
                        dt.Rows[Idx].ItemArray;
                    }
                }
                else if (label == "Sales Report")
                {
                    int f_idx = 0;
                    for (int Idx = 0; Idx < dt.Rows.Count; Idx++)
                    {
                        int g_idx = f_idx + (Idx == 0 ? Idx : 1);
                        var salesDataArray = dt.Rows[Idx].ItemArray;
                        salesDataArray[1] = "Given Below";
                        ws.Range["A2"].Offset[g_idx].Resize[1, dt.Columns.Count].Value = salesDataArray;
                        //dt.Rows[Idx].ItemArray;
                        if (dt.Rows[Idx]["Order Details"] != null)
                        {
                            DataTable dataTable2 = (DataTable)JsonConvert.DeserializeObject(dt.Rows[Idx]["Order Details"].ToString(), (typeof(DataTable)));
                            if (dataTable2 != null && dataTable2.Rows.Count > 0)
                            {
                                int A_cellrange = 3 + g_idx;
                                int D_cellrange = 3 + g_idx + dataTable2.Rows.Count;
                                for (int c_idx = 0; c_idx < dataTable2.Columns.Count; c_idx++)
                                {
                                    ws.Range["A3"].Offset[g_idx, c_idx+1].Value = textInfo.ToTitleCase(dataTable2.Columns[c_idx].ColumnName.Replace("_"," "));
                                    ws.Range["A3"].Offset[g_idx].EntireRow.Font.Bold = true;
                                }
                                for (int idx = 0; idx < dataTable2.Rows.Count; idx++)
                                {
                                    f_idx = g_idx + idx;
                                    ws.Range["A4"].Offset[f_idx,1].Resize[1, 4].Value = dataTable2.Rows[idx].ItemArray;
                                }
                                string _range = string.Format("A{0}:D{1}", A_cellrange, D_cellrange);
                                Excel.Range range = ws.Range[_range] as Excel.Range;
                                range.Rows.Group(misValue, misValue, misValue, misValue);
                                f_idx += 2;
                            }
                        }
                    }
                    ws.Outline.SummaryRow = XlSummaryRow.xlSummaryAbove;
                    ws.Outline.ShowLevels(1, 0);
                }

                ws.Columns.AutoFit();
                wb.RefreshAll();
                excel.Calculate();
                wb.Save();
                wb.Close(true);
                excel.Quit();
            }
            catch (COMException ex)
            {
                MessageBox.Show("Error accessing Excel: " + ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
            return filename;
        }

        private void adminBtn_Click(object sender, RoutedEventArgs e)
        {
            frmAdminControl frm=new frmAdminControl();
            this.Close();
            frm.Show();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtEmailId.Text = string.Empty;
        }

        private bool IsvalidEmailFormat()
        {
            bool result = false;
            if (txtEmailId.Text != null)
            {
                string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(strRegex);
                if (re.IsMatch(txtEmailId.Text))
                    result = true;
                else
                    result = false;
            }
            return result;
        }
    }
}
