using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Controls.Primitives;
using System.Reflection;
using ControlLibraryCore20200620;
using System.Security.Policy;

namespace Pixtack3rd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //アプリの設定Data、DataContextにする
        //public AppConfig MyAppConfig { get; set; }
        public AppData MyAppData { get; set; }

        //アプリ名
        private const string APP_NAME = "Pixtack3rd";
        //保存データの拡張子
        private const string EXT_DATA = ".ps3";
        //アプリの設定ファイルの拡張子
        private const string EXT_APPDATA = ".ps3config";

        //アプリの設定ファイル名
        private const string APPDATA_FILE_NAME = "default" + EXT_APPDATA;

        //zipデータ内のDataファイル名
        private const string XML_FILE_NAME = "Data.xml";
        private const string APP_LAST_END_TIME_FILE_NAME = "LastEndTimeData" + EXT_DATA;
        //読み込んでいるデータファイルのフルパス、上書き保存対象、起動時は前回終了時を読み込み
        private string CurrentDataFilePath = string.Empty;
        //終了時に状態保存、起動時に読み込みするファイルのフルパス
        private string AppLastEndTimeDataFilePath { get; } = string.Empty;
        //拡張子名、全データ(Rootデータとアプリの設定)用の拡張子
        //private const string EXTENSION_NAME_APP = ".p3";
        //拡張子名、Thumbデータだけ用の拡張子
        //private const string EXTENSION_NAME_DATA = ".p3d";

        private const string EXT_FILTER_DATA = "Data|*" + EXT_DATA;
        private const string EXT_FILTER_APP = "アプリ設定|*" + EXT_APPDATA;
        //private const string EXTENSION_FILTER_P3 = "Data + 設定|*" + EXTENSION_NAME_APP;
        //private const string EXTENSION_FILTER_P3D = "Data|*" + EXTENSION_NAME_DATA;

        //アプリのバージョン
        private readonly string AppVersion;
        //アプリのフォルダパス
        private string AppDirectory;

        //datetime.tostringの書式、これを既定値にする
        private const string DATE_TIME_STRING_FORMAT = "yyyyMMdd'_'HHmmss'_'fff";
        //private const string APP_ROOT_DATA_FILENAME = "TTRoot" + EXTENSION_NAME_DATA;

        //マウスクリックでPolyline描画するときの一時的なもの
        private readonly PointCollection MyTempPoints = new();
        private GeometricShape? MyTempShape;


        //右クリックメニュー、複数選択Thumb用
        private ContextMenu MyContextMenuForSelected = new();
        //右クリックメニュー、単体Thumb用
        private ContextTabMenu MyContextTabMenuForSingle = new();

        //範囲選択
        //private TTRange MyTTRange = new();

        //カラーピッカー
        //private readonly ColorPicker MyColorPicker = new();

        public MainWindow()
        {
            InitializeComponent();

            AppVersion = GetAppVersion();
            AppDirectory = Environment.CurrentDirectory;

            //MyAppConfig = GetAppConfig(APP_CONFIG_FILE_NAME);
            MyAppData = GetAppData();

            //前回終了時に保存したファイルのフルパスをセット
            AppLastEndTimeDataFilePath = System.IO.Path.Combine(
                AppDirectory, APP_LAST_END_TIME_FILE_NAME);

            MyInitializeComboBox();

            Drop += MainWindow_Drop;
            Closed += MainWindow_Closed;
            DataContextChanged += MainWindow_DataContextChanged;


            MyInitialize();



            MyTabControl.SelectedIndex = 3;

            // マウスクリックでShape描画
            MyDrawCanvas.MouseLeftButtonDown += MyDrawCanvas_MouseLeftButtonDown;
            MyDrawCanvas.MouseMove += MyDrawCanvas_MouseMove;
            MyDrawCanvas.MouseRightButtonDown += MyDrawCanvas_MouseRightButtonDown;

            //ClickedThumb変更イベント時
            MyRoot.ClickedThumbChanging += MyRoot_ClickedThumbChanging;
            MyRoot.ThumbDragCompleted += MyRoot_ThumbDragCompleted;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetMyBindings();
        }


        #region 初期設定

        /// <summary>
        /// ドラッグ移動終了後にスクロールバーの位置修正
        /// </summary>
        /// <param name="obj"></param>
        private void MyRoot_ThumbDragCompleted(TThumb obj)
        {
            MyScrollViewer.ScrollToHorizontalOffset(obj.TTLeft);
            MyScrollViewer.ScrollToVerticalOffset(obj.TTTop);
        }

        /// <summary>
        /// 実行ファイルと同じフォルダにある設定ファイルをデシリアライズして返す、
        /// 見つからないときは新規作成して返す
        /// </summary>
        /// <returns></returns>
        private AppData GetAppData()
        {
            string filePath = System.IO.Path.Combine(
                AppDirectory,
                APPDATA_FILE_NAME);

            if (File.Exists(filePath) && LoadAppData<AppData>(filePath) is AppData data)
            {
                return data;
                //return LoadAppData<AppData>(filePath);
            }
            else { return new AppData(); }
        }

        private void MyInitialize()
        {
            //タイトルをアプリの名前 + バージョン
            this.Title = APP_NAME + AppVersion;

            //アプリ終了時に保存したDataファイルのフルパスを上書き保存パスにセット
            CurrentDataFilePath = AppLastEndTimeDataFilePath;

            //枠表示の設定Binding、これはいまいちな処理
            MyRoot.SetBinding(TTRoot.TTWakuVisibleTypeProperty, new Binding(nameof(MyAppData.WakuVisibleType)) { Source = this.MyAppData });

            //データコンテキストの設定、Bindingをした後じゃないと反映されない？
            FixWindowLocate();//ウィンドウ位置修正
            DataContext = MyAppData;
            //SetMyBindings();

            //ショートカットキー
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            //右クリックメニュー初期化
            MyContextMenuForSelected = MakeContextMenuForSelected();
            MyContextTabMenuForSingle.AddMenuTab(MakeTabItemForActiveThumbMenu());
            MyContextTabMenuForSingle.AddMenuTab(MakeTabItemForClickedThumbMenu());
            //右クリックメニュー展開時、複数Thumb選択用と単数Thumb用の切り替え
            this.ContextMenuOpening += (s, e) =>
            {
                if (e.Source == MyRoot)
                {
                    bool flag = false;
                    if (ContextMenu == null) flag = true;
                    if (MyRoot.SelectedThumbs.Count >= 2)
                    {
                        ContextMenu = MyContextMenuForSelected;
                    }
                    else
                    {
                        ContextMenu = MyContextTabMenuForSingle;
                    }
                    if (flag) { ContextMenu.IsOpen = true; }
                }
                else
                {
                    ContextMenu = null;
                }
            };

            MyAreaCanvas.ContextMenu = MakeContextMenuForAreaThumb();

            //MyColorPicker.Closing += MyColorPicker_Closing;
            //MyColorPicker.Closed += MyColorPicker_Closed;
        }

        //private void MyColorPicker_Closed(object? sender, EventArgs e)
        //{

        //}

        //private void MyColorPicker_Closing(object? sender, CancelEventArgs e)
        //{
        //    MyColorPicker.Visibility = Visibility.Collapsed;
        //    e.Cancel = true;
        //}

        private void SetMyBindings()
        {


            //AreaThumb範囲選択用
            MyAreaThumb.DataContext = MyAppData;
            MyAreaThumb.SetBinding(WidthProperty, new Binding(nameof(AppData.AreaWidth)) { Mode = BindingMode.TwoWay });
            MyAreaThumb.SetBinding(HeightProperty, new Binding(nameof(AppData.AreaHeight)) { Mode = BindingMode.TwoWay });
            MyAreaThumb.SetBinding(Canvas.LeftProperty, new Binding(nameof(AppData.AreaLeft)) { Mode = BindingMode.TwoWay });
            MyAreaThumb.SetBinding(Canvas.TopProperty, new Binding(nameof(AppData.AreaTop)) { Mode = BindingMode.TwoWay });


            MultiBinding mb;
            //文字列描画
            MyBorderTextForeColor.DataContext = MyAppData;
            mb = new()
            {
                Converter = new MyConverterARGB2SolidBrush(),
                Mode = BindingMode.TwoWay
            };
            mb.Bindings.Add(new Binding(nameof(AppData.TextForeColorA)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextForeColorR)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextForeColorG)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextForeColorB)) { Mode = BindingMode.TwoWay });
            MyBorderTextForeColor.SetBinding(BackgroundProperty, mb);

            MyBorderTextBackColor.DataContext = MyAppData;
            mb = new()
            {
                Converter = new MyConverterARGB2SolidBrush(),
                Mode = BindingMode.TwoWay
            };
            mb.Bindings.Add(new Binding(nameof(AppData.TextBackColorA)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextBackColorR)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextBackColorG)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextBackColorB)) { Mode = BindingMode.TwoWay });
            MyBorderTextBackColor.SetBinding(BackgroundProperty, mb);

            MyBorderTextBorderColor.DataContext = MyAppData;
            mb = new()
            {
                Converter = new MyConverterARGB2SolidBrush(),
                Mode = BindingMode.TwoWay
            };
            mb.Bindings.Add(new Binding(nameof(AppData.TextBorderColorA)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextBorderColorR)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextBorderColorG)) { Mode = BindingMode.TwoWay });
            mb.Bindings.Add(new Binding(nameof(AppData.TextBorderColorB)) { Mode = BindingMode.TwoWay });
            MyBorderTextBorderColor.SetBinding(BackgroundProperty, mb);

            //フォント、設定が空白か存在しないフォント名なら、今のフォントを指定してからBinding
            if (MyAppData.FontName == "" || !GetFontFamilies().ContainsKey(MyAppData.FontName))
            {
                MyAppData.FontName = this.FontFamily.Source;
            }
            MyComboBoxFontFmilyNames.SetBinding(ComboBox.SelectedValueProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.FontNameProperty), Mode = BindingMode.TwoWay });

            MyNumeFontSize.SetBinding(NumericUpDown.MyValueProperty, new Binding(nameof(AppData.FontSize)) { Source = MyAppData, Mode = BindingMode.TwoWay });
            MyNumeTextBoxWakuWidth.SetBinding(NumericUpDown.MyValueProperty, new Binding(nameof(AppData.TextBoxBorderWidth)) { Source = MyAppData, Mode = BindingMode.TwoWay });
            MyCheckIsBold.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(AppData.IsTextBold)) { Source = MyAppData, Mode = BindingMode.TwoWay });
            MyCheckIsItalic.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(AppData.IsTextItalic)) { Source = MyAppData, Mode = BindingMode.TwoWay });

            //図形タブ
            MyNumeStrokeThickness.SetBinding(NumericUpDown.MyValueProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.StrokeWidthProperty), Mode = BindingMode.TwoWay });
            MyNumeShapeStrokeColorA.SetBinding(NumericUpDown.MyValueProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.ShapeStrokeColorAProperty), Mode = BindingMode.TwoWay });
            MyBorderShapeColor.SetBinding(BackgroundProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.ShapeStrokeColorProperty), Converter = new MyConverterColorSolidBrush() });

        }


        //ショートカットキー
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.None:
                    if (e.Key == Key.F4) { MyRoot.RemoveThumb(); }
                    break;
                case ModifierKeys.Alt:
                    break;
                case ModifierKeys.Control:
                    //if (e.Key == Key.S) { SaveRootDataWithConfig(CurrentFileFullPath, MyRoot.Data, true); }
                    if (e.Key == Key.S) { SaveThumbData(MyRoot, CurrentDataFilePath); }
                    //else if (e.Key == Key.D) { DuplicateDataSelectedThumbs(); }
                    break;
                case ModifierKeys.Shift:
                    break;
                case ModifierKeys.Windows:
                    break;
                case (ModifierKeys.Control | ModifierKeys.Shift):
                    //if (e.Key == Key.S) { SaveAll(); }
                    if (e.Key == Key.D) { DuplicateDataRoot(); }
                    else if (e.Key == Key.F4) { MyRoot.RemoveAll(); }
                    break;
            }

        }

        private void MyInitializeComboBox()
        {
            ComboBoxSaveFileType.ItemsSource = Enum.GetValues(typeof(ImageType));
            MyComboBoxFontFmilyNames.ItemsSource = GetFontFamilies();
            MyComboBoxFontFmilyNames.SelectedValue = this.FontFamily;
            MyComboBoxLineHeadBeginType.ItemsSource = Enum.GetValues(typeof(HeadType));
            MyComboBoxLineHeadBeginType.SelectedValue = HeadType.None;
            MyComboBoxLineHeadEndType.ItemsSource = Enum.GetValues(typeof(HeadType));
            MyComboBoxLineHeadEndType.SelectedValue = HeadType.Arrow;
            MyComboBoxShapeType.ItemsSource = Enum.GetValues(typeof(ShapeType));
            MyComboBoxShapeType.SelectedValue = ShapeType.Line;


        }

        /// <summary>
        /// ウィンドウ位置設定が画面外だった場合は0にする
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private void FixWindowLocate()
        {
            if (MyAppData.AppLeft < -10 ||
                MyAppData.AppLeft > SystemParameters.VirtualScreenWidth - 100)
            {
                MyAppData.AppLeft = 0;
            }
            if (MyAppData.AppTop < -10 ||
                MyAppData.AppTop > SystemParameters.VirtualScreenHeight - 100)
            {
                MyAppData.AppTop = 0;
            }
        }

        #endregion 初期設定

        #region 右クリックメニュー

        //AreaThumb(範囲選択)用右クリックメニュー
        private ContextMenu MakeContextMenuForAreaThumb()
        {
            ContextMenu menu = new();
            MenuItem item;
            item = new() { Header = "コピー" }; menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (GetAreaBitmap() is BitmapSource bmp) MyRoot.ClipboardSetBitmapWithPng(bmp);
            };
            item = new() { Header = "コピペ" }; menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (GetAreaBitmap() is BitmapSource bmp) MyRoot.AddThumbDataToActiveGroup2(
                    new Data(TType.Image) { BitmapSource = bmp }, false, true);
                //if (GetAreaBitmap() is BitmapSource bmp) MyRoot.AddThumbDataToActiveGroup(
                //    new Data(TType.Image) { BitmapSource = bmp }, true, true);
            };
            item = new() { Header = "名前を付けて保存" }; menu.Items.Add(item);
            item.Click += (s, e) => { if (GetAreaBitmap() is BitmapSource bmp) SaveBitmap2(bmp); };

            return menu;
        }

        //AreaThumb用、選択範囲の画像作成
        private BitmapSource GetAreaBitmap()
        {
            //描画のRect取得
            var bounds = MyAreaThumb.TransformToVisual(MyRoot)
                .TransformBounds(VisualTreeHelper.GetDescendantBounds(MyAreaThumb));
            DrawingVisual dv = new() { Offset = new Vector(-bounds.X, -bounds.Y) };
            using (var context = dv.RenderOpen())
            {
                VisualBrush vb = new(MyRoot) { Stretch = Stretch.None };
                context.DrawRectangle(vb, null, VisualTreeHelper.GetDescendantBounds(MyRoot));
            }
            RenderTargetBitmap bitmap = new(
                (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height)
                , 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);
            return bitmap;
        }

        //複数選択時用
        private ContextMenu MakeContextMenuForSelected()
        {
            ContextMenu menu = new();
            MenuItem item;
            item = new() { Header = "グループ化" }; menu.Items.Add(item);
            item.Click += (s, e) => { MyRoot.AddGroup(); };
            item = new() { Header = "画像で複製" }; menu.Items.Add(item);
            item.Click += (s, e) => { MyRoot.DuplicateImageSelectedThumbs(); };
            item = new() { Header = "Dataで複製" }; menu.Items.Add(item);
            item.Click += (s, e) => { MyRoot.DuplicateDataSelectedThumbs(); };

            menu.Items.Add(new Separator() { Height = 10 });
            item = new() { Header = "削除" }; menu.Items.Add(item);
            item.Click += (s, e) => { MyRoot.RemoveThumb(); };
            menu.Items.Add(new Separator() { Height = 10 });

            //item = new() { Header = "Z移動" };
            menu.Items.Add(MakeMenuItemZMove());

            item = new() { Header = "複数選択解除" }; menu.Items.Add(item);
            item.Click += (s, e) => { throw new ArgumentException(); };

            return menu;
        }

        //Z移動サブメニュー
        private MenuItem MakeMenuItemZMove()
        {
            MenuItem item = new() { Header = "Z移動" };
            item.MouseEnter += (s, e) => { item.IsSubmenuOpen = true; };
            item.MouseLeave += (s, e) => { item.IsSubmenuOpen = false; };
            MenuItem subItem;
            subItem = new() { Header = "最前面" }; item.Items.Add(subItem);
            subItem.Click += (s, e) => { MyRoot.ZUpFrontMost(); };
            subItem = new() { Header = "前面" }; item.Items.Add(subItem);
            subItem.Click += (s, e) => { MyRoot.ZUp(); };
            subItem = new() { Header = "背面" }; item.Items.Add(subItem);
            subItem.Click += (s, e) => { MyRoot.ZDown(); };
            subItem = new() { Header = "再背面" }; item.Items.Add(subItem);
            subItem.Click += (s, e) => { MyRoot.ZDownBackMost(); };
            return item;
        }


        //ActiveThumb用
        private TabItem MakeTabItemForActiveThumbMenu()
        {
            StackPanel main = new();
            //MenuItem main = new();
            MenuItem item;
            item = new() { Header = "画像でコピー" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.CopyImageActiveThumb(); };
            item = new() { Header = "画像で複製" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.DuplicateImageSelectedThumbs(); };
            item = new() { Header = "画像で保存" }; main.Children.Add(item);
            item.Click += (s, e) => { SaveImageActiveThumb(); };
            item = new() { Header = "Dataで複製" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.DuplicateDataSelectedThumbs(); };
            item = new() { Header = "Dataで保存" }; main.Children.Add(item);
            item.Click += (s, e) => { SaveDataForActiveThumb(); };
            item = new() { Header = "削除" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.RemoveThumb(); };

            //main.Children.Add(MakeMenuEditStart());//編集開始メニュー
            item = new() { Header = "編集開始" };
            item.Click += (s, e) =>
            {
                if (MyRoot.ActiveThumb is TTGeometricShape) { ShapeEditStart(); }
                else if (MyRoot.ActiveThumb is TTTextBox box) { box.IsEdit = true; }
                else if (MyRoot.ActiveThumb is TTGroup) { MyRoot.ChangeActiveGroupInside(); }
            };
            main.Children.Add(item);

            item = new() { Header = "グループ解除" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.UnGroup(); };
            item = new() { Header = "IN" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.ChangeActiveGroupInside(); };
            item = new() { Header = "OUT" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.ChangeActiveGroupOutside(); };

            //Z移動
            main.Children.Add(MakeMenuItemZMove());


            Rectangle rect = new() { Width = 100, Height = 100 };
            VisualBrush vb = new() { Stretch = Stretch.Uniform };
            BindingOperations.SetBinding(vb, VisualBrush.VisualProperty, new Binding(nameof(MyRoot.ActiveThumb)) { Source = MyRoot });
            rect.Fill = vb;
            StackPanel header = new();
            header.Children.Add(new TextBlock() { Text = "Active" });
            header.Children.Add(rect);
            TabItem ti = new() { Header = header };
            ti.Content = main;
            return ti;
        }

        //Clicked用
        private TabItem MakeTabItemForClickedThumbMenu()
        {
            StackPanel main = new();
            MenuItem item;
            item = new() { Header = "画像でコピー" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.CopyImageClickedThumb(); };
            //item = new() { Header = "画像で複製" }; main.Children.Add(item);
            //item.Click += (s, o) => { };
            item = new() { Header = "画像で保存" }; main.Children.Add(item);
            item.Click += (s, e) => { SaveImageClickedThumb(); };
            //item = new() { Header = "Dataで複製" }; main.Children.Add(item);
            //item.Click += (s, e) => {  };
            item = new() { Header = "Dataで保存" }; main.Children.Add(item);
            item.Click += (s, e) => { SaveDataForClickedThumb(); };
            //Clickedに削除はいらない、要素数が2個のグループのときに削除すると
            //要素数1個のグループになってしまう
            //item = new() { Header = "削除" }; main.Children.Add(item);
            //item.Click += (s, e) =>
            //{
            //    if (MyRoot.ClickedThumb is TThumb clicked && clicked.TTParent is TTGroup group)
            //    {
            //        MyRoot.RemoveThumb(clicked, group);
            //    }
            //};

            //main.Children.Add(MakeMenuEditStart());//編集開始メニュー
            item = new() { Header = "編集開始" };
            item.Click += (s, e) =>
            {
                if (MyRoot.ClickedThumb is TTGeometricShape)
                {
                    ShapeEditStart();
                    MyRoot.ChangeActiveGroupFromClickedThumb();
                }
                else if (MyRoot.ClickedThumb is TTTextBox box)
                {
                    box.IsEdit = true;
                    MyRoot.ChangeActiveGroupFromClickedThumb();
                }
            };
            main.Children.Add(item);

            //Z移動
            main.Children.Add(MakeMenuItemZMove());
            item = new() { Header = "ここまでIN" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.ChangeActiveGroupInsideClickedParent(); };
            item = new() { Header = "RootまでOUT" }; main.Children.Add(item);
            item.Click += (s, e) => { MyRoot.ChangeActiveGroupToRoot(); };

            Rectangle rect = new() { Width = 100, Height = 100 };
            VisualBrush vb = new() { Stretch = Stretch.Uniform };
            BindingOperations.SetBinding(vb, VisualBrush.VisualProperty, new Binding(nameof(MyRoot.ClickedThumb)) { Source = MyRoot });
            rect.Fill = vb;
            StackPanel header = new();
            header.Children.Add(new TextBlock() { Text = "Clicked" });
            header.Children.Add(rect);
            TabItem ti = new() { Header = header };
            ti.Content = main;
            return ti;
        }
        #endregion 右クリックメニュー

        #region アプリの設定保存
        private void SaveAppData<T>(string path, T data)
        {
            XmlWriterSettings settings = new()
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                NewLineOnAttributes = false,
                ConformanceLevel = ConformanceLevel.Fragment
            };
            XmlWriter writer;
            DataContractSerializer serializer = new(typeof(T));
            using (writer = XmlWriter.Create(path, settings))
            {
                try { serializer.WriteObject(writer, data); }
                catch (Exception ex) { throw new ArgumentException(ex.Message); }
            }
        }

        #endregion アプリの設定保存

        #region フォント系

        private SortedDictionary<string, FontWeight> MakeFontWeightDictionary()
        {
            System.Reflection.PropertyInfo[] infos = typeof(FontWeights).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Dictionary<string, FontWeight> tempDict = new();
            foreach (var item in infos)
            {
                if (item.GetValue(null) is not FontWeight value)
                {
                    continue;
                }
                tempDict.Add(item.Name, value);
            }
            SortedDictionary<string, FontWeight> sorted = new(tempDict);
            return sorted;
        }

        private SortedDictionary<string, FontStyle> MakeFontStylesDictionary()
        {
            System.Reflection.PropertyInfo[] infos = typeof(FontStyles).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Dictionary<string, FontStyle> tempDict = new();
            foreach (var item in infos)
            {
                if (item.GetValue(null) is not FontStyle value)
                {
                    continue;
                }
                tempDict.Add(item.Name, value);
            }
            SortedDictionary<string, FontStyle> sorted = new(tempDict);
            return sorted;
        }
        private SortedDictionary<string, FontStretch> MakeFontStretchDictionary()
        {
            System.Reflection.PropertyInfo[] stretchInfos = typeof(FontStretches).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Dictionary<string, FontStretch> kv = new();
            foreach (var item in stretchInfos)
            {
                if (item.GetValue(null) is not FontStretch fs)
                {
                    continue;
                }
                kv.Add(item.Name, fs);

                //tempDict.Add(item.Name, item);
            }
            SortedDictionary<string, FontStretch> sorted = new(kv);
            return sorted;
        }
        //WPF、インストールされているフォント一覧取得、Fonts.SystemFontFamiliesそのままでは不十分だった - 午後わてんのブログ
        //        https://gogowaten.hatenablog.com/entry/2021/12/09/125022

        /// <summary>
        /// SystemFontFamiliesから日本語フォント名で並べ替えたフォント一覧を返す、1ファイルに別名のフォントがある場合も取得
        /// </summary>
        /// <returns></returns>
        private SortedDictionary<string, FontFamily> GetFontFamilies()
        {
            //今のPCで使っている言語(日本語)のCulture取得
            //var language =
            // System.Windows.Markup.XmlLanguage.GetLanguage(
            // CultureInfo.CurrentCulture.IetfLanguageTag);
            CultureInfo culture = CultureInfo.CurrentCulture;//日本
            CultureInfo cultureUS = new("en-US");//英語？米国？

            List<string> uName = new();//フォント名の重複判定に使う
            Dictionary<string, FontFamily> tempDictionary = new();
            foreach (var family in Fonts.SystemFontFamilies)
            {
                var typefaces = family.GetTypefaces();
                foreach (var typeface in typefaces)
                {
                    _ = typeface.TryGetGlyphTypeface(out GlyphTypeface gType);
                    if (gType != null)
                    {
                        //フォント名取得はFamilyNamesではなく、Win32FamilyNamesを使う
                        //FamilyNamesだと違うフォントなのに同じフォント名で取得されるものがあるので
                        //Win32FamilyNamesを使う
                        //日本語名がなければ英語名
                        string fontName = gType.Win32FamilyNames[culture] ?? gType.Win32FamilyNames[cultureUS];
                        //string fontName = gType.FamilyNames[culture] ?? gType.FamilyNames[cultureUS];

                        //フォント名で重複判定
                        var uri = gType.FontUri;
                        if (uName.Contains(fontName) == false)
                        {
                            uName.Add(fontName);
                            tempDictionary.Add(fontName, new(uri, fontName));
                        }
                    }
                }
            }
            SortedDictionary<string, FontFamily> fontDictionary = new(tempDictionary);
            return fontDictionary;
        }
        #endregion フォント系

        #region アプリ情報

        /// <summary>
        /// アプリのバージョン取得、できなかかったときはstring.Emptyを返す
        /// </summary>
        /// <returns></returns>
        private static string GetAppVersion()
        {
            //実行ファイルのバージョン取得
            string[] cl = Environment.GetCommandLineArgs();

            //System.Diagnostics.FileVersionInfo
            if (FileVersionInfo.GetVersionInfo(cl[0]).FileVersion is string ver)
            {
                return ver;
            }
            else { return string.Empty; }

        }

        /// <summary>
        /// 保存ディレクトリ取得、未指定ならマイドキュメントにする。存在しない場合はstring.Emptyを返す
        /// </summary>
        /// <returns></returns>
        private string GetSaveDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            if (Directory.Exists(directory) == false)
            {
                MessageBox.Show($"指定されている保存場所は存在しないので保存できない", "注意");
                return string.Empty;
            }
            return directory;
        }

        #endregion アプリ情報        

        #region Data系

        /// <summary>
        /// DataTypeの変換、RootDataだった場合GroupDataに変換する
        /// ディープコピーしてTypeだけGroupに書き換える、Datasもディープコピーする
        /// 変換部分が怪しい、項目が増えた場合はここも増やす必要があるのでバグ発生源になる？
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Data? ConvertDataRootToGroup(Data? data)
        {
            if (data == null) return null;
            if (data.Type == TType.Group) return data;
            if (data.Type != TType.Root) return null;

            if (data.DeepCopy() is Data groupData)
            {
                groupData.Type = TType.Group;
                return groupData;
            }
            return null;
        }
        #endregion Data系


        #region ファイルを開く
        #region アプリの設定ファイルを開く、デシリアライズ

        /// <summary>
        /// 設定ファイルをAppDataにデシリアライズ
        /// </summary>
        /// <param name="path">設定ファイルのフルパス</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private T? LoadAppData<T>(string path)
        {
            if (path == "") return default;
            DataContractSerializer serializer = new(typeof(T));
            try
            {
                using XmlReader reader = XmlReader.Create(path);
                if (serializer.ReadObject(reader) is T t)
                {
                    return t;
                }
                else return default;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

        }


        #endregion ファイルを開く、デシリアライズ

        #region 対応ファイルを開く

        /// <summary>
        /// ダイアログボックスから対応ファイルを開く、開けなかったファイルはメッセージボックスで表示
        /// </summary>
        private void OpenFilesFromDialogBox()
        {
            OpenFileDialog dialog = new();
            dialog.Multiselect = true;
            dialog.Filter = "対応ファイル | *.bmp; *.jpg; *.png; *.gif; *.tiff; *.ps3; | すべて | *.* ";
            if (dialog.ShowDialog() == true)
            {
                string[] paths = dialog.FileNames;
                OpenFiles(paths);
            }
        }

        /// <summary>
        /// ファイルパスリストからThumbを追加、対応外ファイルはメッセージボックスに表示
        /// </summary>
        /// <param name="paths">フルパスの配列</param>
        private void OpenFiles(string[] paths)
        {
            List<string> errorList = new();
            Array.Sort(paths);
            if (MyAppData.IsDecendingSortFileName) { Array.Reverse(paths); }
            foreach (var path in paths)
            {
                if (GetDataFromRelationFile(path) is Data data)
                {
                    MyRoot.AddThumbDataToActiveGroup2(data, MyAppData.IsThumbAddUnder);
                }
                else
                {
                    errorList.Add(path);
                }
            }
            //開けなかったファイルリストを表示
            ShowMessageBoxStringList(errorList);
        }


        /// <summary>
        /// ファイルパスからDataを返す、Dataファイルならデシリアライズ、それ以外は画像として開く、エラー時はnullを返す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Data? GetDataFromRelationFile(string path)
        {
            //拡張子で判定
            var ext = System.IO.Path.GetExtension(path);
            Data? data = null;
            if (ext == EXT_DATA)
            {
                //Dataファイルならデシリアライズ
                data = LoadData3(path);
            }
            else if (GetBitmapFromFilePath(path) is BitmapSource bmp)
            {
                //Dataファイル以外は画像ファイルとして開いてData作成
                data = new(TType.Image) { BitmapSource = bmp };
            }
            return data;
        }

        /// <summary>
        /// Data(Zip)ファイルからData取得、できなかったときはnull
        /// </summary>
        /// <param name="path">Dataファイルのパス</param>
        /// <returns></returns>
        private Data? LoadData3(string path)
        {
            if (!File.Exists(path)) { return null; }
            using FileStream stream = File.OpenRead(path);
            using ZipArchive archive = new(stream, ZipArchiveMode.Read);
            ZipArchiveEntry? entry = archive.GetEntry(XML_FILE_NAME);
            if (entry == null) return null;
            using Stream entryStream = entry.Open();
            DataContractSerializer serializer = new(typeof(Data));
            using XmlReader reader = XmlReader.Create(entryStream);
            Data? data = (Data?)serializer.ReadObject(reader);
            if (data == null) return null;

            SubSetImageSource(data, archive);
            SubLoop(data, archive);
            //Guidの更新、重要
            data.Guid = System.Guid.NewGuid().ToString();
            return data;

            //Dataに画像があれば取得
            void SubSetImageSource(Data data, ZipArchive archive)
            {
                //Guidに一致する画像ファイルを取得
                ZipArchiveEntry? imageEntry = archive.GetEntry(data.Guid + ".png");
                if (imageEntry == null) return;

                using Stream imageStream = imageEntry.Open();
                PngBitmapDecoder decoder =
                    new(imageStream,
                    BitmapCreateOptions.None,
                    BitmapCacheOption.Default);
                //画像の指定
                data.BitmapSource = decoder.Frames[0];
            }

            //子要素が画像タイプだった場合とグループだった場合
            void SubLoop(Data data, ZipArchive archive)
            {
                foreach (Data item in data.Datas)
                {
                    //DataのTypeがImage型ならzipから画像を取り出して設定
                    if (item.Type == TType.Image)
                    {
                        SubSetImageSource(item, archive);
                    }
                    //DataのTypeがGroupなら子要素も取り出す
                    else if (item.Type == TType.Group)
                    {
                        SubLoop(item, archive);
                    }
                    //Guidの更新
                    item.Guid = Guid.NewGuid().ToString();
                }
            }
        }
        /// <summary>
        /// 画像ファイルとして開いて返す、エラーの場合はnullを返す
        /// dpiは96に変換する、このときのピクセルフォーマットはbgra32
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private BitmapSource? GetBitmapFromFilePath(string path)
        {
            using FileStream stream = File.OpenRead(path);
            BitmapSource bmp;
            try
            {
                bmp = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return ConverterBitmapDpi96AndPixFormatBgra32(bmp);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion 対応ファイルを開く

        #endregion ファイルを開く

        #region その他関数

        /// <summary>
        /// 名前をつけて保存時のファイルパス取得
        /// </summary>
        /// <param name="extFilter">拡張子フィルター</param>
        /// <returns></returns>
        private string? GetSaveDataFilePath(string extFilter)
        {
            SaveFileDialog dialog = new();
            dialog.Filter = extFilter;
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 文字列リストをメッセージボックスに表示
        /// </summary>
        /// <param name="list"></param>
        private void ShowMessageBoxStringList(List<string> list)
        {
            if (list.Count != 0)
            {
                string ms = "";
                foreach (var name in list)
                {
                    ms += $"{name}\n";
                }
                MessageBox.Show(ms, "開くことができなかったファイル一覧",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// BitmapSourceのdpiを96に変換する、ピクセルフォーマットもBgra32に変換する
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private BitmapSource ConverterBitmapDpi96AndPixFormatBgra32(BitmapSource bmp)
        {
            //png画像はdpi95.98とかの場合もあるけど、
            //これは問題ないので変換しない
            if (bmp.DpiX < 95.0 || 96.0 < bmp.DpiX)
            {
                FormatConvertedBitmap fc = new(bmp, PixelFormats.Bgra32, null, 0.0);
                int w = fc.PixelWidth;
                int h = fc.PixelHeight;
                int stride = w * 4;
                byte[] pixels = new byte[stride * h];
                fc.CopyPixels(pixels, stride, 0);
                bmp = BitmapSource.Create(w, h, 96.0, 96.0, fc.Format, null, pixels, stride);
            }
            return bmp;
        }

        /// <summary>
        /// ファイル名に使える文字列ならtrueを返す
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckFileNameValidated(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            return name.IndexOfAny(invalid) < 0;
        }


        /// <summary>
        /// 重複回避ファイルパス作成、重複しなくなるまでファイル名末尾に_を追加して返す
        /// </summary>
        /// <returns></returns>
        private string MakeFilePathAvoidDuplicate(string path)
        {
            string extension = System.IO.Path.GetExtension(path);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            string? directory = System.IO.Path.GetDirectoryName(path);
            if (directory != null)
            {
                while (File.Exists(path))
                {
                    name += "_";
                    path = System.IO.Path.Combine(directory, name) + extension;
                }
                return path;
            }
            else return string.Empty;
        }

        #endregion その他関数

        #region 未使用、画像保存


        //private bool SaveBitmap(BitmapSource bitmap, string fullPath)
        //{
        //    bool isSavedDone = false;
        //    bool isSuccess = false;
        //    //ファイルに保存
        //    {
        //        bool result = SaveBitmapSub(bitmap, fullPath);

        //        isSavedDone = result;
        //        isSuccess = result;
        //    }

        //    //クリップボードにコピー、BMPとPNG形式の両方をセットする
        //    //BMPはアルファ値が255になる、PNGはアルファ値保持するけどそれが活かせるかは貼り付けるアプリに依る
        //    if (MyAppConfig.SaveBehaviorType is SaveBehaviorType.Copy or
        //        SaveBehaviorType.SaveAndCopy)
        //    {
        //        try
        //        {
        //            //BMP
        //            DataObject tData = new();
        //            tData.SetData(typeof(BitmapSource), bitmap);
        //            //PNG
        //            PngBitmapEncoder enc = new();
        //            enc.Frames.Add(BitmapFrame.Create(bitmap));
        //            using var ms = new System.IO.MemoryStream();
        //            enc.Save(ms);
        //            tData.SetData("PNG", ms);

        //            Clipboard.SetDataObject(tData, true);//true必須
        //            isSuccess = true;

        //            ////コピーだけのときは連番に加算
        //            //if (MyAppConfig.SaveBehaviorType == SaveBehaviorType.Copy &&
        //            //    MyAppConfig.IsFileNameSerial)
        //            //{
        //            //    AddIncrementToSerial();
        //            //}
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"クリップボードにコピーできなかった\n" +
        //                $"理由は不明、まれに起こる\n\n" +
        //                $"エラーメッセージ\n" +
        //                $"{ex.Message}", "エラー発生");
        //        }
        //    }


        //    //プレビューウィンドウに表示
        //    //DisplayPreviewWindow(bitmap, fullPath, isSavedDone);

        //    return isSuccess;
        //}


        ///// <summary>
        ///// プレビューウィンドウに表示
        ///// </summary>
        ///// <param name="bitmap"></param>
        ///// <param name="fullPath"></param>
        ///// <param name="isSavedDone">保存済みならtrue</param>
        //private bool DisplayPreviewWindow(BitmapSource bitmap, string fullPath, bool isSavedDone)
        //{
        //    if (MyPreviweWindow != null && bitmap != null)
        //    {
        //        MyPreviewItems.Add(new PreviewItem(
        //            System.IO.Path.GetFileNameWithoutExtension(fullPath), bitmap, fullPath, isSavedDone));
        //        ListBox lb = MyPreviweWindow.MyListBox;
        //        lb.SelectedIndex = MyPreviewItems.Count - 1;
        //        lb.ScrollIntoView(lb.SelectedItem);
        //        if (isSavedDone == false)
        //        {
        //            AddIncrementToSerial();
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 複数Rect範囲を組み合わせた形にbitmapを切り抜く
        /// </summary>
        /// <param name="source">元の画像</param>
        /// <param name="rectList">Rectのコレクション</param>
        /// <returns></returns>
        private BitmapSource CroppedBitmapFromRects(BitmapSource source, List<Rect> rectList)
        {
            List<Int32Rect> re = new();
            foreach (var item in rectList)
            {
                re.Add(new Int32Rect((int)item.X, (int)item.Y, (int)item.Width, (int)item.Height));
            }

            return CroppedBitmapFromRects(source, re);
        }
        private BitmapSource CroppedBitmapFromRects(BitmapSource source, List<Int32Rect> rectList)
        {
            var dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                //それぞれのRect範囲で切り抜いた画像を描画していく
                foreach (var rect in rectList)
                {
                    dc.DrawImage(new CroppedBitmap(source, rect), new Rect(rect.X, rect.Y, rect.Width, rect.Height));
                }
            }

            //描画位置調整
            dv.Offset = new Vector(-dv.ContentBounds.X, -dv.ContentBounds.Y);

            //bitmap作成、縦横サイズは切り抜き後の画像全体がピッタリ収まるサイズにする
            //PixelFormatsはPbgra32で決め打ち、これ以外だとエラーになるかも、
            //画像を読み込んだbitmapImageのPixelFormats.Bgr32では、なぜかエラーになった
            var bmp = new RenderTargetBitmap(
                (int)Math.Ceiling(dv.ContentBounds.Width),
                (int)Math.Ceiling(dv.ContentBounds.Height),
                96, 96, PixelFormats.Pbgra32);

            bmp.Render(dv);
            //bmp.Freeze();
            return bmp;
        }

        //RectからInt32Rect作成、小数点以下切り捨て編
        //Rectの数値は整数のはずだから、これでいいはず
        private Int32Rect RectToIntRectWith切り捨て(Rect re)
        {
            return new Int32Rect((int)re.X, (int)re.Y, (int)re.Width, (int)re.Height);
        }

        ////画像にマウスカーソルを描画してからCropp
        //private BitmapSource MakeBitmapForSave(BitmapSource source, List<Rect> reList)
        //{
        //    BitmapSource bitmap;
        //    if (MyAppConfig.IsDrawCursor == true)
        //    {
        //        bitmap = DrawCursor(source);
        //    }
        //    else { bitmap = source; }

        //    return CroppedBitmapFromRects(bitmap, reList);
        //}
        //private BitmapSource MakeBitmapForSave(BitmapSource source, List<Int32Rect> reList)
        //{
        //    List<Rect> re = new();
        //    try
        //    {
        //        foreach (var itemData in reList)
        //        {
        //            re.Add(new Rect(itemData.X, itemData.Y, itemData.Width, itemData.Height));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"{ex}");
        //    }
        //    return MakeBitmapForSave(source, re);
        //}

        //private BitmapSource DrawCursor(BitmapSource source)
        //{
        //    BitmapSource bitmap;
        //    if (IsMaskUse)
        //    {
        //        bitmap = DrawCursorOnBitmapWithMask(source);
        //    }
        //    else
        //    {
        //        bitmap = DrawCursorOnBitmap(source);
        //    }
        //    return bitmap;
        //}

        //internal bool SaveBitmapSub(BitmapSource bitmap, string fullPath)
        //{
        //    //CroppedBitmapで切り抜いた画像でBitmapFrame作成して保存
        //    BitmapEncoder encoder = GetEncoder();
        //    //メタデータ作成、アプリ名記入
        //    BitmapMetadata meta = MakeMetadata();
        //    if (MakeMetadata() is BitmapMetadata meta)
        //    {
        //        encoder.Frames.Add(BitmapFrame.Create(bitmap, null, meta, null));
        //        //重複回避ファイルパス取得
        //        fullPath = MakeFilePathAvoidDuplicate(fullPath);
        //        try
        //        {
        //            using FileStream value = new(fullPath, FileMode.Create, FileAccess.Write);
        //            encoder.Save(value);
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"保存できなかった\n{ex}", "保存できなかった");
        //            return false;
        //        }
        //    }
        //    else { return false; }
        //}
        //internal bool SaveBitmapSub(BitmapSource bitmap, string fullPath, BitmapEncoder encoder)
        //{
        //    if (MakeMetadata() is BitmapMetadata meta)
        //    {
        //        encoder.Frames.Add(BitmapFrame.Create(bitmap, null, meta, null));
        //        //重複回避ファイルパス取得
        //        //fullPath = MakeFilePathAvoidDuplicate(fullPath);
        //        try
        //        {
        //            using FileStream fs = new(fullPath, FileMode.Create, FileAccess.Write);
        //            encoder.Save(fs);
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"保存できなかった\n{ex}", "保存できなかった");
        //            return false;
        //        }
        //    }
        //    else { return false; }
        //}


        ///// <summary>
        ///// 保存ファイルのフルパスを取得、無効なパスの場合はstring.Emptyを返す
        ///// ファイル名の重複を回避、拡張子の前に"_"を付け足す
        ///// </summary>
        ///// <returns></returns>
        //private string GetSaveFileFullPath()
        //{
        //    //ファイル名取得、無効なファイル名なら中止
        //    string fileName = MakeFileName();
        //    if (CheckFileNameValidated(fileName) == false)
        //    {
        //        return string.Empty;
        //    }

        //    //保存ディレクトリ取得、存在しない場合は中止
        //    string directory = GetSaveDirectory("E:\\Pixtack3rdTest");
        //    if (directory == string.Empty)
        //    {
        //        return string.Empty;
        //    }

        //    string dir = System.IO.Path.Combine(directory, fileName);
        //    string extension = "." + MyAppConfig.ImageType.ToString();
        //    string fullPaht = dir + extension;
        //    //重複回避パス取得
        //    return MakeFilePathAvoidDuplicate(fullPaht);
        //}


        ////メタデータ作成
        //private BitmapMetadata? MakeMetadata()
        //{
        //    BitmapMetadata? data = null;
        //    string software = APP_NAME + "_" + AppVersion;
        //    switch (ComboBoxSaveFileType.SelectedValue)
        //    {
        //        case ImageType.png:
        //            data = new BitmapMetadata("png");
        //            data.SetQuery("/tEXt/Software", software);
        //            break;
        //        case ImageType.jpg:
        //            data = new BitmapMetadata("jpg");
        //            data.SetQuery("/app1/ifd/{ushort=305}", software);
        //            break;
        //        case ImageType.bmp:

        //            break;
        //        case ImageType.gif:
        //            data = new BitmapMetadata("Gif");
        //            //tData.SetQuery("/xmp/xmp:CreatorTool", "Pixtrim2");
        //            //tData.SetQuery("/XMP/XMP:CreatorTool", "Pixtrim2");
        //            data.SetQuery("/XMP/XMP:CreatorTool", software);
        //            break;
        //        case ImageType.tiff:
        //            data = new BitmapMetadata("tiff")
        //            {
        //                ApplicationName = software
        //            };
        //            break;
        //        default:
        //            break;
        //    }

        //    return data;
        //}

        private (BitmapEncoder? encoder, BitmapMetadata? meta) GetEncoderWithMetaData(int filterIndex)
        {
            BitmapMetadata? meta = null;
            string software = APP_NAME + "_" + AppVersion;

            switch (filterIndex)
            {
                case 1:
                    meta = new BitmapMetadata("png");
                    meta.SetQuery("/tEXt/Software", software);
                    return (new PngBitmapEncoder(), meta);
                case 2:
                    meta = new BitmapMetadata("jpg");
                    meta.SetQuery("/app1/ifd/{ushort=305}", software);
                    var jpeg = new JpegBitmapEncoder
                    {
                        QualityLevel = MyAppData.JpegQuality,
                        //QualityLevel = MyAppConfig.JpegQuality
                    };
                    return (jpeg, meta);
                case 3:
                    return (new BmpBitmapEncoder(), meta);
                case 4:
                    meta = new BitmapMetadata("Gif");
                    //tData.SetQuery("/xmp/xmp:CreatorTool", "Pixtrim2");
                    //tData.SetQuery("/XMP/XMP:CreatorTool", "Pixtrim2");
                    meta.SetQuery("/XMP/XMP:CreatorTool", software);

                    return (new GifBitmapEncoder(), meta);
                case 5:
                    meta = new BitmapMetadata("tiff")
                    {
                        ApplicationName = software
                    };
                    return (new TiffBitmapEncoder(), meta);
                default:
                    throw new Exception();
            }

        }

        //今の日時をStringで作成
        private string MakeStringNowTime()
        {
            DateTime dt = DateTime.Now;
            //string str = dt.ToString("yyyyMMdd");            
            //string str = dt.ToString("yyyyMMdd" + "_" + "HHmmssfff");
            string str = dt.ToString(DATE_TIME_STRING_FORMAT);
            //string str = dt.ToString("yyyyMMdd" + "_" + "HH" + "_" + "mm" + "_" + "ss" + "_" + "fff");
            return str;
        }


        //private string MakeFileName()
        //{
        //    double count = 0.0;
        //    string fileName = "";
        //    DateTime dateTime = DateTime.Now;
        //    bool isOverDate = false, isOverSerial = false;
        //    if (MyAppConfig.IsFileNameDate == false && MyAppConfig.IsFileNameSerial == false)
        //    {
        //        MyCheckBoxFileNameData.IsChecked = true;
        //    }
        //    if (MyAppConfig.IsFileNameDate == false) isOverDate = true;
        //    if (MyAppConfig.IsFileNameSerial == false) isOverSerial = true;
        //    MyOrder();

        //    if (MyAppConfig.IsFileNameText1) MyAddText(MyComboBoxFileNameText1);
        //    count += 1.5; MyOrder();

        //    if (MyAppConfig.IsFileNameText2) MyAddText(MyComboBoxFileNameText2);
        //    count++; MyOrder();

        //    if (MyAppConfig.IsFileNameText3) MyAddText(MyComboBoxFileNameText3);
        //    count++; MyOrder();

        //    if (MyAppConfig.IsFileNameText4) MyAddText(MyComboBoxFileNameText4);
        //    count += 1.5; MyOrder();

        //    if (string.IsNullOrWhiteSpace(fileName)) fileName = MakeStringNowTime();
        //    fileName = fileName.TrimStart();
        //    fileName = fileName.TrimEnd();
        //    return fileName;


        //    void MyOrder()
        //    {
        //        //日時
        //        if (isOverDate == false && MyAppConfig.FileNameDateOrder == count)
        //        {
        //            var format = MyComboBoxFileNameDateFormat.Text;
        //            if (string.IsNullOrEmpty(format))
        //            {
        //                fileName += MakeStringNowTime();
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    fileName += dateTime.ToString(MyComboBoxFileNameDateFormat.Text);
        //                    isOverDate = true;
        //                }
        //                catch (Exception)
        //                {

        //                }

        //            }
        //        }

        //        //連番
        //        if (isOverSerial == false && MyAppConfig.FileNameSerialOrder == count)
        //        {
        //            //fileName += MyNumericUpDownFileNameSerial.MyValue.ToString(MySerialFormat());
        //            fileName += MyAppConfig.FileNameSerial.ToString(MySerialFormat());

        //            isOverSerial = true;
        //        }
        //    }

        //    string MyAddText(ComboBox comboBox)
        //    {
        //        return fileName += comboBox.Text;
        //    }
        //    string MySerialFormat()
        //    {
        //        string str = "";
        //        for (int i = 0; i < MyAppConfig.FileNameSerialDigit; i++)
        //        {
        //            str += "0";
        //        }
        //        return str;
        //    }

        //}

        //連番に増加値を加算
        //private void AddIncrementToSerial()
        //{
        //    MyNumericUpDownFileNameSerial.MyValue += MyNumericUpDownFileNameSerialIncreace.MyValue;
        //}

        //private bool SaveBitmapFromThumb(TThumb? anchor)
        //{
        //    if (anchor == null) return false;
        //    if (MyRoot.GetBitmap(anchor) is BitmapSource bitmap)
        //    {

        //        Microsoft.Win32.SaveFileDialog dialog = new()
        //        {
        //            Filter = "*.png|*.png|*.jpg|*.jpg;*.jpeg|*.bmp|*.bmp|*.gif|*.gif|*.tiff|*.tiff",
        //            AddExtension = true,
        //        };
        //        if (dialog.ShowDialog() == true)
        //        {
        //            BitmapEncoder encoder = GetEncoder(dialog.FilterIndex);
        //            if (SaveBitmapSub(bitmap, dialog.FileName, encoder))
        //            {
        //                return true;
        //            }
        //            else return false;
        //        }
        //    }
        //    return false;
        //}
        //private bool SaveBitmap(BitmapSource bitmap)
        //{
        //    Microsoft.Win32.SaveFileDialog dialog = new()
        //    {
        //        Filter = "*.png|*.png|*.jpg|*.jpg;*.jpeg|*.bmp|*.bmp|*.gif|*.gif|*.tiff|*.tiff",
        //        AddExtension = true,
        //    };
        //    if (dialog.ShowDialog() == true)
        //    {
        //        BitmapEncoder encoder = GetEncoder(dialog.FilterIndex);
        //        if (SaveBitmapSub(bitmap, dialog.FileName, encoder))
        //        {
        //            return true;
        //        }
        //        else return false;
        //    }
        //    return true;
        //}


        #endregion 未使用、画像保存

        #region 画像保存2

        public bool SaveBitmap2(BitmapSource bitmap)
        {
            Microsoft.Win32.SaveFileDialog dialog = new()
            {
                Filter = "*.png|*.png|*.jpg|*.jpg;*.jpeg|*.bmp|*.bmp|*.gif|*.gif|*.tiff|*.tiff",
                AddExtension = true,
            };
            if (dialog.ShowDialog() == true)
            {
                (BitmapEncoder? encoder, BitmapMetadata? meta) = GetEncoderWithMetaData(dialog.FilterIndex);
                if (encoder is null) { return false; }
                encoder.Frames.Add(BitmapFrame.Create(bitmap, null, meta, null));
                try
                {
                    using FileStream stream = new(dialog.FileName, FileMode.Create, FileAccess.Write);
                    encoder.Save(stream);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        #endregion 画像保存2

        #region ファイルドロップで開く


        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //ファイル名一覧取得
                string[] paths = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToArray();
                OpenFiles(paths);
            }

        }

        #endregion ファイルドロップで開く


        #region データ保存、アプリの設定保存


        /// <summary>
        /// アプリの設定を上書き保存
        /// </summary>
        private void SaveAppDataOverride()
        {
            string path = System.IO.Path.Combine(AppDirectory, APPDATA_FILE_NAME);
            SaveAppData<AppData>(path, MyAppData);
        }

        ///// <summary>
        ///// アプリの設定を読み込み＋設定を反映(Binding)
        ///// </summary>
        //private void LoadAppDataAndSetting()
        //{
        //    string path = System.IO.Path.Combine(AppDirectory, APPDATA_FILE_NAME);
        //    AppData data = LoadAppData<AppData>(path);
        //    MyAppData = data;
        //    DataContext = MyAppData;
        //    //SetMyBindings();
        //}


        ///// <summary>
        ///// Rootデータをアプリの設定とともにファイルに保存
        ///// </summary>
        ///// <param name="filePath">拡張子も含めたフルパス</param>
        ///// <param name="data"></param>
        ///// <param name="isWithAppConfigSave">アプリの設定も保存するときはtrue</param>
        //private void SaveRootDataWithConfig(string filePath, Data data, bool isWithAppConfigSave)
        //{
        //    try
        //    {
        //        using FileStream zipStream = File.Create(filePath);
        //        using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);
        //        XmlWriterSettings settings = new()
        //        {
        //            Indent = true,
        //            Encoding = Encoding.UTF8,
        //            NewLineOnAttributes = true,
        //            ConformanceLevel = ConformanceLevel.Fragment,
        //        };
        //        //シリアライズする型は基底クラス型のDataで大丈夫
        //        DataContractSerializer serializer = new(typeof(Data));
        //        //xml形式にシリアライズして、それをzipに詰め込む
        //        ZipArchiveEntry entry = archive.CreateEntry(XML_FILE_NAME);
        //        using (Stream entryStream = entry.Open())
        //        {
        //            using XmlWriter writer = XmlWriter.Create(entryStream, settings);
        //            try { serializer.WriteObject(writer, data); }
        //            catch (Exception ex) { MessageBox.Show(ex.Message); }
        //        }
        //        //アプリの設定保存
        //        if (isWithAppConfigSave)
        //        {
        //            entry = archive.CreateEntry(APP_CONFIG_FILE_NAME);
        //            serializer = new(typeof(AppConfig));
        //            using (Stream entryStream = entry.Open())
        //            {
        //                using XmlWriter writer = XmlWriter.Create(entryStream, settings);
        //                try { serializer.WriteObject(writer, MyAppConfig); }
        //                catch (Exception ex) { MessageBox.Show(ex.Message); }
        //            }
        //        }

        //        //BitmapSourceの保存
        //        SubLoop(archive, data);

        //    }
        //    catch (Exception ex) { MessageBox.Show(ex.Message); }

        //    void SubLoop(ZipArchive archive, Data subData)
        //    {
        //        if (data.BitmapSource != null)
        //        {
        //            Sub(data, archive);
        //        }
        //        //子要素のBitmapSource保存
        //        foreach (Data item in subData.Datas)
        //        {
        //            Sub(item, archive);
        //            if (item.Type == TType.Group) { SubLoop(archive, item); }
        //        }
        //    }

        //    void Sub(Data itemData, ZipArchive archive)
        //    {
        //        //画像があった場合はpng形式にしてzipに詰め込む
        //        if (itemData.BitmapSource is BitmapSource bmp)
        //        {
        //            ZipArchiveEntry entry = archive.CreateEntry(itemData.Guid + ".png");
        //            using Stream entryStream = entry.Open();
        //            PngBitmapEncoder encoder = new();
        //            encoder.Frames.Add(BitmapFrame.Create(bmp));
        //            using MemoryStream memStream = new();
        //            encoder.Save(memStream);
        //            memStream.Position = 0;
        //            memStream.CopyTo(entryStream);
        //        }
        //    }
        //}

        private bool SaveThumbData(TThumb? thumb, string path)
        {
            if (thumb == null) return false;
            Data data = thumb.Data;

            using FileStream zipStream = File.Create(path);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);
            XmlWriterSettings settings = new()
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                NewLineOnAttributes = true,
                ConformanceLevel = ConformanceLevel.Fragment,
            };

            //シリアライズする型は基底クラス型のDataで大丈夫
            DataContractSerializer serializer = new(typeof(Data));
            ZipArchiveEntry entry = archive.CreateEntry(XML_FILE_NAME);
            using (Stream entryStream = entry.Open())
            {
                using XmlWriter writer = XmlWriter.Create(entryStream, settings);
                try
                {
                    serializer.WriteObject(writer, data);
                }
                catch (Exception ex)
                {
                    return false;
                    throw new ArgumentException(ex.Message);
                }
            }

            //BitmapSourceの保存
            SubLoop(archive, data);
            return true;


            void SubLoop(ZipArchive archive, Data subData)
            {
                if (data.BitmapSource != null)
                {
                    Sub(data, archive);
                }
                //子要素のBitmapSource保存
                foreach (Data item in subData.Datas)
                {
                    Sub(item, archive);
                    if (item.Type == TType.Group) { SubLoop(archive, item); }
                }
            }
            void Sub(Data itemData, ZipArchive archive)
            {
                //画像があった場合はpng形式にしてzipに詰め込む
                if (itemData.BitmapSource is BitmapSource bmp)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(itemData.Guid + ".png");
                    using Stream entryStream = entry.Open();
                    PngBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    using MemoryStream memStream = new();
                    encoder.Save(memStream);
                    memStream.Position = 0;
                    memStream.CopyTo(entryStream);
                }
            }
        }

        /// <summary>
        /// ThumbのDataを名前を付けて保存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private bool SaveThumbData(TThumb? thumb)
        {
            if (thumb == null) return false;
            Data data = thumb.Data;
            if (GetSaveDataFilePath(EXT_FILTER_DATA) is string path)
            {
                return SaveThumbData(thumb, path);
            }
            else return false;

        }





        #endregion データ保存、アプリの設定保存

        #region データ読み込み、アプリの設定読み込み



        /// <summary>
        /// 指定した拡張子フィルターでOpenFileDialogを開いてパスを取得
        /// </summary>
        /// <param name="extFilter"></param>
        /// <returns>キャンセルの場合はstring.Empty</returns>
        private string GetLoadFilePathFromFileDialog(string extFilter)
        {
            OpenFileDialog dialog = new();
            dialog.Filter = extFilter;
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            else return string.Empty;
        }



        //複数ファイル
        private void ButtonLoadFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFilesFromDialogBox();
        }

        #endregion データ読み込み、アプリの設定読み込み


        #region 図形編集の開始と終了

        //クリックされたThumb変更時の動作
        //編集解除するのは図形、文字列、グループなど
        private void MyRoot_ClickedThumbChanging(TThumb? oldThumb, TThumb? newThumb)
        {
            if (oldThumb?.Data.Guid == newThumb?.Data.Guid) { return; }

            if (oldThumb is TTGeometricShape oldShape && oldShape.IsEditing)
            {
                ShapeEditEnd(oldShape);
            }
            //別のThumbクリックならTTTextBoxの編集終了
            else if (oldThumb is TTTextBox box) box.IsEdit = false;
        }

        //図形編集開始、Binding
        private void ShapeEditStart()
        {
            if (MyRoot.ClickedThumb is TTGeometricShape geoThumb)
            {
                geoThumb.IsEditing = true;
                geoThumb.MyAnchorVisible = Visibility.Visible;

                MyTabItemShape.DataContext = geoThumb.Data;
                MyNumeArrowHeadAngle.SetBinding(NumericUpDown.MyValueProperty, nameof(Data.HeadAngle));
                MyNumeStrokeThickness.SetBinding(NumericUpDown.MyValueProperty, nameof(Data.StrokeThickness));
                MyComboBoxLineHeadBeginType.SetBinding(ComboBox.SelectedValueProperty, nameof(Data.HeadBeginType));
                MyComboBoxLineHeadEndType.SetBinding(ComboBox.SelectedValueProperty, nameof(Data.HeadEndType));
                MyComboBoxShapeType.SetBinding(ComboBox.SelectedValueProperty, nameof(Data.ShapeType));
                MyChecBoxIsLineClose.SetBinding(CheckBox.IsCheckedProperty, nameof(Data.IsLineClose));
                MyChecBoxIsLineSmoothJoin.SetBinding(CheckBox.IsCheckedProperty, nameof(Data.IsSmoothJoin));

                //アプリ設定に図形Strokeの色をバインド、これは逆方向の方がいい？
                Data geoData = geoThumb.Data;
                BindingOperations.SetBinding(MyAppData, AppData.ShapeStrokeColorAProperty, new Binding(nameof(Data.StrokeA)) { Source = geoData, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(MyAppData, AppData.ShapeStrokeColorRProperty, new Binding(nameof(Data.StrokeR)) { Source = geoData, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(MyAppData, AppData.ShapeStrokeColorGProperty, new Binding(nameof(Data.StrokeG)) { Source = geoData, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(MyAppData, AppData.ShapeStrokeColorBProperty, new Binding(nameof(Data.StrokeB)) { Source = geoData, Mode = BindingMode.TwoWay });
                


            }
        }

        //編集中の図形のBinding解除
        private void ShapeEditEndForClickedThumb()
        {
            if (MyRoot.ClickedThumb is TTGeometricShape shape)
            {
                ShapeEditEnd(shape);
            }
        }
        private void ShapeEditEnd(TTGeometricShape geoThumb)
        {
            geoThumb.IsEditing = false;
            geoThumb.MyAnchorVisible = Visibility.Collapsed;
            MyTabItemShape.DataContext = null;
            MyNumeStrokeThickness.MyValue = (decimal)geoThumb.StrokeThickness;
            MyNumeArrowHeadAngle.MyValue = (decimal)geoThumb.ArrowHeadAngle;
            MyComboBoxLineHeadBeginType.SelectedValue = geoThumb.HeadBeginType;
            MyComboBoxLineHeadEndType.SelectedValue = geoThumb.HeadEndType;
            MyComboBoxShapeType.SelectedValue = geoThumb.MyShapeType;
            //BindingOperations.ClearAllBindings(MyNumeShapeBackB);
            SolidColorBrush brush = (SolidColorBrush)geoThumb.StrokeBrush;
            //MyNumeShapeBackA.MyValue = (decimal)brush.Color.A;
            //MyNumeShapeBackR.MyValue = (decimal)brush.Color.R;
            //MyNumeShapeBackG.MyValue = (decimal)brush.Color.G;
            //MyNumeShapeBackB.MyValue = (decimal)brush.Color.B;
        }

        #endregion 図形編集の開始と終了





        #region ボタンクリックイベント

        #region 範囲選択系


        //範囲選択用Thumbの表示
        private void ButtonAddTTRange_Click(object sender, RoutedEventArgs e)
        {
            //MyRoot.TTRangeVisible();
            if (MyAreaThumb.Visibility == Visibility.Visible)
            {
                MyAreaThumb.Visibility = Visibility.Collapsed;
            }
            else MyAreaThumb.Visibility = Visibility.Visible;
            MyScrollViewer.ScrollToHorizontalOffset(MyAreaThumb.X);
            MyScrollViewer.ScrollToVerticalOffset(MyAreaThumb.Y);
            //MyScrollViewer.ScrollToHorizontalOffset(MyRoot.MyTTRange.TTLeft);
            //MyScrollViewer.ScrollToVerticalOffset(MyRoot.MyTTRange.TTTop);

        }

        private void ButtonSetNumeSizeClickedThumb_Click(object sender, RoutedEventArgs e)
        {
            SetRangeSize(MyRoot.ClickedThumb);
        }

        private void ButtonSetNumeSizeActiveThumb_Click(object sender, RoutedEventArgs e)
        {
            SetRangeSize(MyRoot.ActiveThumb);
        }
        private void SetRangeSize(TThumb? thumb)
        {
            if (thumb == null) return;
            //切り上げで取得
            MyNumeRangeHeight.MyValue = (int)(Math.Ceiling(thumb.ActualHeight));
            MyNumeRangeWidth.MyValue = (int)(Math.Ceiling(thumb.ActualWidth));
        }

        private void ButtonSetNumeSizeActiveGroup_Click(object sender, RoutedEventArgs e)
        {
            SetRangeSize(MyRoot.ActiveGroup);
        }

        #endregion 範囲選択系


        #region 上書き保存と読み込み

        //上書き保存
        private void ButtonSaveDefault_Click(object sender, RoutedEventArgs e)
        {
            //SaveRootDataWithConfig(CurrentDataFilePath, MyRoot.Data, true);
            SaveThumbData(MyRoot, CurrentDataFilePath);
        }
        ////上書き保存を読み込み
        //private void ButtonLoadDefault_Click(object sender, RoutedEventArgs e)
        //{
        //    (Data? data, AppConfig? config) = LoadDataFromFile(CurrentFileFullPath);
        //    if (data is not null)
        //    {
        //        MyRoot.SetRootData(data);
        //    }
        //}

        ////前回終了時を読み込み
        //private void ButtonLoadLastEndTime_Click(object sender, RoutedEventArgs e)
        //{
        //    (Data? tData, AppConfig? config) = LoadDataFromFile(AppLastEndTimeDataFilePath);
        //    if (tData is not null)
        //    {
        //        MyRoot.SetRootData(tData);
        //    }
        //    if (config is not null)
        //    {
        //        MyAppConfig = config;
        //        DataContext = MyAppConfig;
        //    }
        //}
        #endregion 上書き保存と読み込み



        #region クリックイベント
        #region 画像ファイルとして保存

        private void ButtonSaveToImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImageRootThumb();
        }
        private void SaveImageRootThumb()
        {
            if (MyRoot.GetBitmapRoot() is BitmapSource bmp) { SaveBitmap2(bmp); };
        }

        private void ButtonSaveToImageActive_Click(object sender, RoutedEventArgs e)
        {
            //ActiveThumb
            SaveImageActiveThumb();
        }
        private void SaveImageActiveThumb()
        {
            if (MyRoot.GetBitmapActiveThumb() is BitmapSource bmp)
            {
                SaveBitmap2(bmp);
            }
        }

        private void ButtonSaveToImageClicked_Click(object sender, RoutedEventArgs e)
        {   //ClickedThumb
            SaveImageClickedThumb();
        }
        private void SaveImageClickedThumb()
        {
            if (MyRoot.GetBitmapClickedThumb() is BitmapSource bmp) { SaveBitmap2(bmp); };
        }
        #endregion 画像ファイルとして保存



        #region その他

        private void ButtonToGroup_Click(object sender, RoutedEventArgs e)
        {
            //グループ化
            MyRoot.AddGroup();
        }

        private void ButtonUnGroup_Click(object sender, RoutedEventArgs e)
        {
            //グループ解除、ActiveThumbが対象
            MyRoot.UnGroup();
        }


        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            //選択Thumbを削除
            MyRoot.RemoveThumb();
        }
        private void ButtonRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            //全削除
            MyRoot.RemoveAll();
        }
        //グリッドの値を指定
        private void ButtonGrid1_Click(object sender, RoutedEventArgs e)
        {
            NumeGrid.MyValue = 1;
        }

        private void ButtonGrid8_Click(object sender, RoutedEventArgs e)
        {
            NumeGrid.MyValue = 8;
        }
        #endregion その他
        #region Data保存
        private void ButtonSaveDataThumb_Click(object sender, RoutedEventArgs e)
        {
            //ActiveThumbのDataを保存
            SaveThumbData(MyRoot.ActiveThumb);
        }


        private void ButtonSaveCickedThumb_Click(object sender, RoutedEventArgs e)
        {
            SaveDataForClickedThumb();
        }
        private void ButtonSaveActiveThumb_Click(object sender, RoutedEventArgs e)
        {
            SaveDataForActiveThumb();
        }

        #endregion Data保存
        #region クリップボード
        private void ButtonAddFromClipboard_Click(object sender, RoutedEventArgs e)
        {//クリップボードから画像追加、"PNG"形式優先で取得
            MyRoot.AddImageThumbFromClipboard();
            //AddImageFromClipboard();
        }

        private void ButtonAddFromClipboardPNG_Click(object sender, RoutedEventArgs e)
        {//クリップボードから画像追加、"PNG"形式で取得
            MyRoot.AddImageThumbFromClipboardPng();
            //AddImageFromClipboard(true, false);
        }
        private void ButtonAddFromClipboardBgr32_Click(object sender, RoutedEventArgs e)
        {//クリップボードから画像追加、"PNG"形式で取得＋強制Bgr32
            MyRoot.AddImageThumbFromClipboardBgr32();
            //AddImageFromClipboard(false, true); ;
        }


        private void ButtonCopyImage_Click(object sender, RoutedEventArgs e)
        {//画像として全体をコピー、クリップボードにセット
            MyRoot.CopyImageRoot();
        }

        private void ButtonCopyImageActiveThumb_Click(object sender, RoutedEventArgs e)
        {
            //画像としてActiveThumbをコピー、クリップボードにセット
            MyRoot.CopyImageActiveThumb();
        }

        private void ButtonCopyImageClicedThumb_Click(object sender, RoutedEventArgs e)
        {
            //画像としてClickedThumbをコピー、クリップボードにセット
            MyRoot.CopyImageClickedThumb();
        }

        #endregion クリップボード
        #region 複製
        private void ButtonDuplicateImage_Click(object sender, RoutedEventArgs e)
        {
            //画像として複製、全体
            if (MyRoot.GetBitmapRoot() is BitmapSource bmp)
            {
                MyRoot.AddThumbDataToActiveGroup2(new Data(TType.Image) { BitmapSource = bmp }, false);
                //MyRoot.AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp }, true);
            }
        }

        //画像として複製、選択Thumb
        private void ButtonDuplicateImageSelectedT_Click(object sender, RoutedEventArgs e)
        {
            DuplicateImageSelectedThumbs();
        }
        private int DuplicateImageSelectedThumbs()
        {
            return MyRoot.DuplicateImageSelectedThumbs();
        }
        //private void ButtonDuplicateImageClickedThumb_Click(object sender, RoutedEventArgs e)
        //{
        //    //画像として複製、ClickedThumb
        //    if (MyRoot.GetBitmapClickedThumb() is BitmapSource bmp)
        //    {
        //        MyRoot.AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp });
        //    }
        //}
        private void ButtonDuplicateData_Click(object sender, RoutedEventArgs e)
        {
            DuplicateDataRoot();
        }
        private bool DuplicateDataRoot()
        {
            //Dataとして複製、全体Root
            if (ConvertDataRootToGroup(MyRoot.Data) is Data data && MyRoot.Thumbs.Count > 0)
            {
                MyRoot.AddThumbDataToActiveGroup2(data, false);
                //MyRoot.AddThumbDataToActiveGroup(data, true);
                return true;
            }
            return false;
        }

        private void ButtonDuplicateDataSelectedT_Click(object sender, RoutedEventArgs e)
        {
            DuplicateDataSelectedThumbs();
        }
        private int DuplicateDataSelectedThumbs()
        {
            return MyRoot.DuplicateDataSelectedThumbs();
        }
        #endregion 複製
        #region 移動

        //ActiveGroupの変更
        private void ButtonIn_Click(object sender, RoutedEventArgs e)
        {
            //ActiveGroupの外へ
            MyRoot.ChangeActiveGroupInside();
        }

        private void ButtonOut_Click(object sender, RoutedEventArgs e)
        {
            //ActiveGroupの中へ
            MyRoot.ChangeActiveGroupOutside();
        }
        //ClickedThumbの親GroupをActiveGroupにする
        private void ButtonInToClicked_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ChangeActiveGroupInsideClickedParent();
        }
        //RootをActiveGroupにする
        private void ButtonDoRootActiveGroup_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ChangeActiveGroupToRoot();
        }
        private void ButtonZUp_Click(object sender, RoutedEventArgs e)
        {
            //前面へ移動
            MyRoot.ZUp();
        }

        private void ButtonZDown_Click(object sender, RoutedEventArgs e)
        {
            //背面へ移動
            MyRoot.ZDown();
        }

        private void ButtonZMostUp_Click(object sender, RoutedEventArgs e)
        {
            //最前面へ
            MyRoot.ZUpFrontMost();
        }

        private void ButtonZMostDown_Click(object sender, RoutedEventArgs e)
        {
            //最背面へ移動
            MyRoot.ZDownBackMost();
        }


        //グリッドスナップ移動
        private void ButtonGoUpGrid_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoUpGrid();
        }
        private void ButtonGoDownGrid_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoDownGrid();
        }

        private void ButtonGoLeftGrid_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoLeftGrid();
        }

        private void ButtonGoRightGrid_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoRightGrid();
        }

        //1ピクセル移動
        private void Button1Up_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoUp1Pix();
        }

        private void Button1Down_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoDown1Pix();
        }

        private void Button1Left_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoLeft1Pix();
        }

        private void Button1Right_Click(object sender, RoutedEventArgs e)
        {
            MyRoot.ActiveThumbGoRight1Pix();
        }

        #endregion 移動

        #endregion クリックイベント


        //個別保存、ActiveThumbのDataを保存
        //アプリ終了時、アプリの設定とRootDataの保存
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            //設定保存
            //SaveConfig(System.IO.Path.Combine(
            //  AppDirectory, APP_CONFIG_FILE_NAME), MyAppConfig);
            SaveAppDataOverride();
            //RootData保存
            //SaveRootDataWithConfig(AppLastEndTimeDataFilePath, MyRoot.Data, true);
            SaveThumbData(MyRoot, AppLastEndTimeDataFilePath);
        }


        //名前をつけてClickedThumbのDataを保存
        private void SaveDataForClickedThumb()
        {
            SaveThumbData(MyRoot.ClickedThumb);
        }

        //名前をつけてActiveThumbのDataを保存
        private void SaveDataForActiveThumb()
        {
           if(SaveThumbData(MyRoot.ActiveThumb) == false)
            {
                MessageBox.Show("保存できなかった");
            }
        }


        #endregion ボタンクリックイベント


        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {

            var appdata = MyAppData;
            var textcolor = MyBorderTextForeColor;
            var fontnama = MyAppData.FontName;
            object value = MyComboBoxFontFmilyNames.SelectedValue;// fontfamily
            object item = MyComboBoxFontFmilyNames.SelectedItem;//key(string) value(fontfamily)
            //MyComboBoxFontFmilyNames.SelectedValue = "Meiryo UI";
            var fname = (KeyValuePair<string, FontFamily>)(MyComboBoxFontFmilyNames.SelectedItem);
            var key = fname.Key;
            if (MyRoot.ClickedThumb == null) return;
            var clickdaa = MyRoot.ClickedThumb.Data;
            MyRoot.ClickedThumb.Data.StrokeR = 0;
        }

        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {

        }



        #region 図形関連

        //TestDrawPolyline
        private void ButtonTestDrawPolyline_Click(object sender, RoutedEventArgs e)
        {
            DrawShapeFromMouseClick();
            //DrawPolylineFromClick();
        }

        private void ButtonAddShape_Click(object sender, RoutedEventArgs e)
        {
            if (MyComboBoxShapeType.SelectedItem is ShapeType type)
            {
                switch (type)
                {
                    case ShapeType.Line:
                        AddShapePolyline2(new PointCollection()
                    { new Point(0, 0), new Point(100, 100) });
                        break;
                    case ShapeType.Bezier:
                        AddShapePolyline2(new PointCollection()
                    { new Point(0, 0), new Point(100, 0) ,new Point(100, 100), new Point(0, 100) });
                        break;
                }
            }

        }



        #region マウスクリックでShape描画
        /// <summary>
        /// マウスクリックでShape描画開始
        /// </summary>
        private void DrawShapeFromMouseClick()
        {
            MyTabControl.IsEnabled = false;
            MyDrawCanvas.Visibility = Visibility.Visible;
            MyTempPoints.Clear();

            GeometricShape shape = new()
            {
                ArrowHeadAngle = (double)MyNumeArrowHeadAngle.MyValue,
                //Fill = GetBrush(),
                //Stroke = GetBrush(),
                Fill = MyBorderShapeColor.Background,
                Stroke = MyBorderShapeColor.Background,
                StrokeThickness = (double)MyNumeStrokeThickness.MyValue,
                HeadEndType = (HeadType)MyComboBoxLineHeadEndType.SelectedValue,
                HeadBeginType = (HeadType)MyComboBoxLineHeadBeginType.SelectedValue,
                MyPoints = MyTempPoints,
                MyLineSmoothJoin = MyChecBoxIsLineSmoothJoin.IsChecked == true,
                MyLineClose = MyChecBoxIsLineClose.IsChecked == true,
                MyShapeType = (ShapeType)MyComboBoxShapeType.SelectedValue,
            };

            MyTempShape = shape;
            MyDrawCanvas.Children.Add(MyTempShape);
        }

        //private SolidColorBrush GetBrush()
        //{
        //    return new SolidColorBrush(Color.FromArgb((byte)MyNumeShapeBackA.MyValue,
        //        (byte)MyNumeShapeBackR.MyValue, (byte)MyNumeShapeBackG.MyValue, (byte)MyNumeShapeBackB.MyValue));
        //}

        //右クリックで終了
        //MyTempPointsからData作成してRootに追加
        private void MyDrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MyTempPoints.Count >= 2)
            {
                AddShapePolyline2(new(MyTempPoints), false);
            }

            //後片付け
            MyTempShape = null;
            MyDrawCanvas.Children.Clear();
            MyDrawCanvas.Visibility = Visibility.Collapsed;
            MyTabControl.IsEnabled = true;

        }

        /// <summary>
        /// DrawCanvas上の座標と追加する位置の差を修正する＆追加位置の決定
        /// </summary>
        /// <param name="data"></param>
        private void FixTopLeftPointCollectionData(Data data)
        {
            PointCollection points = data.PointCollection;
            //左上座標を計算
            double x = double.MaxValue;
            double y = double.MaxValue;
            foreach (var item in points)
            {
                if (x > item.X) x = item.X;
                if (y > item.Y) y = item.Y;
            }
            //修正
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(points[i].X - x, points[i].Y - y);
            }
            //決定
            data.X = x; data.Y = y;
        }

        //マウスクリックでCanvasにベジェ曲線で曲線、PolyBezierSegment - 午後わてんのブログ
        //        https://gogowaten.hatenablog.com/entry/15544835
        /// <summary>
        /// マウス移動時、ベジェ曲線のときはなめらかな曲線になるように制御点を設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyDrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MyTempPoints.Count < 2) { return; }
            Point mousePoint = Mouse.GetPosition(MyDrawCanvas);
            //直線の場合
            if (MyTempShape?.MyShapeType == ShapeType.Line)
            {
                MyTempPoints[^1] = mousePoint;
            }
            //ベジェ曲線の場合、2個前までのアンカー点まで考慮して制御点座標を計算
            else if (MyTempShape?.MyShapeType == ShapeType.Bezier)
            {
                //最終アンカー点座標は、今のマウスの位置
                MyTempPoints[^1] = mousePoint;
                Point pre1Anchor = MyTempPoints[^4];//1個前のアンカー点
                Point pre2Anchor;//2個前のアンカー点
                if (MyTempPoints.Count <= 4)
                {
                    double diffX = (mousePoint.X - pre1Anchor.X) / 4.0;
                    double diffY = (mousePoint.Y - pre1Anchor.Y) / 4.0;
                    MyTempPoints[^3] = new Point(pre1Anchor.X + diffX, pre1Anchor.Y + diffY);

                    //最終アンカー点の制御点座標を決定
                    diffX = (mousePoint.X - MyTempPoints[^3].X) / 4.0;
                    diffY = (mousePoint.Y - MyTempPoints[^3].Y) / 4.0;
                    MyTempPoints[^2] = new Point(mousePoint.X - diffX, mousePoint.Y - diffY);

                }
                else
                {
                    pre2Anchor = MyTempPoints[^7];
                    //マウス座標と2個前のアンカー点との差の1/4、これを使って
                    //一個前のアンカー点の制御点座標を決定
                    double diffX = (mousePoint.X - pre2Anchor.X) / 4.0;
                    double diffY = (mousePoint.Y - pre2Anchor.Y) / 4.0;
                    MyTempPoints[^3] = new Point(pre1Anchor.X + diffX, pre1Anchor.Y + diffY);
                    MyTempPoints[^5] = new Point(pre1Anchor.X - diffX, pre1Anchor.Y - diffY);

                    //最終アンカー点の制御点座標を決定
                    diffX = (mousePoint.X - MyTempPoints[^3].X) / 4.0;
                    diffY = (mousePoint.Y - MyTempPoints[^3].Y) / 4.0;
                    MyTempPoints[^2] = new Point(mousePoint.X - diffX, mousePoint.Y - diffY);

                }
            }
        }

        /// <summary>
        /// 左クリックで頂点追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyDrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pp = Mouse.GetPosition(MyDrawCanvas);
            //直線の場合、最初だけは同時に2点追加
            if (MyTempShape?.MyShapeType == ShapeType.Line)
            {
                if (MyTempPoints.Count == 0)
                {
                    MyTempPoints.Add(pp);
                    MyTempPoints.Add(pp);
                }
                else
                {
                    MyTempPoints.Add(pp);
                }
            }
            //ベジェ曲線の場合、最初は4点追加、以降は3点づつ追加
            else if (MyTempShape?.MyShapeType == ShapeType.Bezier)
            {
                if (MyTempPoints.Count == 0)
                {
                    MyTempPoints.Add(pp);
                    MyTempPoints.Add(pp);
                    MyTempPoints.Add(pp);
                    MyTempPoints.Add(pp);
                }
                else
                {
                    MyTempPoints.Add(pp);
                    MyTempPoints.Add(pp);
                    MyTempPoints.Add(pp);
                }
            }


        }
        #endregion マウスクリックでPolyline描画

        #region 図形のアンカーポイント編集開始、終了


        /// <summary>
        /// 2点間距離取得
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public double GetTwoPointDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p2.Y));
        }
        private void ContextAddAnchorExtend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonStartEditAnchor_Click(object sender, RoutedEventArgs e)
        {
            ShapeEditStart();
        }

        private void ButtonEndEditAnchor_Click(object sender, RoutedEventArgs e)
        {
            ShapeEditEndForClickedThumb();
        }
        #endregion 図形のアンカーポイント編集開始、終了




        #region 図形追加
        private void AddShapePolyline2(PointCollection points, bool locateFix = true)
        {
            Data data = new(TType.Geometric)
            {
                HeadAngle = (double)MyNumeArrowHeadAngle.MyValue,
                StrokeA = (byte)MyNumeShapeStrokeColorA.MyValue,
                StrokeR = MyAppData.ShapeStrokeColorR,
                StrokeG = MyAppData.ShapeStrokeColorG,
                StrokeB = MyAppData.ShapeStrokeColorB,
                //StrokeR = (byte)MyNumeShapeBackR.MyValue,
                //StrokeG = (byte)MyNumeShapeBackG.MyValue,
                //StrokeB = (byte)MyNumeShapeBackB.MyValue,

                StrokeThickness = (double)MyNumeStrokeThickness.MyValue,
                Fill = MyBorderShapeColor.Background,
                //Fill = GetBrush(),
                PointCollection = points,
                HeadBeginType = (HeadType)MyComboBoxLineHeadBeginType.SelectedItem,
                HeadEndType = (HeadType)MyComboBoxLineHeadEndType.SelectedItem,
                ShapeType = (ShapeType)MyComboBoxShapeType.SelectedItem,
                IsBezier = MyCheckBoxIsBezier.IsChecked == true,
                IsLineClose = MyChecBoxIsLineClose.IsChecked == true,
                IsSmoothJoin = MyChecBoxIsLineSmoothJoin.IsChecked == true,

            };


            FixTopLeftPointCollectionData(data);
            MyRoot.AddThumbDataToActiveGroup2(data, MyAppData.IsThumbAddUnder, locateFix);
            //MyRoot.AddThumbDataToActiveGroup(data, MyAppConfig.IsAddUpper, locateFix);
        }


        #endregion 図形追加

        private void ButtonShapeStrokeColor_Click(object sender, RoutedEventArgs e)
        {
            ShowColorPickerForShape();
        }

        /// <summary>
        /// カラーピッカー表示、図形用
        /// </summary>
        private void ShowColorPickerForShape()
        {
            Color backup = MyAppData.ShapeStrokeColor;
            ColorPicker picker = new();
            picker.SetBinding(ColorPicker.AProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.ShapeStrokeColorAProperty), Mode = BindingMode.TwoWay });
            picker.SetBinding(ColorPicker.RProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.ShapeStrokeColorRProperty), Mode = BindingMode.TwoWay });
            picker.SetBinding(ColorPicker.GProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.ShapeStrokeColorGProperty), Mode = BindingMode.TwoWay });
            picker.SetBinding(ColorPicker.BProperty, new Binding() { Source = MyAppData, Path = new PropertyPath(AppData.ShapeStrokeColorBProperty), Mode = BindingMode.TwoWay });

            if (picker.ShowDialog() != true) MyAppData.ShapeStrokeColor = backup;
        }

        #endregion 図形関連


        private void GroupBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MyRoot.ZUp();
            }
            else
            {
                MyRoot.ZDown();
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { MyRoot.ChangeActiveThumbToFrontThumb(); }
            else { MyRoot.ChangeActiveThumbToBackThumb(); }
        }


        //文字列描画
        private void ButtonRenderText_Click2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MyTextBox.Text)) { return; }


            Data data = new(TType.TextBox)
            {
                Text = MyTextBox.Text,
                FontSize = (double)MyNumeFontSize.MyValue,
            };
            if (MyComboBoxFontFmilyNames.SelectedItem is KeyValuePair<string, FontFamily> kvp)
            {
                data.FontName = kvp.Key;
            }

            data.ForeColor = ((SolidColorBrush)(MyBorderTextForeColor.Background)).Color;
            data.BackColor = ((SolidColorBrush)(MyBorderTextBackColor.Background)).Color;
            data.BorderColor = ((SolidColorBrush)(MyBorderTextBorderColor.Background)).Color;
            data.BorderThickness = new Thickness((double)MyNumeTextBoxWakuWidth.MyValue);
            if (MyCheckIsBold.IsChecked == true) { data.IsBold = true; }
            if (MyCheckIsItalic.IsChecked == true) { data.IsItalic = true; }

            MyRoot.AddThumbDataToActiveGroup2(data, MyAppData.IsThumbAddUnder);
        }




        private void ContextAddAnchor_Click(object sender, RoutedEventArgs e)
        {

        }

        #region 文字列色

        private void MyButtonTextForeColor_Click(object sender, RoutedEventArgs e)
        {
            TextColorPicker(MyBorderTextForeColor);
        }

        private void ButtonTextBackColor_Click(object sender, RoutedEventArgs e)
        {
            TextColorPicker(MyBorderTextBackColor);
        }
        private void ButtonTextBorderColor_Click(object sender, RoutedEventArgs e)
        {
            TextColorPicker(MyBorderTextBorderColor);
        }

        private void TextColorPicker(Border border)
        {
            SolidColorBrush b = (SolidColorBrush)border.Background;
            ColorPicker picker = new(b);

            if (picker.ShowDialog() == true)
            {
                border.Background = picker.PickColorBrush;
            }
            else border.Background = b;


            //Brush b = border.Background;
            //MyColorPicker.PickColorBrush = (SolidColorBrush)border.Background;
            //MyColorPicker.Visibility = Visibility.Visible;
            //if (MyColorPicker.ShowDialog() == true)
            //{
            //    border.Background = MyColorPicker.PickColorBrush;
            //}
            //else border.Background = b;
            //MyColorPicker.Visibility = Visibility.Collapsed;


        }

        #endregion 文字列色

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // アプリの設定を上書き保存
            SaveAppDataOverride();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // アプリの設定を読み込み＋設定を反映(Binding)
            //LoadAppDataAndSetting();
            string path = GetLoadFilePathFromFileDialog(EXT_FILTER_APP);
            if (string.IsNullOrEmpty(path)) return;
            if (LoadAppData<AppData>(path) is AppData data)
            {
                MyAppData = data;
                DataContext = MyAppData;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAppDataReset_Click(object sender, RoutedEventArgs e)
        {
            MyAppData = new AppData();
            DataContext = MyAppData;
        }

        //アプリの設定を名前を付けて保存
        private void ButtonSaveAppData_Click(object sender, RoutedEventArgs e)
        {
            if (GetSaveDataFilePath(EXT_FILTER_APP) is string path)
            {
                SaveAppData(path, MyAppData);
            }

        }



    }
}






