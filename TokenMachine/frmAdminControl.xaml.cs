using log4net;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace TokenMachine
{
    /// <summary>
    /// Interaction logic for frmAdminControl.xaml
    /// </summary>
    public partial class frmAdminControl : Window
    {
        public frmAdminControl()
        {
            InitializeComponent();
        }

        ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void btnRefill_Click(object sender, RoutedEventArgs e)
        {
            frmRefill frmRefill = new frmRefill();
            this.Close();
            frmRefill.Show();

        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            frmReport frmReport = new frmReport();
            this.Close();
            frmReport.Show();
        }

        private void btnGotoHome_Click(object sender, RoutedEventArgs e)
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

        private void btnShutdown_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            log.Info("Application Shutdown by user");
            Process.Start("shutdown", "/s /t 0");
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            log.Info("Application Shutdown by user");
            Process.Start("shutdown", "/r /t 0");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            frmAddProduct frmAddProduct = new frmAddProduct();
            this.Close();
            frmAddProduct.Show();
        }
    }
}
