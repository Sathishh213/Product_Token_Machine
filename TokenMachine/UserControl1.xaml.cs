using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VendingMachine
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }


        public int BlockHeight
        {
            get { return (int)GetValue(BlockHeightProperty); }
            set { SetValue(BlockHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlockHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockHeightProperty =
            DependencyProperty.Register("BlockHeight", typeof(int), typeof(UserControl1), new PropertyMetadata(10));





        public int BlockMargin
        {
            get { return (int)GetValue(BlockMarginProperty); }
            set { SetValue(BlockMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlockMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockMarginProperty =
            DependencyProperty.Register("BlockMargin", typeof(int), typeof(UserControl1), new PropertyMetadata(2));




        public Brush BlockColor
        {
            get { return (Brush)GetValue(BlockColorProperty); }
            set { SetValue(BlockColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlockColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockColorProperty =
            DependencyProperty.Register("BlockColor", typeof(Brush), typeof(UserControl1), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(20, 0, 0, 0))));

        
    }
}
