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

namespace TokenMachine
{
    /// <summary>
    /// Interaction logic for UCmain.xaml
    /// </summary>
    public partial class UCmain : UserControl
    {
        public UCmain()
        {
            InitializeComponent();
            //  UserControlClicked += UCmain_UserControlClicked;
        }

        //private void UCmain_UserControlClicked(object sender, EventArgs e)
        //{
        //    MessageBox.Show("HI");
        //    UserControlClicked(this, e);
        //}

        public event EventHandler UserControlClicked;

        private void Mybtn_Click(object sender, RoutedEventArgs e)
        {
            if (UserControlClicked != null)
            {
                UserControlClicked(this, e);
            }
        }


        public int NoofBlocks
        {
            get { return (int)GetValue(NoofBlocksProperty); }
            set
            {
                SetValue(NoofBlocksProperty, value);

                StackPanel BlockContainer = (StackPanel)Mybtn.Template.FindName("BlockContainer", Mybtn);

                BlockContainer.Children.Clear();

                int myheight = ((int)this.ActualHeight - 25) / value; //((int)this.Height - 25) / value; //
                int margin = 1;
                if (value > 20)
                {
                    margin = 1;
                }
                else
                {
                    margin = 2;
                }


                for (int i = 0; i < value; i++)
                {
                    UserControl1 block = new UserControl1();
                    block.BlockHeight = myheight; // BlockHeight;
                    block.BlockMargin = margin;
                    if (i >= (value - NoofFill))
                    {

                        if (i >= (value - expired))
                        {
                            block.BlockColor = new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Expired
                        }
                        else
                        {
                            block.BlockColor = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));  // Filled
                        }

                    }
                    else
                    {
                        block.BlockColor = new SolidColorBrush(Color.FromArgb(30, 169, 169, 16));   // Empty
                    }
                    BlockContainer.Children.Add(block);
                }
            }
        }

        // Using a DependencyProperty as the backing store for NoofBlocks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoofBlocksProperty =
            DependencyProperty.Register("NoofBlocks", typeof(int), typeof(UCmain), new PropertyMetadata(5));


        //public int BlockHeight
        //{
        //    get { return (int)GetValue(BlockHeightProperty); }
        //    set { SetValue(BlockHeightProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for BlockHeight.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty BlockHeightProperty =
        //    DependencyProperty.Register("BlockHeight", typeof(int), typeof(UCmain), new PropertyMetadata(10));


        public int NoofFill
        {
            get { return (int)GetValue(NoofFillProperty); }
            set { SetValue(NoofFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoofFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoofFillProperty =
            DependencyProperty.Register("NoofFill", typeof(int), typeof(UCmain), new PropertyMetadata(0));





        public int expired
        {
            get { return (int)GetValue(expiredProperty); }
            set { SetValue(expiredProperty, value); }
        }

        // Using a DependencyProperty as the backing store for expired.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty expiredProperty =
            DependencyProperty.Register("expired", typeof(int), typeof(UCmain), new PropertyMetadata(0));



        public int NewFill
        {
            get { return (int)GetValue(NewFillProperty); }
            set
            {
                SetValue(NewFillProperty, value);

                //if ((NoofBlocks - NoofFill) >= value)
                //{
                StackPanel BlockContainer = (StackPanel)Mybtn.Template.FindName("BlockContainer", Mybtn);
                var blocks = BlockContainer.Children;

                int empty = NoofBlocks - (NoofFill + value);

                for (int i = 0; i < blocks.Count; i++)
                {
                    if (blocks[i].GetType() == typeof(UserControl1))
                    {
                        if (i < empty)
                        {
                            ((UserControl1)blocks[i]).BlockColor = new SolidColorBrush(Color.FromArgb(30, 169, 169, 16));
                        }

                        if (i >= empty && i < (value + empty))
                        {
                            ((UserControl1)blocks[i]).BlockColor = Brushes.Green;
                        }
                    }
                }
                //}

            }
        }

        // Using a DependencyProperty as the backing store for NewFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewFillProperty =
            DependencyProperty.Register("NewFill", typeof(int), typeof(UCmain), new PropertyMetadata(0));




        public string DisplayValue
        {
            get { return (string)GetValue(DisplayValueProperty); }
            set { SetValue(DisplayValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayValueProperty =
            DependencyProperty.Register("DisplayValue", typeof(string), typeof(UCmain), new PropertyMetadata(""));

        public string ImgPath
        {
            get { return (string)GetValue(ImgPathProperty); }
            set { SetValue(ImgPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImgPathProperty =
            DependencyProperty.Register("ImgPath", typeof(string), typeof(UCmain), new PropertyMetadata(""));


    }
}
