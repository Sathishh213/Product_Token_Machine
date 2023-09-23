using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using VendingMachine.Helpers;
namespace VendingMachine
{
    /// <summary>
    /// Interaction logic for frmHomeScreen.xaml
    /// </summary>
    public partial class frmHomeScreen : Window
    {

        

        public frmHomeScreen()
        {
            InitializeComponent();

            adImage();
           
        }
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Access acc = new Access();
        BackgroundWorker bw = new BackgroundWorker();
        DataTable tbl_message = new DataTable();
        int message_count = 0;
        DispatcherTimer tmr_msg = new DispatcherTimer();



        List<string> imageList = new List<string>(); // List to store the image file paths
        private int currentIndex = 0; // Index of the currently displayed image
        private DispatcherTimer timer; // Timer for automatic slideshow
        
        private void adImage()
        {
            string imgpath = AppDomain.CurrentDomain.BaseDirectory + "HomeImage";
            DirectoryInfo directoryInfo = new DirectoryInfo(imgpath);
            var images = directoryInfo.GetFiles();
            foreach ( var image in images)
            {
                if (image.Extension.Equals(".jpg"))
                {
                    imageList.Add(string.Format(@"{0}/{1}","HomeImage",image.Name));
                }
            }
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // Set the time interval between slides (5 seconds in this example)
            timer.Tick += Timer_Tick;
            timer.Start();

            // Display the first image
            ShowImage(currentIndex);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Move to the next image in the list
            currentIndex++;
            if (currentIndex >= imageList.Count)
                currentIndex = 0;

            // Display the next image
            ShowImage(currentIndex);
        }
        private void ShowImage(int index)
        {
            // Load the image from the file path and set it as the source for the Image control
            BitmapImage image = new BitmapImage(new Uri(imageList[index], UriKind.Relative));
            imageControl.Source = image;
        }



        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            try
            {
                string cmd = "select concat('Recharge Rs.' , recharge_amount , ', get extra Rs.' , bonus_amount) as message from tbl_offer_recharge where IsActive = 1 and from_date <= curdate() and to_date >= curdate() and machine_id = " + config.machine_id + " union all  select message from tbl_message where IsActive = 1 and start_date <= curdate() and end_date >= curdate() and machine_id = " + config.machine_id;
                tbl_message = acc.GetTable(cmd);
            }
            catch (Exception ex)
            {
                log.Error(ex);

            }
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (tbl_message.Rows.Count > 0)
                {
                    txtKron.Text = tbl_message.Rows[0]["message"].ToString();
                    tmr_msg.Tick += tmr_msg_Tick;
                    tmr_msg.Interval = new TimeSpan(0, 0, 0, 10);
                    tmr_msg.Start();
                }


                //frmback.closefingerprint();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        void tmr_msg_Tick(object sender, EventArgs e)
        {
            try
            {
                message_count++;
                txtKron.Text = tbl_message.Rows[(message_count % tbl_message.Rows.Count)]["message"].ToString();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (config.login_session_id > 0)
                {
                    string cmd = "update trn_emp_login set logout = now() , is_viewed = 0 where login_id = " + config.login_session_id;
                    acc.ExecuteCmd(cmd);
                    log.Info("Employee Log out : " + config.emp_name + ", Login Session Id : " + config.login_session_id);
                }

                //if (!config.IsCoupon)
                //{
                //    btnCoupon.Visibility = Visibility.Hidden;
                //}

                config.emp_id = 0;
                config.emp_name = "";
                config.login_session_id = 0;

                config.isadmin = 0;
                config.tot_amt = 0;
                config.tot_item = 0;
                config.in_amt = 0;
                config.inmode = "";
                config.sales_code = "";
                config.thanks_msg = "";

                config.cus_id = 0;
                config.cus_code = "";
                config.cus_name = "";
                config.cus_bal = 0;
                config.cus_limit = 0;
                config.cus_mobile_no = "";
                config.cus_dob = DateTime.Now.AddDays(-1);

                config.cus_limit = 0;
                config.idcardnumber = "";
                config.paytm_upi_order_id = "";

                config.ordered.Clear();


                txtHelpLine.Text = "Help Line : " + config.helpline;

                if (config.videos.Count > 0)
                {
                    if (config.paly_index >= config.videos.Count)
                    {
                        config.paly_index = 0;
                    }
                    meVideo.Source = new Uri(config.videos[config.paly_index % config.videos.Count]);
                    meVideo.Volume = 100;
                }


                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                if (bw.IsBusy != true)
                {
                    bw.RunWorkerAsync();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnBuyNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Audio.Speak("Welcome");
                frmOrderNow frm = new frmOrderNow();
                this.Close();
                frm.Show();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }


        //private void btnCoupon_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Audio.Speak("Welcome");
        //        frmCouponVending frm = new frmCouponVending();
        //        this.Close();
        //        frm.Show();
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex);
        //    }
        //}

        private void meVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (config.videos.Count > 0)
                {
                    config.paly_index++;
                    if (config.paly_index >= config.videos.Count)
                    {
                        config.paly_index = 0;
                    }
                    meVideo.Source = new Uri(config.videos[config.paly_index % config.videos.Count]);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void meVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                config.paly_index++;
                if (config.paly_index >= config.videos.Count)
                {
                    config.paly_index = 0;
                }
                meVideo.Source = new Uri(config.videos[config.paly_index % config.videos.Count]);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                tmr_msg.Stop();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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


        //private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    EmployeeHome();
        //}

        //private void Image_TouchDown(object sender, TouchEventArgs e)
        //{
        //    EmployeeHome();
        //}

        //private void EmployeeHome()
        //{
        //    try
        //    {
        //        frmEmployeeLogin frm = new frmEmployeeLogin();
        //        this.Close();
        //        frm.Show();
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex);
        //    }
        //}


    }
}
