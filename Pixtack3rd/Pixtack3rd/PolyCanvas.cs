using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

//2023WPF/20230301_PolylineAnchorCanvas2 at main · gogowaten/2023WPF
//https://github.com/gogowaten/2023WPF/tree/main/20230301_PolylineAnchorCanvas2

//ShapeのPolylineZとアンカー点を表示してマウスドラッグ移動するクラス

namespace Pixtack3rd
{
    public class PolyCanvas : Canvas
    {
        #region 依存プロパティ

        public PointCollection MyPoints
        {
            get { return (PointCollection)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }
        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(PolyCanvas),
                new FrameworkPropertyMetadata(new PointCollection(),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Visibility MyAnchorVisible
        {
            get { return (Visibility)GetValue(MyAnchorVisibleProperty); }
            set { SetValue(MyAnchorVisibleProperty, value); }
        }
        public static readonly DependencyProperty MyAnchorVisibleProperty =
            DependencyProperty.Register(nameof(MyAnchorVisible), typeof(Visibility), typeof(PolyCanvas),
                new FrameworkPropertyMetadata(Visibility.Collapsed,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(double), typeof(PolyCanvas),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register(nameof(Y), typeof(double), typeof(PolyCanvas),
                new FrameworkPropertyMetadata(0.0,
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
            DependencyProperty.Register(nameof(HeadEndType), typeof(HeadType), typeof(PolyCanvas),
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
            DependencyProperty.Register(nameof(HeadBeginType), typeof(HeadType), typeof(PolyCanvas),
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
            DependencyProperty.Register(nameof(Angle), typeof(double), typeof(PolyCanvas),
                new FrameworkPropertyMetadata(30.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(PolyCanvas),
                new FrameworkPropertyMetadata(Brushes.Red,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public Brush TTFill
        //{
        //    get { return (Brush)GetValue(TTFillProperty); }
        //    set { SetValue(TTFillProperty, value); }
        //}
        //public static readonly DependencyProperty TTFillProperty =
        //    DependencyProperty.Register(nameof(TTFill), typeof(Brush), typeof(PolyCanvas),
        //        new FrameworkPropertyMetadata(Brushes.Red,
        //            FrameworkPropertyMetadataOptions.AffectsRender |
        //            FrameworkPropertyMetadataOptions.AffectsMeasure |
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(PolyCanvas),
                new FrameworkPropertyMetadata(5.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));





        #endregion 依存プロパティ

        public ObservableCollection<AnchorThumb> MyAnchorThumbs { get; set; } = new();

        public PolylineZ MyShape { get; set; }
        public ContextMenu MyAnchorMenu { get; set; } = new();
        public ContextMenu MyMenu { get; set; } = new();
        public AnchorThumb? MyCurrentAnchorThumb { get; set; }
        public int MyCurrentAnchorIndex;//操作中のアンカー点(Thumb)のインデックス、マウス移動で使う
        public Point MyClickedPoint;//クリックした座標、アンカー点追加時に使う


        public PolyCanvas()
        {
            //背景色は透明色、色を指定しないとクリックが無視される
            this.Background = Brushes.Transparent;

            //表示するPolyline、色と太さは決め打ちしてる、したくないときは依存プロパティをつける
            MyShape = new PolylineZ();
            Children.Add(MyShape);
            //MyShape.Stroke = Brushes.MediumAquamarine;
            //MyShape.StrokeThickness = 20;

            //Loaded時にXAMLで指定されているPointsを使ってアンカーThumb作成追加と関連付けする
            Loaded += PolylinCanvas_Loaded;

            //CanvasのサイズはPolylineのActualに追従
            SetBinding(WidthProperty, new Binding() { Source = MyShape, Path = new PropertyPath(ActualWidthProperty) });
            SetBinding(HeightProperty, new Binding() { Source = MyShape, Path = new PropertyPath(ActualHeightProperty) });

            MyShape.SetBinding(PolylineZ.StrokeProperty, new Binding() { Source = this, Path = new PropertyPath(StrokeProperty) });
            MyShape.SetBinding(PolylineZ.StrokeThicknessProperty, new Binding() { Source = this, Path = new PropertyPath(StrokeThicknessProperty) });
            //MyShape.SetBinding(PolylineZ.FillProperty, new Binding() { Source = this, Path = new PropertyPath(TTFillProperty) });
            MyShape.SetBinding(PolylineZ.AngleProperty, new Binding() { Source = this, Path = new PropertyPath(AngleProperty) });
            MyShape.SetBinding(PolylineZ.HeadBeginTypeProperty, new Binding() { Source = this, Path = new PropertyPath(HeadBeginTypeProperty) });
            MyShape.SetBinding(PolylineZ.HeadEndTypeProperty, new Binding() { Source = this, Path = new PropertyPath(HeadEndTypeProperty) });
            MyShape.SetBinding(PolylineZ.MyPointsProperty, new Binding() { Source = this, Path = new PropertyPath(MyPointsProperty) });



            //アンカーThumbの右クリックメニュー
            MenuItem item = new() { Header = "削除" };
            item.Click += (o, e) => { RemovePoint(MyCurrentAnchorThumb); };
            MyAnchorMenu.Items.Add(item);

            //Canvasの右クリックメニュー
            this.ContextMenu = MyMenu;
            item = new() { Header = "ここに追加" };
            item.Click += (o, e) => { AddPoint(MyClickedPoint); };
            MyMenu.Items.Add(item);


        }

        private void PolylinCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            //MyShape.Points = MyPoints;
            foreach (var item in MyPoints)
            {
                AddAnchorThumb(item);
            }
        }


        //マウスクリック時にクリックされたPoint記録
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            MyClickedPoint = Mouse.GetPosition(this);
        }

        #region アンカー点追加


        //アンカー点追加時には同時にアンカーThumbも追加する
        public void AddPoint(Point point)
        {
            InsertPoint(point, MyPoints.Count);
        }

        public void InsertPoint(Point point, int i)
        {
            MyPoints.Insert(i, point);
            InsertAnchorThumb(point, i);
        }
        private void AddAnchorThumb(Point point)
        {
            InsertAnchorThumb(point, MyAnchorThumbs.Count);
        }
        private void InsertAnchorThumb(Point point, int i)
        {
            AnchorThumb thumb = new(point);
            MyAnchorThumbs.Insert(i, thumb);
            Children.Add(thumb);
            //thumb.DragDelta += Thumb_DragDelta;
            //thumb.DragCompleted += Thumb_DragCompleted;
            thumb.DragDelta += Thumb_DragDelta2;
            thumb.PreviewMouseDown += Thumb_PreviewMouseDown;
            thumb.SetBinding(VisibilityProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(MyAnchorVisibleProperty)
            });
            thumb.ContextMenu = MyAnchorMenu;
        }
        #endregion アンカー点追加

        #region アンカー点削除
        //アンカー点削除、関連するアンカーThumbも削除する
        public void RemovePoint(int pointIndex)
        {
            if (MyPoints.Count <= 2) { return; }
            if (pointIndex < 0) { return; }
            MyPoints.RemoveAt(pointIndex);
            AnchorThumb thumb = MyAnchorThumbs[pointIndex];
            MyAnchorThumbs.Remove(thumb);
            Children.Remove(thumb);
            MyCurrentAnchorThumb = null;
        }
        public void RemovePoint(AnchorThumb? thumb)
        {
            if (thumb == null) { return; }
            RemovePoint(MyCurrentAnchorIndex);
        }
        #endregion アンカー点削除


        //アンカーThumbをクリックしたときIndexの更新
        private void Thumb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is AnchorThumb thumb)
            {
                MyCurrentAnchorThumb = thumb;
                MyCurrentAnchorIndex = MyAnchorThumbs.IndexOf(thumb);
            }
            else { MyCurrentAnchorIndex = -1; }
        }

        #region ドラッグ移動
        //未使用
        //Delta時には動かしているPointだけの更新にとどめて
        //本体の座標や全体Pointの修正はCompleted時に行う版
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e.OriginalSource is AnchorThumb anchorT)
            {
                //該当のアンカーThumbの座標修正
                anchorT.X += e.HorizontalChange;
                anchorT.Y += e.VerticalChange;
                MyPoints[MyCurrentAnchorIndex] = new Point(anchorT.X, anchorT.Y);
            }
        }

        //移動終了後に本体の座標修正
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (e.OriginalSource is AnchorThumb anchorT)
            {
                //全体のアンカー点から左上座標取得
                double minX = anchorT.X;
                double minY = anchorT.Y;
                foreach (var item in MyAnchorThumbs)
                {
                    if (minX > item.X) { minX = item.X; }
                    if (minY > item.Y) { minY = item.Y; }
                }

                //左上座標が0なら該当Pointだけ変更、
                //違う場合は本体と全アンカー点を修正
                if (minX == 0 && minY == 0)
                {
                    MyPoints[MyCurrentAnchorIndex] = new Point(anchorT.X, anchorT.Y);
                }
                else
                {
                    X += minX; Y += minY;

                    for (int i = 0; i < MyPoints.Count; i++)
                    {
                        MyAnchorThumbs[i].X -= minX;
                        MyAnchorThumbs[i].Y -= minY;
                        MyPoints[i] = new Point(MyAnchorThumbs[i].X, MyAnchorThumbs[i].Y);
                    }
                }
            }
        }
        #endregion ドラッグ移動

        #region ドラッグ移動2
        //負荷が高そうだと思ったけど誤差程度なのでこっちのほうがいいかも
        //移動終了後の処理はないので、DragCompleatedは必要ない
        /// <summary>
        /// 移動中常に本体の座標修正する版
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_DragDelta2(object sender, DragDeltaEventArgs e)
        {
            if (e.OriginalSource is AnchorThumb anchorT)
            {
                //該当のアンカーThumbの座標修正
                anchorT.X += e.HorizontalChange;
                anchorT.Y += e.VerticalChange;

                //全体のアンカー点から左上座標取得
                double minX = anchorT.X;
                double minY = anchorT.Y;
                foreach (var item in MyAnchorThumbs)
                {
                    if (minX > item.X) { minX = item.X; }
                    if (minY > item.Y) { minY = item.Y; }
                }

                //左上座標が0なら該当Pointだけ変更、
                //違う場合は本体と全アンカー点を修正
                if (minX == 0 && minY == 0)
                {
                    MyPoints[MyCurrentAnchorIndex] = new Point(anchorT.X, anchorT.Y);
                }
                else
                {
                    //SetLeft(this, GetLeft(this) + minX);
                    //SetTop(this, GetTop(this) + minY);
                    X += minX; Y += minY;

                    for (int i = 0; i < MyPoints.Count; i++)
                    {
                        MyAnchorThumbs[i].X -= minX;
                        MyAnchorThumbs[i].Y -= minY;
                        MyPoints[i] = new Point(MyAnchorThumbs[i].X, MyAnchorThumbs[i].Y);
                    }
                }
            }
        }
        #endregion ドラッグ移動2

    }
}
