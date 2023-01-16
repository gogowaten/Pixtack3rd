using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pixtack3rd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //ImageSourceConverter converter = new();
            //object? neko = converter.ConvertFromString("D:\\ブログ用\\テスト用画像\\collection5.png");
            //BitmapSource? inu=(BitmapSource?)neko;
            //MyRoot.AddItem(new DataTextBlock() { Text = "adddata", X = 0, Y = 0, });



            DataImage tidata = new() { Source = new BitmapImage(
                new Uri("D:\\ブログ用\\テスト用画像\\collection5.png")) };
            tidata = new() { Source = GetBitmap("D:\\ブログ用\\テスト用画像\\collection5.png") };
            MyRoot.AddItem(tidata);
        //    tidata.Source = new BitmapImage(new Uri("D:\\ブログ用\\テスト用画像\\collection4.png"));
            tidata.Source = GetBitmap("D:\\ブログ用\\テスト用画像\\collection4.png");

            MyImage.MyData.Source = GetBitmap("D:\\ブログ用\\テスト用画像\\collection1.png");
        }
        private static BitmapImage GetBitmap(string filePath)
        {
            BitmapImage bmp = new();
            FileStream stream =new(filePath,FileMode.Open, FileAccess.Read);
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.EndInit();
            return bmp;
        }
    }
}
