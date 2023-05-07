using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Media.Imaging;

//WPF Arrow and Custom Shapes - CodeProject
//https://www.codeproject.com/Articles/23116/WPF-Arrow-and-Custom-Shapes

//2022WPF/Arrow.cs at master · gogowaten/2022WPF
//https://github.com/gogowaten/2022WPF/blob/master/20221203_%E7%9F%A2%E5%8D%B0%E5%9B%B3%E5%BD%A2/20221203_%E7%9F%A2%E5%8D%B0%E5%9B%B3%E5%BD%A2/Arrow.cs

//E:\オレ\エクセル\WPFでPixtack紫陽花.xlsm_三角関数_$B$95

//WPFで矢印直線描画、Shapeクラスを継承して作成してみた - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2023/02/20/162212

//2023WPF/20230331_BezierCanvasThumb/20230331_BezierCanvasThumb at main · gogowaten/2023WPF
//https://github.com/gogowaten/2023WPF/tree/main/20230331_BezierCanvasThumb/20230331_BezierCanvasThumb

namespace Pixtack3rd
{
    public enum HeadType { None = 0, Arrow, }
    public enum ShapeType { Line = 0, Bezier, }
    public class GeometricShape : Shape, INotifyPropertyChanged
    {
        #region 依存関係プロパティと通知プロパティ

