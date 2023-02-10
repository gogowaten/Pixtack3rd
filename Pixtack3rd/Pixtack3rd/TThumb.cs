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
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Shapes;
using System.IO;
using System.Dynamic;

namespace Pixtack3rd
{
    //public enum WakuType { None = 0, Selected, Group, ActiveGroup, Clicked, ActiveThumb }
    public enum WakuVisibleType { None = 0, All, OnlyActiveGroup, NotGroup }

    [DebuggerDisplay("Type = {" + nameof(Type) + "}")]
    public abstract class TThumb : Thumb, INotifyPropertyChanged
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
        #region 通知プロパティ

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        //private WakuType _wakuType;
        //public WakuType WakuType { get => _wakuType; set => SetProperty(ref _wakuType, value); }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        private bool _isClickedThumb;
        public bool IsClickedThumb { get => _isClickedThumb; set => SetProperty(ref _isClickedThumb, value); }

        private bool _isActiveThum;
        public bool IsActiveThumb { get => _isActiveThum; set => SetProperty(ref _isActiveThum, value); }
        private WakuVisibleType _wakuVisibleType;
        public WakuVisibleType WakuVisibleType { get => _wakuVisibleType; set => SetProperty(ref _wakuVisibleType, value); }

        #endregion 通知プロパティ

        protected readonly string TEMPLATE_NAME = "NEMO";

        public TTGroup? TTParent { get; set; } = null;//親Group
        public TType Type { get; private set; }
        public Data Data { get; set; }// = new(TType.None);
        protected List<Brush> WakuBrush { get; set; }



