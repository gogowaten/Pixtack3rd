using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Pixtack3rd
{
    public abstract class TThumb : Thumb
    {
        #region 依存プロパティ

        public double TTLeft
        {
            get { return (double)GetValue(TTLeftProperty); }
            set { SetValue(TTLeftProperty, value); }
        }
        public static readonly DependencyProperty TTLeftProperty =
            DependencyProperty.Register(nameof(TTLeft), typeof(double), typeof(TThumb),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double TTTop
        {
            get { return (double)GetValue(TTTopProperty); }
            set { SetValue(TTTopProperty, value); }
        }
        public static readonly DependencyProperty TTTopProperty =
            DependencyProperty.Register(nameof(TTTop), typeof(double), typeof(TThumb),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存プロパティ
        protected readonly string TEMPLATE_NAME = "NEMO";
        public TThumb()
        {
            SetBinding(Canvas.LeftProperty,
                new Binding() { Path = new PropertyPath(TTLeftProperty), Source = this });
            SetBinding(Canvas.TopProperty,
                new Binding() { Path = new PropertyPath(TTTopProperty), Source = this });


        }
        protected T? MakeTemplate<T>()
        {
            FrameworkElementFactory factory = new(typeof(T), TEMPLATE_NAME);
            this.Template = new() { VisualTree = factory };
            this.ApplyTemplate();
            if (this.Template.FindName(TEMPLATE_NAME, this) is T element)
            {
                return element;
            }
            else return default;
        }


    }

    [ContentProperty(nameof(Items))]
    public class TTGroup : TThumb
    {
        public DataGroup MyData { get; set; }
        //        protected readonly string TEMPLATE_NAME = "NEMO";
        private ItemsControl MyTemplateElement;
        public ObservableCollection<TThumb> Items { get; private set; } = new();

        public TTGroup()
        {
            MyData = new DataGroup();
            MyTemplateElement = MyInitializeBinding();
            MyTemplateElement.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Items)) { Source = this });
        }
        public TTGroup(DataGroup data)
        {
            MyData = data;
            MyTemplateElement = MyInitializeBinding();
            MyTemplateElement.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Items)) { Source = this });
        }
        private ItemsControl MyInitializeBinding()
        {
            this.DataContext = MyData;
            SetBinding(TTLeftProperty, nameof(MyData.X));
            SetBinding(TTTopProperty, nameof(MyData.Y));

            return MakeTemplate();

        }
        private ItemsControl MakeTemplate()
        {
            FrameworkElementFactory factory = new(typeof(ItemsControl), TEMPLATE_NAME);
            factory.SetValue(ItemsControl.ItemsPanelProperty,
                new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas))));
            this.Template = new() { VisualTree = factory };
            this.ApplyTemplate();
            if (this.Template.FindName(TEMPLATE_NAME, this) is ItemsControl element)
            {
                return element;
            }
            else { throw new ArgumentException("テンプレート作成できんかった"); }
        }
        public void AddItem(TThumb thumb, Data data)
        {
            Items.Add(thumb);
            MyData.Datas.Add(data);
        }
        public void RemoveItem(TThumb thumb, Data data)
        {
            Items.Remove(thumb);
            MyData.Datas.Remove(data);
        }
        public void AddItem(Data data)
        {
            switch (data.Type)
            {
                case TType.None:
                    break;
                case TType.TextBlock:
                    AddItem(new TTTextBlock((DataTextBlock)data), data);
                    break;
                case TType.Group:
                    AddItem(new TTGroup((DataGroup)data), data);
                    break;
                case TType.Image:
                    AddItem(new TTImage((DataImage)data), data);
                    break;
                case TType.Rectangle:
                    throw new NotImplementedException();
            }
        }
    }

    public class TTTextBlock : TThumb
    {
        #region 依存プロパティ

        public string TTText
        {
            get { return (string)GetValue(TTTextProperty); }
            set { SetValue(TTTextProperty, value); }
        }
        public static readonly DependencyProperty TTTextProperty =
            DependencyProperty.Register(nameof(TTText), typeof(string), typeof(TTTextBlock),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存プロパティ

        public DataTextBlock MyData { get; set; }
        private readonly TextBlock MyTemplateElement;

        public TTTextBlock() : this(new DataTextBlock()) { }
        public TTTextBlock(DataTextBlock data)
        {
            MyData = data;
            this.DataContext = MyData;
            if (MakeTemplate<TextBlock>() is TextBlock element)
            {
                MyTemplateElement = element;
            }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            SetBinding(TTTextProperty, nameof(MyData.Text));
            MyTemplateElement.SetBinding(TextBlock.TextProperty, nameof(MyData.Text));
        }
    }

    public class TTImage : TThumb
    {

        public BitmapSource TTSource
        {
            get { return (BitmapSource)GetValue(TTSourceProperty); }
            set { SetValue(TTSourceProperty, value); }
        }
        public static readonly DependencyProperty TTSourceProperty =
            DependencyProperty.Register(nameof(TTSource), typeof(BitmapSource), typeof(TTImage),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public string TTSourcePath
        {
            get { return (string)GetValue(TTSourcePathProperty); }
            set { SetValue(TTSourcePathProperty, value); }
        }
        public static readonly DependencyProperty TTSourcePathProperty =
            DependencyProperty.Register(nameof(TTSourcePath), typeof(string), typeof(TTImage),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTTUriChanged)));

        private static void OnTTUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TTImage obj)
            {
                obj.MyData.Source = new BitmapImage(new Uri((string)e.NewValue));
            }
        }

        public DataImage MyData { get; set; }
        private readonly Image MyTemplateElement;


        public TTImage() : this(new DataImage()) { }
        public TTImage(DataImage data)
        {
            MyData = data;
            this.DataContext = MyData;
            if (MakeTemplate<Image>() is Image element) { MyTemplateElement = element; }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            //SetBinding(TTSourceProperty, nameof(MyData.Source));
            MyTemplateElement.SetBinding(Image.SourceProperty, nameof(MyData.Source));

        }

    }

}
