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

namespace Pixtack3rd
{
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


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //以下必要？
        //private Rect _myBounds;
        //public Rect MyBounds { get => _myBounds; set => SetProperty(ref _myBounds, value); }

        private Rect _myTFBounds;
        public Rect MyTFBounds { get => _myTFBounds; set => SetProperty(ref _myTFBounds, value); }

        private double _myTFWidth;
        public double MyTFWidth { get => _myTFWidth; set => SetProperty(ref _myTFWidth, value); }

        private double _myTFHeight;
        public double MyTFHeight { get => _myTFHeight; set => SetProperty(ref _myTFHeight, value); }


        #endregion 依存関係プロパティと通知プロパティ


        public Geometry MyGeometry { get; protected set; }
        public Rect MyExternalBoundsNotTF { get; protected set; }//外観のRect、変形なし時
        public Rect MyExternalBounds { get; protected set; }//外観のRect、変形加味
        public Rect MyInternalBoundsNotTF { get; protected set; }//PointsだけのRect、変形なし時
        public Rect MyInternalBounds { get; protected set; }//PointsだけのRect、変形なし時


        //public List<Thumb> MyThumbs { get; protected set; } = new();
        public TTGeometricShape? MyOwnerTThumb { get; set; }
        public GeometryAdorner MyAdorner { get; protected set; }

        public AdornerLayer? MyAdornerLayer { get; protected set; }

        #region コンストラクタ
        public GeometricShape()
        {
            Stroke = Brushes.Orange;
            StrokeThickness = 10;
            Fill = Brushes.Red;
            MyAdorner = new GeometryAdorner(this);
            MyGeometry = this.DefiningGeometry.Clone();
            Loaded += This_Loaded;

            MySetBinding();
        }


        #endregion コンストラクタ

        private void MySetBinding()
        {
            MyAdorner.SetBinding(VisibilityProperty, new Binding() { Source = this, Path = new PropertyPath(MyAnchorVisibleProperty) });
        }

        //未使用
        //Thumbの位置修正、再配置、Pointに合わせる
        public void RelocationAdornerThumbs()
        {
            MyAdorner.RelocationThumbs();
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

        private void This_Loaded(object sender, RoutedEventArgs e)
        {
            //アドーナーの登録
            //グループ化の処理直後にも、なぜかLoadedが発生するので
            //二重登録にならないようにParentを調べてから登録処理している
            MyAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (MyAdornerLayer != MyAdorner.Parent)
            {
                MyAdornerLayer.Add(MyAdorner);
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
        protected override Size ArrangeOverride(Size finalSize)
        {

            return base.ArrangeOverride(finalSize);

            //return base.ArrangeOverride(MyExternalBounds.Size);
        }
        protected override Size MeasureOverride(Size constraint)
        {
            Pen pen = new(Stroke, StrokeThickness);
            Geometry geo = this.DefiningGeometry.Clone();
            MyInternalBoundsNotTF = geo.Bounds;//内部Rect
            MyExternalBoundsNotTF = geo.GetRenderBounds(pen);//外部Rect

            Geometry exGeo = geo.Clone();
            exGeo.Transform = RenderTransform;//変形
            MyInternalBounds = exGeo.Bounds;//変形内部Rect
            //MyInternalBounds = this.RenderTransform.TransformBounds(MyInternalBoundsNotTF);//変形内部Rect
            MyExternalBounds = exGeo.GetRenderBounds(pen);//変形外部Rect


            MyTFBounds = MyExternalBounds;
            MyTFWidth = MyExternalBounds.Width;
            MyTFHeight = MyExternalBounds.Height;

            Rect r = new(MyExternalBounds.Size);

            var aw = ActualWidth;


            return base.MeasureOverride(constraint);
            //return base.MeasureOverride(MyExternalBounds.Size);
        }
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            //Arrange(MyExternalBounds);
            return base.GetLayoutClip(layoutSlotSize);
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
            RenderTargetBitmap bitmap = new((int)(rect.Width + 1), (int)(rect.Height + 1), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);
            return bitmap;
        }



    }


    /// <summary>
    /// 頂点座標にThumb表示するアドーナー。GeometricShape専用
    /// VisualCollectionにはCanvasだけを追加
    /// ThumbはCanvasに追加
    /// </summary>
    public class GeometryAdorner : Adorner
    {
        public VisualCollection MyVisuals { get; private set; }
        protected override int VisualChildrenCount => MyVisuals.Count;
        protected override Visual GetVisualChild(int index) => MyVisuals[index];

        public Canvas MyCanvas { get; private set; } = new();
        public List<AnchorThumb> MyThumbs { get; private set; } = new();
        //public List<Thumb> MyThumbs { get; private set; } = new();
        public GeometricShape MyTargetGeoShape { get; private set; }
        public GeometryAdorner(GeometricShape adornedElement) : base(adornedElement)
        {
            MyVisuals = new VisualCollection(this)
            {
                MyCanvas
            };
            MyTargetGeoShape = adornedElement;
            Loaded += GeometryAdorner_Loaded;
        }

        /// <summary>
        /// Thumbの再配置、Pointと位置がずれたときに使う
        /// </summary>
        public void RelocationThumbs()
        {
            for (int i = 0; i < MyTargetGeoShape.MyPoints.Count; i++)
            {
                SetAnchorLocate(MyThumbs[i], MyTargetGeoShape.MyPoints[i]);
            }
        }
        private void InitializeThumbs()
        {
            foreach (var item in MyTargetGeoShape.MyPoints)
            {
                AnchorThumb thumb = new(item);
                MyThumbs.Add(thumb);
                MyCanvas.Children.Add(thumb);
                SetAnchorLocate(thumb, item);
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += Thumb_DragCompleted;
            }
        }

        private void GeometryAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeThumbs();
        }

        #region ドラッグ移動

        //ドラッグ移動終了後に親Thumbを位置修正＋頂点Thumbの位置修正
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (MyTargetGeoShape.MyOwnerTThumb?.FixLocate() == true)
            {
                RelocationThumbs();
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is AnchorThumb t)
            {
                int i = MyThumbs.IndexOf(t);
                PointCollection points = MyTargetGeoShape.MyPoints;
                double x = points[i].X + e.HorizontalChange;
                double y = points[i].Y + e.VerticalChange;
                points[i] = new Point(x, y);
                SetAnchorLocate(t, points[i]);
            }
        }
        #endregion ドラッグ移動

        private static void SetAnchorLocate(AnchorThumb anchor, Point point)
        {
            Canvas.SetLeft(anchor, point.X - (anchor.Size / 2.0));
            Canvas.SetTop(anchor, point.Y - (anchor.Size / 2.0));
        }

        //最終的？なRectの指定
        protected override Size ArrangeOverride(Size finalSize)
        {
            //Thumbが収まるRectをCanvasのArrangeに指定する
            Rect canvasRect = VisualTreeHelper.GetDescendantBounds(MyCanvas);
            if (canvasRect.IsEmpty)
            {
                MyCanvas.Arrange(new Rect(finalSize));
            }
            else
            {
                //座標を0,0したRectにする、こうしないとマイナス座表示に不具合
                canvasRect = new(canvasRect.Size);
                MyCanvas.Arrange(canvasRect);
            }
            return base.ArrangeOverride(finalSize);
        }

    }




}