        public TThumb() : this(new Data(TType.None)) { }
        public TThumb(Data data)
        {
            WakuBrush = new();
            MakeWakuBrushList();

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
            Type = data.Type;
            this.Data = data;
            DataContext = data;
            SetBinding(TTLeftProperty, nameof(data.X));
            SetBinding(TTTopProperty, nameof(data.Y));

            //フォーカスできるようにする→ActiveThumbだけにしたのでRootで制御
            //this.Focusable = true;
            //フォーカス時の点線を表示しない
            this.FocusVisualStyle = null;

            //カーソルキーで移動させているときに他のThumbに
            //フォーカスを移動させないようにする、とくにSetDirectionalNavigationが重要、カーソルキーで移動しなくなる
            //      WPFでのメニューとキーボード操作時のフォーカス移動の話 - プログラム系統備忘録ブログ
            //        https://tan.hatenadiary.jp/entry/20151115/1447574474
            KeyboardNavigation.SetControlTabNavigation(this, KeyboardNavigationMode.None);
            KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.None);
            KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.None);

        }


        //ショートカットキー
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Source is TTRoot root)
            {
                switch (Keyboard.Modifiers)
                {
                    case ModifierKeys.None:
                        {
                            //スクロールバーを動かさないように
                            //カーソルキーとPageupdownキーを含むキー操作の後にはe.Handled = true;
                            switch (e.Key)
                            {
                                case Key.Up:
                                    root.ActiveThumbGoUpGrid(); e.Handled = true; break;
                                case Key.Down:
                                    root.ActiveThumbGoDownGrid(); e.Handled = true; break;
                                case Key.Left:
                                    root.ActiveThumbGoLeftGrid(); e.Handled = true; break;
                                case Key.Right:
                                    root.ActiveThumbGoRightGrid(); e.Handled = true; break;
                                case Key.PageUp:
                                    root.ZUp(); e.Handled = true; break;
                                case Key.PageDown:
                                    root.ZDown(); e.Handled = true; break;
                                case Key.Home:
                                    root.ChangeActiveGroupOutside(); e.Handled = true; break;
                                case Key.End:
                                    root.ChangeActiveGroupInside(); e.Handled = true; break;
                                case Key.C:
                                    root.CopyImageActiveThumb(); break;
                            }
                        }
                        break;
                    case ModifierKeys.Alt:
                        break;
                    case ModifierKeys.Control:
                        if (e.Key == Key.Up) { e.Handled = true; }
                        else if (e.Key == Key.Down) { e.Handled = true; }
                        else if (e.Key == Key.Left) { e.Handled = true; }
                        else if (e.Key == Key.Right) { e.Handled = true; }
                        else if (e.Key == Key.PageUp) { e.Handled = true; }
                        else if (e.Key == Key.PageDown) { e.Handled = true; }
                        else if (e.Key == Key.Home) { root.ChangeActiveGroupToRoot(); e.Handled = true; }//最外
                        else if (e.Key == Key.End) { root.ChangeActiveGroupInsideClickedParent(); e.Handled = true; }//最奥
                        else if (e.Key == Key.V) { root.AddImageThumbFromClipboard(); }//画像貼り付け
                        else if (e.Key == Key.D) { root.DuplicateDataSelectedThumbs(); }//複製
                        break;
                    case ModifierKeys.Shift:
                        {
                            if (e.Key == Key.Up) { root.ActiveThumbGoUp1Pix(); e.Handled = true; }
                            else if (e.Key == Key.Down) { root.ActiveThumbGoDown1Pix(); e.Handled = true; }
                            else if (e.Key == Key.Left) { root.ActiveThumbGoLeft1Pix(); e.Handled = true; }
                            else if (e.Key == Key.Right) { root.ActiveThumbGoRight1Pix(); e.Handled = true; }
                            else if (e.Key == Key.PageUp) { e.Handled = true; }
                            else if (e.Key == Key.PageDown) { e.Handled = true; }
                            else if (e.Key == Key.Home) { e.Handled = true; }
                            else if (e.Key == Key.End) { e.Handled = true; }
                        }
                        break;
                    case ModifierKeys.Windows:
                        break;
                    case (ModifierKeys.Control | ModifierKeys.Shift):
                        if (e.Key == Key.Up) { e.Handled = true; }
                        else if (e.Key == Key.Down) { e.Handled = true; }
                        else if (e.Key == Key.Left) { e.Handled = true; }
                        else if (e.Key == Key.Right) { e.Handled = true; }
                        else if (e.Key == Key.PageUp) { root.ZUpFrontMost(); e.Handled = true; }
                        else if (e.Key == Key.PageDown) { root.ZDownBackMost(); e.Handled = true; }
                        else if (e.Key == Key.Home) { e.Handled = true; }
                        else if (e.Key == Key.End) { e.Handled = true; }
                        else if (e.Key == Key.C) { root.CopyImageRoot(); }
                        break;
                    case (ModifierKeys.Control | ModifierKeys.Alt):
                        if (e.Key == Key.C) { root.CopyImageClickedThumb(); }
                        break;
                }
                //これだとTTTextBoxに文字入力できない
                //e.Handled = true;
            }

        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.None:
                    if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
                    {
                        if (TTParent is TTGroup parent)
                        {
                            parent.TTGroupUpdateLayout();
                        }
                    }
                    break;
                case ModifierKeys.Alt:
                    break;
                case ModifierKeys.Control:
                    break;
                case ModifierKeys.Shift:
                    if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
                    {
                        if (TTParent is TTGroup parent)
                        {
                            parent.TTGroupUpdateLayout();
                        }
                    }
                    break;
                case ModifierKeys.Windows:
                    break;
            }
        }


        //#endregion XYZ移動
        //サイズ変更時には親要素の位置とサイズ更新
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            TTParent?.TTGroupUpdateLayout();
        }

        protected T? MakeTemplate<T>()
        {
            FrameworkElementFactory fGrid = new(typeof(Grid));
            FrameworkElementFactory fContent = new(typeof(T), TEMPLATE_NAME);
            FrameworkElementFactory waku = new(typeof(Rectangle));
            Binding b1 = new(nameof(IsSelected)) { Source = this };
            Binding b2 = new(nameof(IsClickedThumb)) { Source = this };
            Binding b3 = new(nameof(IsActiveThumb)) { Source = this };
            Binding bType = new(nameof(WakuVisibleType)) { Source = this, Mode = BindingMode.TwoWay };
            MultiBinding mb = new();
            mb.Bindings.Add(b1);
            mb.Bindings.Add(b2);
            mb.Bindings.Add(b3);
            mb.Bindings.Add(bType);
            mb.Converter = new ConverterWakuBrush();
            mb.ConverterParameter = WakuBrush;
            waku.SetBinding(Rectangle.StrokeProperty, mb);
            waku.SetValue(Panel.ZIndexProperty, 1);
            fGrid.AppendChild(fContent);
            fGrid.AppendChild(waku);

            this.Template = new() { VisualTree = fGrid };
            this.ApplyTemplate();
            if (this.Template.FindName(TEMPLATE_NAME, this) is T element)
            {
                return element;
            }
            else return default;
        }
        private void MakeWakuBrushList()
        {
            WakuBrush.Add(MakeBrush2ColorsDash(4, Colors.DeepSkyBlue, Colors.White));//Selected
            WakuBrush.Add(MakeBrush2ColorsDash(4,
                Color.FromArgb(64, 128, 128, 128),
                Color.FromArgb(64, 255, 255, 255)));//Group
            //WakuBrush.Add(MakeBrush2ColorsDash(4, Colors.Gray, Colors.White));//Group
            WakuBrush.Add(MakeBrush2ColorsDash(4, Colors.MediumOrchid, Colors.White));//ActiveGroup
            WakuBrush.Add(MakeBrush2ColorsDash(4, Colors.Lime, Colors.White));//Clicked
            WakuBrush.Add(MakeBrush2ColorsDash(4, Colors.Tomato, Colors.White));//ActiveThumb
        }
        #region 枠線ブラシ作成
        //        WPF、Rectangleとかに2色の破線(点線)枠表示 - 午後わてんのブログ
        //https://gogowaten.hatenablog.com/entry/2022/05/29/140321

        /// <summary>
        /// 指定した2色で破線ブラシ作成
        /// </summary>
        /// <param name="thickness">線の幅</param>
        /// <param name="c1">色1</param>
        /// <param name="c2">色2</param>
        /// <returns></returns>
        public static ImageBrush MakeBrush2ColorsDash(int thickness, Color c1, Color c2)
        {
            WriteableBitmap bitmap = MakeCheckPattern(thickness, c1, c2);
            ImageBrush brush = new(bitmap)
            {
                Stretch = Stretch.None,
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                ViewportUnits = BrushMappingMode.Absolute
            };
            return brush;
        }
        /// <summary>
        /// 指定した2色から市松模様のbitmapを作成
        /// </summary>
        /// <param name="cellSize">1以上を指定、1指定なら2x2ピクセル、2なら4x4ピクセルの画像作成</param>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        private static WriteableBitmap MakeCheckPattern(int cellSize, Color c1, Color c2)
        {
            int width = cellSize * 2;
            int height = cellSize * 2;
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            int stride = wb.Format.BitsPerPixel / 8 * width;
            byte[] pixels = new byte[stride * height];
            Color iro;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((y < cellSize & x < cellSize) | (y >= cellSize & x >= cellSize))
                    {
                        iro = c1;
                    }
                    else { iro = c2; }

                    int p = y * stride + x * 4;
                    pixels[p] = iro.B;
                    pixels[p + 1] = iro.G;
                    pixels[p + 2] = iro.R;
                    pixels[p + 3] = iro.A;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }
        #endregion 枠線ブラシ作成

    }




    //[DebuggerDisplay("Name = {" + nameof(Name) + "}")]
    [ContentProperty(nameof(Thumbs))]
    public class TTGroup : TThumb
    {
        #region 依存プロパティ

        public int TTXShift
        {
            get { return (int)GetValue(TTXShiftProperty); }
            set { SetValue(TTXShiftProperty, value); }
        }
        public static readonly DependencyProperty TTXShiftProperty =
            DependencyProperty.Register(nameof(TTXShift), typeof(int), typeof(TTGroup),
                new FrameworkPropertyMetadata(32,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int TTYShift
        {
            get { return (int)GetValue(TTYShiftProperty); }
            set { SetValue(TTYShiftProperty, value); }
        }
        public static readonly DependencyProperty TTYShiftProperty =
            DependencyProperty.Register(nameof(TTYShift), typeof(int), typeof(TTGroup),
                new FrameworkPropertyMetadata(32,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int TTGrid
        {
            get { return (int)GetValue(TTGridProperty); }
            set { SetValue(TTGridProperty, value); }
        }
        public static readonly DependencyProperty TTGridProperty =
            DependencyProperty.Register(nameof(TTGrid), typeof(int), typeof(TTGroup),
                new FrameworkPropertyMetadata(8,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public Visibility TTVisibleBorder
        {
            get { return (Visibility)GetValue(VisibleBorderProperty); }
            set { SetValue(VisibleBorderProperty, value); }
        }
        public static readonly DependencyProperty VisibleBorderProperty =
            DependencyProperty.Register(nameof(TTVisibleBorder), typeof(Visibility), typeof(TTGroup),
                new FrameworkPropertyMetadata(Visibility.Hidden,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存プロパティ


        private bool _isActiveGroup;
        public bool IsActiveGroup { get => _isActiveGroup; set => SetProperty(ref _isActiveGroup, value); }

        private bool _isGroup;
        public bool IsGroup { get => _isGroup; set => SetProperty(ref _isGroup, value); }


        private ItemsControl MyTemplateElement;
        public ObservableCollection<TThumb> Thumbs { get; private set; } = new();



        public TTGroup() : this(new Data(TType.Group))
        {

        }
        public TTGroup(Data data) : base(data)
        {
            //初期設定
            Thumbs.CollectionChanged += Thumbs_CollectionChanged;
            Data = data;
            MyTemplateElement = MyInitializeBinding();
            MyTemplateElement.SetBinding(ItemsControl.ItemsSourceProperty,
                new Binding(nameof(Thumbs)) { Source = this });

            SetBinding(TTXShiftProperty, nameof(Data.XShift));
            SetBinding(TTYShiftProperty, nameof(Data.YShift));
            SetBinding(TTGridProperty, nameof(Data.Grid));
            //SetBinding(IsVisibleProperty, nameof(data.IsVisiblleThumb));
            Binding myB = new(nameof(data.IsNotVisiblle));
            myB.Converter = new ConverterVisible();
            myB.Mode = BindingMode.TwoWay;
            SetBinding(VisibilityProperty, myB);

            IsGroup = true;
        }

        private void Thumbs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems?[0] is TThumb thumb)
                    {
                        thumb.TTParent = this;
                    }
                    break;
                default: break;
            }
        }

        private ItemsControl MyInitializeBinding()
        {
            this.DataContext = Data;
            return MakeTemplate();

        }
        private ItemsControl MakeTemplate()
        {
            FrameworkElementFactory fGrid = new(typeof(Grid));
            FrameworkElementFactory fWaku = new(typeof(Rectangle));
            Binding b1 = new Binding(nameof(IsActiveThumb)) { Source = this };
            Binding b2 = new Binding(nameof(IsSelected)) { Source = this };
            Binding b3 = new Binding(nameof(IsActiveGroup)) { Source = this };
            Binding b4 = new Binding(nameof(IsGroup)) { Source = this };
            Binding bType = new Binding(nameof(WakuVisibleType)) { Source = this };
            MultiBinding mb = new();
            mb.Bindings.Add(b1);
            mb.Bindings.Add(b2);
            mb.Bindings.Add(b3);
            mb.Bindings.Add(b4);
            mb.Bindings.Add(bType);
            mb.ConverterParameter = WakuBrush;
            mb.Converter = new ConverterWakuBrushForTTGroup();
            fWaku.SetBinding(Rectangle.StrokeProperty, mb);

            //fWaku.SetValue(Panel.ZIndexProperty, 1);



            //fWaku.SetValue(VisibilityProperty, new Binding(nameof(TTVisibleBorder)) { Source = this });


            FrameworkElementFactory factory = new(typeof(ItemsControl), TEMPLATE_NAME);
            factory.SetValue(ItemsControl.ItemsPanelProperty,
                new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas))));
            fGrid.AppendChild(fWaku);
            fGrid.AppendChild(factory);
            this.Template = new() { VisualTree = fGrid };
            this.ApplyTemplate();
            if (this.Template.FindName(TEMPLATE_NAME, this) is ItemsControl element)
            {
                return element;
            }
            else { throw new ArgumentException("テンプレート作成できんかった"); }
        }

        #region サイズと位置の更新

        //TTGroupのRect取得
        public static (double x, double y, double w, double h) GetRect(TTGroup? group)
        {
            if (group == null) { return (0, 0, 0, 0); }
            return GetRect(group.Thumbs);
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

        /// <summary>
        /// サイズと位置の更新
        /// </summary>
        public void TTGroupUpdateLayout()
        {
            //Rect取得
            (double x, double y, double w, double h) = GetRect(this);

            //子要素位置修正
            foreach (var item in Thumbs)
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
        #endregion サイズと位置の更新

    }


    public class TTRoot : TTGroup
    {

        #region 通知プロパティ
        //最後にクリックしたThumb
        private TThumb? _clickedThumb;
        public TThumb? ClickedThumb
        {
            get => _clickedThumb;
            set
            {
                if (_clickedThumb != null) { _clickedThumb.IsClickedThumb = false; }
                SetProperty(ref _clickedThumb, value);
                if (_clickedThumb != null) { _clickedThumb.IsClickedThumb = true; }
            }
        }

        //注目しているThumb、選択Thumb群の筆頭
        private TThumb? _activeThumb;
        public TThumb? ActiveThumb
        {
            get => _activeThumb;
            set
            {
                if (_activeThumb != null)
                {
                    _activeThumb.IsActiveThumb = false;
                    _activeThumb.Focusable = false;
                }
                SetProperty(ref _activeThumb, value);
                if (_activeThumb != null)
                {
                    _activeThumb.IsActiveThumb = true;
                    _activeThumb.Focusable = true;
                    _activeThumb.Focus();
                }
                //FrontActiveThumbとBackActiveThumbを更新する
                ChangedActiveFrontAndBackThumb(value);
            }
        }

        private TThumb? _frontActiveThumb;
        public TThumb? FrontActiveThumb { get => _frontActiveThumb; set => SetProperty(ref _frontActiveThumb, value); }

        private TThumb? _backActiveThumb;
        public TThumb? BackActiveThumb { get => _backActiveThumb; set => SetProperty(ref _backActiveThumb, value); }

        //ActiveThumbの親要素、移動可能なThumbはこの要素の中のThumbだけ。起動直後はRootThumbがこれになる
        private TTGroup _activeGroup;
        public TTGroup ActiveGroup
        {
            get => _activeGroup;
            set
            {
                ChildrenDragEventDesoption(_activeGroup, value);
                _activeGroup.IsActiveGroup = false;
                SetProperty(ref _activeGroup, value);
                _activeGroup.IsActiveGroup = true;
            }
        }




        #endregion 通知プロパティ

        #region 枠表示プロパティ

        //枠の表示設定、かなり煩雑
        //コールバックですべての要素のプロパティを変更＋自身のプロパティも変更する
        public WakuVisibleType TTWakuVisibleType
        {
            get { return (WakuVisibleType)GetValue(TTWakuVisibleTypeProperty); }
            set { SetValue(TTWakuVisibleTypeProperty, value); }
        }
        public static readonly DependencyProperty TTWakuVisibleTypeProperty =
            DependencyProperty.Register(nameof(TTWakuVisibleType), typeof(WakuVisibleType), typeof(TTRoot),
                new FrameworkPropertyMetadata(WakuVisibleType.None,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnTTWakuVisibleTypeChanged)));
        private static void OnTTWakuVisibleTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TTRoot root)
            {
                root.WakuVisibleType = root.TTWakuVisibleType;//自身のプロパティ
                //すべての要素のプロパティを変更
                foreach (var item in root.Thumbs)
                {
                    item.WakuVisibleType = root.TTWakuVisibleType;
                    if (item is TTGroup group)
                    {
                        SetWaku(group.Thumbs, root.TTWakuVisibleType);
                    }
                }
            }
        }
        private static void SetWaku(ObservableCollection<TThumb> thumbs, WakuVisibleType wakuType)
        {
            foreach (var item in thumbs)
            {
                item.WakuVisibleType = wakuType;
                if (item is TTGroup group)
                {
                    SetWaku(group.Thumbs, wakuType);
                }
            }
        }
        #endregion 枠表示プロパティ

        //選択状態の要素を保持
        public ExObservableCollection SelectedThumbs { get; private set; } = new();
        //直属のグループ一覧、レイヤー
        public ObservableCollection<TTGroup> GroupsDirectlyBelow { get; private set; } = new();

        //クリック前の選択状態、クリックUp時の選択Thumb削除に使う
        private bool IsSelectedPreviewMouseDown { get; set; }

        #region コンストラクタ、初期化

        public TTRoot() : base(new Data(TType.Root))
        {

            _activeGroup ??= this;
            IsActiveGroup = true;

            //SetBinding(TTWakuVisibleTypeProperty, new Binding(nameof(WakuVisibleType)) { Source = this });
            //起動直後に位置とサイズ更新
            //TTGroupUpdateLayout();//XAML上でThumb設置しても、この時点ではThumbsが0個
            Loaded += (a, b) =>
            {
                TTGroupUpdateLayout();
                FixDataDatas();
            };

            //Rootにはフォーカスを移動させない
            this.Focusable = false;

        }


        /// <summary>
        /// DataとThumbの整合性修正、XAMLに配置した場合はThumbがあるのにDataがない状態なので追加する
        /// ついでに枠表示設定も合わせる
        /// </summary>
        private void FixDataDatas()
        {
            if (Data.Datas == null) return;

            foreach (var item in Thumbs)
            {
                if (Data.Datas.Contains(item.Data) == false)
                {
                    item.WakuVisibleType = WakuVisibleType;
                    Data.Datas.Add(item.Data);
                }
            }
        }

        /// <summary>
        /// RootDataをセット、初期化
        /// </summary>
        /// <param name="data"></param>
        public void SetRootData(Data data)
        {
            //初期化
            if (data.Type != TType.Root) return;
            Thumbs.Clear();
            SelectedThumbs.Clear();
            GroupsDirectlyBelow.Clear();
            this.Data = data;
            DataContext = this.Data;
            ActiveGroup = this;
            ClickedThumb = null;
            ActiveThumb = null;

            //子要素追加
            foreach (var item in data.Datas)
            {
                TThumb thumb = BuildThumb(item);
                Thumbs.Add(thumb);
                //直下のThumbにはドラッグ移動イベント付加
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += Thumb_DragCompleted;
                thumb.DragStarted += Thumb_DragStarted;
                //追加子要素がGroupだった場合
                if (thumb is TTGroup group)
                {
                    SetData(group);
                    //直属のグループ
                    GroupsDirectlyBelow.Add(group);
                }
            }
        }


        //GroupのThumbsに子要素追加
        private void SetData(TTGroup group)
        {
            foreach (Data data in group.Data.Datas)
            {
                TThumb thumb = BuildThumb(data);
                group.Thumbs.Add(thumb);
                //newGroup.Data.Datas.Add(thumb.Data);
                if (thumb is TTGroup gThumb)
                {
                    SetData(gThumb);
                }
            }
        }

        #endregion コンストラクタ、初期化


        #region ドラッグ移動

        //ActiveGroup用、ドラッグ移動イベント脱着
        private void ChildrenDragEventDesoption(TTGroup removeTarget, TTGroup addTarget)
        {
            foreach (var item in removeTarget.Thumbs)
            {
                item.DragDelta -= Thumb_DragDelta;
                item.DragCompleted -= Thumb_DragCompleted;
                item.DragStarted -= Thumb_DragStarted;
            }
            foreach (var item in addTarget.Thumbs)
            {
                item.DragDelta += Thumb_DragDelta;
                item.DragCompleted += Thumb_DragCompleted;
                item.DragStarted += Thumb_DragStarted;
            }
        }
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            //グリッドスナップ
            if (sender is TThumb thumb)
            {
                int gridSize = ActiveGroup.Data.Grid;
                thumb.TTLeft = (int)(thumb.TTLeft / gridSize + 0.5) * gridSize;
                thumb.TTTop = (int)(thumb.TTTop / gridSize + 0.5) * gridSize;
            }
        }

        private void Thumb_DragDelta(object seneer, DragDeltaEventArgs e)
        {
            //複数選択時は全てを移動
            foreach (TThumb item in SelectedThumbs)
            {
                double hc = e.HorizontalChange;
                int gridSize = ActiveGroup.Data.Grid;
                if (hc > 0)
                {
                    item.TTLeft += (int)(hc / gridSize + 0.5) * gridSize;
                }
                else if (hc < 0)
                {
                    item.TTLeft += (int)(hc / gridSize - 0.5) * gridSize;
                }

                double vc = e.VerticalChange;
                if (vc > 0)
                {
                    item.TTTop += (int)(vc / gridSize + 0.5) * gridSize;
                }
                else if (vc < 0)
                {
                    item.TTTop += (int)(vc / gridSize - 0.5) * gridSize;
                }
            }
        }
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is TThumb thumb) { thumb.TTParent?.TTGroupUpdateLayout(); }
        }
        #endregion ドラッグ移動

        #region オーバーライド関連

        //起動直後、自身がActiveGroupならChildrenにドラッグ移動登録
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (ActiveGroup == this)
            {
                foreach (var item in Thumbs)
                {
                    item.DragDelta += Thumb_DragDelta;
                    item.DragCompleted += Thumb_DragCompleted;
                    item.DragStarted += Thumb_DragStarted;
                }
            }
            //TTGroupUpdateLayout();
        }

        /// <summary>
        /// クリックしたときに使う、クリックイベントからクリックされたThumbを探す
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private TThumb? GetClickedThumbFromMouseEvent(object obj)
        {
            if (obj is FrameworkElement element)
            {
                if (element.TemplatedParent is TThumb tt) { return tt; }
                else if (element.TemplatedParent is DependencyObject dObj)
                {
                    return GetClickedThumbFromMouseEvent(dObj);
                }
            }
            return null;
        }
        //クリックしたとき、ClickedThumbの更新とActiveThumbの更新、SelectedThumbsの更新
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);//要る？

            //OriginalSourceにテンプレートに使っている要素が入っているので、
            //そのTemplateParentプロパティから目的のThumbが取得できる
            var clicked = GetClickedThumbFromMouseEvent(e.OriginalSource);
            ClickedThumb = clicked;
            TThumb? active = GetActiveThumb(clicked);
            if (active != ActiveThumb)
            {
                if (active == null && Thumbs.Count != 0) { return; }
                ActiveThumb = active;
            }
            //SelectedThumbsの更新
            if (active != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (SelectedThumbs.Contains(active) == false)
                    {
                        SelectedThumbs.Add(active);
                        IsSelectedPreviewMouseDown = false;
                    }
                    else
                    {
                        //フラグ
                        //ctrl+クリックされたものがもともと選択状態だったら
                        //マウスアップ時に削除するためのフラグ
                        IsSelectedPreviewMouseDown = true;
                    }
                }
                else
                {
                    if (SelectedThumbs.Contains(active) == false)
                    {
                        SelectedThumbs.Clear();
                        SelectedThumbs.Add(active);
                        IsSelectedPreviewMouseDown = false;
                    }
                }
            }
            else { IsSelectedPreviewMouseDown = false; }


        }

        //マウスレフトアップ、フラグがあればSelectedThumbsから削除する
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //
            if (SelectedThumbs.Count > 1 && IsSelectedPreviewMouseDown && ActiveThumb != null)
            {
                SelectedThumbs.Remove(ActiveThumb);//削除
                IsSelectedPreviewMouseDown = false;//フラグ切り替え
                ClickedThumb = SelectedThumbs[^1];
                ActiveThumb = SelectedThumbs[^1];
            }

        }

        #endregion オーバーライド関連

        #region その他関数


        /// <summary>
        /// 画像ファイルからBitmapImageを作成、ファイルロックなし
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static BitmapImage GetBitmap(string filePath)
        {
            BitmapImage bmp = new();
            FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.EndInit();
            return bmp;
        }
        public void ChangeActiveThumbToFrontThumb()
        {
            if (ActiveThumb == null) return;
            if (Thumbs[^1] == ActiveThumb) { return; }
            if (Thumbs.Count == 1) { return; }
            int ii = Thumbs.IndexOf(ActiveThumb);
            ActiveThumb = Thumbs[ii + 1];
            SelectedThumbs.Clear();
            SelectedThumbs.Add(ActiveThumb);
        }
        public void ChangeActiveThumbToBackThumb()
        {
            if (ActiveThumb == null) return;
            if (Thumbs[0] == ActiveThumb) { return; }
            if (Thumbs.Count == 1) { return; }
            int ii = Thumbs.IndexOf(ActiveThumb);
            ActiveThumb = Thumbs[ii - 1];
            SelectedThumbs.Clear();
            SelectedThumbs.Add(ActiveThumb);
        }


        /// <summary>
        /// ActiveThumb変更時に実行、FrontActiveThumbとBackActiveThumbを更新する
        /// </summary>
        /// <param name="value"></param>
        private void ChangedActiveFrontAndBackThumb(TThumb? value)
        {
            if (value == null)
            {
                FrontActiveThumb = null;
                BackActiveThumb = null;
            }
            else
            {
                int ii = ActiveGroup.Thumbs.IndexOf(value);
                int tc = ActiveGroup.Thumbs.Count;

                if (tc <= 1)
                {
                    FrontActiveThumb = null;
                    BackActiveThumb = null;
                }
                else
                {
                    if (ii - 1 >= 0)
                    {
                        BackActiveThumb = ActiveGroup.Thumbs[ii - 1];
                    }
                    else
                    {
                        BackActiveThumb = null;
                    }
                    if (ii + 1 >= tc)
                    {
                        FrontActiveThumb = null;
                    }
                    else
                    {
                        FrontActiveThumb = ActiveGroup.Thumbs[ii + 1];
                    }
                }
            }
        }

        private bool CheckIsActive(TThumb thumb)
        {
            if (thumb.TTParent is TTGroup ttg && ttg == ActiveGroup)
            {
                return true;
            }
            return false;

        }
        //起点からActiveThumbをサーチ
        //ActiveはActiveThumbのChildrenの中で起点に連なるもの
        private TThumb? GetActiveThumb(TThumb? start)
        {
            if (start == null) { return null; }
            if (CheckIsActive(start))
            {
                return start;
            }
            else if (start.TTParent is TTGroup ttg)
            {
                return GetActiveThumb(ttg);
            }
            return null;
        }
        /// <summary>
        /// SelectedThumbsを並べ替えたList作成、基準はActiveGroupのChildren
        /// </summary>
        /// <param name="selected">SelectedThumbs</param>
        /// <param name="group">並べ替えの基準にするGroup</param>
        /// <returns></returns>
        private List<TThumb> MakeSortedList(IEnumerable<TThumb> selected, TTGroup group)
        {
            List<TThumb> tempList = new();
            foreach (var item in group.Thumbs)
            {
                if (selected.Contains(item)) { tempList.Add(item); }
            }
            return tempList;
        }
        /// <summary>
        /// 要素すべてがGroupのChildrenに存在するか判定
        /// </summary>
        /// <param name="thums">要素群</param>
        /// <param name="group">ParentGroup</param>
        /// <returns></returns>
        private bool IsAllContains(IEnumerable<TThumb> thums, TTGroup group)
        {
            if (!thums.Any()) { return false; }//要素が一つもなければfalse
            foreach (var item in thums)
            {
                if (group.Thumbs.Contains(item) == false)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion その他関数

        #region 追加

        /// <summary>
        /// 追加先Groupを指定して追加、挿入Indexは最後尾(最前面)
        /// </summary>
        /// <param name="thumb">追加する子要素</param>
        /// <param name="destGroup">追加先Group</param>
        protected void AddThumb(TThumb thumb, TTGroup destGroup)
        {
            AddThumb(thumb, destGroup, destGroup.Thumbs.Count);
            //AddThumbToActiveGroup(thumb, destGroup, destGroup.Thumbs.Count - 1);
        }
        /// <summary>
        /// グループ化で使用、追加先Groupと挿入Indexを指定して追加
        /// </summary>
        /// <param name="thumb">追加する子要素</param>
        /// <param name="destGroup">追加先Group</param>
        /// <param name="insert">挿入Index</param>
        protected void AddThumb(TThumb thumb, TTGroup destGroup, int insert)
        {
            destGroup.Thumbs.Insert(insert, thumb);
            destGroup.Data.Datas.Insert(insert, thumb.Data);
            //ドラッグ移動イベント付加
            thumb.DragDelta += Thumb_DragDelta;
            thumb.DragCompleted += Thumb_DragCompleted;
            thumb.DragStarted += Thumb_DragStarted;

            //TTGroupの場合はGroupsDirectlyBelowに追加する
            if (destGroup.Type == TType.Root && thumb is TTGroup group)
            {
                GroupsDirectlyBelow.Add(group);
            }
        }
        protected void AddThumbWithoutDragEvent(TThumb thumb, TTGroup destGroup)
        {
            destGroup.Thumbs.Add(thumb);
            destGroup.Data.Datas.Add(thumb.Data);
        }

        //基本的にActiveThumbのChildrenに対して行う
        //削除対象はActiveThumbになる
        //ドラッグ移動イベントの着脱も行う
        public void AddThumbToActiveGroup(TThumb thumb)
        {
            AddThumb(thumb, ActiveGroup);
        }
        /// <summary>
        /// ActiveThumbに要素をDataで追加
        /// </summary>
        /// <param name="data"></param>
        /// <param name="addUpper">trueで上層に追加、falseで下層に追加</param>
        /// <param name="locateFix">追加位置修正、通常はtrue。複製時にはfalse</param>
        public TThumb? AddThumbDataToActiveGroup(Data data, bool addUpper, bool locateFix = true)
        {

            if (BuildThumb(data) is TThumb thumb)
            {
                if (addUpper)
                {
                    AddThumb(thumb, ActiveGroup);//直下にはドラッグ移動イベント付加
                }
                else
                {

                    AddThumb(thumb, ActiveGroup, 0);
                }
                //位置修正、追加先のActiveThumbに合わせる
                if (locateFix)
                {
                    if (ActiveThumb != null)
                    {
                        data.X = ActiveThumb.Data.X + ActiveGroup.TTXShift;
                        data.Y = ActiveThumb.Data.Y + ActiveGroup.TTYShift;
                    }
                    else
                    {
                        data.X = 0;
                        data.Y = 0;
                    }
                }

                //Groupだった場合は子要素も追加、子要素にドラッグ移動イベント追加しない
                if (thumb is TTGroup group)
                {
                    SetData(group);
                }
                ActiveThumb = thumb;
                SelectedThumbs.Clear();
                SelectedThumbs.Add(thumb);
                ClickedThumb = thumb;
                return thumb;
            }
            return null;
        }

        /// <summary>
        /// Dataから各種Thumbを構築
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public TThumb BuildThumb(Data data)
        {
            //data.WakuVisibleType = TTWakuVisibleType;
            TThumb? result;
            switch (data.Type)
            {
                case TType.None:
                    throw new NotImplementedException();
                case TType.Root:
                    throw new NotImplementedException();
                case TType.Group:
                    result = new TTGroup(data);
                    //return new TTGroup(data);
                    break;
                case TType.TextBlock:
                    result = new TTTextBlock(data);
                    break;
                //return new TTTextBlock(data);
                case TType.Image:
                    result = new TTImage(data);
                    break;
                //return new TTImage(data);
                case TType.Rectangle:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            if (result != null)
            {
                result.WakuVisibleType = this.TTWakuVisibleType;
                return result;
            }
            else { throw new NotImplementedException(); }
        }
        #endregion 追加
        #region 削除

        /// <summary>
        /// 選択されているThumbすべてを削除
        /// </summary>
        /// <returns></returns>
        public bool RemoveThumb()
        {
            if (SelectedThumbs == null) return false;
            if (SelectedThumbs.Count == ActiveGroup.Thumbs.Count)
            {
                if (ActiveGroup.TTParent != null)
                {
                    RemoveThumb(ActiveGroup, ActiveGroup.TTParent);
                    SelectedThumbs.Clear();
                    ClickedThumb = null;
                    ChangeActiveGroupOutside();
                    return true;
                }
                else if (ActiveGroup.Type == TType.Root)
                {
                    RemoveAll();
                    return true;
                }
                else { return false; }
            }

            bool flag = true;
            int indexNextActThumb = 0;
            foreach (var item in SelectedThumbs.ToArray())
            {
                indexNextActThumb = ActiveGroup.Thumbs.IndexOf(item);

                if (RemoveThumb(item, ActiveGroup))
                {
                    SelectedThumbs.Remove(item);
                }
                else
                {
                    flag = false;
                    break;
                }
            }

            //削除後の処理、ActiveThumbの指定とSelectedThumbsの指定
            //削除失敗時
            if (flag == false)
            {
                ActiveThumb = null;
            }
            //削除成功時
            else
            {
                //次のActiveThumbは削除されたThumbの下にあるThumb、ただし
                //それがない場合は上のThumb、これもない場合はnull
                if (indexNextActThumb > 0)
                {
                    indexNextActThumb--;
                }

                if (ActiveGroup.Thumbs.Count == 0)
                {
                    ActiveThumb = null;
                }
                else
                {
                    ActiveThumb = ActiveGroup.Thumbs[indexNextActThumb];
                    SelectedThumbs.Add(ActiveThumb);
                }
            }

            ClickedThumb = null;
            //Thumbsが1個になっていたらグループ解除
            if (ActiveGroup.Thumbs.Count == 1 && ActiveGroup.TTParent is TTGroup parent)
            {
                //out
                TTGroup temp = ActiveGroup;
                ChangeActiveGroupOutside();
                UnGroup(temp, ActiveGroup);
            }
            return flag;
        }
        /// <summary>
        /// 指定Thumbだけを指定Groupから削除
        /// </summary>
        /// <param name="thumb"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool RemoveThumb(TThumb thumb, TTGroup group)
        {
            if (group.Thumbs.Remove(thumb))
            {
                group.Data.Datas.Remove(thumb.Data);
                thumb.DragCompleted -= Thumb_DragCompleted;
                thumb.DragDelta -= Thumb_DragDelta;
                thumb.DragStarted -= Thumb_DragStarted;
                group.TTGroupUpdateLayout();
                //直属のグループならGroupsDirectlyBelowからも削除
                if (group.Type == TType.Root && thumb is TTGroup isTTGroup)
                {
                    GroupsDirectlyBelow.Remove(isTTGroup);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveAll()
        {
            Thumbs.Clear();
            GroupsDirectlyBelow.Clear();
            SelectedThumbs.Clear();
            Data.Datas.Clear();
            ActiveThumb = null;
            ClickedThumb = null;
            ActiveGroup = this;
            TTGroupUpdateLayout();
        }
        #endregion 削除


        #region グループ化

        //基本的にSelectedThumbsの要素群でグループ化、それをActiveGroupに追加する
        public void AddGroup()
        {
            if (SelectedThumbs.Count < 2) { return; }
            TTGroup? group = MakeAndAddGroup(SelectedThumbs, ActiveGroup);
            if (group != null)
            {
                SelectedThumbs.Clear();
                SelectedThumbs.Add(group);
                //ActiveThumb = group;//これとXAMLでのActiveThumbとClickedThumbの表示の組み合わせでフリーズする

                //フリーズ回避？
                //Clickedを一旦nullにしてからActiveThumbを入れ替える
                //入れ替えた後にClickedを元に戻す
                var temp = ClickedThumb;
                ClickedThumb = null;//一旦null
                ActiveThumb = group;//入れ替え
                ClickedThumb = temp;//元に戻す
            }
        }

        /// <summary>
        /// グループ化
        /// </summary>
        /// <param name="thumbs">グループ化する要素群</param>
        /// <param name="destGroup">新グループの追加先</param>
        private TTGroup? MakeAndAddGroup(ObservableCollection<TThumb> thumbs, TTGroup destGroup)
        {
            //選択要素群をActiveGroupを基準に並べ替え
            List<TThumb> sortedList = MakeSortedList(thumbs, destGroup);

            //新グループの挿入Index、[^1]は末尾から数えて1番目の要素って意味
            int insertIndex = destGroup.Thumbs.IndexOf(sortedList[^1]) - (sortedList.Count - 1);

            if (CheckAddGroup(sortedList, destGroup) == false) { return null; }
            var (x, y, w, h) = GetRect(sortedList);
            TTGroup newGroup = new()
            {
                TTLeft = x,
                TTTop = y,
                TTGrid = destGroup.TTGrid,
                TTXShift = destGroup.TTXShift,
                TTYShift = destGroup.TTYShift,
                WakuVisibleType = this.WakuVisibleType,
            };

            //各要素のドラッグイベントを外す、新グループに追加
            foreach (var item in sortedList)
            {
                if (destGroup.Thumbs.Remove(item) && destGroup.Data.Datas.Remove(item.Data))
                {
                    thumbs.Remove(item);

                    item.DragDelta -= Thumb_DragDelta;
                    item.DragCompleted -= Thumb_DragCompleted;
                    item.DragStarted -= Thumb_DragStarted;
                    //要素が直属のグループだったなら外す
                    if (destGroup.Type == TType.Root && item is TTGroup group)
                    {
                        this.GroupsDirectlyBelow.Remove(group);
                    }

                    newGroup.Thumbs.Add(item);
                    newGroup.Data.Datas.Add(item.Data);
                    item.TTLeft -= x;
                    item.TTTop -= y;
                }
            }

            AddThumb(newGroup, destGroup, insertIndex);
            newGroup.Arrange(new(0, 0, w, h));//再配置？このタイミングで必須、Actualサイズに値が入る
            //↓はこのタイミングではいらないかも？RenderSizeChangeで実行するようにした
            //→要る！！！ここじゃないと枠表示のサイズがなぜか0x0のままになる
            newGroup.TTGroupUpdateLayout();//必須、サイズと位置の更新

            return newGroup;
        }

        /// <summary>
        /// グループ化の条件が揃っているのかの判定
        /// </summary>
        /// <param name="thumbs">グループ化要素群</param>
        /// <param name="destGroup">グループ追加先グループ</param>
        /// <returns></returns>
        private static bool CheckAddGroup(IEnumerable<TThumb> thumbs, TTGroup destGroup)
        {
            //要素群数が2以上
            //要素群数が追加先グループの子要素数より少ない、ただし
            //追加先グループがTTRootなら子要素数と同じでもいい
            //要素群すべてが追加先グループの子要素
            //これらすべてを満たした場合はtrue
            if (thumbs.Count() < 2) { return false; }
            if (thumbs.Count() == destGroup.Thumbs.Count)
            {
                if (destGroup.Type == TType.Root) { return true; }
                else { return false; }
            }
            foreach (TThumb thumb in thumbs)
            {
                if (destGroup.Thumbs.Contains(thumb) == false) { return false; }
            }
            return true;
        }
        #endregion グループ化
        #region グループ解除
        /// <summary>
        /// ActiveThumbのグループ解除
        /// </summary>
        public void UnGroup()
        {
            if (ActiveThumb is TTGroup group)
            {
                UnGroup(group, ActiveGroup);
                SelectedThumbs.Clear();
                ActiveThumb = GetActiveThumb(ClickedThumb);
            }
        }


        /// <summary>
        /// 指定グループのグループ解除
        /// </summary>
        /// <param name="group">解除するグループを指定</param>
        /// <param name="destGroup">指定グループの親Group</param>
        public void UnGroup(TTGroup group, TTGroup destGroup)
        {
            //解除対象のグループの要素群に対して
            //ドラッグイベント解除＋解除対象から削除してから
            //その親Groupに追加＋ドラッグイベント追加+位置修正
            int insertIndex = destGroup.Thumbs.IndexOf(group);//挿入Index
            foreach (var item in group.Thumbs.ToArray())
            {
                group.Thumbs.Remove(item);//親Groupから削除
                group.Data.Datas.Remove(item.Data);
                item.DragDelta -= Thumb_DragDelta;
                item.DragCompleted -= Thumb_DragCompleted;
                item.DragStarted -= Thumb_DragStarted;
                //親親Groupに挿入
                AddThumb(item, destGroup, insertIndex);
                insertIndex++;
                item.TTLeft += group.TTLeft;//位置修正
                item.TTTop += group.TTTop;
            }
            //抜け殻になった元のグループ要素削除
            destGroup.Thumbs.Remove(group);
            destGroup.Data.Datas.Remove(group.Data);
            group.DragCompleted -= Thumb_DragCompleted;//いる？
            group.DragDelta -= Thumb_DragDelta;
            group.DragStarted -= Thumb_DragStarted;

            //直属のグループから外す
            if (destGroup.Type == TType.Root)
            {
                this.GroupsDirectlyBelow.Remove(group);
            }
        }
        #endregion グループ解除

        #region ActiveGroupの切り替え
        //ActiveThumbを内側(ActiveThumbの親)へ切り替える
        public void ChangeActiveGroupInside()
        {
            if (ActiveThumb is TTGroup group)
            {
                ActiveGroup = group;
                ActiveThumb = GetActiveThumb(ClickedThumb);
                SelectedThumbs.Clear();
            }
        }

        //ActiveThumbを外側(親)へ切り替える
        public void ChangeActiveGroupOutside()
        {
            if (ActiveGroup.TTParent is TTGroup parent)
            {
                ActiveGroup = parent;
                ActiveThumb = GetActiveThumb(ClickedThumb);
                SelectedThumbs.Clear();
            }
        }
        //ClickedThumbの親GroupをActive
        public void ChangeActiveGroupInsideClickedParent()
        {
            if (ClickedThumb?.TTParent is TTGroup parent)
            {
                ActiveGroup = parent;
                ActiveThumb = GetActiveThumb(ClickedThumb);
                SelectedThumbs.Clear();
            }
        }
        public void ChangeActiveGroupToRoot()
        {
            ActiveGroup = this;
            ActiveThumb = GetActiveThumb(ClickedThumb);
            SelectedThumbs.Clear();
        }
        #endregion ActiveGroupの切り替え
        #region ZIndex
        //ZIndexが同じ場合はThumbsIndexが前後関係になるのを利用して
        //Thumbs要素の入れ替えによって前面、背面移動させる
        #region 背面に移動


        /// <summary>
        /// 選択Thumbを最背面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZDownBackMost()
        {
            return ZDownBackMost(SelectedThumbs, ActiveGroup);
        }
        /// <summary>
        /// 指定Thumbを最背面へ移動
        /// </summary>
        /// <param name="thumbs">移動させるThumb群</param>
        /// <param name="group">Thumb群の親Group</param>
        /// <returns></returns>
        public bool ZDownBackMost(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            if (IsAllContains(thumbs, group) == false) { return false; }
            //下側にある要素から処理したいので、並べ替えたListを作成
            List<TThumb> tempList = MakeSortedList(thumbs, group);
            //削除してから先頭から挿入
            for (int i = 0; i < tempList.Count; i++)
            {
                if (group.Thumbs.Remove(tempList[i]))
                {
                    group.Thumbs.Insert(i, tempList[i]);
                }
                else { return false; }
            }
            //ActiveThumbのFrontThumbとBackThumbの更新
            ChangedActiveFrontAndBackThumb(ActiveThumb);
            return true;
        }
        /// <summary>
        /// 選択Thumbを背面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZDown()
        {
            return ZDown(SelectedThumbs, ActiveGroup);
        }
        /// <summary>
        /// 指定Thumbを背面へ移動
        /// </summary>
        /// <param name="thumbs"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool ZDown(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            if (IsAllContains(thumbs, group) == false) { return false; }
            //順番を揃えたリスト作成
            List<TThumb> tempList = MakeSortedList(thumbs, group);

            //一番下の要素がもともと一番下だった場合は処理しない
            if (group.Thumbs[0] == tempList[0]) { return false; }

            //順番に処理、削除してから挿入、挿入箇所は元のインデックス-1
            foreach (var item in tempList)
            {
                int ii = group.Thumbs.IndexOf(item);
                ii--;
                if (group.Thumbs.Remove(item))
                {
                    group.Thumbs.Insert(ii, item);
                }
                else
                {
                    return false;
                    throw new ArgumentException("対象要素が親要素から見つからなかった");
                }
            }
            //ActiveThumbのFrontThumbとBackThumbの更新
            ChangedActiveFrontAndBackThumb(ActiveThumb);
            return true;
        }
        #endregion 背面に移動

        #region 前面に移動

        /// <summary>
        /// 指定Thumbを最前面へ移動
        /// </summary>
        /// <param name="thumbs">移動させるThumb群</param>
        /// <param name="group">親Group</param>
        /// <returns></returns>
        public bool ZUpFrontMost(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            //要素すべてがGroupのChildrenに存在するか判定、存在しない要素があれば処理しない            
            if (IsAllContains(thumbs, group) == false) { return false; }

            //下側にある要素から処理したいので、並べ替えたListを作成
            List<TThumb> tempList = MakeSortedList(thumbs, group);
            //削除してから追加(末尾に追加)
            foreach (var item in tempList)
            {
                if (group.Thumbs.Remove(item))
                {
                    group.Thumbs.Add(item);
                }
                else { return false; }
            }
            //ActiveThumbのFrontThumbとBackThumbの更新
            ChangedActiveFrontAndBackThumb(ActiveThumb);
            return true;
        }
        /// <summary>
        /// 選択Thumbを最前面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZUpFrontMost()
        {
            return ZUpFrontMost(SelectedThumbs, ActiveGroup);
        }

        /// <summary>
        /// 指定Thumbを前面へ移動
        /// </summary>
        /// <param name="thumbs"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool ZUp(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            if (IsAllContains(thumbs, group) == false) { return false; }
            //順番を揃えてから削除して追加
            List<TThumb> tempList = MakeSortedList(thumbs, group);

            //一番上の要素がもともと一番上だった場合は処理しない
            if (group.Thumbs[^1] == tempList[^1])
            {
                return false;
            }
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                int ii = group.Thumbs.IndexOf(tempList[i]);
                ii++;
                if (group.Thumbs.Remove(tempList[i]))
                {
                    group.Thumbs.Insert(ii, tempList[i]);
                }
                else { return false; }
            }
            //ActiveThumbのFrontThumbとBackThumbの更新
            ChangedActiveFrontAndBackThumb(ActiveThumb);
            return true;

        }
        /// <summary>
        /// 選択Thumbを前面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZUp()
        {
            return ZUp(SelectedThumbs, ActiveGroup);
        }
        #endregion 前面に移動


        //backmost 最背面 back to back最背面にする
        //frontmsot 一番前
        //front 最前面
        //put one back  一つ後ろにする
        #endregion ZIndex

        #region 画像として取得
        public BitmapSource? GetBitmapRoot()
        {
            return MakeBitmapFromThumb(this, this);
        }
        public BitmapSource? GetBitmapActiveThumb()
        {
            return MakeBitmapFromThumb(ActiveThumb, ActiveThumb?.TTParent);
        }
        public BitmapSource? GetBitmapClickedThumb()
        {
            return MakeBitmapFromThumb(ClickedThumb, ClickedThumb?.TTParent);
        }
        public BitmapSource? GetBitmapThumb(TThumb thumb)
        {
            return MakeBitmapFromThumb(thumb, thumb?.TTParent);
        }
        /// <summary>
        /// 指定Thumbを画像として取得
        /// </summary>
        /// <param name="thumb">画像として取得したいThumb</param>
        /// <returns></returns>
        //public BitmapSource? GetBitmap(TThumb thumb)
        //{
        //    if (thumb.Type != TType.Root)
        //    {
        //        return MakeBitmapFromThumb(thumb, thumb.TTParent);
        //    }
        //    else
        //    {
        //        return MakeBitmapFromThumb(thumb, thumb);
        //    }
        //}
        /// <summary>
        /// 要素をBitmapに変換したものを返す
        /// </summary>
        /// <param name="el">Bitmapにする要素</param>
        /// <param name="parentPanel">Bitmapにする要素の親要素</param>
        /// <returns></returns>
        private BitmapSource? MakeBitmapFromThumb(FrameworkElement? el, FrameworkElement? parentPanel)
        {
            if (el == null || parentPanel == null) { return null; }
            if (el.ActualHeight == 0 || el.ActualWidth == 0) { return null; }

            //枠を一時的に非表示にする
            WakuVisibleType waku = TTWakuVisibleType;
            TTWakuVisibleType = WakuVisibleType.None;
            UpdateLayout();//再描画？これで枠が消える


            GeneralTransform gt = el.TransformToVisual(parentPanel);
            Rect bounds = gt.TransformBounds(new Rect(0, 0, el.ActualWidth, el.ActualHeight));
            DrawingVisual dVisual = new();
            //var debounds = VisualTreeHelper.GetDescendantBounds(parentPanel);
            //四捨五入しているけど、UselayoutRoundingをtrueにしていたら必要なさそう
            bounds.Width = (int)(bounds.Width + 0.5);
            bounds.Height = (int)(bounds.Height + 0.5);

            using (DrawingContext context = dVisual.RenderOpen())
            {
                VisualBrush vBrush = new(el) { Stretch = Stretch.None };
                //context.DrawRectangle(vBrush, null, new Rect(0, 0, bounds.Width, bounds.Height));
                //context.DrawRectangle(vBrush, null, bounds);

                context.DrawRectangle(vBrush, null, new Rect(bounds.Size));
            }
            RenderTargetBitmap bitmap
                = new((int)bounds.Width, (int)bounds.Height, 96.0, 96.0, PixelFormats.Pbgra32);
            bitmap.Render(dVisual);

            //枠表示を元に戻す
            TTWakuVisibleType = waku;

            return bitmap;
        }
        #endregion 画像として取得

        #region クリップボード系

        //アルファ値を失わずに画像のコピペできた、.NET WPFのClipboard - 午後わてんのブログ
        //        https://gogowaten.hatenablog.com/entry/2021/02/10/134406

        /// <summary>
        /// クリップボードに画像をコピーする。BitmapSourceとそれをPNG形式に変換したもの両方
        /// </summary>
        /// <param name="source"></param>
        private static void ClipboardSetBitmapWithPng(BitmapSource source)
        {
            //DataObjectに入れたいデータを入れて、それをクリップボードにセットする
            DataObject data = new();

            //BitmapSource形式そのままでセット
            data.SetData(typeof(BitmapSource), source);

            //PNG形式にエンコードしたものをMemoryStreamして、それをセット
            //画像をPNGにエンコード
            PngBitmapEncoder pngEnc = new();
            pngEnc.Frames.Add(BitmapFrame.Create(source));
            //エンコードした画像をMemoryStreamにSava
            using var ms = new System.IO.MemoryStream();
            pngEnc.Save(ms);
            data.SetData("PNG", ms);

            //クリップボードにセット
            Clipboard.SetDataObject(data, true);
        }
        /// <summary>
        /// Rootを画像としてクリップボードにコピー
        /// </summary>
        public void CopyImageRoot()
        {
            if (GetBitmapRoot() is BitmapSource bmp) { ClipboardSetBitmapWithPng(bmp); }
        }
        public void CopyImageActiveThumb()
        {
            if (GetBitmapActiveThumb() is BitmapSource bmp) { ClipboardSetBitmapWithPng(bmp); }
        }
        public void CopyImageClickedThumb()
        {
            if (GetBitmapClickedThumb() is BitmapSource bmp) { ClipboardSetBitmapWithPng(bmp); }
        }

        //クリップボードの画像をImageThumbとして追加
        /// <summary>
        /// クリップボードから画像を取得してActiveGroupに追加
        /// "PNG"形式優先で取得、できなければGetImageで取得
        /// </summary>
        public void AddImageThumbFromClipboard()
        {
            if (MyClipboard.GetImageFromClipboardPreferPNG() is BitmapSource bmp)
            {
                AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp }, true);
            }
            else { MessageBox.Show("画像は得られなかった"); }
        }
        //クリップボードから画像追加、"PNG"形式で取得
        public void AddImageThumbFromClipboardPng()
        {
            if (MyClipboard.GetClipboardImagePngWithAlphaFix() is BitmapSource bmp)
            {
                AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp }, true);
            }
            else { MessageBox.Show("画像は得られなかった"); }
        }
        //クリップボードから画像追加、"PNG"形式で取得＋強制Bgr32変換
        public void AddImageThumbFromClipboardBgr32()
        {
            if (MyClipboard.GetClipboardImageBgr32() is BitmapSource bmp)
            {
                AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp }, true);
            }
            else { MessageBox.Show("画像は得られなかった"); }
        }


        #endregion クリップボード系

        #region XYZ移動
        /// <summary>
        /// ActiveThumbを1グリッド上へ移動
        /// </summary>
        public void ActiveThumbGoUpGrid()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is TTGroup parent)
            {
                thumb.TTTop -= parent.TTGrid;
            }
        }
        public void ActiveThumbGoDownGrid()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is TTGroup parent)
            {
                thumb.TTTop += parent.TTGrid;
            }
        }
        public void ActiveThumbGoLeftGrid()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is TTGroup parent)
            {
                thumb.TTLeft -= parent.TTGrid;
            }
        }
        public void ActiveThumbGoRightGrid()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is TTGroup parent)
            {
                thumb.TTLeft += parent.TTGrid;
            }
        }
        public void ActiveThumbGoUp1Pix()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is not null)
            {
                thumb.TTTop--;
            }
        }
        public void ActiveThumbGoDown1Pix()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is not null)
            {
                thumb.TTTop++;
            }
        }
        public void ActiveThumbGoLeft1Pix()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is not null)
            {
                thumb.TTLeft--;
            }
        }
        public void ActiveThumbGoRight1Pix()
        {
            if (ActiveThumb is TThumb thumb && thumb.TTParent is not null)
            {
                thumb.TTLeft++;
            }
        }


        #endregion XYZ移動

        #region 複製
        #region Data複製して追加

        //Clickedの複製は中止。追加する座標や追加するGroupをどこにするか考え中

        //public bool DuplicateClickedThumb()
        //{
        //    if (ClickedThumb?.Data.DeepCopy() is Data data)
        //    {
        //        //data.X += ActiveGroup.TTXShift;
        //        //data.Y += ActiveGroup.TTYShift;
        //        //AddThumbDataToActiveGroup(data, false);

        //        AddThumbDataToActiveGroup(data);
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 選択Thumbを複製する
        /// </summary>
        /// <returns>複製した個数</returns>
        public int DuplicateDataSelectedThumbs()
        {
            //Dataとして複製、SelectedThumbs
            int count = 0;
            List<Data> datas = new();
            var thumbs = MakeSortedList(SelectedThumbs, ActiveGroup);
            //Data複製、ディープコピー
            foreach (var item in thumbs)
            {
                if (item.Data.DeepCopy() is Data data)
                {
                    //座標修正
                    data.X += ActiveGroup.TTXShift;
                    data.Y += ActiveGroup.TTYShift;
                    datas.Add(data);
                }
            }

            //DataからThumb作成して追加
            List<TThumb> selection = new();
            foreach (var item in datas)
            {
                if (AddThumbDataToActiveGroup(item, true, false) is TThumb thumb)
                {
                    selection.Add(thumb);
                    count++;
                }
            }

            //SelectedThumbsを複製したThumbに置き換える
            SelectedThumbs.Clear();
            foreach (var item in selection)
            {
                SelectedThumbs.Add(item);
            }
            return count;
        }
        #endregion Data複製して追加

        #region 画像として複製
        public int DuplicateImageSelectedThumbs()
        {

            //画像として複製、SelectedThumbs
            int count = 0;
            List<Data> datas = new();
            var sortedThumbs = MakeSortedList(SelectedThumbs, ActiveGroup);
            //Data複製
            foreach (var item in sortedThumbs)
            {
                if (GetBitmapThumb(item) is BitmapSource bmp)
                {
                    //座標修正
                    Data data = new(TType.Image)
                    {
                        BitmapSource = bmp,
                        X = item.Data.X + ActiveGroup.TTXShift,
                        Y = item.Data.Y + ActiveGroup.TTYShift,
                    };
                    datas.Add(data);
                }
            }

            //DataからThumb作成して追加
            List<TThumb> selection = new();
            foreach (var item in datas)
            {
                if (AddThumbDataToActiveGroup(item, true, false) is TThumb thumb)
                {
                    selection.Add(thumb);
                    count++;
                }
            }

            //SelectedThumbsを複製したThumbに置き換える
            SelectedThumbs.Clear();
            foreach (var item in selection)
            {
                SelectedThumbs.Add(item);
            }
            return count;
        }
        #endregion 画像として複製
        #endregion 複製

        #region ショートカットキー

        #endregion ショートカットキー

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


        private readonly TextBlock MyTemplateElement;

        public TTTextBlock() : this(new Data(TType.TextBlock)) { }

        public TTTextBlock(Data data) : base(data)
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
            //MySetXYBinging(this.Data);
        }
    }
    public class TTTextBox : TThumb
    {
        #region 依存プロパティ

        public string TTText
        {
            get { return (string)GetValue(TTTextProperty); }
            set { SetValue(TTTextProperty, value); }
        }
        public static readonly DependencyProperty TTTextProperty =
            DependencyProperty.Register(nameof(TTText), typeof(string), typeof(TTTextBox),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Text編集状態の切り替え、編集終了時にはコールバックでTTTextBoxにフォーカスを移す
        /// </summary>
        public bool IsEdit
        {
            get { return (bool)GetValue(IsEditProperty); }
            set { SetValue(IsEditProperty, value); }
        }
        public static readonly DependencyProperty IsEditProperty =
            DependencyProperty.Register(nameof(IsEdit), typeof(bool), typeof(TTTextBox),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsEditChanged)));
        /// <summary>
        /// 編集終了時にはTTTextBoxにフォーカスを移す
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnIsEditChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TTTextBox ttt && e.NewValue is bool b && b == false)
            {
                ttt.Focus();
            }
        }

        #endregion 依存プロパティ


        private readonly HutaTextBox MyTemplateElement;

        public TTTextBox() : this(new Data(TType.TextBox)) { }

        public TTTextBox(Data data) : base(data)
        {
            Data = data;
            this.DataContext = Data;
            if (MakeTemplate<HutaTextBox>() is HutaTextBox element)
            {
                MyTemplateElement = element;
            }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            SetBinding(TTTextProperty, nameof(Data.Text));
            MyTemplateElement.SetBinding(HutaTextBox.TextProperty, nameof(Data.Text));
            MyTemplateElement.SetBinding(HutaTextBox.IsEditProperty, new Binding() { Source = this, Path = new PropertyPath(IsEditProperty) });
            
            Binding b = new(nameof(data.FontName));
            b.Mode = BindingMode.TwoWay;
            b.Converter = new ConverterFontFamilyName();
            SetBinding(FontFamilyProperty, b);
            MyTemplateElement.SetBinding(FontFamilyProperty, b);
            
            b = new(nameof(data.FontSize));            
            b.Mode = BindingMode.TwoWay;
            SetBinding(TextBox.FontSizeProperty, b);

            //b = new(nameof(data.FontStretch));
            //b.Mode= BindingMode.TwoWay;
            //SetBinding(TextBox.FontStretchProperty, b);

            b = new(nameof(data.FontStyle));
            b.Mode= BindingMode.TwoWay;
            SetBinding(TextBox.FontStyleProperty, b);
            
            b = new(nameof(data.FontWeight));
            b.Mode= BindingMode.TwoWay;
            SetBinding(TextBox.FontWeightProperty, b);

            //SetBinding(TextBox.FontStretchProperty,new Binding(nameof(data.FontStretch)) { Mode= BindingMode.TwoWay });
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F2) { IsEdit = !IsEdit; }
        }
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            IsEdit = !IsEdit;
        }
    }
    /// <summary>
    /// 編集可能状態を切り替えるTextBox、Gridの蓋の取り外しをIsEditプロパティで切り替える
    /// TTTextBoxのテンプレート用
    /// </summary>
    class HutaTextBox : ContentControl
    {
        private readonly string HUTA = "huta";
        private readonly string TEXTBOX = "mytextbox";
        private readonly Grid HutaGrid;
        private readonly TextBox MyTextBox;
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HutaTextBox),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 編集状態の切り替え、プロパティ変更時にcallbackを使って切り替える
        /// 蓋の背景色が透明色ならnullにしてTextBoxを編集状態にする
        /// 蓋の背景色がnullだった場合は透明色にして編集状態終了
        /// </summary>
        public bool IsEdit
        {
            get { return (bool)GetValue(IsEditProperty); }
            set { SetValue(IsEditProperty, value); }
        }
        public static readonly DependencyProperty IsEditProperty =
            DependencyProperty.Register(nameof(IsEdit), typeof(bool), typeof(HutaTextBox),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsEditChanged)));

        private static void OnIsEditChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HutaTextBox huta)
            {
                if (huta.HutaGrid.Background == Brushes.Transparent)
                {
                    huta.HutaGrid.Background = null;
                    huta.MyTextBox.Focusable = true;
                    Keyboard.Focus(huta.MyTextBox);
                    huta.MyTextBox.SelectAll();
                }
                else
                {
                    huta.HutaGrid.Background = Brushes.Transparent;
                    Keyboard.ClearFocus();
                    huta.MyTextBox.Focusable = false;
                }
            }
        }

        public HutaTextBox()
        {
            SetTemplata();
            HutaGrid = (Grid)Template.FindName(HUTA, this);
            MyTextBox = (TextBox)Template.FindName(TEXTBOX, this);
        }

        private void SetTemplata()
        {
            FrameworkElementFactory baseGrid = new(typeof(Grid));
            FrameworkElementFactory factory = new(typeof(TextBox), TEXTBOX);
            FrameworkElementFactory huta = new(typeof(Grid), HUTA);
            huta.SetValue(Grid.BackgroundProperty, Brushes.Transparent);
            factory.SetValue(TextBox.TextProperty,
                new Binding() { Source = this, Path = new PropertyPath(TextProperty) });

            factory.SetValue(TextBox.TextWrappingProperty, TextWrapping.Wrap);
            factory.SetValue(TextBox.AcceptsReturnProperty, true);//Enterで改行入力
            factory.SetValue(TextBox.AcceptsTabProperty, true);//Tabでタブ文字入力

            baseGrid.AppendChild(factory);
            baseGrid.AppendChild(huta);
            Template = new() { VisualTree = baseGrid };
            ApplyTemplate();
        }

    }

    public class TTImage : TThumb
    {
        //画像ファイルのフルパス、変更時にコールバックでBitmapSourceを作成して表示する
        public string TTSourcePath
        {
            get { return (string)GetValue(TTSourcePathProperty); }
            set { SetValue(TTSourcePathProperty, value); }
        }
        public static readonly DependencyProperty TTSourcePathProperty =
            DependencyProperty.Register(nameof(TTSourcePath), typeof(string), typeof(TTImage),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTTUriChanged)));
        //コールバック、BitmapSourceを作成して表示する
        private static void OnTTUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TTImage thumb)
            {
                thumb.Data.BitmapSource = TTRoot.GetBitmap((string)e.NewValue);
            }
        }

        private readonly Image MyTemplateElement;


        public TTImage() : this(new Data(TType.Image)) { }
        public TTImage(Data data) : base(data)
        {
            Data = data;
            this.DataContext = Data;
            if (MakeTemplate<Image>() is Image element) { MyTemplateElement = element; }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            MyTemplateElement.SetBinding(Image.SourceProperty, nameof(Data.BitmapSource));
            //MySetXYBinging(this.Data);
        }

    }




    #region コンバーター
    public class ConverterFontFamilyName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            FontFamily font = new(name);
            return font;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontFamily font = (FontFamily)value;
            return font.Source;
        }
    }

    public class ConverterWakuBrush : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<Brush> myBrushes = (List<Brush>)parameter;
            if (values[3] is WakuVisibleType type)
            {
                if (type == WakuVisibleType.None || type == WakuVisibleType.OnlyActiveGroup)
                {
                    return Brushes.Transparent;
                }
                else
                {
                    if ((bool)values[2]) { return myBrushes[4]; }
                    else if ((bool)values[1]) { return myBrushes[3]; }
                    else if ((bool)values[0]) { return myBrushes[0]; }
                    else return Brushes.Transparent;
                }

            }
            else return Brushes.Transparent;
            //WakuVisibleType type = (WakuVisibleType)values[3];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ConverterWakuBrushForTTGroup : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<Brush> myBrushes = (List<Brush>)parameter;
            Brush result = Brushes.Transparent;
            if (values[4] is WakuVisibleType type)
            {



                //WakuVisibleType type = (WakuVisibleType)values[4];
                switch (type)
                {
                    case WakuVisibleType.None:
                        break;
                    case WakuVisibleType.All:
                        if ((bool)values[0]) { result = myBrushes[4]; }
                        else if ((bool)values[1]) { result = myBrushes[3]; }
                        else if ((bool)values[2]) { result = myBrushes[2]; }
                        else if ((bool)values[3]) { result = myBrushes[1]; }
                        break;
                    case WakuVisibleType.OnlyActiveGroup:
                        if ((bool)values[2]) { result = myBrushes[2]; }
                        break;
                    case WakuVisibleType.NotGroup:
                        if ((bool)values[0]) { result = myBrushes[4]; }
                        else if ((bool)values[1]) { result = myBrushes[3]; }
                        else if ((bool)values[2]) { result = myBrushes[2]; }
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //列挙型の値をラジオボタンにバインドするには？［ユニバーサルWindowsアプリ開発］：WinRT／Metro TIPS - ＠IT
    //    https://atmarkit.itmedia.co.jp/ait/articles/1507/29/news019.html
    //wpf - How to bind RadioButtons to an enum? - Stack Overflow
    //    https://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum

    public class ConverterEnumToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(true)) { return true; }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(true)) { return parameter; }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }

    //表示非表示の切り替え用
    public class ConverterVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bb = (bool)value;
            if (bb)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vi = (Visibility)value;
            if (vi == Visibility.Visible) { return false; }
            else { return true; }
        }
    }

    public class ConverterBoolInverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bb = (bool)value;
            if (bb is true) { return false; }
            else { return true; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? bb = (bool?)value;
            if (bb is null or true) { return false; }
            else { return true; }
        }
    }

    #endregion コンバーター


    public class ExObservableCollection : ObservableCollection<TThumb>
    {
        protected override void ClearItems()
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
            base.ClearItems();
        }
        protected override void SetItem(int index, TThumb item)
        {
            item.IsSelected = true;
            base.SetItem(index, item);
        }
        protected override void RemoveItem(int index)
        {
            Items[index].IsSelected = false;
            base.RemoveItem(index);
        }
        protected override void InsertItem(int index, TThumb item)
        {
            item.IsSelected = true;
            base.InsertItem(index, item);
        }
    }



}