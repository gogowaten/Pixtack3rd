using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pixtack3rd
{
    /// <summary>
    /// Root.xaml の相互作用ロジック
    /// </summary>
    [ContentProperty(nameof(Thumbs))]
    public partial class Root : UserControl
    {

        #region 依存プロパティ

        public int TTXShift
        {
            get { return (int)GetValue(TTXShiftProperty); }
            set { SetValue(TTXShiftProperty, value); }
        }
        public static readonly DependencyProperty TTXShiftProperty =
            DependencyProperty.Register(nameof(TTXShift), typeof(int), typeof(Root),
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
            DependencyProperty.Register(nameof(TTYShift), typeof(int), typeof(Root),
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
            DependencyProperty.Register(nameof(TTGrid), typeof(int), typeof(Root),
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
            DependencyProperty.Register(nameof(TTVisibleBorder), typeof(Visibility), typeof(Root),
                new FrameworkPropertyMetadata(Visibility.Hidden,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存プロパティ


        #region 通知プロパティ

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        //最後にクリックしたThumb
        private TThumb? _clickedThumb;
        public TThumb? ClickedThumb { get => _clickedThumb; set => SetProperty(ref _clickedThumb, value); }

        //注目しているThumb、選択Thumb群の筆頭
        private TThumb? _activeThumb;
        public TThumb? ActiveThumb
        {
            get => _activeThumb;
            set
            {
                SetProperty(ref _activeThumb, value);
                //FrontActiveThumbとBackActiveThumbを更新する
                //ChangedActiveThumb(value);
            }
        }

        private TThumb? _frontActiveThumb;
        public TThumb? FrontActiveThumb { get => _frontActiveThumb; set => SetProperty(ref _frontActiveThumb, value); }

        private TThumb? _backActiveThumb;
        public TThumb? BackActiveThumb { get => _backActiveThumb; set => SetProperty(ref _backActiveThumb, value); }

        //ActiveThumbの親要素、移動可能なThumbはこの要素の中のThumbだけ。起動直後はRootThumbがこれになる
        private TTGroup? _activeGroup;
        public TTGroup? ActiveGroup
        {
            get => _activeGroup;
            set
            {
                //ChildrenDragEventDesoption(_activeGroup, value);
                SetProperty(ref _activeGroup, value);
            }
        }

        #endregion 通知プロパティ

        #region 通常プロパティ
        protected readonly string TEMPLATE_NAME = "NEMO";
        //選択状態の要素を保持
        public ObservableCollection<TThumb> SelectedThumbs { get; private set; } = new();

        //クリック前の選択状態、クリックUp時の削除に使う
        private bool IsSelectedPreviewMouseDown { get; set; }

        private ItemsControl MyTemplateElement;
        public ObservableCollection<TThumb> Thumbs { get; private set; } = new();
        public Data Data { get; set; }
        #endregion 通常プロパティ

        public Root():this(new Data(TType.Root))
        {
            
        }
        public Root(Data data)
        {
            InitializeComponent();
            DataContext = this;

            //初期設定
            Thumbs.CollectionChanged += Thumbs_CollectionChanged;
            Data = data;
            MyTemplateElement = MyInitializeBinding();
            MyTemplateElement.SetBinding(ItemsControl.ItemsSourceProperty,
                new Binding(nameof(Thumbs)) { Source = this });

            SetBinding(TTXShiftProperty, nameof(Data.XShift));
            SetBinding(TTYShiftProperty, nameof(Data.YShift));
            SetBinding(TTGridProperty, nameof(Data.Grid));

            Loaded += (a, b) =>
            {
                //TTGroupUpdateLayout();
                FixDataDatas();
            };
        }
        private void FixDataDatas()
        {
            foreach (var item in Thumbs)
            {
                if (Data.Datas.Contains(item.Data) == false)
                {
                    Data.Datas.Add(item.Data);
                }
            }
        }
        private void Thumbs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems?[0] is TThumb thumb)
                    {
                        thumb.TTParent = MyRoot;
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

            fWaku.SetValue(VisibilityProperty, new Binding(nameof(TTVisibleBorder)) { Source = this });
            fWaku.SetValue(Shape.StrokeProperty, Brushes.Red);
            fWaku.SetValue(Shape.StrokeThicknessProperty, 10.0);
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






    }
}
