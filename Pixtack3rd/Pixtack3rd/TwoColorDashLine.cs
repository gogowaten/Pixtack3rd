using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Pixtack3rd
{

    //[028722]ベジエ曲線の各部の名称
    //https://support.justsystems.com/faq/1032/app/servlet/qadoc?QID=028722

    //ベジェ曲線の方向線とアンカーポイント、制御点を表示してみた - 午後わてんのブログ
    //https://gogowaten.hatenablog.com/entry/15547295
    //WPF、ベジェ曲線で直線表示、アンカー点の追加と削除 - 午後わてんのブログ
    //https://gogowaten.hatenablog.com/entry/2022/06/14/132217

    /// <summary>
    /// ベジェ曲線の方向線表示用、2色破線
    /// OnRenderで直線描画、その上にDefiningGeometryで破線描画
    /// </summary>
    public class TwoColorDashLine : Shape
    {
        public PointCollection MyPoints
        {
            get { return (PointCollection)GetValue(MyPointsProperty); }
            set { SetValue(MyPointsProperty, value); }
        }
        public static readonly DependencyProperty MyPointsProperty =
            DependencyProperty.Register(nameof(MyPoints), typeof(PointCollection), typeof(TwoColorDashLine),
                new FrameworkPropertyMetadata(new PointCollection(),
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// ベースの色、下地になる実線の色
        /// </summary>
        public Brush StrokeBase
        {
            get { return (Brush)GetValue(StrokeBaseProperty); }
            set { SetValue(StrokeBaseProperty, value); }
        }
        public static readonly DependencyProperty StrokeBaseProperty =
            DependencyProperty.Register(nameof(StrokeBase), typeof(Brush), typeof(TwoColorDashLine),
                new FrameworkPropertyMetadata(Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));


        public TwoColorDashLine()
        {
            Stroke = Brushes.Gold;
            StrokeThickness = 2.0;
            StrokeDashArray = new DoubleCollection() { 10.0 };
            //StrokeDashArray = new DoubleCollection() { 10.0,5.0 };
        }

        //下地になる実線描画
        protected override void OnRender(DrawingContext drawingContext)
        {
            for (int i = 0; i < MyPoints.Count - 1; i++)
            {
                if ((i - 1) % 3 != 0)
                {
                    drawingContext.DrawLine(new Pen(StrokeBase, StrokeThickness), MyPoints[i], MyPoints[i + 1]);
                }
            }
            base.OnRender(drawingContext);
        }

        //破線描画
        protected override Geometry DefiningGeometry
        {
            get
            {
                StreamGeometry geometry = new();
                using (var context = geometry.Open())
                {
                    for (int i = 0; i < MyPoints.Count - 1; i++)
                    {
                        if ((i - 1) % 3 != 0)
                        {
                            //context.BeginFigure(MyPoints[0], false, false);
                            //context.LineTo(MyPoints[1], true, false);
                            //context.BeginFigure(MyPoints[2], false, false);
                            //context.LineTo(MyPoints[3], true, false);
                            //context.BeginFigure(MyPoints[3], false, false);
                            //context.LineTo(MyPoints[4], true, false);
                            //context.BeginFigure(MyPoints[5], false, false);
                            //context.LineTo(MyPoints[6], true, false);
                            //context.BeginFigure(MyPoints[6], false, false);
                            //context.LineTo(MyPoints[7], true, false);
                            //context.BeginFigure(MyPoints[8], false, false);
                            //context.LineTo(MyPoints[9], true, false);

                            context.BeginFigure(MyPoints[i], false, false);
                            context.LineTo(MyPoints[i + 1], true, false);
                        }
                    }
                }
                geometry.Freeze();
                return geometry;
            }
        }
    }
}
