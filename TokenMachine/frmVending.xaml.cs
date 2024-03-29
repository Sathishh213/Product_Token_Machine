﻿using TokenMachine.Helpers.paytm.refund;
using log4net;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
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
using System.IO.Ports;
using Newtonsoft.Json;
using Paytm;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Drawing;
using Brushes = System.Drawing.Brushes;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace TokenMachine
{
    /// <summary>
    /// Interaction logic for frmVending.xaml
    /// </summary>
    public partial class frmVending : Window
    {

        public frmVending()
        {
            InitializeComponent();
            comport.DataReceived += new SerialDataReceivedEventHandler(billvalidator_DataReceived);
            coin_port.DataReceived += new SerialDataReceivedEventHandler(Coin_port_DataReceived);
        }

        decimal total_vended = 0;

        Access acc = new Access();
        modbus mb = new modbus();
        Reports report = new Reports();

        HttpClient httpClient = new HttpClient();

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DispatcherTimer tmr_msg = new DispatcherTimer();
        int msg_dispaly_time = 0;

        int issued_amount = 0;
        int note_val = 0, coin_val = 0;
        bool wait_time = false;
        bool coin_wait_time = false;
        int coin_issued_count = 0;
        int issued_balance = 0;

        SerialPort coin_port = new SerialPort();
        SerialPort comport = new SerialPort();

        void tmr_msg_Tick(object sender, EventArgs e)
        {
            try
            {
                msg_dispaly_time++;

                switch (msg_dispaly_time % 2)
                {
                    case 0:
                        lblMessage.Foreground = System.Windows.Media.Brushes.Black;
                        break;
                    case 1:
                        lblMessage.Foreground = System.Windows.Media.Brushes.Transparent;
                        break;

                    default:
                        lblMessage.Foreground = System.Windows.Media.Brushes.Black;
                        break;
                }

                if (msg_dispaly_time > 20)
                {
                    clear_msg_timer();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void display_msg(string msg)
        {
            try
            {
                tmr_msg.Stop();
                msg_dispaly_time = 0;
                Dispatcher.Invoke((Action)(() =>
                {
                    lblMessage.Text = msg;

                }));
                Audio.Speak(msg);
                tmr_msg.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void clear_msg_timer()
        {
            tmr_msg.Stop();
            msg_dispaly_time = 0;

            Dispatcher.Invoke((Action)(() =>
            {
                lblMessage.Text = "";
                lblMessage.Foreground = System.Windows.Media.Brushes.Black;
            }));

        }

        public async void DisplayMsg(string msg)
        {
            try
            {
                var sampleMessageDialog = new Dialog { Message = { Text = msg } };
                await DialogHost.Show(sampleMessageDialog, "myVendingDialog");
            }
            catch (Exception ex)
            {
                log.Error(ex);
                MessageBox.Show(msg);
            }
        }

        public void close()
        {
           frmThankyou frm = new frmThankyou();
           mb.Close();
           tmr_msg.Stop();
           this.Close();
           frm.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblMessage.Text = $"Printing your token...please wait!!!";
        }


        private void DialogHost_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                log.Info("Vending Process started");

                tmr_msg.Tick += tmr_msg_Tick; ;
                tmr_msg.Interval = new TimeSpan(0, 0, 0, 0, 500);

                string cmd = "";
                log.Info("Bill No: " + config.sales_code);

                for (int i = 0; i < config.ordered.Count; i++)
                {
                    config.ordered[i].Sno = i + 1;
                    config.ordered[i].amt = config.ordered[i].qty * config.ordered[i].price;
                    log.Info(config.ordered[i].Sno + "\t" + config.ordered[i].product_name + "\t" + config.ordered[i].qty);
                }

                PaidOrderDetailsDataGrid.ItemsSource = null;
                PaidOrderDetailsDataGrid.Items.Clear();
                PaidOrderDetailsDataGrid.ItemsSource = config.ordered;

                this.UpdateLayout();
                PaidOrderDetailsDataGrid.UpdateLayout();
                VendingAsync();

            }
            catch (Exception ex)
            {
                log.Error(ex);
                DisplayMsg("Machine running into trouble, Please try aftersome times.");
                close();
            }
        }

        private async void VendingAsync()
        {
            int bal = -1;
            try
            {

                //VirtualizingStackPanel.SetIsVirtualizing(lstItems, false);

                string cmd = "";

                TextBlock lblStatus = null;
                string msg = "";
                int wait_count = 0;
                await Task.Delay(000);
                //if (machine_ready()) // machine_ready()
                //{
                    try
                    {
                        ListView btnlst = null; string value = null;
                        
                        //string bindedvalue = string.Format("[{0}]\n", value);//string
                        //mb.Open(Hardware.vending_port, Hardware.vending_BaudRate, Hardware.vending_DataBits, Hardware.vending_Parity, Hardware.vending_StopBits);
                        //bool write = mb.SerialCmdSendProductandQuantity(bindedvalue);
                        //mb.Close();

                        wait_count = 0; int quantity = 0; int soldout = 0;
                        for (int i = 0; i < config.ordered.Count; i++)
                        {
                            cmd = $"select * from mst_product where product_id = {config.ordered[i].product_id}";
                            DataTable dt_product = acc.GetTable(cmd);
                            if (dt_product.Rows.Count > 0)
                            {
                                quantity = Convert.ToInt32(dt_product.Rows[0]["stock"]) - config.ordered[i].qty;
                                soldout = quantity > 0 ? 0 : 1;
                                try
                                {
                                    cmd = $"update mst_product p set p.stock = {quantity} , soldout = {soldout}  where p.product_id = {config.ordered[i].product_id}";
                                    acc.ExecuteCmd(cmd);
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }

                            for (int j = 1; j <= config.ordered[i].qty; j++)
                            {
                                bool delivery = true;
                                if (delivery)
                                {

                                    config.ordered[i].vend = config.ordered[i].vend + 1;
                                    config.ordered[i].amt = config.ordered[i].vend * config.ordered[i].price;
                                    //lblStatus.Text = "Vended...";

                                }
                            }

                        }
                    }
                    catch (Exception ex2)
                    {
                        log.Error(ex2);
                    }
                //}
                //else
                //{
                //    log.Info("Machine out of order");
                //    DisplayMsg("Machine out of order");
                //}

                try
                {

                    int t_qty = 0;
                    t_qty = (from k in config.ordered select k.vend).Sum();
                    total_vended = (from k in config.ordered select k.amt).Sum();
                    log.Info("Total Vended Qty : " + t_qty + " Amt : " + total_vended);

                    List<product_lineItem> product_LineItems = new List<product_lineItem>();
                    for (int i = 0; i < config.ordered.Count; i++)
                    {
                        product_lineItem product = new product_lineItem { price = config.ordered[i].amt, product_id = config.ordered[i].product_id, product_name = config.ordered[i].product_name, quantity = config.ordered[i].qty };
                        product_LineItems.Add(product);
                    }
                    string product_LineItems_JsonData = JsonConvert.SerializeObject(product_LineItems);

                    if (t_qty > 0)
                    {
                        cmd = @"insert into sales_order(product_lineitems, total_amount , total_quantity , order_datetime , payment_method , transaction_id, machine_id) 
                                value ('" + product_LineItems_JsonData + "' ," + total_vended + "," + t_qty + ",now() ,'" + config.inmode + "','" + config.paytm_upi_txnId + "','" + config.machine_id + "' )";
                        acc.ExecuteCmd(cmd);
                        //LEDOn();
                    }
                    bal = (int)(config.in_amt - total_vended);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (total_vended > 0 && bal == 0)
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += new PrintPageEventHandler(PrintPageHandler);

                try
                {
                    printDocument.Print();
                    Audio.Speak("Get Your Token, !!! Thank you, Visit again...");
                    lblMessage.Text = "Token Generated";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Printing error: " + ex.Message);
                    Audio.Speak("Sorry for the Inconvenience, Visit again...");
                    lblMessage.Text = "Unable to Generate the Token";
                }
            }
            else
            {
                Audio.Speak("Sorry for the Inconvenience, Visit again...");
                lblMessage.Text = "Unable to Generate the Token";
            }

            config.tot_amt = total_vended;
            //close();
        }

        public bool machine_ready()
        {
            bool state = false;

            try
            {
                state = mb.Open(Hardware.vending_port, Hardware.vending_BaudRate, Hardware.vending_DataBits, Hardware.vending_Parity, Hardware.vending_StopBits);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            mb.Close();
            return state;
        }


        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) child = GetVisualChild<T>(v);
                if (child != null) break;
            }
            return child;
        }



        #region billDispnser
        bool IsDispenserReady = false;

        public bool issue_amount(int bal)
        {
            if (bal > 0)
            {

                try
                {
                    if (!comport.IsOpen)
                    {
                        comport.PortName = "COM3";
                        comport.BaudRate = 9600;
                        comport.DataBits = 8;
                        comport.Parity = Parity.None;
                        comport.StopBits = StopBits.One;
                        comport.Handshake = Handshake.None;
                        comport.Open();
                        if (!comport.IsOpen)
                        {
                            log.Info("Dispenser Port connected failed");
                            wait_time = false;
                            return false;
                        }
                    }


                    int count = bal / note_val;

                    log.Info("Dispenser Requested Note : " + count + ", Bal : " + bal + ", Note_Value : " + note_val);

                    IsDispenserReady = false;
                    BillDispenser_Status();

                    int status_wait = 0;
                    while (!IsDispenserReady && status_wait < 30)
                    {
                        status_wait++;
                        Thread.Sleep(100);
                    }

                    if (IsDispenserReady)
                    {
                        string cmd = "02 30 30 42 30";

                        foreach (char _eachChar in count.ToString("000"))
                        {
                            int value = Convert.ToInt32(_eachChar);
                            string hexOutput = String.Format("{0:X}", value);
                            cmd = cmd + " " + hexOutput;
                        }
                        cmd = cmd + " " + find_checksum(cmd) + " 03";

                        write_data(cmd);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    report.SendMail("Note Dispenser Error", "Error : " + ex.Message);
                    wait_time = false;
                    return false;
                }
            }
            return false;
        }

        public void BillDispenser_Status()
        {
            try
            {
                log.Info("Clear Dispenser Status");
                write_data("02 30 30 49 30 30 30 33 6E 03"); // Clear Command
                Thread.Sleep(100);
                log.Info("Read Dispenser Status");
                write_data("02 30 30 53 30 30 30 30 75 03"); // Read Status
            }
            catch (Exception ex)
            {
                log.Error(ex);
                wait_time = false;
                IsDispenserReady = false;
            }
        }

        public static byte[] StrToByteArray(string str)
        {
            Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
            for (int i = 0; i <= 255; i++)
                hexindex.Add(i.ToString("X2"), (byte)i);

            List<byte> hexres = new List<byte>();
            for (int i = 0; i < str.Length; i += 3)
                hexres.Add(hexindex[str.Substring(i, 2)]);

            return hexres.ToArray();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        public string HextoAscii(string hexString)
        {
            string sb = "";
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string hs = hexString.Substring(i, 2);
                sb += Convert.ToChar(Convert.ToUInt32(hs, 16));
            }
            return sb;
        }

        public bool verfiy_checksum(byte[] buf)
        {
            bool ret = false;


            if (buf.Length > 2)
            {
                int b;
                b = buf[0] ^ buf[1];
                for (int i = 2; i < buf.Length - 1; i++)
                {
                    b = b ^ buf[i];
                }

                if (b == buf[buf.Length - 1])
                {
                    ret = true;
                }
            }

            return ret;
        }

        public string find_checksum(string cmd)
        {
            string rt = "";
            byte[] buf = StrToByteArray(cmd);
            if (buf.Length > 2)
            {
                int b;
                b = buf[0] + buf[1];
                for (int i = 2; i < buf.Length; i++)
                {
                    b = b + buf[i];
                }

                b = b % 256;
                rt = String.Format("{0:X}", b);
            }
            return rt;
        }

        public void write_data(string s)
        {
            if (comport.IsOpen)
            {
                log.Info("Dispenser <- " + s);
                byte[] bytesToSend = StrToByteArray(s.ToUpper());
                comport.Write(bytesToSend, 0, bytesToSend.Length);
            }
            else
            {
                log.Info("Dispenser port Disconnected");
            }
        }

        public void billvalidator_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(50);
                int length = comport.BytesToRead;
                byte[] buf = new byte[length];
                comport.Read(buf, 0, length);
                string s = ByteArrayToString(buf);
                log.Info("Dispenser -> " + s);
                if (s.Length > 5)
                {
                    data_action(s.ToUpper());
                    //if (verfiy_checksum(buf))
                    //{
                    //    write_data("06");
                    //    data_action(s.ToUpper());
                    //}
                    //else
                    //{
                    //    acc.writelog("Dispenser -> " + "Check sum Faild");
                    //}
                }
                else
                {
                    if (s.Trim() == "15")
                    {
                        writelbl("", "Invalid Command");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        private void data_action(string data)
        {
            string[] response_data = data.Trim().Split(' ');
            string response = response_data[3];

            string msg = "";
            string cmd = "";
            switch (response)
            {

                case "06":
                    writelbl("", "");
                    break;
                case "15":
                    writelbl("", "Ack Faild");
                    break;
                case "62":


                    string dispense = HextoAscii(response_data[5] + response_data[6] + response_data[7]);

                    issued_amount = issued_amount + (Convert.ToInt32(dispense) * note_val);
                    wait_time = false;

                    cmd = "update tbl_note set note_available = note_available - " + dispense + " , updatedon = now(), is_viewed = 0 where IsActive = 1 and machine_id = " + config.machine_id;
                    acc.ExecuteCmd(cmd);

                    msg = msg + dispense + " Note Issued";
                    log.Info(msg);

                    break;
                case "73":
                    string p4 = response_data[4];
                    if (p4 == "72") // Ready
                    {
                        IsDispenserReady = true;
                    }
                    else if (p4 == "65") // Error
                    {
                        IsDispenserReady = false;
                        string p5 = response_data[5];
                        string err = error_code(p5);
                        log.Info("Dispenser Error : " + err);

                        cmd = "update tbl_note set  is_stoped = 0, error_msg = '" + err + "', updatedon = now(), is_viewed = 0 where IsActive = 1 and machine_id = " + config.machine_id;
                        acc.ExecuteCmd(cmd);

                    }
                    break;

                case "45":
                    string error = error_code(response_data[4]);
                    string dispense_note = HextoAscii(response_data[5] + response_data[6] + response_data[7]);

                    log.Info("Dispense error : " + error + ", note Dispensed : " + dispense_note);


                    issued_amount = issued_amount + (Convert.ToInt32(dispense_note) * note_val);
                    //    msg = msg + dispense + " Note Issued,        " + reject + " Note send to Rejection tray";
                    wait_time = false;

                    cmd = "update tbl_note set note_available = note_available - " + dispense_note + " , is_stoped = 0, error_msg = '" + error + "',  updatedon = now(), is_viewed = 0 where IsActive = 1 and machine_id = " + config.machine_id;
                    acc.ExecuteCmd(cmd);

                    writelbl(msg, error);
                    break;
                //case "44":
                //    string p4 = response_data[4];
                //    if (p4 == "30" || p4 == "31")
                //    {
                //        writelbl("Test Run Success", "");
                //    }
                //    else
                //    {
                //        writelbl("", error_code(p4));
                //        wait_time = false;
                //    }
                //    break;
                //case "45":
                //    if (!(response_data[8] == "30" || response_data[8] == "31"))
                //    {
                //        err = error_code(response_data[8]);
                //        if (response_data[8] == "38")
                //        {
                //            string cmd1 = "update trn_dispense d set  d.rejected = d.rejected + (d.cash_filled - d.dispensed - d.rejected) , d.updated_on = now(), d.is_viewed = 0  where d.IsActive = 1 and d.machine_id = " + config.machine_id;
                //            acc.ExecuteCmd(cmd1);
                //        }
                //    }

                //    string dispense = HextoAscii(response_data[6] + response_data[7]);
                //    string reject = HextoAscii(response_data[10] + response_data[11]);

                //    string cmd = "update trn_dispense d set d.dispensed =  d.dispensed + " + dispense + " , d.rejected = d.rejected + " + reject + " where IsActive = 1 and machine_id = " + config.machine_id; // for database update
                //    acc.ExecuteCmd(cmd);
                //    issed_amt = issed_amt + (Convert.ToInt32(dispense) * note_val);
                //    msg = msg + dispense + " Note Issued,        " + reject + " Note send to Rejection tray";

                //    if (response_data[9] == "31")
                //    {
                //        err += err.Length < 2 ? "Cash near to end" : "";
                //        cmd = "update trn_dispense d set  d.rejected = d.rejected + (d.cash_filled - d.dispensed - d.rejected) , d.updated_on = now(), d.is_viewed = 0  where d.IsActive = 1 and d.machine_id = " + config.machine_id;
                //        acc.ExecuteCmd(cmd);
                //        //err = err + "Cash near to end";
                //    }
                //    wait_time = false;
                //    writelbl(msg, err);
                //    break;
                //case "46":
                //    if (!(response_data[5] == "30" || response_data[5] == "31"))
                //    {
                //        err = error_code(response_data[5]);
                //    }
                //    writelbl("", err);
                //    msg = ""; // sensor1_status(response_data[6]) + sensor2_status(response_data[7]);
                //    break;

                //case "47":
                //    msg = "Version : " + Convert.ToChar(Convert.ToInt32(response_data[5], 16)) + "." + Convert.ToChar(Convert.ToInt32(response_data[6], 16));
                //    writelbl(msg, err);
                //    break;

                //case "76":
                //case "77":
                //    if (!(response_data[8] == "30" || response_data[8] == "31"))
                //    {
                //        err = error_code(response_data[5]);
                //    }
                //    else
                //    {
                //        msg = "One note Enject to Rejection Tray";
                //    }
                //    writelbl(msg, err);
                //    break;

                default:

                    break;
            }

        }

        public string error_code(string code)
        {
            string retn = "";
            switch (code)
            {
                case "30":
                    break;
                case "31":
                    retn = "no bills during being dispensed by host commend.";
                    break;
                case "32":
                    retn = "Jam";
                    break;
                case "33":
                    retn = "JChain";
                    break;
                case "34":
                    retn = "Half";
                    break;
                case "35":
                    retn = "Short";
                    break;
                case "36":
                    retn = "no bills during being dispensed by start button.";
                    break;
                case "37":
                    retn = "Double";
                    break;
                case "38":
                    retn = "over count 4000 pcs";
                    break;
                case "39":
                    retn = "E11 -> Receiving error during commnunication test.";
                    break;
                case "41":
                    retn = "E12 -> Encoder_error";
                    break;
                case "42":
                    retn = "E13 -> IR_LED_L_error";
                    break;
                case "43":
                    retn = "E14 ->IR_LED_R_error";
                    break;
                case "44":
                    retn = "E15 -> IR_Sensor_L_error";
                    break;
                case "45":
                    retn = "";
                    break;
                case "46":
                    retn = "E16 -> IR_Sensor_R_error";
                    break;
                case "47":
                    retn = "E17 -> IR_Different_error";
                    break;
                case "48":
                    retn = "BillLowLevel_warning";
                    break;
                default:
                    break;
            }

            return retn;
        }

        public void writelbl(string msg, string warn)
        {
            if (warn.Trim().Length > 0)
            {
                msg = msg + "    " + warn;
            }

            log.Info(msg);
        }

        #endregion

        #region CoinDispenser
        //bool coin_wait_time = false;

        //int coin_issued_count = 0;

        public void IssueCoins(int CoinCount)
        {

            if (CoinCount > 0 && CoinCount < 256)
            {

                try
                {
                    if (!coin_port.IsOpen)
                    {
                        coin_port.PortName = "COM5";
                        coin_port.BaudRate = 9600;
                        coin_port.DataBits = 8;
                        coin_port.Parity = Parity.Even;
                        coin_port.StopBits = StopBits.One;
                        coin_port.Handshake = Handshake.None;
                        coin_port.ReadTimeout = 50;
                        coin_port.Open();
                        if (!coin_port.IsOpen)
                        {
                            log.Info("Coin Port connected failed");
                            coin_wait_time = false;
                        }
                    }

                    //coin_is_reset = false;
                    //ResetCoinDispenser();


                    //int status_wait = 0;
                    //while (!coin_is_reset && status_wait < 11)
                    //{
                    //    status_wait++;
                    //    Thread.Sleep(100);
                    //}


                    //if (coin_is_reset)
                    //{



                    string cmd = "05 10 00 14 " + String.Format("{0:X2}", CoinCount);
                    cmd = cmd + " " + find_checksum(cmd);
                    coin_issued_count = 0;
                    Write_Coin(cmd);
                    coin_wait_time = true;
                    //}
                    //else
                    //{
                    //    log.Info("Coin reset faild");
                    //    coin_wait_time = false;
                    //}
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    coin_wait_time = false;
                }
            }
            else
            {
                log.Info("Coin count is not enough");
                coin_wait_time = false;
            }

        }

        public void ResetCoinDispenser()
        {
            try
            {
                string cmd = "05 10 00 12 00 27";
                log.Info("Reset Coin Status");
                Write_Coin(cmd);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void Write_Coin(string s)
        {
            try
            {
                if (coin_port.IsOpen)
                {
                    log.Info("Coin <- " + s);
                    byte[] bytesToSend = StrToByteArray(s.ToUpper());
                    coin_port.Write(bytesToSend, 0, bytesToSend.Length);
                }
                else
                {
                    log.Info("Coin port Disconnected");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Coin_port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

                Thread.Sleep(49);
                int length = coin_port.BytesToRead;
                byte[] buf = new byte[length];
                coin_port.Read(buf, 0, length);

                string s = "";

                if (length % 6 == 0)
                {
                    for (int i = 0; i < length; i = i + 6)
                    {
                        byte[] bt = new byte[6];
                        Buffer.BlockCopy(buf, i, bt, 0, 6);

                        s = ByteArrayToString(bt);
                        s = s.ToUpper();
                        log.Info("Coin -> " + s);
                        CoinAction(s);
                    }
                }
                else
                {
                    s = ByteArrayToString(buf);
                    s = s.ToUpper();
                    log.Info("Coin -> " + s);
                    if (s.Length > 10)
                    {
                        CoinAction(s);
                    }
                    else
                    {
                        if (s.Trim().Length == 2)
                        {
                            //coin_is_reset = true;
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void CoinAction(string data)
        {
            try
            {
                string[] response_data = data.Trim().Split(' ');
                if (response_data.Length > 4)
                {
                    string response = response_data[3];

                    switch (response)
                    {
                        case "07":
                            coin_issued_count++;

                            break;

                        case "08":
                            coin_wait_time = false;
                            break;

                        case "04":
                            coin_wait_time = false;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        #endregion



        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (total_vended > 0)
            {
                Audio.Speak("Collect you product, !!! Thank you, Visit again...");
            }
            else
            {
                Audio.Speak("Thank you, Visit again...");
            }
            this.UpdateLayout();

            config.tot_amt = total_vended;

            close();
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                tmr_msg.Stop();
                log.Info("Vending Completed");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            //Graphics graphics = e.Graphics;
            //Font font = new Font("Arial", 12);
            //string textToPrint = "Hello, Thermal Printer!";
            //graphics.DrawString(textToPrint, font, System.Drawing.Brushes.Black, new PointF(10, 10));

            Graphics graphics = e.Graphics;

            Font regular = new Font(System.Drawing.FontFamily.GenericSansSerif, 10.0f, System.Drawing.FontStyle.Regular);
            Font bold = new Font(System.Drawing.FontFamily.GenericSansSerif, 10.0f, System.Drawing.FontStyle.Bold);

            graphics.DrawString("CHATTERS CLUB CAFE", bold, Brushes.Black, 20, 10);
            graphics.DrawString("Coimbatore", regular, Brushes.Black, 30, 30);
            graphics.DrawString("Mobile: +91 91597 67791", regular, Brushes.Black, 110, 50);
            graphics.DrawLine(Pens.Black, 80, 70, 320, 70);
            graphics.DrawString("TOKEN NO", bold, Brushes.Black, 110, 80);
            graphics.DrawLine(Pens.Black, 80, 100, 320, 100);

            //print items
            graphics.DrawString("S.no | Items                      | Price | Qty | Sub Total |", bold, Brushes.Black, 10, 120);
            graphics.DrawLine(Pens.Black, 10, 140, 430, 140);

            for (int i = 0; i < config.ordered.Count; i++)
            {
                string toprint = string.Format("{0} {1} {2} {3} {4}", config.ordered[i].Sno, config.ordered[i].product_name, config.ordered[i].price, config.ordered[i].qty, config.ordered[i].amt);
                graphics.DrawString(toprint, regular, Brushes.Black, 20, 150 + i * 20);
            }
            graphics.DrawLine(Pens.Black, 10, 140, 430, 140);
            graphics.DrawString(string.Format("Total Amount : {0}", (from k in config.ordered select k.amt).Sum()), bold, Brushes.Black, 110, 180 + (config.ordered.Count * 20));
            graphics.DrawLine(Pens.Black, 10, 140, 430, 140);

            //print footer
            //...

            regular.Dispose();
            bold.Dispose();

            // Check to see if more pages are to be printed.
            //e.HasMorePages = (itemList.Count > 20);
        }

    }
}
