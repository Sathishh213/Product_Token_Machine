using log4net;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
using TokenMachine.Helpers;

namespace TokenMachine
{
    /// <summary>
    /// Interaction logic for frmFileUpload.xaml
    /// </summary>
    public partial class frmFileUpload : Window
    {
        public frmFileUpload()
        {
            InitializeComponent();
        }

        Access acc = new Access();string cmd = null;DataTable dt = new DataTable();
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TimeSpan start = TimeSpan.Parse("08:00"); // 08 AM
            TimeSpan end = TimeSpan.Parse("10:00");   // 10 AM
            TimeSpan now = DateTime.Now.TimeOfDay;

            if (now >= start && now <= end)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            try
            {
                List<string> cmbdata = new List<string> { "File A", "File B", "File C" };
                cmd = @"SELECT * from Daily_Report_Details where CAST(uploaded_datetime AS DATE) = CAST(now() AS DATE);";
                dt = acc.GetTable(cmd);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string value = dr["filename"].ToString() ?? "";
                        cmbdata.Remove(value);
                    }
                }
                cmbFileType.ItemsSource = cmbdata;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void adminBtn_Click(object sender, RoutedEventArgs e)
        {
            frmAdminControl frm = new frmAdminControl();
            this.Close();
            frm.Show();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            int exe = -1;
            try
            {
                if (cmbFileType.SelectedIndex > -1)
                {
                    OpenFileDialog open = new OpenFileDialog();
                    open.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                    if (open.ShowDialog() == true)
                    {
                        var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        var assemblyParentPath = System.IO.Path.GetDirectoryName(assemblyPath);
                        var fileDir = System.IO.Path.Combine(assemblyParentPath, "DailyReport");

                        if (!Directory.Exists(fileDir))
                            Directory.CreateDirectory(fileDir);

                        
                        if (!File.Exists(fileDir + "\\" + System.IO.Path.GetFileName(open.FileName)))
                        {
                            using (Stream s = File.Open(fileDir + "\\" + System.IO.Path.GetFileName(open.FileName), FileMode.CreateNew))
                            using (StreamWriter sw = new StreamWriter(s))
                            {
                                sw.Write(open.FileName);
                            }

                            var filepath = System.IO.Path.Combine(fileDir, open.FileName).ToString().Replace("\\", "\\\\");

                            cmd = @"insert into Daily_Report_Details(filename ,  filepath , uploaded_datetime) 
                            values( '" + cmbFileType.SelectedValue + "' , '" + filepath + "' , now())";
                            exe = Convert.ToInt16(acc.ExecuteCmd(cmd));

                            if (exe > 0)
                            {
                                LoadData();
                                DisplayMsg("Uploaded Successfully");
                            }
                            else
                                DisplayMsg("Saved Failed");
                        }
                        else
                        {
                            DisplayMsg("File Already Exists!!");
                        }
                    }
                }
                else
                {
                    DisplayMsg("Please Select File Type!!!");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public async void DisplayMsg(string msg)
        {
            try
            {
                var sampleMessageDialog = new Dialog { Message = { Text = msg } };
                await DialogHost.Show(sampleMessageDialog, "frmFileUploadDialog");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

    }
}