#region 列挙型

public enum ImageType
{
    png,
    bmp,
    jpg,
    gif,
    tiff,

}
public enum CaptureRectType
{
    Screen,
    Window,
    WindowClient,
    UnderCursor,
    UnderCursorClient,
    WindowWithMenu,
    WindowWithRelatedWindow,
    WindowWithRelatedWindowPlus,

}

public enum MySoundPlay
{
    None,
    PlayDefault,
    PlayOrder
}

public enum SaveBehaviorType
{
    Save,
    Copy,
    SaveAndCopy,
    SaveAtClipboardChange,
    AddPreviewWindowFromClopboard
}
#endregion 列挙型

/// <summary>
/// TabControlに改変したContextMenu右クリックメニュー
/// </summary>
public class ContextTabMenu : ContextMenu
{
    public TabControl TempTabControl { get; private set; }
    public ContextTabMenu()
    {
        TempTabControl = SetTemplate();
        //マウスホイールでタブ切り替え
        TempTabControl.MouseWheel += (s, e) =>
        {
            int index = TempTabControl.SelectedIndex;
            if (e.Delta > 0 && index < TempTabControl.Items.Count - 1)
            {
                TempTabControl.SelectedIndex++;
            }
            else if (index > 0) TempTabControl.SelectedIndex--;
        };
    }
    private TabControl SetTemplate()
    {
        FrameworkElementFactory factory = new(typeof(TabControl), "nemo");
        Template = new() { VisualTree = factory };
        ApplyTemplate();
        if (Template.FindName("nemo", this) is TabControl tab)
        {
            return tab;
        }
        else throw new ArgumentException();
    }
    public void AddMenuTab(TabItem item)
    {
        TempTabControl.Items.Add(item);
    }
}



