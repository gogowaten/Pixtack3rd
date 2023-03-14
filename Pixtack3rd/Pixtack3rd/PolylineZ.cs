using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

//WPF Arrow and Custom Shapes - CodeProject
//https://www.codeproject.com/Articles/23116/WPF-Arrow-and-Custom-Shapes

//2022WPF/Arrow.cs at master · gogowaten/2022WPF
//https://github.com/gogowaten/2022WPF/blob/master/20221203_%E7%9F%A2%E5%8D%B0%E5%9B%B3%E5%BD%A2/20221203_%E7%9F%A2%E5%8D%B0%E5%9B%B3%E5%BD%A2/Arrow.cs

//E:\オレ\エクセル\WPFでPixtack紫陽花.xlsm_三角関数_$B$95

//WPFで矢印直線描画、Shapeクラスを継承して作成してみた - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2023/02/20/162212

namespace Pixtack3rd
{
    public enum HeadType { None = 0, Arrow, }
    public class PolylineZ : Shape, INotifyPropertyChanged
    {
        #region 依存プロパティ

        public bool IsBezier
        {
            get { return (bool)GetValue(IsBezierProperty); }
            set { SetValue(IsBezierProperty, value); }
        }
        public static readonly DependencyProperty IsBezierProperty =
            DependencyProperty.Register(nameof(IsBezier), typeof(bool), typeof(PolylineZ),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 終点のヘッドタイプ
        /// </summary>
        public HeadType HeadEndType
        {
            get { return (HeadType)GetValue(HeadEndTypeProperty); }
            set { SetValue(HeadEndTypeProperty, value); }
        }
        public static readonly DependencyProperty HeadEndTypeProperty =
            DependencyProperty.Register(nameof(HeadEndType), typeof(HeadType), typeof(PolylineZ),
                new FrameworkPropertyMetadata(HeadType.None,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        /// <summary>
        /// 始点のヘッドタイプ
        /// </summary>
        public HeadType HeadBeginType
        {
            get { return (HeadType)GetValue(HeadBeginTypeProperty); }
            set { SetValue(HeadBeginTypeProperty, value); }
        }
        public static readonly DependencyProperty HeadBeginTypeProperty =
            DependencyProperty.Register(nameof(HeadBeginType), typeof(HeadType), typeof(PolylineZ),
                new FrameworkPropertyMetadata(HeadType.None,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 矢印角度、初期値は30.0にしている。30～40くらいが適当
        /// </summary>
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(double), typeof(PolylineZ),
                new FrameworkPropertyMetadata(30.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //[TypeConverter(typeof(MyTypeConverterPoints))]
        //public ObservableCollection<Point> MyPoints
        //{
        //    get { return (ObservableCollection<Point>)GetValue(MyPointsProperty); }
        //    set { SetValue(MyPointsProperty, value); }
        //}
        //public static readonly DependencyProperty MyPointsProperty =
        //    DependencyProperty.Register(nameof(MyPoints), typeof(ObservableCollection<Point>), typeof(PolylineZ),
        //        new FrameworkPropertyMetadata(new ObservableCollection<Point>() { new Point(0, 0), new Point(100, 100) },
        //            FrameworkPropertyMetadataOptions.AffectsRender |
        //            FrameworkPropertyMetadataOptions.AffectsMeasure));
        public PointCollection MyPoints
        {
            get { return (PointCollection)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }

        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(PolylineZ),
                new FrameworkPropertyMetadata(new PointCollection(),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        #endregion 依存プロパティ

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        //StrokeThicknessを考慮した実際の表示Rect
        private Rect _contentRect;
        public Rect MyContentRect { get => _contentRect; private set => SetProperty(ref _contentRect, value); }

        public PolylineZ()
        {
            SizeChanged += PolylineZ_SizeChanged;
        }

        private void PolylineZ_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyContentRect = VisualTreeHelper.GetContentBounds(this);
        }

        private static double DegreeToRadian(double degree)
        {
            return degree / 360.0 * (Math.PI * 2.0);
        }

        ///// <summary>
        ///// 直線の描画
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="begin"></param>
        ///// <param name="end"></param>
        //private void DrawLine(StreamGeometryContext context, Point begin, Point end)
        //{
        //    context.BeginFigure(begin, false, false);
        //    for (int i = 1; i < MyPoints.Count - 1; i++)
        //    {
        //        context.LineTo(MyPoints[i], true, false);
        //    }
        //    context.LineTo(end, true, false);
        //}

        /// <summary>
        /// ベジェ曲線部分の描画
        /// </summary>
        /// <param name="context"></param>
        /// <param name="begin">始点図形との接点</param>
        /// <param name="end">終点図形との接点</param>
        private void DrawBezier(StreamGeometryContext context, Point begin, Point end)
        {
            context.BeginFigure(begin, false, false);
            List<Point> bezier = MyPoints.Skip(1).Take(MyPoints.Count - 2).ToList();
            bezier.Add(end);
            context.PolyBezierTo(bezier, true, false);
        }

        /// <summary>
        /// 直線部分の描画
        /// </summary>
        /// <param name="context"></param>
        /// <param name="begin">始点図形との接点</param>
        /// <param name="end">終点図形との接点</param>
        private void DrawLine(StreamGeometryContext context, Point begin, Point end)
        {
            context.BeginFigure(begin, false, false);
            context.PolyLineTo(MyPoints.Skip(1).Take(MyPoints.Count - 2).ToList(), true, false);
            context.LineTo(end, true, false);
        }
        /// <summary>
        /// アローヘッド(三角形)描画
        /// </summary>
        /// <param name="context"></param>
        /// <param name="edge">端のPoint、始点ならPoints[0]、終点ならPoints[^1]</param>
        /// <param name="next">端から2番めのPoint、始点ならPoints[1]、終点ならPoints[^2]</param>
        /// <returns></returns>
        private Point DrawArrow(StreamGeometryContext context, Point edge, Point next)
        {
            //斜辺 hypotenuse ここでは二等辺三角形の底辺じゃない方の2辺
            //頂角 apex angle 先端の角
            //アローヘッドの斜辺(hypotenuse)の角度(ラジアン)を計算
            double lineRadian = Math.Atan2(next.Y - edge.Y, next.X - edge.X);
            double apexRadian = DegreeToRadian(Angle);
            double edgeSize = StrokeThickness * 2.0;
            double hypotenuseLength = edgeSize / Math.Cos(apexRadian);
            double hypotenuseRadian1 = lineRadian + apexRadian;

            //底角座標
            Point p1 = new(
                hypotenuseLength * Math.Cos(hypotenuseRadian1) + edge.X,
                hypotenuseLength * Math.Sin(hypotenuseRadian1) + edge.Y);

            double hypotenuseRadian2 = lineRadian - DegreeToRadian(Angle);
            Point p2 = new(
                hypotenuseLength * Math.Cos(hypotenuseRadian2) + edge.X,
                hypotenuseLength * Math.Sin(hypotenuseRadian2) + edge.Y);

            //アローヘッド描画、Fill(塗りつぶし)で描画
            context.BeginFigure(edge, true, false);//isFilled, isClose
            context.LineTo(p1, false, false);//isStroke, isSmoothJoin
            context.LineTo(p2, false, false);

            //アローヘッドと中間線の接点座標計算、
            //HeadSizeぴったりで計算すると僅かな隙間ができるので-1.0している、
            //-0.5でも隙間になる、-0.7で隙間なくなる
            return new Point(
                (edgeSize - 1.0) * Math.Cos(lineRadian) + edge.X,
                (edgeSize - 1.0) * Math.Sin(lineRadian) + edge.Y);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (MyPoints.Count < 2) { return Geometry.Empty; }
                //線はStrokeで描画、矢じりはFillで描画している
                //色は統一するので別々に色を指定するのがめんどくさいから
                //Strokeで指定に統一
                Fill = Stroke;

                StreamGeometry geometry = new() { FillRule = FillRule.Nonzero };
                using (var context = geometry.Open())
                {
                    Point begin = MyPoints[0];
                    Point end = MyPoints[^1];
                    switch (HeadBeginType)
                    {
                        case HeadType.None:
                            break;
                        case HeadType.Arrow:
                            begin = DrawArrow(context, MyPoints[0], MyPoints[1]);
                            break;
                    }
                    switch (HeadEndType)
                    {
                        case HeadType.None:
                            break;
                        case HeadType.Arrow:
                            end = DrawArrow(context, MyPoints[^1], MyPoints[^2]);
                            break;
                    }
                    if (IsBezier) { DrawBezier(context, begin, end); }
                    else { DrawLine(context, begin, end); }

                }
                geometry.Freeze();
                return geometry;
            }
        }

        ///// <summary>
        ///// 始点にアローヘッド描画
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns>アローヘッドと直線との接点座標</returns>
        //private Point DrawBeginArrow(StreamGeometryContext context)
        //{
        //    double x0 = MyPoints[0].X;
        //    double y0 = MyPoints[0].Y;
        //    double x1 = MyPoints[1].X;
        //    double y1 = MyPoints[1].Y;

        //    double lineRadian = Math.Atan2(y1 - y0, x1 - x0);
        //    double arrowRadian = DegreeToRadian(ArrowHeadAngle);
        //    double headSize = StrokeThickness * 2.0;
        //    double wingLength = headSize / Math.Cos(arrowRadian);

        //    double wingRadian1 = lineRadian + arrowRadian;
        //    Point arrowP1 = new(
        //        wingLength * Math.Cos(wingRadian1) + x0,
        //        wingLength * Math.Sin(wingRadian1) + y0);
        //    double wingRadian2 = lineRadian - DegreeToRadian(ArrowHeadAngle);
        //    Point arrowP2 = new(
        //        wingLength * Math.Cos(wingRadian2) + x0,
        //        wingLength * Math.Sin(wingRadian2) + y0);
        //    //アローヘッド描画、Fill(塗りつぶし)で描画
        //    context.BeginFigure(MyPoints[0], true, false);//fill, close
        //    context.LineTo(arrowP1, false, false);//isStroke, isSmoothJoin
        //    context.LineTo(arrowP2, false, false);

        //    //アローヘッドと直線との接点座標、
        //    //HeadSizeぴったりで計算すると僅かな隙間ができるので-1.0している、
        //    //-0.5でも隙間になる、-0.7で隙間なくなる
        //    return new Point(
        //        (headSize - 1.0) * Math.Cos(lineRadian) + x0,
        //        (headSize - 1.0) * Math.Sin(lineRadian) + y0);

        //}

        ///// <summary>
        ///// 終点にアローヘッド描画
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns>アローヘッドと直線との接点座標</returns>
        //private Point DrawEndArrow(StreamGeometryContext context)
        //{
        //    double x0 = MyPoints[^1].X;
        //    double x1 = MyPoints[^2].X;
        //    double y0 = MyPoints[^1].Y;
        //    double y1 = MyPoints[^2].Y;

        //    double lineRadian = Math.Atan2(y1 - y0, x1 - x0);
        //    double arrowRadian = DegreeToRadian(ArrowHeadAngle);
        //    double headSize = StrokeThickness * 2.0;
        //    double wingLength = headSize / Math.Cos(arrowRadian);

        //    double wingRadian1 = lineRadian + arrowRadian;
        //    Point arrowP1 = new(
        //        wingLength * Math.Cos(wingRadian1) + x0,
        //        wingLength * Math.Sin(wingRadian1) + y0);
        //    double wingRadian2 = lineRadian - DegreeToRadian(ArrowHeadAngle);
        //    Point arrowP2 = new(
        //        wingLength * Math.Cos(wingRadian2) + x0,
        //        wingLength * Math.Sin(wingRadian2) + y0);
        //    //アローヘッド描画、Fill(塗りつぶし)で描画
        //    context.BeginFigure(MyPoints[^1], true, false);//fill, close
        //    context.LineTo(arrowP1, false, false);//isStroke, isSmoothJoin
        //    context.LineTo(arrowP2, false, false);

        //    return new Point(
        //        (headSize - 1.0) * Math.Cos(lineRadian) + x0,
        //        (headSize - 1.0) * Math.Sin(lineRadian) + y0);
        //}

    }

    public class MyConverterRectToWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect rect = (Rect)value;
            return rect.Width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyConverterRectToHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect rect = (Rect)value;
            return rect.Height;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyConverterRectToX : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect rect = (Rect)value;
            return rect.X;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyConverterRectToY : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect rect = (Rect)value;
            return rect.Y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
