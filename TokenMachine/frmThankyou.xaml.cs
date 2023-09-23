using log4net;
using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VendingMachine.Helpers;

namespace VendingMachine
{
    /// <summary>
    /// Interaction logic for frmThankyou.xaml
    /// </summary>
    public partial class frmThankyou : Window
    {
        public frmThankyou()
        {
            InitializeComponent();
        }

        ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DispatcherTimer tmr_msg = new DispatcherTimer();
       
      
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           



            try
            {
                tmr_msg.Tick += tmr_msg_Tick; ;
                tmr_msg.Interval = new TimeSpan(0, 0, 0, 10);
                tmr_msg.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        void tmr_msg_Tick(object sender, EventArgs e)
        {
            GotoHome();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            GotoHome();
        }

        public void GotoHome()
        {
            try
            {
                tmr_msg.Stop();
                frmHomeScreen frm = new frmHomeScreen();
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
            tmr_msg.Stop();
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
    }
}
