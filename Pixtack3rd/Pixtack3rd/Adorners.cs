using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace Pixtack3rd
{

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
        public TwoColorDashLine MyDirectionLine { get; private set; }
        public PathFigureCollection MyControlLines { get; private set; } = new();

        public GeometricShape MyTargetGeoShape { get; private set; }


        //コンストラクタ
        public GeometryAdorner(GeometricShape adornedElement) : base(adornedElement)
        {
            MyDirectionLine= new TwoColorDashLine();
            MyCanvas.Children.Add(MyDirectionLine);

            MyVisuals = new VisualCollection(this)
            {
                MyCanvas,
            };
            MyTargetGeoShape = adornedElement;
            Loaded += GeometryAdorner_Loaded;

        }


        private void GeometryAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeThumbs();
            //MyDirectionLine.Stroke = Brushes.Red;
            //MyDirectionLine.StrokeBase = Brushes.Black;
            //MyDirectionLine.StrokeThickness = 1.0;
            MyDirectionLine.MyPoints = MyTargetGeoShape.MyPoints;
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

                int figureCount = MyControlLines.Count;
                
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
    
    ///// <summary>
    ///// 頂点座標にThumb表示するアドーナー。GeometricShape専用
    ///// VisualCollectionにはCanvasだけを追加
    ///// ThumbはCanvasに追加
    ///// </summary>
    //public class GeometryAdorner : Adorner
    //{
    //    public VisualCollection MyVisuals { get; private set; }
    //    protected override int VisualChildrenCount => MyVisuals.Count;
    //    protected override Visual GetVisualChild(int index) => MyVisuals[index];

    //    public Canvas MyCanvas { get; private set; } = new();
    //    public List<AnchorThumb> MyThumbs { get; private set; } = new();
    //    //public List<Thumb> MyThumbs { get; private set; } = new();
    //    public GeometricShape MyTargetGeoShape { get; private set; }
    //    public GeometryAdorner(GeometricShape adornedElement) : base(adornedElement)
    //    {
    //        MyVisuals = new VisualCollection(this)
    //        {
    //            MyCanvas
    //        };
    //        MyTargetGeoShape = adornedElement;
    //        Loaded += GeometryAdorner_Loaded;
    //    }

    //    /// <summary>
    //    /// Thumbの再配置、Pointと位置がずれたときに使う
    //    /// </summary>
    //    public void RelocationThumbs()
    //    {
    //        for (int i = 0; i < MyTargetGeoShape.MyPoints.Count; i++)
    //        {
    //            SetAnchorLocate(MyThumbs[i], MyTargetGeoShape.MyPoints[i]);
    //        }
    //    }
    //    private void InitializeThumbs()
    //    {
    //        foreach (var item in MyTargetGeoShape.MyPoints)
    //        {
    //            AnchorThumb thumb = new(item);
    //            MyThumbs.Add(thumb);
    //            MyCanvas.Children.Add(thumb);
    //            SetAnchorLocate(thumb, item);
    //            thumb.DragDelta += Thumb_DragDelta;
    //            thumb.DragCompleted += Thumb_DragCompleted;
    //        }
    //    }

    //    private void GeometryAdorner_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        InitializeThumbs();
    //    }

    //    #region ドラッグ移動

    //    //ドラッグ移動終了後に親Thumbを位置修正＋頂点Thumbの位置修正
    //    private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
    //    {
    //        if (MyTargetGeoShape.MyOwnerTThumb?.FixLocate() == true)
    //        {
    //            RelocationThumbs();
    //        }
    //    }

    //    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    //    {
    //        if (sender is AnchorThumb t)
    //        {
    //            int i = MyThumbs.IndexOf(t);
    //            PointCollection points = MyTargetGeoShape.MyPoints;
    //            double x = points[i].X + e.HorizontalChange;
    //            double y = points[i].Y + e.VerticalChange;
    //            points[i] = new Point(x, y);
    //            SetAnchorLocate(t, points[i]);
    //        }
    //    }
    //    #endregion ドラッグ移動

    //    private static void SetAnchorLocate(AnchorThumb anchor, Point point)
    //    {
    //        Canvas.SetLeft(anchor, point.X - (anchor.Size / 2.0));
    //        Canvas.SetTop(anchor, point.Y - (anchor.Size / 2.0));
    //    }

    //    //最終的？なRectの指定
    //    protected override Size ArrangeOverride(Size finalSize)
    //    {
    //        //Thumbが収まるRectをCanvasのArrangeに指定する
    //        Rect canvasRect = VisualTreeHelper.GetDescendantBounds(MyCanvas);
    //        if (canvasRect.IsEmpty)
    //        {
    //            MyCanvas.Arrange(new Rect(finalSize));
    //        }
    //        else
    //        {
    //            //座標を0,0したRectにする、こうしないとマイナス座表示に不具合
    //            canvasRect = new(canvasRect.Size);
    //            MyCanvas.Arrange(canvasRect);
    //        }
    //        return base.ArrangeOverride(finalSize);
    //    }

    //}


}
