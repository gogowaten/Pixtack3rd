using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pixtack3rd
{
    public class AnchorThumb : Thumb
    {
        public Rectangle MyTemplateElement { get; private set; }
        private Point MyPoint;
        public AnchorThumb(Point point)
        {
            MyTemplateElement = SetTemplate();
            Canvas.SetLeft(this, point.X);
            Canvas.SetTop(this, point.Y);
            Width = 20;
            Height = 20;
            DragDelta += AnchorThumb_DragDelta;
        }

        private void AnchorThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is AnchorThumb at)
            {
                Canvas.SetLeft(at, Canvas.GetLeft(at) + e.HorizontalChange);
                Canvas.SetTop(at, Canvas.GetTop(at) + e.VerticalChange);
            }
        }

        private Rectangle SetTemplate()
        {
            FrameworkElementFactory fRect = new(typeof(Rectangle), "rect");
            fRect.SetValue(Rectangle.FillProperty, Brushes.Transparent);
            fRect.SetValue(Rectangle.StrokeProperty, Brushes.Black);
            fRect.SetValue(Rectangle.StrokeDashArrayProperty, new DoubleCollection() { 2.0 });
            this.Template = new() { VisualTree = fRect };
            this.ApplyTemplate();
            if (this.Template.FindName("rect", this) is Rectangle rect)
            {
                return rect;
            }
            else { throw new Exception(); }
        }
    }

}
