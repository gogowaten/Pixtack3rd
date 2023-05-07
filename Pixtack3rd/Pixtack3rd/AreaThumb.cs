using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Media;

namespace Pixtack3rd
{
    public class AreaThumb : Thumb
    {

        #region 依存関係プロパティ
        public double AreaOpacity
        {
            get { return (double)GetValue(AreaOpacityProperty); }
            set { SetValue(AreaOpacityProperty, value); }
        }
        public static readonly DependencyProperty AreaOpacityProperty =
            DependencyProperty.Register(nameof(AreaOpacity), typeof(double), typeof(AreaThumb),
                new FrameworkPropertyMetadata(0.2,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(double), typeof(AreaThumb),
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
            DependencyProperty.Register(nameof(Y), typeof(double), typeof(AreaThumb),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存関係プロパティ

        public Canvas MyTemplate { get; private set; }
        public ContextMenu MyContextMenu { get; private set; }
        public ResizeAdorner MyAdorner { get; private set; }
        public AreaThumb()
        {
            MyTemplate = SetTemplate();
            MyAdorner = new ResizeAdorner(this) { ThumbSize = 20.0 };
            Loaded += AreaThumb_Loaded;
            MyContextMenu = new ContextMenu();
            MenuItem item = new() { Header = "testarea" };
            MyContextMenu.Items.Add(item);
            //MyTemplate.Background = Brushes.Red;
            
            SetMyEvents();
        }

        
        private void SetMyEvents()
        {
            DragDelta += AreaThumb_DragDelta;
        }

        private void AreaThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            X += e.HorizontalChange;
            Y += e.VerticalChange;
            //Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);

        }

        private void AreaThumb_Loaded(object sender, RoutedEventArgs e)
        {
            MyAdorner.ContextMenu = MyContextMenu;
            AdornerLayer.GetAdornerLayer(this).Add(MyAdorner);

            MyTemplate.SetBinding(BackgroundProperty, new Binding() { Source = this, Path = new PropertyPath(BackgroundProperty) });
            MyTemplate.SetBinding(OpacityProperty, new Binding() { Source = this, Path = new PropertyPath(AreaOpacityProperty) });

            SetBinding(XProperty, new Binding() { Source = this, Path = new PropertyPath(Canvas.LeftProperty), Mode = BindingMode.TwoWay });
            SetBinding(YProperty, new Binding() { Source = this, Path = new PropertyPath(Canvas.TopProperty), Mode = BindingMode.TwoWay });

            //表示の有無をマーカーと連動
            MyAdorner.SetBinding(VisibilityProperty,new Binding() { Source = this,Path=new PropertyPath(VisibilityProperty) });
        }

        private Canvas SetTemplate()
        {
            FrameworkElementFactory factory = new(typeof(Canvas), "nemo");
            Template = new ControlTemplate() { VisualTree = factory };
            ApplyTemplate();
            if (Template.FindName("nemo", this) is Canvas panel)
            {
                return panel;
            }
            else throw new NullReferenceException();
        }


        private void SetMyContextMenu()
        {
            this.ContextMenu = MyContextMenu;
            MenuItem item = new() { Header = "コピー" };
            MyContextMenu.Items.Add(item);
            item.Click += Item_Click;
            item = new() { Header = "複製" };
            MyContextMenu.Items.Add(item);
            item.Click += Item_Click1;
            item = new() { Header = "名前をつけて保存" };
            MyContextMenu.Items.Add(item);
            item.Click += Item_Click2;
        }

        private void Item_Click2(object sender, RoutedEventArgs e)
        {

        }

        private void Item_Click1(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

}
