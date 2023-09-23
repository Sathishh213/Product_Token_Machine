using VendingMachine.Helpers.paytm.create_qr;
using VendingMachine.Helpers.paytm.transaction_status;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json;
using Paytm;

namespace VendingMachine
{
    /// <summary>
    /// Interaction logic for frmUPIPayTM.xaml
    /// </summary>
    public partial class frmUPIPayTM : Window
    {
        public frmUPIPayTM()
        {
            InitializeComponent();

           
        }

        BackgroundWorker bw = new BackgroundWorker();
        DispatcherTimer tmr_status = new DispatcherTimer();

        ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Access acc = new Access();


        string orderId = "";
        string posId = "";
        string clientId = Properties.Settings.Default.paytm_clientId;
        string version = Properties.Settings.Default.paytm_version;
        string Mid = Properties.Settings.Default.paytm_mid;
        string M_key = Properties.Settings.Default.paytm_m_key.Replace("%", "&");

        int create_retry_count = 0;
        int timer_count = 0;


        paytm_create_qr_response create_qr_response;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                config.paytm_upi_order_id = "";
                config.paytm_upi_txnId = "";

                lblMessage.Text = "Total Amount Rs." + config.tot_amt;

                //if (config.sales_code.Trim().Length < 4)
                //{
                //    string cmd = "SELECT  concat('" + config.machine_id + "' , LPAD( count(sales_code) + 1, 6, 0 )) as sales_code FROM tbl_sales where machine_id = '" + config.machine_id + "'";
                //    config.sales_code = acc.GetValue(cmd);
                //}

                dgTimer.Visibility = Visibility.Collapsed;

                posId = clientId + "_" + acc.GetDeviceLoginId();


                bw.DoWork += Bw_DoWork;
                bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

                tmr_status.Interval = new TimeSpan(0, 0, 1);
                tmr_status.Tick += Tmr_status_Tick;

                lblTimeRemains.Text = "";

                if (!bw.IsBusy)
                    bw.RunWorkerAsync();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }


        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                orderId = DateTime.Now.ToString("yyMMddHHmmss");
                config.paytm_upi_order_id = orderId;


                string cmd = $"insert into paytm_upi(order_date, order_id, order_amount) values ( current_timestamp , '{orderId}','{config.tot_amt.ToString("0.00")}' )";
                RunDbCommand(cmd);

                Dictionary<string, string> body = new Dictionary<string, string>();
                Dictionary<string, string> head = new Dictionary<string, string>();
                Dictionary<string, Dictionary<string, string>> requestBody = new Dictionary<string, Dictionary<string, string>>();

                body.Add("mid", Mid);
                body.Add("orderId", orderId);
                body.Add("amount", config.tot_amt.ToString("0.00"));
                body.Add("businessType", "UPI_QR_CODE");
                body.Add("posId", posId);

                var result = JsonConvert.SerializeObject(body);
                /*
                * Generate checksum by parameters we have in body
                * Find your Merchant Key in your Paytm Dashboard at https://dashboard.paytm.com/next/apikeys 
                */
                string paytmChecksum = Checksum.generateSignature(JsonConvert.SerializeObject(body), M_key);

                head.Add("clientId", clientId);
                head.Add("version", version);
                head.Add("signature", paytmChecksum);

                requestBody.Add("body", body);
                requestBody.Add("head", head);

                string post_data = JsonConvert.SerializeObject(requestBody);

                //For  Staging
                //string url = "https://securegw-stage.paytm.in/paymentservices/qr/create";

                //For  Production  url
                string url = "https://securegw.paytm.in/paymentservices/qr/create";

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                //  webRequest.ContentLength = post_data.Length;

