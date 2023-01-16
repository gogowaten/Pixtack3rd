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
using System.ComponentModel;
using System.Diagnostics;

namespace Pixtack3rd
{
    [DebuggerDisplay(nameof(Name))]
    public abstract class TThumb : Thumb
    {
        //依存プロパティは主にデザイナー画面で、要素を追加して表示の確認する用
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
        public TTGroup? TTParent { get; set; } = null;//親Group

        public TThumb()
        {
            SetBinding(Canvas.LeftProperty, new Binding()
            {
                Path = new PropertyPath(TTLeftProperty),
                Source = this
            });
            SetBinding(Canvas.TopProperty, new Binding()
            {
                Path = new PropertyPath(TTTopProperty),
                Source = this
            });


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
        protected void MySetXYBinging(Data data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            SetBinding(Canvas.LeftProperty, nameof(data.X));
            SetBinding(Canvas.TopProperty, nameof(data.Y));
        }

    }


    //[DebuggerDisplay("Name = {" + nameof(Name) + "}")]
    [ContentProperty(nameof(Items))]
    public class TTGroup : TThumb
    {
        public DataGroup Data { get; set; }
        private ItemsControl MyTemplateElement;
        public ObservableCollection<TThumb> Items { get; private set; } = new();

        
        public TTGroup() : this(new DataGroup())
        {

        }
        public TTGroup(DataGroup data)
        {
            Data = data;
            MyTemplateElement = MyInitializeBinding();
            MyTemplateElement.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Items)) { Source = this });
        }
        private ItemsControl MyInitializeBinding()
        {
            this.DataContext = Data;
            SetBinding(TTLeftProperty, nameof(Data.X));
            SetBinding(TTTopProperty, nameof(Data.Y));
            MySetXYBinging(this.Data);
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
            Data.Datas.Add(data);
        }
        public void RemoveItem(TThumb thumb, Data data)
        {
            Items.Remove(thumb);
            Data.Datas.Remove(data);
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

        //TTGroupのRect取得
        public static (double x, double y, double w, double h) GetRect(TTGroup? group)
        {
            if (group == null) { return (0, 0, 0, 0); }
            return GetRect(group.Items);
        }
        public static (double x, double y, double w, double h) GetRect(IEnumerable<TThumb> thumbs)
        {
            double x = double.MaxValue, y = double.MaxValue;
            double w = 0, h = 0;
            if (thumbs != null)
            {
                foreach (var item in thumbs)
                {
                    var left = item.TTLeft; if (x > left) x = left;
                    var top = item.TTTop; if (y > top) y = top;
                    var width = left + item.ActualWidth;
                    var height = top + item.ActualHeight;
                    if (w < width) w = width;
                    if (h < height) h = height;
                }
            }
            return (x, y, w, h);
        }
        //サイズと位置の更新
        public void TTGroupUpdateLayout()
        {
            //Rect取得
            (double x, double y, double w, double h) = GetRect(this);

            //子要素位置修正
            foreach (var item in Items)
            {
                item.TTLeft -= x;
                item.TTTop -= y;
            }
            //自身がRoot以外なら自身の位置を更新
            if (this.GetType() != typeof(TTRoot))
            {
                TTLeft += x;
                TTTop += y;
            }

            //自身のサイズ更新
            w -= x; h -= y;
            if (w < 0) w = 0;
            if (h < 0) h = 0;
            if (w >= 0) Width = w;
            if (h >= 0) Height = h;

            //必要、これがないと見た目が変化しない、実行直後にSizeChangedが発生
            UpdateLayout();

            //親要素Groupがあれば遡って更新
            if (TTParent is TTGroup parent)
            {
                parent.TTGroupUpdateLayout();
            }
        }
    }


    public class TTRoot : TTGroup,INotifyPropertyChanged
    {
        #region 通知プロパティ
        
        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private TThumb? _clickedThumb;
        public TThumb? ClickedThumb { get => _clickedThumb; set => SetProperty(ref _clickedThumb, value); }

        private TThumb? _activeGroup;
        public TThumb? ActiveGroup { get => _activeGroup; set => SetProperty(ref _activeGroup, value); }
        #endregion 通知プロパティ

        //選択状態の要素を保持
        public ObservableCollection<TThumb> SelectedItems { get; private set; } = new();

        //クリック前の選択状態、クリックUp時の削除に使う
        private bool IsSelectedPreviewMouseDown { get; set; }

        public TTRoot()
        {
            _activeGroup ??= this;
        }

        #region ドラッグ移動
        private void Thumb_DragDelta(object seneer, DragDeltaEventArgs e)
        {
            //複数選択時は全てを移動
            foreach (TThumb item in SelectedItems)
            {
                item.TTLeft += e.HorizontalChange;
                item.TTTop += e.VerticalChange;
            }
        }
        private void Thumb_DragCompleted(object sender,DragCompletedEventArgs e)
        {
            if(sender is TThumb thumb) { thumb.TTParent?.TTGroupUpdateLayout(); }
        }
        #endregion ドラッグ移動


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

        public DataTextBlock Data { get; set; }
        private readonly TextBlock MyTemplateElement;

        public TTTextBlock() : this(new DataTextBlock()) { }
        public TTTextBlock(DataTextBlock data)
        {
            Data = data;
            this.DataContext = Data;
            if (MakeTemplate<TextBlock>() is TextBlock element)
            {
                MyTemplateElement = element;
            }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            SetBinding(TTTextProperty, nameof(Data.Text));
            MyTemplateElement.SetBinding(TextBlock.TextProperty, nameof(Data.Text));
            MySetXYBinging(this.Data);
        }
    }


    public class TTImage : TThumb
    {

        //public BitmapSource TTSource
        //{
        //    get { return (BitmapSource)GetValue(TTSourceProperty); }
        //    set { SetValue(TTSourceProperty, value); }
        //}
        //public static readonly DependencyProperty TTSourceProperty =
        //    DependencyProperty.Register(nameof(TTSource), typeof(BitmapSource), typeof(TTImage),
        //        new FrameworkPropertyMetadata(null,
        //            FrameworkPropertyMetadataOptions.AffectsRender |
        //            FrameworkPropertyMetadataOptions.AffectsMeasure));

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
                obj.Data.Source = new BitmapImage(new Uri((string)e.NewValue));
            }
        }

        public DataImage Data { get; set; }
        private readonly Image MyTemplateElement;


        public TTImage() : this(new DataImage()) { }
        public TTImage(DataImage data)
        {
            Data = data;
            this.DataContext = Data;
            if (MakeTemplate<Image>() is Image element) { MyTemplateElement = element; }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            //SetBinding(TTSourceProperty, nameof(Data.Source));
            MyTemplateElement.SetBinding(Image.SourceProperty, nameof(Data.Source));
            MySetXYBinging(this.Data);
        }

    }

}