        public PointCollection MyPoints
        {
            get { return (PointCollection)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }
        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(GeometricShape),
                new FrameworkPropertyMetadata(new PointCollection(),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 頂点座標のThumbsの表示設定
        /// </summary>
        public Visibility MyAnchorVisible
        {
            get { return (Visibility)GetValue(MyAnchorVisibleProperty); }
            set { SetValue(MyAnchorVisibleProperty, value); }
        }
        public static readonly DependencyProperty MyAnchorVisibleProperty =
            DependencyProperty.Register(nameof(MyAnchorVisible), typeof(Visibility), typeof(GeometricShape),
                new FrameworkPropertyMetadata(Visibility.Collapsed,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// ラインのつなぎ目をtrueで丸める、falseで鋭角にする
        /// </summary>
        public bool MyLineSmoothJoin
        {
            get { return (bool)GetValue(MyLineSmoothJoinProperty); }
            set { SetValue(MyLineSmoothJoinProperty, value); }
        }
        public static readonly DependencyProperty MyLineSmoothJoinProperty =
            DependencyProperty.Register(nameof(MyLineSmoothJoin), typeof(bool), typeof(GeometricShape),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        /// <summary>
        /// ラインの始点と終点を繋ぐかどうか
        /// </summary>
        public bool MyLineClose
        {
            get { return (bool)GetValue(MyLineCloseProperty); }
            set { SetValue(MyLineCloseProperty, value); }
        }
        public static readonly DependencyProperty MyLineCloseProperty =
            DependencyProperty.Register(nameof(MyLineClose), typeof(bool), typeof(GeometricShape),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public ShapeType MyShapeType
        {
            get { return (ShapeType)GetValue(MyShapeTypeProperty); }
            set { SetValue(MyShapeTypeProperty, value); }
        }
        public static readonly DependencyProperty MyShapeTypeProperty =
            DependencyProperty.Register(nameof(MyShapeType), typeof(ShapeType), typeof(GeometricShape),
                new FrameworkPropertyMetadata(ShapeType.Line,
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
            DependencyProperty.Register(nameof(HeadEndType), typeof(HeadType), typeof(GeometricShape),
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
            DependencyProperty.Register(nameof(HeadBeginType), typeof(HeadType), typeof(GeometricShape),
                new FrameworkPropertyMetadata(HeadType.None,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 矢印角度、初期値は30.0にしている。30～40くらいが適当
        /// </summary>
        public double ArrowHeadAngle
        {
            get { return (double)GetValue(ArrowHeadAngleProperty); }
            set { SetValue(ArrowHeadAngleProperty, value); }
        }
        public static readonly DependencyProperty ArrowHeadAngleProperty =
            DependencyProperty.Register(nameof(ArrowHeadAngle), typeof(double), typeof(GeometricShape),
                new FrameworkPropertyMetadata(30.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //頂点編集状態
        public bool MyIsEditing
        {
            get { return (bool)GetValue(MyIsEditingProperty); }
            set { SetValue(MyIsEditingProperty, value); }
        }
        public static readonly DependencyProperty MyIsEditingProperty =
            DependencyProperty.Register(nameof(MyIsEditing), typeof(bool), typeof(GeometricShape),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //読み取り専用の依存関係プロパティ
        //WPF4.5入門 その43 「読み取り専用の依存関係プロパティ」 - かずきのBlog@hatena
        //        https://blog.okazuki.jp/entry/2014/08/18/083455
        /// <summary>
        /// Descendant、見た目上のRect
        /// </summary>
        private static readonly DependencyPropertyKey MyExternalBoundsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(MyExternalBounds), typeof(Rect), typeof(GeometricShape),
                new FrameworkPropertyMetadata(Rect.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty MyExternalBoundsProperty =
            MyExternalBoundsPropertyKey.DependencyProperty;
        public Rect MyExternalBounds
        {
            get { return (Rect)GetValue(MyExternalBoundsProperty); }
            private set { SetValue(MyExternalBoundsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey MyExternalWidenBoundsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(MyExternalWidenBounds), typeof(Rect), typeof(GeometricShape),
                new FrameworkPropertyMetadata(Rect.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty MyExternalWidenBoundsProperty =
            MyExternalWidenBoundsPropertyKey.DependencyProperty;

        public Rect MyAllBounds
        {
            get { return (Rect)GetValue(MyAllBoundsProperty); }
            set { SetValue(MyAllBoundsProperty, value); }
        }
        public static readonly DependencyProperty MyAllBoundsProperty =
            DependencyProperty.Register(nameof(MyAllBounds), typeof(Rect), typeof(GeometricShape),
                new FrameworkPropertyMetadata(Rect.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));


        public Rect MyExternalWidenBounds
        {
            get { return (Rect)GetValue(MyExternalWidenBoundsProperty); }
            private set { SetValue(MyExternalWidenBoundsPropertyKey, value); }
        }

        public double MyAnchorThumbSize
        {
            get { return (double)GetValue(MyAnchorThumbSizeProperty); }
            set { SetValue(MyAnchorThumbSizeProperty, value); }
        }
        public static readonly DependencyProperty MyAnchorThumbSizeProperty =
        DependencyProperty.Register(nameof(MyAnchorThumbSize), typeof(double), typeof(GeometricShape),
                new FrameworkPropertyMetadata(20.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #endregion 依存関係プロパティと通知プロパティ


        public Geometry MyGeometry { get; protected set; }

        //public List<Thumb> MyThumbs { get; protected set; } = new();
        //public TTGeometricShape? MyOwnerTThumb { get; set; }
        public GeometryAdorner MyAdorner { get; protected set; }

        public AdornerLayer? MyAdornerLayer { get; protected set; }
        public Rect MyExWidenRenderBounds { get; private set; }

        #region コンストラクタ
        public GeometricShape()
        {
            Stroke = Brushes.Orange;
            StrokeThickness = 10;
            Fill = Brushes.Red;
            MyAdorner = new GeometryAdorner(this);
            MyGeometry = this.DefiningGeometry.Clone();
            Loaded += This_Loaded;
            SizeChanged += GeometricShape_SizeChanged;

        }
        #endregion コンストラクタ

        private void This_Loaded(object sender, RoutedEventArgs e)
        {
            SetMyBounds();
            //アドーナーの登録
            //グループ化の処理直後にも、なぜかLoadedが発生するので
            //二重登録にならないようにParentを調べてから登録処理している
            MyAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (MyAdornerLayer != MyAdorner.Parent)
            {
                MyAdornerLayer.Add(MyAdorner);
            }
            MySetBinding();
            SetMyBounds();
        }



        private void MySetBinding()
        {
            SetBinding(MyAnchorVisibleProperty, new Binding() { Source = this, Path = new PropertyPath(MyIsEditingProperty), Converter = new MyConverterBoolVisible() });
            MyAdorner.SetBinding(VisibilityProperty, new Binding() { Source = this, Path = new PropertyPath(MyAnchorVisibleProperty) });
            MyAdorner.SetBinding(GeometryAdorner.MyAnchorThumbSizeProperty, new Binding() { Source = this, Path = new PropertyPath(MyAnchorThumbSizeProperty) });

            MultiBinding mb = new();
            Binding b0 = new() { Source = MyAdorner, Path = new PropertyPath(GeometryAdorner.MyVThumbsBoundsProperty) };
            Binding b1 = new() { Source = this, Path = new PropertyPath(MyExternalBoundsProperty) };
            mb.Bindings.Add(b0); mb.Bindings.Add(b1);
            mb.Converter = new MyConverterRectRect();
            SetBinding(MyAllBoundsProperty, mb);
        }

        private void GeometricShape_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //いらない？MeasureOverrideのときだけでいい？
            SetMyBounds();
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            SetMyBounds();
            return base.ArrangeOverride(finalSize);
        }

        //未使用
        //Thumbの位置修正、再配置、Pointに合わせる
        public void RelocationAdornerThumbs()
        {
            MyAdorner.FixThumbsLocate();
        }
        //未使用
        //一旦Thumbを削除して、表示し直す
        public void UpdateAdorner()
        {
            if (MyAdornerLayer != null)
            {
                var adols = MyAdornerLayer.GetAdorners(this);
                foreach (var adol in adols)
                {
                    MyAdornerLayer.Remove(adol);
                }
                MyAdornerLayer.Add(new GeometryAdorner(this));
            }
        }



        #region 描画

        /// <summary>
        /// ベジェ曲線部分の描画
        /// </summary>
        /// <param name="context"></param>
        /// <param name="begin">始点図形との接点</param>
        /// <param name="end">終点図形との接点</param>
        private void DrawBezier(StreamGeometryContext context, Point begin, Point end, bool isFill)
        {
            //if (isFill)
            //{
            //    context.BeginFigure(begin, true, MyLineClose);
            //    List<Point> bezier = MyPoints.Skip(1).Take(MyPoints.Count - 2).ToList();
            //    bezier.Add(end);
            //    context.PolyBezierTo(bezier, true, MyLineSmoothJoin);
            //}
            context.BeginFigure(begin, isFill, MyLineClose);
            List<Point> bezier = MyPoints.Skip(1).Take(MyPoints.Count - 2).ToList();
            bezier.Add(end);
            context.PolyBezierTo(bezier, true, MyLineSmoothJoin);
        }

        /// <summary>
        /// 直線部分の描画
        /// </summary>
        /// <param name="context"></param>
        /// <param name="begin">始点図形との接点</param>
        /// <param name="end">終点図形との接点</param>
        private void DrawLine(StreamGeometryContext context, Point begin, Point end, bool isFill)
        {
            //if (isFill)
            //{
            //    context.BeginFigure(begin, true, MyLineClose);
            //    context.PolyLineTo(MyPoints.Skip(1).Take(MyPoints.Count - 2).ToList(), true, MyLineSmoothJoin);
            //    context.LineTo(end, true, MyLineSmoothJoin);
            //}

            context.BeginFigure(begin, isFill, MyLineClose);
            context.PolyLineTo(MyPoints.Skip(1).Take(MyPoints.Count - 2).ToList(), true, MyLineSmoothJoin);
            context.LineTo(end, true, MyLineSmoothJoin);


        }

        private static double DegreeToRadian(double degree)
        {
            return degree / 360.0 * (Math.PI * 2.0);
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
            double apexRadian = DegreeToRadian(ArrowHeadAngle);
            double edgeSize = StrokeThickness * 2.0;
            double hypotenuseLength = edgeSize / Math.Cos(apexRadian);
            double hypotenuseRadian1 = lineRadian + apexRadian;

            //底角座標
            Point p1 = new(
                hypotenuseLength * Math.Cos(hypotenuseRadian1) + edge.X,
                hypotenuseLength * Math.Sin(hypotenuseRadian1) + edge.Y);

            double hypotenuseRadian2 = lineRadian - DegreeToRadian(ArrowHeadAngle);
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
                if (MyPoints.Count <= 1) return Geometry.Empty;

                if (HeadBeginType != HeadType.None || HeadEndType != HeadType.None)
                {
                    Fill = Stroke;
                }
                bool isFill = false;
                if (MyLineClose && HeadBeginType == HeadType.None && HeadEndType == HeadType.None)
                {
                    isFill = true;
                }
                StreamGeometry geometry = new();
                using (var context = geometry.Open())
                {
                    Point begin = MyPoints[0];
                    Point end = MyPoints[^1];
                    switch (HeadBeginType)
                    {
                        case HeadType.None:
                            break;
                        case HeadType.Arrow:
                            begin = DrawArrow(context, begin, MyPoints[1]);
                            break;
                    }

                    switch (HeadEndType)
                    {
                        case HeadType.None:
                            break;
                        case HeadType.Arrow:
                            end = DrawArrow(context, end, MyPoints[^2]);
                            break;
                        default:
                            break;
                    }

                    switch (MyShapeType)
                    {
                        case ShapeType.Line:
                            DrawLine(context, begin, end, isFill);
                            break;
                        case ShapeType.Bezier:
                            DrawBezier(context, begin, end, isFill);
                            break;
                        default:
                            break;
                    }

                    //switch (MyShapeType)
                    //{
                    //    case ShapeType.Line:
                    //        context.BeginFigure(MyPoints[0], false, MyLineClose);
                    //        context.PolyLineTo(MyPoints.Skip(1).ToArray(), true, MyLineSmoothJoin);
                    //        context.BeginFigure(MyPoints[0], true, MyLineClose);
                    //        context.PolyLineTo(MyPoints.Skip(1).ToArray(), false, MyLineSmoothJoin);

                    //        break;
                    //    case ShapeType.Bezier:
                    //        context.BeginFigure(MyPoints[0], false, MyLineClose);
                    //        context.PolyBezierTo(MyPoints.Skip(1).ToArray(), true, MyLineSmoothJoin);
                    //        break;
                    //        ////FillはLineで代用できるからいらないかも
                    //        //case ShapeType.Fill:
                    //        //    context.BeginFigure(MyPoints[0], true, MyLineClose);
                    //        //    context.PolyLineTo(MyPoints.Skip(1).ToArray(), false, MyLineSmoothJoin);
                    //        //    break;
                    //}
                }
                geometry.Freeze();
                MyGeometry = geometry.Clone();
                return geometry;
            }
        }

        #endregion 描画


        ////変形時にBoundsを更新、これは変形してもArrangeは無反応だから→
        ////Arrangeでも反応していた
        //protected override Geometry GetLayoutClip(Size layoutSlotSize)
        //{
        //    return base.GetLayoutClip(layoutSlotSize);
        //}

        //各種Bounds更新

        private void SetMyBounds()
        {
            Pen pen = new(Stroke, StrokeThickness);

            //Geometry geo = this.DefiningGeometry.Clone();
            //MyInternalBoundsNotTF = geo.Bounds;//内部Rect
            //MyExternalBoundsNotTF = geo.GetRenderBounds(pen);//外部Rect

            //Geometry exGeo = geo.Clone();
            //exGeo.Transform = RenderTransform;//変形
            //MyInternalBounds = exGeo.Bounds;//変形内部Rect
            //MyInternalBounds = this.RenderTransform.TransformBounds(MyInternalBoundsNotTF);//変形内部Rect
            //MyExternalBounds = exGeo.GetRenderBounds(pen);//変形外部Rect

            //MyTFBounds = MyExternalBounds;
            //MyTFWidth = MyExternalBounds.Width;
            //MyTFHeight = MyExternalBounds.Height;

            //変形がなければGetDescendantBoundsだけでboundsが得られる
            var bb = VisualTreeHelper.GetDescendantBounds(this);
            if (bb.IsEmpty) return;
            MyExternalBounds = bb;

            var geo = DefiningGeometry.Clone();
            var render = geo.GetRenderBounds(pen);
            var widenGeo = geo.GetWidenedPathGeometry(pen);
            var widenRect = widenGeo.Bounds;
            var widenRenderRect = widenGeo.GetRenderBounds(pen);
            MyExWidenRenderBounds = widenRenderRect;
            MyExternalWidenBounds = widenRenderRect;
            //MyExternalBounds = widenRenderRect;
        }
        
        /// <summary>
        /// ピッタリのサイズのBitmap取得
        /// </summary>
        /// <returns></returns>
        public BitmapSource GetBitmap()
        {
            Geometry geo = MyGeometry.Clone();
            geo.Transform = RenderTransform;
            PathGeometry pg = geo.GetWidenedPathGeometry(new Pen(Stroke, StrokeThickness));
            Rect rect = pg.Bounds;
            DrawingVisual dv = new() { Offset = new Vector(-rect.X, -rect.Y) };
            using (var context = dv.RenderOpen())
            {
                context.DrawGeometry(Stroke, null, pg);
            }
            RenderTargetBitmap bitmap = new((int)Math.Ceiling( rect.Width + 1), (int)Math.Ceiling(rect.Height + 1), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);
            return bitmap;
        }



    }




}
