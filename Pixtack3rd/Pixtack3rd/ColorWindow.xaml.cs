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
using System.Windows.Shapes;

namespace Pixtack3rd
{
    /// <summary>
    /// ColorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorWindow : Window
    {
        public ColorWindow()
        {
            InitializeComponent();

            ImageBrush ib = new(GetHueBitmap361(true,0.8,0.95)) { Stretch = Stretch.Fill };
            //ImageBrush ib = new(GetHueBitmap361(true)) { Stretch = Stretch.Fill };
            Loaded += (s, e) => { MySliderHue.Background = ib; };
        }

        /// <summary>
        /// Hue画像作成、幅1、高さ361、Hueは0から360まで
        /// </summary>
        /// <param name="isReverse">Hueの増え方、falseで0から、trueで360から</param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private BitmapSource GetHueBitmap361(bool isReverse = false, double saturation = 1.0, double value = 1.0)
        {
            int w = 1; int h = 361;
            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int p = y * stride + (x * 3);
                    Color hue;

                    if (isReverse) hue = MathHSV.HSV2Color((h - y), saturation, value);
                    else hue = MathHSV.HSV2Color(y, saturation, value);

                    pixels[p] = hue.R;
                    pixels[p + 1] = hue.G;
                    pixels[p + 2] = hue.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

    }
}