                using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestWriter.Write(post_data);
                }

                string responseData = string.Empty;

                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    responseData = responseReader.ReadToEnd();
                    log.Info(responseData);
                    create_qr_response = JsonConvert.DeserializeObject<paytm_create_qr_response>(responseData);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            create_retry_count++;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (create_qr_response != null)
                {
                    string cmd = $"update paytm_upi set qr_status_code = '{create_qr_response.body.resultInfo.resultCode}', qr_status_msg = '{create_qr_response.body.resultInfo.resultMsg}' where order_id = '{orderId}'";
                    RunDbCommand(cmd);

                    if (create_qr_response.body.resultInfo.resultStatus == "SUCCESS")
                    {
                        byte[] binaryData = Convert.FromBase64String(create_qr_response.body.image);
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = new MemoryStream(binaryData);
                        bi.EndInit();
                        img_QR.Source = bi;

                        tmr_status.Start();

                        lblUserMessage.Text = "Scan to Pay";
                        Pbstatus.Visibility = Visibility.Collapsed;
                        dgTimer.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (create_retry_count < 2)
                        {
                            bw.RunWorkerAsync();
                        }
                        else
                        {
                            Pbstatus.Visibility = Visibility.Collapsed;
                            lblUserMessage.Text = "Unable to generate QR code, try again";
                        }
                    }
                }
                else
                {
                    if (create_retry_count < 2)
                    {
                        bw.RunWorkerAsync();
                    }
                    else
                    {
                        Pbstatus.Visibility = Visibility.Collapsed;
                        lblUserMessage.Text = "Unable to generate QR code, try again";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Tmr_status_Tick(object sender, EventArgs e)
        {
            try
            {
                timer_count++;
                int time_remains = 120 - timer_count;
                pbTimeRemaining.Value = time_remains;
                lblTimeRemains.Text = time_remains.ToString();

                if (timer_count > 20)
                {
                    if (timer_count % 8 == 0)
                    {
                        CheckStatus();
                    }
                }

                if (timer_count > 119)
                {
                    tmr_status.Stop();
                    Goback();
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void CheckStatus()
        {
            try
            {
                Dictionary<string, string> body = new Dictionary<string, string>();
                Dictionary<string, string> head = new Dictionary<string, string>();
                Dictionary<string, Dictionary<string, string>> requestBody = new Dictionary<string, Dictionary<string, string>>();

                body.Add("mid", Mid);
                body.Add("orderId", orderId);

                /*
                * Generate checksum by parameters we have in body
                * Find your Merchant Key in your Paytm Dashboard at https://dashboard.paytm.com/next/apikeys 
                */
                string paytmChecksum = Checksum.generateSignature(JsonConvert.SerializeObject(body), M_key);

                head.Add("signature", paytmChecksum);

                requestBody.Add("body", body);
                requestBody.Add("head", head);

                string post_data = JsonConvert.SerializeObject(requestBody);

                //For  Staging
                // string url = "https://securegw.paytm.in/v3/order/status";

                //For  Production 
                string url = "https://securegw.paytm.in/v3/order/status";

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                // webRequest.ContentLength = post_data.Length;

                using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestWriter.Write(post_data);
                }

                string responseData = string.Empty;

                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    responseData = responseReader.ReadToEnd();
                    log.Info(responseData);
                }

                paytm_transaction_status_response response = JsonConvert.DeserializeObject<paytm_transaction_status_response>(responseData);

                int.TryParse(response.body.resultInfo.resultCode, out int status_code);
                string cmd = "";
                switch (status_code)
                {
                    case 1:
                        // success 
                        cmd = $"update paytm_upi set transaction_id = '{response.body.txnId}', transaction_date = current_timestamp(), transaction_code = '{response.body.resultInfo.resultCode}', transaction_msg = '{response.body.resultInfo.resultMsg}' , sales_code = '{config.sales_code}' where order_id = '{orderId}'";
                        RunDbCommand(cmd);
                        config.paytm_upi_txnId = response.body.txnId;
                        tmr_status.Stop();
                        config.inmode = "UPI";
                        config.in_amt = config.tot_amt;
                        frmVending frm = new frmVending();
                        this.Close();
                        frm.Show();

                        break;
                    case 400:
                    case 402:
                        // pending
                        break;
                    default:
                        // failed
                        cmd = $"update paytm_upi set transaction_date = current_timestamp(), transaction_code = '{response.body.resultInfo.resultCode}', transaction_msg = '{response.body.resultInfo.resultMsg}'  where order_id = '{orderId}'";
                        RunDbCommand(cmd);
                        lblUserMessage.Text = "Transaction failed";
                        img_QR.Visibility = Visibility.Hidden;
                        tmr_status.Stop();
                        dgTimer.Visibility = Visibility.Hidden;
                        Task.Delay(3000).Wait();
                        Goback();
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

     //   private void btnBack_Click(object sender, RoutedEventArgs e)
       // {
         //   Goback();
        //}

        private void Goback()
        {
            try
            {
                tmr_status.Stop();
                frmOrderNow frm = new frmOrderNow();
                this.Close();
                frm.Show();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void RunDbCommand(string cmd)
        {
            try
            {
                acc.ExecuteCmd(cmd);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            tmr_status.Stop();
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

        private void VidPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            VidPlayer.Play();
        }

        private void VidPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            
            VidPlayer.Position = TimeSpan.FromSeconds(0);
        }
    }
}
