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
using ControlLibraryCore20200620;

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
            DragEnter += MainWindow_DragEnter;
            DragOver += MainWindow_DragOver;
            Drop += MainWindow_Drop;

            //string imagePath = "D:\\ブログ用\\テスト用画像\\collection5.png";
            //string imagePath1 = "D:\\ブログ用\\テスト用画像\\collection4.png";

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

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //var fileList3 = ((string[])e.Data.GetData(DataFormats.FileDrop)).OrderBy(x => x);
                var fileList2 = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToArray();
                Array.Sort(fileList2);




            }
        }

        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effects = DragDropEffects.All;

            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            //throw new NotImplementedException();
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
        protected override void OnPreviewDrop(DragEventArgs e)
        {
            base.OnPreviewDrop(e);

        }
        protected override void OnDrop(DragEventArgs e)
        {

            base.OnDrop(e);

        }
    }
}
