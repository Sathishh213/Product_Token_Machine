using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;
using TokenMachine.Helpers;

namespace TokenMachine
{
    /// <summary>
    /// Interaction logic for frmBackground.xaml
    /// </summary>
    public partial class frmBackground : Window
    {
        public frmBackground()
        {
            InitializeComponent();
            bw.DoWork += bw_DoWork;
        }

        DispatcherTimer tmr_close = new DispatcherTimer();
        BackgroundWorker bw = new BackgroundWorker();

        Access acc = new Access();
        ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = "";
            try
            {
                config.machine_id = Properties.Settings.Default.machine_id;
                config.IsCoupon = Properties.Settings.Default.IsCoupon;

                tmr_close.Interval = TimeSpan.FromMinutes(5);
                tmr_close.Tick += tmr_Tick;
                tmr_close.Start();


                string videoroot = "";


                var driveList = DriveInfo.GetDrives();

                DriveInfo drive = driveList[driveList.Length - 1];
                if (drive.Name != "C:\\" && drive.DriveType == DriveType.Removable)
                {
                    videoroot = drive.Name + "BetaAutomation\\Videos\\Home\\";
                }
                else
                {
                    videoroot = AppDomain.CurrentDomain.BaseDirectory + @"Videos\Home\";
                }

                if (!Directory.Exists(videoroot))
                {
                    Directory.CreateDirectory(videoroot);
                }

                string[] files = Directory.GetFiles(videoroot);
                if (files.Length == 0)
                {
                    files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"Videos\Home\");
                }

                foreach (string vpath in files)
                {
                    config.videos.Add(vpath);
                }
                if (config.videos.Count > 0)
                {
                    config.paly_index = 0;
                }
                else
                {
                    config.paly_index = -1;
                }



                if (!Directory.Exists(@"D:\Betaautomation\backup\"))
                {
                    try
                    {
                        Directory.CreateDirectory(@"D:\Betaautomation\backup\");
                        path = @"D:\Betaautomation\backup\";
                    }
                    catch
                    {
                        path = System.AppDomain.CurrentDomain.BaseDirectory + "backup\\";
                    }
                }
                else
                {
                    path = @"D:\Betaautomation\backup\";
                }
                string file = path + Process.GetCurrentProcess().ProcessName + config.machine_id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".sql";

                acc.backup(file);

                config.helpline = Properties.Settings.Default.helpline;

                Process[] oskProcessArray = Process.GetProcessesByName("explorer");
                foreach (Process onscreenProcess in oskProcessArray)
                {
                    onscreenProcess.Kill();
                }


                if (bw.IsBusy != true)
                {
                    bw.RunWorkerAsync();
                }

                frmHomeScreen frm = new frmHomeScreen();
                frm.Show();
                // tmr_focus.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            try
            {
                foreach (Window w in App.Current.Windows)
                {
                    if (w.Title == "frmHomeScreen")
                    {
                        if (DateTime.Now.Hour == 0)
                        {
                            string path = "";

                            if (!Directory.Exists(@"D:\Betaautomation\backup\"))
                            {
                                try
                                {
                                    Directory.CreateDirectory(@"D:\Betaautomation\backup\");
                                    path = @"D:\Betaautomation\backup\";
                                }
                                catch
                                {
                                    path = AppDomain.CurrentDomain.BaseDirectory + "backup\\";
                                }
                            }
                            else
                            {
                                path = @"D:\Betaautomation\backup\";
                            }

                            string file = path + Process.GetCurrentProcess().ProcessName + config.machine_id + "_" + DateTime.Now.ToString("yyyyMMdd") + ".sql";

                            // string file = path + DateTime.Now.ToString("yyyyMMdd") + ".sql"; 
                            if (!File.Exists(file))
                            {
                                // File.Create(file);
                                acc.backup(file);
                                log.Info("System goto Restart");
                                Process.Start("shutdown", "/r /t 0");
                                return;
                            }

                        }


                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DateTime date = DateTime.Now.AddDays(-1);
                Reports report = new Reports();
                report.EmailExcelReport(date, date);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                tmr_close.Stop();
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.G))
                {
                    if (e.Key == Key.G)
                    {
                        e.Handled = true;
                        frmAdminControl frm = new frmAdminControl();
                        this.Close();
                        frm.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
