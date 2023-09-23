using log4net;
using Microsoft.Win32;
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
using System.Windows.Threading;
using VendingMachine.Helpers;

namespace VendingMachine
{
    /// <summary>
    /// Interaction logic for frmAddProduct.xaml
    /// </summary>
    public partial class frmAddProduct : Window
    {
        public frmAddProduct()
        {
            InitializeComponent();
        }

        TextBox currentText;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DispatcherTimer tmr_msg = new DispatcherTimer();
        int msg_dispaly_time = 0;
        int product_id = 0;
        bool image_upload = false;
        DataTable dt = new DataTable();
        Access acc = new Access();


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                tmr_msg.Tick += tmr_msg_Tick;
                tmr_msg.Interval = new TimeSpan(0, 0, 0, 0, 500);
                txtProduct.Focus();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }


        private void DialogHost_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Filldgv();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }


        private void btnImageUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
                //   open.InitialDirectory = path;
                if (open.ShowDialog() == true)
                {
                    //  pbPhoto.Source = new BitmapImage(new Uri(open.FileName));

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(open.FileName);
                    bitmap.EndInit();
                    pbPhoto.Source = bitmap;

                    image_upload = true;
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }


        private void dgStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
            txtProduct.Focus();
        }

        public void LoadData()
        {
            try
            {

                int itm_index = dgStock.Items.IndexOf(dgStock.SelectedItem);
                product_id = Convert.ToInt32(dt.Rows[itm_index]["product_id"].ToString());

                txtProduct.Text = dt.Rows[itm_index]["Product"].ToString();
                txtPrice.Text = dt.Rows[itm_index]["Price"].ToString();
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + dt.Rows[itm_index]["img_path"].ToString()))
                {
                    pbPhoto.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + dt.Rows[itm_index]["img_path"].ToString()));
                }
                else
                {
                    pbPhoto.Source = null;
                }

            }
            catch (Exception ex)
            {
                display_msg("Error in the process : " + ex.Message);
                log.Error(ex);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                dt.DefaultView.RowFilter = string.Format("Product LIKE '%{0}%'", txtSearch.Text);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        void Save()
        {
            try
            {
                string product_name = txtProduct.Text.Trim();
                decimal price;
                string cmd = "";
                int logid = 1;

                decimal.TryParse(txtPrice.Text.Trim(), out price);

                if (product_name.Length <= 2)
                {
                    display_msg("Please Enter Product Name");
                    return;
                }


                string imgPath = "";


                if (image_upload)
                {
                    string pp = ((BitmapImage)pbPhoto.Source).UriSource.LocalPath;
                    FileInfo f = new FileInfo(pp);
                    imgPath = "Images\\Product\\" + f.Name;

                    if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Images\\Product\\"))
                    {
                        Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "Images\\Product\\");
                    }

                    if (!File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + imgPath))
                    {
                        File.Copy(pp, AppDomain.CurrentDomain.BaseDirectory + imgPath);
                    }
                }
                else
                {
                    if (pbPhoto.Source != null)
                    {
                        string pp = ((BitmapImage)pbPhoto.Source).UriSource.LocalPath;
                        FileInfo f = new FileInfo(pp);
                        imgPath = "Images\\Product\\" + f.Name;
                    }
                }

                int default_stock = 20; int default_soldout = 0;

                if (product_id > 0)
                {
                    cmd = @"update mst_product p set  p.product_name = '" + product_name + "', p.price = '" + price + "' , p.img_path = '" + imgPath.Replace("\\", "\\\\") + "' where p.product_id = " + product_id;
                }
                else
                {
                    cmd = @"insert into mst_product(product_name ,  price , img_path , stock , soldout) 
                            values( '" + product_name + "' , '" + price + "' , '" + imgPath.Replace("\\", "\\\\") + "' , " + default_stock + " , "+ default_soldout + ")";
                }
                int exe = Convert.ToInt16(acc.ExecuteCmd(cmd));

                if (exe > 0)
                {
                    Clear();
                    Filldgv();
                    display_msg("Saved Successfully");
                }
                else
                {
                    display_msg("Saved Failed");
                }
            }
            catch (Exception ex)
            {
                display_msg(ex.Message);
                log.Error(ex);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (product_id > 0)
                {
                    string cmd = "delete from mst_product where product_id = " + product_id;
                    acc.ExecuteCmd(cmd);
                    Clear();
                    Filldgv();
                    display_msg("Deleted Successfully...");
                }
                else
                {
                    display_msg("Please select product...");
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
                frmAdminControl frm = new frmAdminControl();
                this.Close();
                frm.Show();
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

        public void Clear()
        {
            product_id = 0;
            txtProduct.Text = "";
            txtPrice.Text = "";
            pbPhoto.Source = null;
            image_upload = false;
            txtProduct.Focus();
        }

        public void Filldgv()
        {

            dgStock.ItemsSource = null;
            dgStock.Items.Clear();
            dgStock.Columns.Clear();

            string cmd = @"select p.Product_id , 0 as SNo , p.product_name 'Product' ,   p.Price , img_path
                            from mst_product p ";

            dt = acc.GetTable(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["SNo"] = i + 1;
            }

            dt.AcceptChanges();

            if (dt.Rows.Count > 0)
            {
                dgStock.ItemsSource = dt.DefaultView;

                dgStock.Columns[0].Visibility = Visibility.Hidden;
                dgStock.Columns[3].Visibility = Visibility.Hidden;
                dgStock.Columns[4].Visibility = Visibility.Hidden;
                dgStock.RowHeaderWidth = 0;

            }

        }


        #region DisplayMsg

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

        #endregion


        #region touchinput

        private void btnNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clear_msg_timer();
                Button btn = (Button)sender;
                char c = btn.Tag.ToString()[0];
                if (currentText.Text.Length < currentText.MaxLength)
                {
                    if (currentText.MaxLength < 11)
                    {
                        if (char.IsDigit(c))
                        {
                            currentText.Text = currentText.Text + c;
                        }
                        else
                        {
                            display_msg("Numbers Only");
                        }
                    }
                    else
                    {
                        if (currentText.Tag != null)
                        {
                            if (currentText.Tag.ToString() == "small")
                            {
                                currentText.Text = currentText.Text + c.ToString().ToLower();
                            }
                            else
                            {
                                currentText.Text = currentText.Text + c;
                            }
                        }
                        else
                        {
                            currentText.Text = currentText.Text + c;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnBackSpace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentText.Text.Length > 0)
                {
                    currentText.Text = currentText.Text.Substring(0, currentText.Text.Length - 1);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void btnSpace_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (currentText.Text.Length < currentText.MaxLength)
                {
                    currentText.Text = currentText.Text + " ";
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                currentText = (TextBox)sender;
                currentText.CaretIndex = currentText.Text.Length;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }




        #endregion

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
