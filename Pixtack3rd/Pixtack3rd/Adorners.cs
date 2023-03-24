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
    /// GeometricShape専用アドーナー、頂点座標にThumb表示と制御線も表示
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
        //Canvas要素をVisualCollectionに追加
        //Canvasに頂点Thumbと制御線を追加する
        public GeometryAdorner(GeometricShape adornedElement) : base(adornedElement)
        {
            MyDirectionLine = new TwoColorDashLine();
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
            //制御線のプロパティ設定
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
            //ベジェ曲線のときは操作によって他の頂点も連動させたい
            //+ctrlで対角線上連動、これは使わないからいらないかも
            //+shiftで対角線上連動＋距離連動、+shiftは取り消し、初期値true
            //固定点と制御点の連動、初期値true
            //角度固定(長さのみ可変)

            if (sender is AnchorThumb t)
            {
                int i = MyThumbs.IndexOf(t);
                PointCollection points = MyTargetGeoShape.MyPoints;
                double xAdd = e.HorizontalChange;
                double yAdd = e.VerticalChange;
                double x = points[i].X + xAdd;
                double y = points[i].Y + yAdd;
                if (MyTargetGeoShape.MyShapeType == ShapeType.Line)
                {
                    points[i] = new Point(x, y);
                    SetAnchorLocate(t, points[i]);

                }
                //ベジェ曲線のとき、今のところ固定点移動時に制御点も同時移動、制御点移動は対角線上になるように移動
                else if (MyTargetGeoShape.MyShapeType == ShapeType.Bezier)
                {
                    //前制御点、固定点、後制御点の判定
                    Point frontP, anchorP, rearP;
                    int mod = i % 3;
                    if (mod == 0)
                    {
                        //固定点、前後の制御点も同じ移動
                        anchorP = points[i];
                        SetAnchorLocate2(i, new Point(anchorP.X + xAdd, anchorP.Y + yAdd));

                        //自身が先頭固定点じゃないときは前制御点があるので移動させる
                        if (i != 0)
                        {
                            frontP = points[i - 1];
                            SetAnchorLocate2(i - 1, new Point(frontP.X + xAdd, frontP.Y + yAdd));
                        }

                        //自身が最終固定点じゃないときは後制御点があるので移動させる
                        if (i != points.Count - 1)
                        {
                            rearP = points[i + 1];
                            SetAnchorLocate2(i + 1, new Point(rearP.X + xAdd, rearP.Y + yAdd));
                        }

                    }
                    //自身が後制御点
                    else if (mod == 1)
                    {
                        rearP = points[i];
                        SetAnchorLocate2(i, new Point(rearP.X + xAdd, rearP.Y + yAdd));

                        //前制御点の移動、対角線上になるように移動
                        if (i != 1)
                        {
                            anchorP = points[i - 1];
                            double xDiff = rearP.X - anchorP.X;
                            double yDiff = rearP.Y - anchorP.Y;
                            SetAnchorLocate2(i - 2, new Point(anchorP.X - xDiff, anchorP.Y - yDiff));
                        }
                    }
                    //自身が前制御点
                    else
                    {
                        //後制御点の移動、対角線上になるように移動
                        frontP = points[i];
                        SetAnchorLocate2(i, new Point(frontP.X + xAdd, frontP.Y + yAdd));

                        if (i != points.Count - 2)
                        {
                            anchorP = points[i + 1];
                            double xDiff = frontP.X - anchorP.X;
                            double yDiff = frontP.Y - anchorP.Y;
                            SetAnchorLocate2(i + 2, new Point(anchorP.X - xDiff, anchorP.Y - yDiff));
                        }
                    }
                }


            }
        }
        #endregion ドラッグ移動

        private void SetAnchorLocate2(int index, Point point)
        {
            MyTargetGeoShape.MyPoints[index] = point;
            SetAnchorLocate(MyThumbs[index], point);
        }


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
