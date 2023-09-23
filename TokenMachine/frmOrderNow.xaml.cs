using log4net;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TokenMachine.Helpers;

namespace TokenMachine
{
    /// <summary>
    /// Interaction logic for frmOrderNow.xaml
    /// </summary>
    public partial class frmOrderNow : Window
    {

        public frmOrderNow()
        {
            InitializeComponent();

            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            bw_loadtable.DoWork += Bw_loadtable_DoWork;
            bw_loadtable.RunWorkerCompleted += Bw_loadtable_RunWorkerCompleted;

            bw_plc.DoWork += Bw_plc_DoWork;
            bw_plc.RunWorkerCompleted += Bw_plc_RunWorkerCompleted;

        }


        ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Access acc = new Access();
        int ProcessCode = -1;
        DataTable dt;
        Storyboard sb = new Storyboard();
        public List<order_item> ordered = new List<order_item>();
        // List<Products> lstProducts = new List<Products>();
        BackgroundWorker bw = new BackgroundWorker();
        BackgroundWorker bw_loadtable = new BackgroundWorker();
        BackgroundWorker bw_plc = new BackgroundWorker();
        bool IsPLC = false;
        bool IsPLCComplete = false;
        modbus mb = new modbus();

        public async void DisplayMsg(string msg)
        {
            try
            {
                var sampleMessageDialog = new Dialog { Message = { Text = msg } };
                await DialogHost.Show(sampleMessageDialog, "frmOrderNowDialog");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Bw_loadtable_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                config.lstProducts = new List<Products>();
                string cmd = "";

                //cmd = @"select  m.product_id, m.category_id, m.cabin_id, m.product_name, m.price, stock , case when stock > 0  then 1 else 0 end as is_stock ,  exp, soldout, fix, img_path, tamil_path, info_path, offer_type , offer, product_offer
                //        from (
                //        select s.product_id, p.category_id, cabin_id, p.product_name, p.price,   sum( case when exp > now() and soldout = 0 and fix = 0 then  stock else 0 end ) as stock, max(exp) AS exp , min(soldout) as soldout, min(fix) as fix, img_path, tamil_path, info_path
                //        from  vw_stock_settings s 
                //        inner join mst_product p on p.product_id = s.product_id
                //        where s.machine_id= @ma group by s.product_id ) m 
                //        left outer join vw_offer_order o on o.product_id = m.product_id
                //        order by  is_stock desc, product_id";



                // cmd = cmd.Replace("@ma", " " + config.machine_id.ToString() + " ");
                // concat('" + AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "\\\\") + "', img_path) as 

                cmd = @"SELECT product_id, product_name ,price, sum(stock) as stock, img_path  from (" +
                        "Select product_id, product_name ,price, img_path," +
                        "case when soldout > 0 then 0 else stock end as stock from mst_product p" +
                        ") as M group by M.product_id;";

                dt = acc.GetTable(cmd);

                foreach (DataRow dr in dt.Rows)
                {
                    Products pd = new Products();
                    pd.product_id = Convert.ToInt32(dr["product_id"].ToString());
                    pd.product_name = dr["product_name"].ToString();
                    pd.price = Convert.ToDecimal(dr["price"].ToString());
                    pd.Stock = Convert.ToInt32(dr["stock"].ToString());

                    pd.img_path = AppDomain.CurrentDomain.BaseDirectory + dr["img_path"].ToString();
                    //pd.offer_type = dr["offer_type"].ToString();
                    //pd.offer = dr["offer"].ToString();
                    //pd.product_offer = dr["product_offer"].ToString();
                    //pd.Exp = Convert.ToDateTime(dr["Exp"].ToString()).ToString("dd-MM-yyyy");
                    //pd.tamil_path = AppDomain.CurrentDomain.BaseDirectory + dr["tamil_path"].ToString();
                    //pd.info_path = AppDomain.CurrentDomain.BaseDirectory + dr["info_path"].ToString();

                    //if (Convert.ToString(dr["offer_type"]).Trim().Length > 0) // && dr["offer_type"].ToString().Trim() != "Item"
                    //{
                    //    pd.show_offer = "Visible";
                    //}
                    //else
                    //{
                    //    pd.show_offer = "Hidden";
                    //}

                    pd.show_offer = "Hidden";

                    if (pd.Stock == 0)
                    {
                        pd.IsAvailable = false;
                        pd.opacity = 0.4;

                        if (pd.Stock < 1 || pd.soldout == 1)
                        {
                            pd.show_offer = "Visible";
                            pd.offer = "Sold Out";
                        }
                        else if (Convert.ToDateTime(dr["Exp"].ToString()).Date <= DateTime.Now.Date)
                        {
                            pd.show_offer = "Visible";
                            pd.offer = "Expired";
                        }

                    }
                    else
                    {
                        pd.IsAvailable = true;
                        pd.opacity = 1;
                    }

                    pd.qty = "Qty";
                    pd.back_color = "White";

                    config.lstProducts.Add(pd);

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Bw_loadtable_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                load_item();
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


                Dispatcher.Invoke((Action)(() =>
                {
                    MyList.ItemsSource = null;
                    MyList.Items.Clear();

                }));

                foreach (Products pd in config.lstProducts)
                {
                    var item = (from k in ordered where k.product_id == pd.product_id select k).FirstOrDefault();

                    if (item != null)
                    {
                        pd.qty = item.qty.ToString();
                        pd.back_color = "Gray";
                    }
                    else
                    {
                        pd.qty = "Qty";
                        pd.back_color = "White";
                    }

                    Dispatcher.Invoke((Action)(() =>
                    {
                        MyList.Items.Add(pd);
                    }));
                }
                ProcessCode = processData();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            display_cart();

        }

        private void Bw_plc_DoWork(object sender, DoWorkEventArgs e)
        {
            IsPLC = PLCCheck();
        }

        private void Bw_plc_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsPLCComplete = true;
        }


        public void load_item()
        {
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (config.ordered.Count > 0)
                {
                    ordered = config.ordered;
                    foreach (order_item odr in ordered)
                    {
                        MyCart.Items.Add(odr);
                    }
                }
                if (bw_loadtable.IsBusy != true)
                {
                    bw_loadtable.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }



        public void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                var txt = ((Grid)(btn.Parent)).Children.OfType<TextBlock>().FirstOrDefault();
                if (txt != null)
                {
                    int v = 0;
                    int.TryParse(txt.Text.ToString(), out v);
                    int pre_qty = 0;

                    var exist = (from k in ordered where k.product_id == Convert.ToInt32(btn.Tag.ToString()) select k).FirstOrDefault();
                    int tot_item = (from k in ordered select k.qty).Sum();
                    if (exist != null)
                    {
                        pre_qty = exist.qty;
                    }

                    if (((tot_item - pre_qty) + v) < Properties.Settings.Default.max_quantity)
                    {
                        if (v < Properties.Settings.Default.max_quantity)
                        {
                            int max_stock = 0;
                            DataRow pdr = dt.Select("product_id = " + btn.Tag.ToString()).FirstOrDefault();
                            if (dt.Rows.Count > 0)
                            {

                                int.TryParse(pdr["Stock"].ToString(), out max_stock);
                            }
                            if (v < max_stock)
                            {
                                v++;
                                txt.Text = v.ToString();
                                if (exist != null)
                                {
                                    exist.qty = v;
                                    exist.amt = (v * exist.price) - (v * exist.discount);
                                }

                                MyCart.Items.Refresh();
                                foreach (Products itm in MyList.Items)
                                {
                                    var ext = (from k in ordered where k.product_id == itm.product_id select k).FirstOrDefault();
                                    if (ext != null)
                                    {
                                        itm.back_color = "Gray";
                                        itm.qty = ext.qty.ToString();
                                    }
                                }
                                MyList.Items.Refresh();


                                if (v == 1)
                                {
                                    Audio.Speak(pdr["product_name"].ToString() + "! " + v.ToString() + "Number");
                                }
                                else
                                {
                                    Audio.Speak(v.ToString() + "Number");
                                }

                                display_cart();

                                if (Properties.Settings.Default.max_quantity == 1)
                                {
                                    Buy();
                                }
                            }
                            else
                            {
                                DisplayMsg(max_stock + " Items only Available");
                            }
                        }
                    }
                    else
                    {
                        DisplayMsg(Properties.Settings.Default.max_quantity + " Items only Allowed");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        public void btnLess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                var txt = ((Grid)(btn.Parent)).Children.OfType<TextBlock>().FirstOrDefault();
                if (txt != null)
                {
                    int cnt = 0;
                    int.TryParse(txt.Text.ToString(), out cnt);
                    if (cnt > 0)
                    {
                        var exist = (from k in ordered where k.product_id == Convert.ToInt32(btn.Tag.ToString()) select k).FirstOrDefault();
                        if (exist != null)
                        {
                            if (cnt > 1)
                            {
                                cnt--;
                                exist.qty = cnt;
                                exist.amt = (cnt * exist.price) - (cnt * exist.discount);

                                MyCart.Items.Refresh();
                                foreach (Products itm in MyList.Items)
                                {
                                    var ext = (from k in ordered where k.product_id == itm.product_id select k).FirstOrDefault();
                                    if (ext != null)
                                    {
                                        itm.back_color = "Gray";
                                        itm.qty = ext.qty.ToString();
                                    }
                                }
                                MyList.Items.Refresh();
                            }
                            else
                            {
                                ordered.Remove(exist);
                                MyCart.Items.Remove(exist);
                                foreach (Products itm in MyList.Items)
                                {
                                    if (itm.product_id == int.Parse(btn.Tag.ToString()))
                                    {
                                        itm.qty = "0";
                                        itm.back_color = "White";
                                    }

                                    var ext = (from k in ordered where k.product_id == itm.product_id select k).FirstOrDefault();
                                    if (ext != null)
                                    {
                                        itm.back_color = "Gray";
                                    }
                                }
                                MyList.Items.Refresh();
                            }

                        }
                        else
                        {
                            cnt--;
                        }

                        txt.Text = cnt.ToString();
                        display_cart();
                    }
                    else
                    {
                        txt.Text = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        private void btnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                var item = (from k in config.lstProducts where k.product_id == (int)btn.Tag select k).FirstOrDefault();

                if (File.Exists(item.info_path))
                {
                    DisplayInfo(item.info_path);
                }
                else
                {
                    DisplayMsg("Sorry, Information not found");
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            e.Handled = true;
        }

        public async void DisplayInfo(string info_path)
        {
            try
            {
                var sampleMessageDialog = new DialogImage { img_info = { Source = new BitmapImage(new Uri(info_path)) } };
                await DialogHost.Show(sampleMessageDialog, "frmOrderNowDialog");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                string pid = btn.Tag.ToString();

                var item = (from k in ordered where k.product_id == Convert.ToInt32(pid) select k).FirstOrDefault();

                int qty = 0;
                if (item != null)
                {
                    qty = item.qty;
                }

                int tot_item = (from k in ordered select k.qty).Sum();

                var product = (from k in config.lstProducts where k.product_id == Convert.ToInt32(pid) select k).FirstOrDefault();

                int max_stock = 0;
                max_stock = product.Stock;

                if (product.IsAvailable == true)
                {
                    if (qty < max_stock)
                    {

                        if (((tot_item - qty) + qty) < Properties.Settings.Default.max_quantity)
                        {
                            if (item != null)
                            {
                                qty++;
                                item.qty = qty;
                                item.amt = (qty * item.price) - (qty * item.discount);

                                if (qty == 1)
                                {
                                    Audio.Speak(item.product_name + "! " + qty.ToString() + "Number");
                                }
                                else
                                {
                                    Audio.Speak(qty.ToString() + "Number");
                                }


                                MyCart.Items.Refresh();

                            }
                            else
                            {
                                qty++;
                                DataRow dr = dt.Select("product_id = " + pid).FirstOrDefault();
                                if (dr != null)
                                {
                                    order_item o_item = new order_item();
                                    o_item.product_id = Convert.ToInt32(pid);
                                    o_item.product_name = dr["product_name"].ToString();
                                    o_item.price = Convert.ToDecimal((dr["price"].ToString()));
                                    o_item.qty = qty;

                                    o_item.discount = 0;


                                    o_item.amt = (qty * o_item.price) - (qty * o_item.discount);
                                    o_item.img_path = AppDomain.CurrentDomain.BaseDirectory + dr["img_path"].ToString();
                                    ordered.Add(o_item);

                                    MyCart.Items.Add(o_item);

                                    if (qty == 1)
                                    {
                                        Audio.Speak(o_item.product_name + "! " + qty.ToString() + "Number");
                                    }
                                    else
                                    {
                                        Audio.Speak(qty.ToString() + "Number");
                                    }

                                    if (Properties.Settings.Default.max_quantity == 1)
                                    {
                                        Buy();
                                    }
                                }

                            }
                        }
                        else
                        {
                            DisplayMsg(Properties.Settings.Default.max_quantity + " Items only Allowed");
                        }
                    }
                    else
                    {
                        DisplayMsg(max_stock + " Items only Available");
                    }
                }
                else
                {
                    DisplayMsg("Stock sold out");
                }
                display_cart();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var exist = (from k in ordered where k.product_id == Convert.ToInt32(((Button)sender).Tag.ToString()) select k).FirstOrDefault();
                if (exist != null)
                {
                    ordered.Remove(exist);
                    MyCart.Items.Remove(exist);
                    foreach (Products itm in MyList.Items)
                    {
                        if (itm.product_id == exist.product_id)
                        {
                            itm.qty = "0";
                            itm.back_color = "White";
                        }
                    }
                    MyList.Items.Refresh();

                    display_cart();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnItem_Click(object sender, RoutedEventArgs e)
        {
            btnAddToCart_Click(sender, e);
        }

        public void display_cart()
        {
            try
            {
                txtTotQty.Text = (from k in ordered select k.qty).Sum().ToString();
                txtTotAmt.Text = (from k in ordered select k.amt).Sum().ToString("#0.00");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            Buy();
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
        private void Buy(string paymentWay = "paymentpage")
        {
            try
            {
                int tot_item = (from k in ordered select k.qty).Sum();
                decimal tot_amt = (from k in ordered select k.qty * k.price).Sum();
                if (tot_amt > 0 && tot_item > 0)
                {
                    config.tot_amt = tot_amt;
                    config.tot_item = tot_item;
                    config.ordered = ordered;


                    //if (config.sales_code.Trim().Length < 4)
                    //{
                    //    string cmd = "SELECT  concat('" + config.machine_id + "' , LPAD( count(sales_code) + 1, 6, 0 )) as sales_code FROM tbl_sales where machine_id = '" + config.machine_id + "'";
                    //    config.sales_code = acc.GetValue(cmd);
                    //}


                    //if (!Properties.Settings.Default.plc_check_before_order)
                    //{
                    IsPLC = true;
                    IsPLCComplete = true;
                    //}
                    //else
                    //{
                    //    if (!IsPLCComplete)
                    //    {
                    //        DisplayMsg("Please wait, checking machine status");
                    //    }
                    //    int count = 0;
                    //    while (!IsPLCComplete && count < 5)
                    //    {
                    //        Task.Delay(500).Wait();
                    //        count++;
                    //    }
                    //}

                    if (IsPLC)
                    {
                        switch (paymentWay)
                        {
                            case "Account":
                                //{
                                //    frmRFIDReader frm = new frmRFIDReader();
                                //    this.Close();
                                //    frm.Show();
                                //}
                                break;
                            case "UPI":
                                {
                                    if (machine_ready())
                                    {
                                       // frmUPIPayTM frm = new frmUPIPayTM();
                                       frmOrderConfirmation frm = new frmOrderConfirmation();
                                        this.Close();
                                        frm.Show();
                                    }
                                    else
                                    {
                                        log.Info("Machine Out Of Order");
                                        DisplayMsg("Machine out of Order");
                                    }
                                    
                                }
                                break;
                            default:
                                //{
                                //    frmPaymentWay frm = new frmPaymentWay();
                                //    this.Close();
                                //    frm.Show();
                                //}
                                break;
                        }


                    }
                    else
                    {
                        DisplayMsg("Sorry machine out of order");
                    }

                }
                else
                {
                    DisplayMsg("No item in the cart \nPlease select atleast one item to buy.");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }


        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frmHomeScreen frm = new frmHomeScreen();
                this.Close();
                frm.Show();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void Window_TouchDown(object sender, TouchEventArgs e)
        {

        }


        public int processData()
        {
            int process = -1;
            modbus mb = new modbus();
            try
            {
                ushort start = 0; // 30000
                short[] values = new short[2];
                int address = 1;
                ushort registers = 1;
                mb.Open(Hardware.vending_port, Hardware.vending_BaudRate, Hardware.vending_DataBits, Hardware.vending_Parity, Hardware.vending_StopBits);
                //bool send = mb.SendFc4(Convert.ToByte(address), start, registers, ref values);
             //   if (send)
                //{
                    process = values[0];
                //}
                //else
               // {
                //    log.Info("modbusStatus = " + mb.modbusStatus);
              //  }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                mb.Close();
            }
            mb.Close();
            return process;
        }

        private bool PLCCheck()
        {
            try
            {
                if (ProcessCode < 0)
                {
                    ProcessCode = processData();
                }
                log.Info("ProcessCode : " + ProcessCode);

                if (ProcessCode >= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        private void btnAccount_Click(object sender, RoutedEventArgs e)
        {
            Buy("Account");
        }

        private void btnUPI_Click(object sender, RoutedEventArgs e)
        {
            Buy("UPI");
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
