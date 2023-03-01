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

namespace Pixtack3rd
{
    public class PolylineCanvas : Canvas
    {
        #region 依存プロパティ

        public PointCollection MyPoints
        {
            get { return (PointCollection)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }
        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(PolylineCanvas),
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
            DependencyProperty.Register(nameof(MyAnchorVisible), typeof(Visibility), typeof(PolylineCanvas),
                new FrameworkPropertyMetadata(Visibility.Collapsed,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(double), typeof(PolylineCanvas),
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
            DependencyProperty.Register(nameof(Y), typeof(double), typeof(PolylineCanvas),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion 依存プロパティ
        public ObservableCollection<AnchorThumb> MyAnchorThumbs { get; set; } = new();

        public Polyline MyShape { get; set; }
        public ContextMenu MyAnchorMenu { get; set; } = new();
        public ContextMenu MyMenu { get; set; } = new();
        public AnchorThumb? MyCurrentAnchorThumb { get; set; }
        public int MyCurrentAnchorIndex;//操作中のアンカー点(Thumb)のインデックス、マウス移動で使う
        public Point MyClickedPoint;//クリックした座標、アンカー点追加時に使う

        public PolylineCanvas()
        {
            //背景色は透明色、色を指定しないとクリックが無視される
            this.Background = Brushes.Transparent;

            //表示するPolyline、色と太さは決め打ちしてる、したくないときは依存プロパティをつける
            MyShape = new Polyline();
            Children.Add(MyShape);
            MyShape.Stroke = Brushes.MediumAquamarine;
            MyShape.StrokeThickness = 20;

            //Loaded時にXAMLで指定されているPointsを使ってアンカーThumb作成追加と関連付けする
            Loaded += PolylinCanvas_Loaded;

            //CanvasのサイズはPolylineのActualに追従
            SetBinding(WidthProperty, new Binding() { Source = MyShape, Path = new PropertyPath(ActualWidthProperty) });
            SetBinding(HeightProperty, new Binding() { Source = MyShape, Path = new PropertyPath(ActualHeightProperty) });
            SetBinding(MyPointsProperty, new Binding() { Source = MyShape, Path = new PropertyPath(Polyline.PointsProperty) });
            SetBinding(LeftProperty, new Binding() { Source = this, Path = new PropertyPath(XProperty) });
            SetBinding(TopProperty, new Binding() { Source = this, Path = new PropertyPath(YProperty) });

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

        //マウスクリック時にクリックされたPoint記録
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            MyClickedPoint = Mouse.GetPosition(this);
        }


        private void PolylinCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            //MyShape.Points = MyPoints;
            foreach (var item in MyPoints)
            {
                AddAnchorThumb(item);
            }
        }

        #region アンカー点追加と削除

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
            thumb.DragDelta += Thumb_DragDelta;
            thumb.PreviewMouseDown += Thumb_PreviewMouseDown;
            thumb.SetBinding(VisibilityProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(MyAnchorVisibleProperty)
            });
            thumb.ContextMenu = MyAnchorMenu;
        }

        //アンカー点削除、関連するアンカーThumbも削除する
        public void RemovePoint(int pointIndex)
        {
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
        #endregion アンカー点追加と削除

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


        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
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
            //if (e.OriginalSource is AnchorThumb anchorT)
            //{
            //    anchorT.X += e.HorizontalChange;
            //    anchorT.Y += e.VerticalChange;
            //    MyPoints[MyCurrentAnchorIndex] = new Point(anchorT.X, anchorT.Y);
            //}


        }
    }
}
