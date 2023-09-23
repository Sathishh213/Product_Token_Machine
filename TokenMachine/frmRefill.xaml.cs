using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for frmRefill.xaml
    /// </summary>
    public partial class frmRefill : Window
    {
        public frmRefill()
        {
            InitializeComponent();
        }



        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DispatcherTimer tmr_msg = new DispatcherTimer();
        int msg_dispaly_time = 0;


        Access acc = new Access();
        List<Stocks> lststock = new List<Stocks>();

        void tmr_msg_Tick(object sender, EventArgs e)
        {
            try
            {
                msg_dispaly_time++;

                switch (msg_dispaly_time % 2)
                {
                    case 0:
                        lblMessage.Foreground = Brushes.White;
                        break;
                    case 1:
                        lblMessage.Foreground = Brushes.Transparent;
                        break;

                    default:
                        lblMessage.Foreground = Brushes.White;
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
                lblMessage.Foreground = Brushes.White;
            }));

        }

        public void loadgrid()
        {
            try
            {

                string cmd = @"SELECT p.product_id, p.product_name, p.img_path,  p.soldout, coalesce(stock , 0) as stock
                                 FROM mst_product p order by p.product_id";
                DataTable dt = acc.GetTable(cmd);
                lststock = new List<Stocks>(); 
                foreach (DataRow dr in dt.Rows)
                {
                    Stocks s = new Stocks();
                    s.product_no = dr["product_id"].ToString();
                    s.product_id = dr["product_id"].ToString();
                    s.product_name = dr["product_name"].ToString();
                    s.stock = Convert.ToInt32(dr["stock"].ToString());
                    s.capacity = 20;
                    s.Exp = 0;

                    s.img_path = AppDomain.CurrentDomain.BaseDirectory + dr["img_path"].ToString();
                    s.Wanted = 0;
                    if (Convert.ToInt32(dr["soldout"].ToString()) == 1)
                    {
                        s.exp_color = "Red";
                        s.background = Brushes.Red;
                    }
                    else
                    {
                        s.exp_color = "Transparent";
                        s.background = Brushes.Transparent;
                    }
                    lststock.Add(s);

                }

                int product_no = 1;
                foreach (Stocks s in lststock)
                {
                    var c = (UCmain)this.FindName("uc" + product_no);
                    if (c != null)
                    {
                        //  c.Visibility = Visibility.Visible;
                        c.Tag = s.product_id;
                        c.expired = s.Exp;
                        c.NoofFill = s.stock;
                        c.NoofBlocks = s.capacity;
                        c.Background = s.background;
                        c.DisplayValue = s.product_name;
                        c.ImgPath = s.img_path;
                        c.NewFill = 0;
                    }
                    product_no++;
                }


                for (int i = 1; i < 81; i++)
                {
                    var c = (UCmain)this.FindName("uc" + i);
                    if (c != null)
                    {
                        if (c.Tag == null)
                        {
                            c.Visibility = Visibility.Collapsed;
                        }
                    }
                }

                //int grid = 0;

                //for (int i = 1; i <= 8; i++)
                //{
                //    int col = (from k in lststock where k.motor_no > ((i - 1) * 10) && k.motor_no <= (i * 10) select k.motor_no).ToList().Count();
                //    grid++;
                //    var unigrid = (UniformGrid)this.FindName("grid" + grid);
                //    if (unigrid != null)
                //    {
                //        unigrid.Columns = col;
                //    }
                //}

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

                tmr_msg.Tick += tmr_msg_Tick; ;
                tmr_msg.Interval = new TimeSpan(0, 0, 0, 0, 500);

                loadgrid();
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
                tmr_msg.Stop();
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
                frmAdminControl frm = new frmAdminControl();
               // frmHomeScreen frm = new frmHomeScreen();
                this.Close();
                frm.Show();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        UCmain uc;

        public void reset()
        {
            try
            {
                lblProductName.Text = "Product";
                lblRefiled.Text = "0";
                lblStock.Text = "0";
                lblTotal.Text = "0";
                img_product.ImageSource = null;
                slQty.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                display_msg("Error in the refilling...");
                log.Error(ex);
            }
        }

        private void uc2_UserControlClicked(object sender, EventArgs e)
        {
            try
            {


                uc = (UCmain)sender;

                var stock = (from k in lststock where k.product_no == uc.Tag.ToString() select k).FirstOrDefault();

                lblProductName.Text = stock.product_name;
                slQty.Visibility = Visibility.Visible;
                slQty.Minimum = 0;
                slQty.Maximum = uc.NoofBlocks - uc.NoofFill;

                if (stock.Exp == 0)
                {
                    if (uc.NoofFill < uc.NoofBlocks)
                    {
                        if (uc.NewFill == 0)
                        {
                            uc.NewFill = (uc.NoofBlocks - uc.NoofFill);
                            var s = (from k in lststock where k.product_no == uc.Tag.ToString() select k).FirstOrDefault();
                            s.Wanted = uc.NewFill;
                        }

                        slQty.IsEnabled = true;

                        slQty.Value = uc.NewFill;
                    }
                    else
                    {
                        slQty.IsEnabled = false;
                    }
                }
                else
                {
                    display_msg("Please clear expired item then try to refill");
                    slQty.Visibility = Visibility.Hidden;
                    slQty.IsEnabled = false;
                }

                lblStock.Text = uc.NoofFill.ToString();
                lblRefiled.Text = uc.NewFill.ToString();
                lblTotal.Text = (uc.NoofFill + uc.NewFill).ToString();

                string img_path = stock.img_path;

                if (File.Exists(img_path))
                {
                    img_product.ImageSource = new BitmapImage(new Uri(img_path, UriKind.Absolute));
                }
                ShowExpireDate(stock.product_no);

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void slQty_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (uc != null)
                {
                    uc.NewFill = Convert.ToInt32(slQty.Value);

                    lblStock.Text = uc.NoofFill.ToString();
                    lblRefiled.Text = uc.NewFill.ToString();
                    lblTotal.Text = (uc.NoofFill + uc.NewFill).ToString();

                    var s = (from k in lststock where k.product_no == uc.Tag.ToString() select k).FirstOrDefault();
                    s.Wanted = uc.NewFill;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnSaveAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var newStocks = (from k in lststock where k.Wanted > 0 select k).ToList();
                foreach (Stocks s in newStocks)
                {
                    string cmd = "update mst_product m set m.stock = " + (s.stock + s.Wanted) + ", m.soldout = 0 where m.product_id = " + s.product_id;
                    acc.ExecuteCmd(cmd);
                }

                display_msg("Refilled Successfully...");
            }
            catch (Exception ex)
            {
                display_msg("Error in the refilling...");
                log.Error(ex);
            }
            reset();
            loadgrid();
        }

        private void btnClearStock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cmd = "";
                var s = (from k in lststock where k.product_no == uc.Tag.ToString() select k).FirstOrDefault();
                if (s.stock > 0)
                {

                    //cmd = " select stock_id from trn_stock where product_no = " + s.product_no + " order by expired_on, stock_id limit " + slClear.Value;
                    //DataTable dt = acc.GetTable(cmd);
                    //string id = string.Join(", ", dt.Rows.OfType<DataRow>().Select(r => r[0].ToString()));

                    //cmd = @"delete from trn_stock where stock_id in (" + id + ")";
                    //string exe = acc.ExecuteCmd(cmd).ToString();

                    cmd = @"insert into trn_cleared(machine_id , product_no , product_id, product_name, cleared_quantity , updatedby, updatedon, is_viewed) 
                                    values ('" + config.machine_id + "' , '" + s.product_no + "' , '" + s.product_id + "' , '" + s.product_name + "' , '" + s.stock + "' , " + config.emp_id + " , now() , 0)";
                    acc.ExecuteCmd(cmd);

                    log.Info("Stocked Item cleared. Item : " + s.product_name + ", Product No : " + s.product_no + ", Qty : " + s.stock);

                    s.stock = 0;
                    s.Exp = 0;

                    cmd = "update tbl_motor_settings set stock = " + s.stock + " , soldout = 0 , fix = 0 where machine_id = " + config.machine_id + " and motor_no = " + s.motor_no + " and product_id = " + s.product_id;
                    acc.ExecuteCmd(cmd);

                    //cmd = "select ifnull((select sum(quantity) from trn_stock where IsActive = 1  and product_no = " + s.product_no + ") , 0)";
                    //int qty = Convert.ToInt32(acc.GetValue(cmd));
                    //s.stock = qty;

                    //cmd = "select ifnull((select sum(quantity) from trn_stock where IsActive = 1 and  expired_on <= now() and product_no = " + s.product_no + ") , 0)";
                    //s.Exp = Convert.ToInt32(acc.GetValue(cmd));

                    UpdateItem(s);

                    display_msg("Stock Cleared on that tray...");
                }
            }
            catch (Exception ex)
            {
                display_msg("Error in the Clearing...");
                log.Error(ex);
            }
        }

        private void btnExpired_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cmd = "";
                var stock = (from k in lststock where k.product_no == uc.Tag.ToString() select k).FirstOrDefault();
                if (stock.Exp > 0)
                {
                    cmd = "delete from trn_stock where product_no = '" + stock.product_no + "' and product_id = " + stock.product_id + " and expired_on <= now()";
                    string exe = acc.ExecuteCmd(cmd).ToString();
                    cmd = @"insert into trn_cleared(machine_id , product_no , product_id, product_name, cleared_quantity , updatedby, updatedon, is_viewed) 
                                    values ('" + config.machine_id + "' , '" + stock.product_no + "' , '" + stock.product_id + "' , '" + stock.product_name + "' , '" + exe + "' , '1' , now() , 0)";
                    acc.ExecuteCmd(cmd);

                    log.Info("Expired Item removed. Item : " + stock.product_name + ", Product No : " + stock.product_no + ", Qty : " + exe);

                    cmd = "select ifnull((select sum(quantity) from trn_stock where IsActive = 1  and product_no = " + stock.product_no + ") , 0)";

                    int qty = Convert.ToInt32(acc.GetValue(cmd));
                    stock.Exp = 0;
                    stock.stock = qty;
                    stock.Wanted = 0;
                    UpdateItem(stock);
                    display_msg("Expired item removed.");
                }
            }
            catch (Exception ex)
            {
                display_msg("Error");
                log.Error(ex);
            }
        }

        void UpdateItem(Stocks s)
        {
            try
            {
                var c = (UCmain)this.FindName("uc" + s.motor_no);
                if (c != null)
                {
                    c.Tag = s.product_no;
                    c.expired = s.Exp;
                    c.NoofFill = s.stock;
                    c.NoofBlocks = s.capacity;
                    c.Background = s.background;
                    c.ImgPath = s.img_path;
                    c.DisplayValue = s.product_name;
                    c.NewFill = 0;

                    slQty.Visibility = Visibility.Visible;
                    slQty.IsEnabled = true;
                    slQty.Minimum = 0;
                    slQty.Maximum = uc.NoofBlocks - uc.NoofFill;
                    slQty.Value = 0;

                    lblStock.Text = uc.NoofFill.ToString();
                    lblRefiled.Text = uc.NewFill.ToString();
                    lblTotal.Text = (uc.NoofFill + uc.NewFill).ToString();

                    ShowExpireDate(s.product_no);
                }
            }
            catch (Exception ex)
            {
                display_msg("Error");
                log.Error(ex);
            }
        }

        void ShowExpireDate(string pno)
        {
            try
            {
                //lblExpireDate.Text = "";
                //string cmd = "select stock_id, expired_on from trn_stock where IsActive = 1 and product_no = " + pno + " order by stock_id";
                //DataTable dt = acc.GetTable(cmd);
                //if (dt.Rows.Count > 0)
                //{
                //    string msg = "Sno\tExpire";
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        msg = msg + "\n" + (i + 1) + "\t" + Convert.ToDateTime(dt.Rows[i]["expired_on"]).ToString("dd-MM-yyyy hh:mmtt");
                //    }
                //    lblExpireDate.Text = msg;
                //}
            }
            catch (Exception ex)
            {
                display_msg("Error");
                log.Error(ex);
            }
        }

    }
}
