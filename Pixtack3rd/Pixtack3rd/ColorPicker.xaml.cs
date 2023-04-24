using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Pixtack3rd
{
    /// <summary>
    /// ColorColorPicker.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorPicker : Window
    {
        #region 依存関係プロパティ

        public Color PickColor
        {
            get { return (Color)GetValue(PickColorProperty); }
            set { SetValue(PickColorProperty, value); }
        }
        public static readonly DependencyProperty PickColorProperty =
            DependencyProperty.Register(nameof(PickColor), typeof(Color), typeof(ColorPicker),
                new FrameworkPropertyMetadata(Color.FromArgb(0, 0, 0, 0),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public SolidColorBrush PickColorBrush
        {
            get { return (SolidColorBrush)GetValue(PickColorBrushProperty); }
            set { SetValue(PickColorBrushProperty, value); }
        }
        public static readonly DependencyProperty PickColorBrushProperty =
            DependencyProperty.Register(nameof(PickColorBrush), typeof(SolidColorBrush), typeof(ColorPicker),
                new FrameworkPropertyMetadata(Brushes.Red,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// HSVを再計算、RGB変更時に使用
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnRGB(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker mw)
            {
                if (mw.IsHSVChangNow) return;
                mw.IsRGBChangNow = true;
                (mw.H, mw.S, mw.V) = MathHSV.RGB2hsv(mw.R, mw.G, mw.B);
                mw.UpdateSVWriteableBitmap(mw.H);
                mw.IsRGBChangNow = false;
            }
        }

        /// <summary>
        /// RGBを再計算、SV変更時に使用
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnHSV(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker mw)
            {
                if (mw.IsRGBChangNow) return;
                mw.IsHSVChangNow = true;
                (mw.R, mw.G, mw.B) = MathHSV.Hsv2rgb(mw.H, mw.S, mw.V);
                mw.IsHSVChangNow = false;
            }
        }
        private static void OnHue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker mw)
            {
                if (mw.IsRGBChangNow) return;
                mw.IsHSVChangNow = true;
                (mw.R, mw.G, mw.B) = MathHSV.Hsv2rgb(mw.H, mw.S, mw.V);
                mw.UpdateSVWriteableBitmap(mw.H);
                mw.IsHSVChangNow = false;
            }
        }

        public byte R
        {
            get { return (byte)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }
        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register(nameof(R), typeof(byte), typeof(ColorPicker),
                new FrameworkPropertyMetadata(byte.MinValue,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnRGB)));


        public byte G
        {
            get { return (byte)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }
        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register(nameof(G), typeof(byte), typeof(ColorPicker),
                new FrameworkPropertyMetadata(byte.MinValue,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnRGB)));

        public byte B
        {
            get { return (byte)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register(nameof(B), typeof(byte), typeof(ColorPicker),
                new FrameworkPropertyMetadata(byte.MinValue,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnRGB)));

        public byte A
        {
            get { return (byte)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }
        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register(nameof(A), typeof(byte), typeof(ColorPicker),
                new FrameworkPropertyMetadata(byte.MinValue,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double H
        {
            get { return (double)GetValue(HProperty); }
            set { SetValue(HProperty, value); }
        }
        public static readonly DependencyProperty HProperty =
            DependencyProperty.Register(nameof(H), typeof(double), typeof(ColorPicker),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnHue)));
        public double S
        {
            get { return (double)GetValue(SProperty); }
            set { SetValue(SProperty, value); }
        }
        public static readonly DependencyProperty SProperty =
            DependencyProperty.Register(nameof(S), typeof(double), typeof(ColorPicker),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnHSV)));

        public double V
        {
            get { return (double)GetValue(VProperty); }
            set { SetValue(VProperty, value); }
        }
        public static readonly DependencyProperty VProperty =
            DependencyProperty.Register(nameof(V), typeof(double), typeof(ColorPicker),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnHSV)));


        public double MarkerSize
        {
            get { return (double)GetValue(MarkerSizeProperty); }
            set { SetValue(MarkerSizeProperty, value); }
        }
        public static readonly DependencyProperty MarkerSizeProperty =
            DependencyProperty.Register(nameof(MarkerSize), typeof(double), typeof(ColorPicker),
                new FrameworkPropertyMetadata(20.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存関係プロパティ

        ////無限ループ防止用フラグ
        private bool IsRGBChangNow;
        private bool IsHSVChangNow;
        public Marker MyMarker { get; set; }
        //SVimageのBitmapSourceのサイズは16あれば十分？
        private readonly int SVBitmapSize = 16;
        public WriteableBitmap SVWriteableBitmap { get; private set; }
        public byte[] SVPixels { get; private set; }
        private readonly int SVStride;


        #region コンストラクタ

        public ColorPicker()
        {
            InitializeComponent();

            //SV画像のSourceはWriterableBitmap、これのPixelsを書き換えるようにした
            //PixelFormats.Rgb24の1ピクセルあたりのbyte数は24/8=3
            SVStride = SVBitmapSize * 3;
            MyMarker = new Marker(MyImageSV);
            SVPixels = new byte[SVBitmapSize * SVStride];
            SVWriteableBitmap = new(SVBitmapSize, SVBitmapSize, 96, 96, PixelFormats.Rgb24, null);
            MyImageSV.Source = SVWriteableBitmap;

            DataContext = this;
            SetMyBindings();
            SetMarkerBinding();
            MyImageSV.Stretch = Stretch.Fill;

            //初期色は赤
            PickColor = Color.FromArgb(255, 255, 0, 0);
            Loaded += Picker_Loaded;
            //Closing += Picker_Closing;
        }

        //色指定あり
        public ColorPicker(Color color) : this()
        {
            PickColor = color;
        }
        public ColorPicker(SolidColorBrush brush) : this()
        {
            PickColorBrush = brush;
        }

        #endregion コンストラクタ

        //外からの色の指定
        public void SetColor(Color color)
        {
            PickColor = color;
        }

        ////Windowは閉じないで非表示
        //private void Picker_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    e.Cancel = true;
        //    this.Hide();
        //}



        private void Picker_Loaded(object sender, RoutedEventArgs e)
        {
            if (AdornerLayer.GetAdornerLayer(MyImageSV) is AdornerLayer layer)
            {
                layer.Add(MyMarker);
            }

            ImageBrush ib = new(GetHueBitmap361(true, 0.8, 0.95)) { Stretch = Stretch.Fill };
            MySliderHue.Background = ib;
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



        private void SetMarkerBinding()
        {


            MyMarker.SetBinding(Marker.MarkerSizeProperty, new Binding() { Source = this, Path = new PropertyPath(MarkerSizeProperty) });
            MyMarker.SetBinding(Marker.SaturationProperty, new Binding() { Source = this, Path = new PropertyPath(SProperty) });
            MyMarker.SetBinding(Marker.ValueProperty, new Binding() { Source = this, Path = new PropertyPath(VProperty) });


        }

        private void UpdateSVWriteableBitmap(double hue)
        {
            int p = 0;
            Parallel.For(0, SVBitmapSize, y =>
            {
                ParallelImageSV(p, y, SVStride, SVPixels, hue, SVBitmapSize, SVBitmapSize);
            });
            SVWriteableBitmap.WritePixels(new Int32Rect(0, 0, SVBitmapSize, SVBitmapSize), SVPixels, SVStride, 0);
        }

        private void ParallelImageSV(int p, int y, int stride, byte[] pixels, double hue, int w, int h)
        {
            double v = y / (h - 1.0);
            double ww = w - 1;
            for (int x = 0; x < w; ++x)
            {
                p = y * stride + (x * 3);
                (pixels[p], pixels[p + 1], pixels[p + 2]) = MathHSV.Hsv2rgb(hue, x / ww, v);
            }
        }


        private void SetMyBindings()
        {
            MultiBinding mb = new();
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(AProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(RProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(GProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(BProperty) });
            mb.Converter = new MyConverterARGB2Color();
            SetBinding(PickColorProperty, mb);

            SetBinding(PickColorBrushProperty, new Binding() { Source = this, Path = new PropertyPath(PickColorProperty), Converter = new MyConverterColorSolidBrush() });

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PickColor = Colors.Blue;
            var mark = MyMarker.Saturation;
            var neko = S;
            var blue = B;
            var green = G;

        }

        private void Button_Click_Ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void MySliderHue_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { MySliderHue.Value += MySliderHue.SmallChange; }
            else MySliderHue.Value -= MySliderHue.SmallChange;
        }
    }


    public class Marker : Adorner
    {
        #region 依存関係プロパティ

        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }
        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(Marker),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(Marker),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double MarkerSize
        {
            get { return (double)GetValue(MarkerSizeProperty); }
            set { SetValue(MarkerSizeProperty, value); }
        }
        public static readonly DependencyProperty MarkerSizeProperty =
            DependencyProperty.Register(nameof(MarkerSize), typeof(double), typeof(Marker),
                new FrameworkPropertyMetadata(20.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public SolidColorBrush Color1
        {
            get { return (SolidColorBrush)GetValue(Color1Property); }
            set { SetValue(Color1Property, value); }
        }
        public static readonly DependencyProperty Color1Property =
            DependencyProperty.Register(nameof(Color1), typeof(SolidColorBrush), typeof(Marker),
                new FrameworkPropertyMetadata(Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public SolidColorBrush Color2
        {
            get { return (SolidColorBrush)GetValue(Color2Property); }
            set { SetValue(Color2Property, value); }
        }
        public static readonly DependencyProperty Color2Property =
            DependencyProperty.Register(nameof(Color2), typeof(SolidColorBrush), typeof(Marker),
                new FrameworkPropertyMetadata(Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion 依存関係プロパティ



        private VisualCollection MyVisuals { get; set; }
        protected override int VisualChildrenCount => MyVisuals.Count;
        protected override Visual GetVisualChild(int index) => MyVisuals[index];

        public Thumb MarkerThumb;
        public Canvas MyCanvas;
        public FrameworkElement TargetElement;

        private Point DiffPoint;

        #region コンストラクタ

        //通常
        public Marker(FrameworkElement adornedElement) : base(adornedElement)
        {
            TargetElement = adornedElement;

            MyVisuals = new(this);
            MyCanvas = new();
            MyVisuals.Add(MyCanvas);
            MarkerThumb = new Thumb();
            SetMyCanvas();
            SetMarker();
            SetMarkerTemplate();
            MarkerThumb.DragDelta += Marker_DragDelta;
            MarkerThumb.DragCompleted += (s, e) => { DiffPoint = new(); };

        }

        //色指定で開くとき、彩度と輝度の指定が必要
        //public Marker(FrameworkElement adornedElement, double saturation, double value) : this(adornedElement)
        //{
        //    Saturation = saturation;
        //    Value = value;
        //}

        #endregion コンストラクタ

        //
        private void SetMyCanvas()
        {
            MyCanvas.Background = Brushes.Transparent;
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            MyCanvas.Children.Add(MarkerThumb);
            MyCanvas.SetBinding(WidthProperty, new Binding() { Source = this, Path = new PropertyPath(ActualWidthProperty) });
            MyCanvas.SetBinding(HeightProperty, new Binding() { Source = this, Path = new PropertyPath(ActualHeightProperty) });
        }

        //SaturationとValueをクリック座標に相当する値に更新＋
        //更新前後の座標差を記録、これは
        //続けてドラッグ移動した場合に使う
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pp = Mouse.GetPosition(MyCanvas);
            double dx = pp.X - Canvas.GetLeft(MarkerThumb) - (MarkerSize / 2.0);
            double dy = pp.Y - Canvas.GetTop(MarkerThumb) - (MarkerSize / 2.0);
            DiffPoint = new Point(dx, dy);

            double xx = pp.X / TargetElement.ActualWidth;
            if (xx < 0) xx = 0; if (xx > 1.0) xx = 1.0;
            Saturation = xx;

            double yy = pp.Y / TargetElement.ActualHeight;
            if (yy < 0) yy = 0; if (yy > 1.0) yy = 1.0;
            Value = yy;

            MarkerThumb.RaiseEvent(e);
        }

        private void SetMarker()
        {
            //マーカーThumb座標とSVのBinding
            MultiBinding mb = new();
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(MarkerSizeProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(SaturationProperty) });
            mb.Bindings.Add(new Binding() { Source = TargetElement, Path = new PropertyPath(ActualWidthProperty) });
            mb.Converter = new ConverterTopLeft2XY();
            MarkerThumb.SetBinding(Canvas.LeftProperty, mb);

            mb = new();
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(MarkerSizeProperty) });
            mb.Bindings.Add(new Binding() { Source = this, Path = new PropertyPath(ValueProperty) });
            mb.Bindings.Add(new Binding() { Source = TargetElement, Path = new PropertyPath(ActualHeightProperty) });
            mb.Converter = new ConverterTopLeft2XY();
            MarkerThumb.SetBinding(Canvas.TopProperty, mb);

        }
        private void SetMarkerTemplate()
        {
            FrameworkElementFactory factory = new(typeof(Grid));
            FrameworkElementFactory e1 = new(typeof(Ellipse));
            e1.SetValue(WidthProperty, new Binding() { Source = this, Path = new PropertyPath(MarkerSizeProperty) });
            e1.SetValue(HeightProperty, new Binding() { Source = this, Path = new PropertyPath(MarkerSizeProperty) });
            e1.SetValue(Ellipse.StrokeProperty, new Binding() { Source = this, Path = new PropertyPath(Color1Property) });
            e1.SetValue(Ellipse.FillProperty, Brushes.Transparent);
            FrameworkElementFactory e2 = new(typeof(Ellipse));
            e2.SetValue(WidthProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(MarkerSizeProperty),
                Converter = new ConverterDownSize()
            });
            e2.SetValue(HeightProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(MarkerSizeProperty),
                Converter = new ConverterDownSize()
            });
            e2.SetValue(Ellipse.StrokeProperty, new Binding() { Source = this, Path = new PropertyPath(Color2Property), });
            factory.AppendChild(e1);
            factory.AppendChild(e2);
            MarkerThumb.Template = new() { VisualTree = factory };

        }

        private void Marker_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var left = Canvas.GetLeft(MarkerThumb);
            var top = Canvas.GetTop(MarkerThumb);
            var h = e.HorizontalChange;
            var v = e.VerticalChange;
            // ドラッグ移動ではtopleftを指定ではなく、saturation,Valueを計算して指定
            var dx = DiffPoint.X + (MarkerSize / 2.0);
            var dy = DiffPoint.Y + (MarkerSize / 2.0);
            double ll = left + h + dx;
            double xx = ll / TargetElement.ActualWidth;
            if (xx < 0) xx = 0; if (xx > 1.0) xx = 1.0;
            Saturation = xx;
            double tt = top + v + dy;
            double yy = tt / TargetElement.ActualHeight;
            if (yy < 0) yy = 0; if (yy > 1.0) yy = 1.0;
            Value = yy;
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            MyCanvas.Arrange(new Rect(finalSize));
            return base.ArrangeOverride(finalSize);
        }
    }



    public class ConverterTopLeft2XY : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double markerSize = (double)values[0];
            double sv = (double)values[1];
            double targetSize = (double)values[2];
            double result = (sv * targetSize) - (markerSize / 2.0);
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ConverterDownSize : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double size = ((double)value) - 2.0;
            if (size < 0) { size = 0; }
            return size;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
