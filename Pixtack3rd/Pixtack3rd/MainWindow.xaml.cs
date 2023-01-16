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
            string imagePath = "D:\\ブログ用\\テスト用画像\\collection5.png";
            string imagePath1 = "D:\\ブログ用\\テスト用画像\\collection4.png";

            var neko = MyRoot.Name;
            //DataImage dataImg1 = new() { Source = GetBitmap(imagePath) };
            //DataImage dataImg2 = new() { Source = GetBitmap(imagePath1), X = 100, Y = 100 };

            //TTGroup group = new(new DataGroup() { X = 100, Y = 100 });
            //group.AddItem(dataImg1);
            //group.AddItem(dataImg2);

            //MyRoot.AddItem(group, group.Data);
            ////    dataImg1.Source = new BitmapImage(new Uri("D:\\ブログ用\\テスト用画像\\collection4.png"));
            ////dataImg1.Source = GetBitmap("D:\\ブログ用\\テスト用画像\\collection4.png");

            ////MyImage.Data.Source = GetBitmap("D:\\ブログ用\\テスト用画像\\collection1.png");
        }
        private static BitmapImage GetBitmap(string filePath)
        {
            BitmapImage bmp = new();
            FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.EndInit();
            return bmp;
        }
    }
}
