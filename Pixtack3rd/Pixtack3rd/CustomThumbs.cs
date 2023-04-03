using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;

namespace Pixtack3rd
{
    class CustomThumbs
    {
    }

    public class TwoColorWakuThumb : Thumb
    {
        #region 依存関係プロパティ

        public Brush MyWakuSotoColor
        {
            get { return (Brush)GetValue(MyWakuSotoColorProperty); }
            set { SetValue(MyWakuSotoColorProperty, value); }
        }
        public static readonly DependencyProperty MyWakuSotoColorProperty =
            DependencyProperty.Register(nameof(MyWakuSotoColor), typeof(Brush), typeof(TwoColorWakuThumb),
                new FrameworkPropertyMetadata(Brushes.Red,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public Brush MyWakuNakaColor
        {
            get { return (Brush)GetValue(MyWakuNakaColorProperty); }
            set { SetValue(MyWakuNakaColorProperty, value); }
        }
        public static readonly DependencyProperty MyWakuNakaColorProperty =
            DependencyProperty.Register(nameof(MyWakuNakaColor), typeof(Brush), typeof(TwoColorWakuThumb),
                new FrameworkPropertyMetadata(Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        #endregion 依存関係プロパティ

        //public Rectangle MyMarker1 { get; private set; } = new();
        //public Rectangle MyMarker2 { get; private set; } = new();
        public Ellipse MyMarker1 { get; private set; } = new();
        public Ellipse MyMarker2 { get; private set; } = new();
        public Canvas MyCanvas { get; private set; }
        public TwoColorWakuThumb()
        {
            MyCanvas = SetTemplate();
            MyCanvas.Children.Add(MyMarker1);
            MyCanvas.Children.Add(MyMarker2);
            
            MyCanvas.Background = Brushes.Transparent;
            SetMyBindings();
            Loaded += TwoColorWakuThumb_Loaded;
        }

        private void TwoColorWakuThumb_Loaded(object sender, RoutedEventArgs e)
        {
            MyMarker2.Height = MyMarker1.Height - 2;
            MyMarker2.Width = MyMarker1.Width - 2;
            Canvas.SetLeft(MyMarker2, 1);
            Canvas.SetTop(MyMarker2, 1);
        }

        private Canvas SetTemplate()
        {
            FrameworkElementFactory factory = new(typeof(Canvas), "nemo");
            this.Template = new() { VisualTree = factory };
            this.ApplyTemplate();
            if (Template.FindName("nemo", this) is Canvas panel)
            {
                return panel;
            }
            else throw new ArgumentException();
        }
        private void SetMyBindings()
        {
            //MyCanvas.SetBinding(Canvas.BackgroundProperty, new Binding() { Source = this, Path = new PropertyPath(BackgroundProperty) ,Mode= BindingMode.TwoWay});

            MyMarker1.SetBinding(Rectangle.StrokeProperty, new Binding() { Source = this, Path = new PropertyPath(MyWakuSotoColorProperty) });
            MyMarker1.SetBinding(Rectangle.WidthProperty, new Binding() { Source = this, Path = new PropertyPath(ActualWidthProperty) });
            MyMarker1.SetBinding(Rectangle.HeightProperty, new Binding() { Source = this, Path = new PropertyPath(ActualHeightProperty) });

            MyMarker2.SetBinding(Rectangle.StrokeProperty, new Binding() { Source = this, Path = new PropertyPath(MyWakuNakaColorProperty) });
            //MyRect2.SetBinding(Rectangle.WidthProperty, new Binding() { Source = this, Path = new PropertyPath(ActualWidthProperty) });
            //MyRect2.SetBinding(Rectangle.HeightProperty, new Binding() { Source = this, Path = new PropertyPath(ActualHeightProperty) });

        }
    }


    //2色の点線枠のThumb
    //塗りつぶしは透明で決め打ち
    //public class TwoToneWakuThumb : Thumb
    //{
    //    #region 依存関係プロパティ

    //    public Brush MyWakuColor1
    //    {
    //        get { return (Brush)GetValue(MyWakuColor1Property); }
    //        set { SetValue(MyWakuColor1Property, value); }
    //    }
    //    public static readonly DependencyProperty MyWakuColor1Property =
    //        DependencyProperty.Register(nameof(MyWakuColor1), typeof(Brush), typeof(TwoToneWakuThumb),
    //            new FrameworkPropertyMetadata(Brushes.Red,
    //                FrameworkPropertyMetadataOptions.AffectsRender |
    //                FrameworkPropertyMetadataOptions.AffectsMeasure |
    //                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    //    public Brush MyWakuColor2
    //    {
    //        get { return (Brush)GetValue(MyWakuColor2Property); }
    //        set { SetValue(MyWakuColor2Property, value); }
    //    }
    //    public static readonly DependencyProperty MyWakuColor2Property =
    //        DependencyProperty.Register(nameof(MyWakuColor2), typeof(Brush), typeof(TwoToneWakuThumb),
    //            new FrameworkPropertyMetadata(Brushes.White,
    //                FrameworkPropertyMetadataOptions.AffectsRender |
    //                FrameworkPropertyMetadataOptions.AffectsMeasure |
    //                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    //    public DoubleCollection MyWakuDash
    //    {
    //        get { return (DoubleCollection)GetValue(MyWakuDashProperty); }
    //        set { SetValue(MyWakuDashProperty, value); }
    //    }
    //    public static readonly DependencyProperty MyWakuDashProperty =
    //        DependencyProperty.Register(nameof(MyWakuDash), typeof(DoubleCollection), typeof(TwoToneWakuThumb),
    //            new FrameworkPropertyMetadata(new DoubleCollection() { 4.0 },
    //                FrameworkPropertyMetadataOptions.AffectsRender |
    //                FrameworkPropertyMetadataOptions.AffectsMeasure |
    //                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    //    #endregion 依存関係プロパティ

    //    public Rectangle MyRect1 { get; private set; } = new();
    //    public Rectangle MyRect2 { get; private set; } = new();
    //    public Canvas MyCanvas { get; private set; }
    //    public TwoToneWakuThumb()
    //    {
    //        MyCanvas = SetTemplate();
    //        MyCanvas.Children.Add(MyRect1);
    //        MyCanvas.Children.Add(MyRect2);
    //        MyCanvas.Background = Brushes.Transparent;
    //        SetMyBindings();
    //    }
    //    private Canvas SetTemplate()
    //    {
    //        FrameworkElementFactory factory = new(typeof(Canvas), "nemo");
    //        this.Template = new() { VisualTree = factory };
    //        this.ApplyTemplate();
    //        if (Template.FindName("nemo", this) is Canvas panel)
    //        {
    //            return panel;
    //        }
    //        else throw new ArgumentException();
    //    }
    //    private void SetMyBindings()
    //    {
    //        //MyCanvas.SetBinding(Canvas.BackgroundProperty, new Binding() { Source = this, Path = new PropertyPath(BackgroundProperty) ,Mode= BindingMode.TwoWay});

    //        MyRect1.SetBinding(Rectangle.StrokeProperty, new Binding() { Source = this, Path = new PropertyPath(MyWakuColor1Property) });
    //        MyRect1.SetBinding(Rectangle.WidthProperty, new Binding() { Source = this, Path = new PropertyPath(ActualWidthProperty) });
    //        MyRect1.SetBinding(Rectangle.HeightProperty, new Binding() { Source = this, Path = new PropertyPath(ActualHeightProperty) });

    //        MyRect2.SetBinding(Rectangle.WidthProperty, new Binding() { Source = this, Path = new PropertyPath(ActualWidthProperty) });
    //        MyRect2.SetBinding(Rectangle.HeightProperty, new Binding() { Source = this, Path = new PropertyPath(ActualHeightProperty) });
    //        MyRect2.SetBinding(Rectangle.StrokeProperty, new Binding() { Source = this, Path = new PropertyPath(MyWakuColor2Property) });
    //        MyRect2.SetBinding(Rectangle.StrokeDashArrayProperty, new Binding() { Source = this, Path = new PropertyPath(MyWakuDashProperty), Mode = BindingMode.TwoWay });
    //    }
    //}

}
