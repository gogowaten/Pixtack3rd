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

namespace Pixtack3rd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public AppConfig MyAppConfig { get; set; }
        //アプリ情報
        private const string APP_NAME = "Pixtack3rd";
        //アプリの設定ファイル名
        private const string APP_CONFIG_FILE_NAME = "config.xml";
        //データのファイル名
        private readonly string XML_FILE_NAME = "Data.xml";
        private const string APP_LAST_END_TIME_FILE_NAME = "LastEndTimeData" + EXTENSION_NAME_APP;
        //読み込んでいるデータファイルのフルパス、上書き保存対象、起動時は前回終了時を読み込み
        private string CurrentFileFullPath = string.Empty;
        //終了時に状態保存、起動時に読み込みするファイルのフルパス
        private string AppLastEndTimeDataFilePath { get; } = string.Empty;
        //拡張子名、全データ(Rootデータとアプリの設定)用の拡張子
        private const string EXTENSION_NAME_APP = ".p3";
        //拡張子名、Thumbデータだけ用の拡張子
        private const string EXTENSION_NAME_DATA = ".p3d";

        private const string EXTENSION_FILTER_P3 = "Data + 設定|*" + EXTENSION_NAME_APP;
        private const string EXTENSION_FILTER_P3D = "Data|*" + EXTENSION_NAME_DATA;

        private string AppVersion;
        //datetime.tostringの書式、これを既定値にする
        private const string DATE_TIME_STRING_FORMAT = "yyyyMMdd'_'HHmmss'_'fff";
        private const string APP_ROOT_DATA_FILENAME = "TTRoot" + EXTENSION_NAME_DATA;

        //マウスクリックでPolyline描画するときの一時的なもの
        private readonly PointCollection MyTempPoints = new();
        private Shape? MyTempShape;

        //
        private List<AnchorThumb> MyAnchoredThumbs { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            MyAppConfig = GetAppConfig(APP_CONFIG_FILE_NAME);

            //前回終了時に保存したファイルのフルパスをセット
            AppLastEndTimeDataFilePath = System.IO.Path.Combine(
                Environment.CurrentDirectory, APP_LAST_END_TIME_FILE_NAME);


            AppVersion = GetAppVersion();
            MyInitialize();

            MyInitializeComboBox();

            Drop += MainWindow_Drop;
            Closed += MainWindow_Closed;

            MyTabControl.SelectedIndex = 3;

            //string imagePath = "D:\\ブログ用\\テスト用画像\\collection5.png";
            //string imagePath1 = "D:\\ブログ用\\テスト用画像\\collection4.png";

            //Data dataImg1 = new(TType.Image) { BitmapSource = TTRoot.GetBitmap(imagePath) };
            //Data dataImg2 = new(TType.Image) { BitmapSource = TTRoot.GetBitmap(imagePath1), X = 100, Y = 100 };
            //MyTextBox1.KeyDown += (a, b) =>
            //{
            //    var maiX = b.Source;
            //};

            // マウスクリックでPolyline描画
            MyDrawCanvas.MouseLeftButtonDown += MyDrawCanvas_MouseLeftButtonDown;
            MyDrawCanvas.MouseMove += MyDrawCanvas_MouseMove;
            MyDrawCanvas.MouseRightButtonDown += MyDrawCanvas_MouseRightButtonDown;

        }


        #region 初期設定
        /// <summary>
        /// 実行ファイルと同じフォルダにある設定ファイルをデシリアライズして返す、
        /// 見つからないときは新規作成して返す
        /// </summary>
        /// <fileName>拡張子を含めたファイル名</fileName>
        /// <returns></returns>
        private static AppConfig GetAppConfig(string fileName)
        {
            string configFile = System.IO.Path.Combine(
                Environment.CurrentDirectory, fileName);

            if (File.Exists(configFile)
                && LoadConfig(configFile) is AppConfig config)
            {
                return config;
            }
            else
            {
                return new AppConfig();
            }
        }
        private void MyInitialize()
        {
            //タイトルをアプリの名前 + バージョン
            this.Title = APP_NAME + AppVersion;

            //アプリ終了時に保存したファイルのフルパスを上書き保存パスにセット
            CurrentFileFullPath = AppLastEndTimeDataFilePath;

            //アプリ終了時のデータを読み込み
            if (MyAppConfig.IsLoadPreviewData)
            {
                //アプリ終了時のデータと設定を読み込んでセット
                (Data? data, AppConfig? config) = LoadDataFromFile(CurrentFileFullPath);
                if (data is not null) { MyRoot.SetRootData(data); }
            }

            //枠表示の設定Binding、これはいまいちな処理
            MyRoot.SetBinding(TTRoot.TTWakuVisibleTypeProperty, new Binding(nameof(MyAppConfig.WakuVisibleType)) { Source = this.MyAppConfig });

            //データコンテキストの設定、Bindingをした後じゃないと反映されない
            DataContext = MyAppConfig;

            //ショートカットキー
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;


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
                    if (e.Key == Key.S) { SaveRootDataWithConfig(CurrentFileFullPath, MyRoot.Data, true); }
                    //else if (e.Key == Key.D) { DuplicateDataSelectedThumbs(); }
                    break;
                case ModifierKeys.Shift:
                    break;
                case ModifierKeys.Windows:
                    break;
                case (ModifierKeys.Control | ModifierKeys.Shift):
                    if (e.Key == Key.S) { SaveAll(); }
                    else if (e.Key == Key.D) { DuplicateDataRoot(); }
                    else if (e.Key == Key.F4) { MyRoot.RemoveAll(); }
                    break;
            }

        }

        private void MyInitializeComboBox()
        {
            ComboBoxSaveFileType.ItemsSource = Enum.GetValues(typeof(ImageType));
            MyCombBoxFontFmilyNames.ItemsSource = GetFontFamilies();
            MyCombBoxFontFmilyNames.SelectedValue = this.FontFamily;
            MyComboBoxLineHeadBeginType.ItemsSource = Enum.GetValues(typeof(HeadType));
            MyComboBoxLineHeadBeginType.SelectedValue = HeadType.None;
            MyComboBoxLineHeadEndType.ItemsSource = Enum.GetValues(typeof(HeadType));
            MyComboBoxLineHeadEndType.SelectedValue = HeadType.Arrow;
            //MyComboBoxFontStretchs.ItemsSource = MakeFontStretchDictionary();
            //MyComboBoxFontStyle.ItemsSource = MakeFontStylesDictionary();
            //MyComboBoxFontStyle.SelectedValue = this.FontStyle;
            //MyComboBoxFontWeight.ItemsSource = MakeFontWeightDictionary();
            //MyComboBoxFontWeight.SelectedValue = this.FontWeight;

            //List<double> vs = new() { 0, 1.5, 2.5, 3.5, 5 };
            //MyComboBoxFileNameDateOrder.ItemsSource = vs;
            //MyComboBoxFileNameSerialOrder.ItemsSource = vs;

            //MyComboBoxCaputureRect.ItemsSource = new Dictionary<CaptureRectType, string>
            //{
            //    { CaptureRectType.Screen, "画面全体" },
            //    { CaptureRectType.Window, "ウィンドウ" },
            //    { CaptureRectType.WindowClient, "ウィンドウのクライアント領域" },
            //    { CaptureRectType.UnderCursor, "カーソル下のコントロール" },
            //    { CaptureRectType.UnderCursorClient, "カーソル下コントロールのクライアント領域" },
            //    { CaptureRectType.WindowWithMenu, "ウィンドウ + メニューウィンドウ" },
            //    { CaptureRectType.WindowWithRelatedWindow, "ウィンドウ + 関連ウィンドウ" },
            //    { CaptureRectType.WindowWithRelatedWindowPlus, "ウィンドウ + より多くの関連ウィンドウ" },
            //};


            //MyComboBoxHotKey.ItemsSource = Enum.GetValues(typeof(Key));


            //MyComboBoxSoundType.ItemsSource = new Dictionary<MySoundPlay, string> {
            //    { MySoundPlay.None, "無音"},
            //    { MySoundPlay.PlayDefault, "既定の音" },
            //    { MySoundPlay.PlayOrder, "指定した音" }
            //};

            //MyComboBoxSaveBehavior.ItemsSource = new Dictionary<SaveBehaviorType, string> {
            //    { SaveBehaviorType.Save, "画像ファイルとして保存する" },
            //    { SaveBehaviorType.Copy, "クリップボードにコピーする (保存はしない)" },
            //    { SaveBehaviorType.SaveAndCopy, "保存 + コピー" },
            //    { SaveBehaviorType.SaveAtClipboardChange, "クリップボード監視、更新されたら保存" },
            //    { SaveBehaviorType.AddPreviewWindowFromClopboard, "クリップボード監視、更新されたらプレビューウィンドウに追加 (保存はしない)" }
            //};

        }

        /// <summary>
        /// アプリの設定のウィンドウ位置が画面外だった場合は0に変更して返す
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static AppConfig FixAppWindowLocate(AppConfig config)
        {
            if (config.Left < -10 ||
                config.Left > SystemParameters.VirtualScreenWidth - 100)
            {
                config.Left = 0;
            }
            if (config.Top < -10 ||
                config.Top > SystemParameters.VirtualScreenHeight - 100)
            {
                config.Top = 0;
            }

            return config;
        }

        #endregion 初期設定
        #region 設定保存と読み込み
        private bool SaveConfig<T>(string path, T obj)
        {
            try
            {
                XmlWriterSettings settings = new()
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                    NewLineOnAttributes = true,
                    ConformanceLevel = ConformanceLevel.Fragment,
                };
                using (XmlWriter writer = XmlWriter.Create(path, settings))
                {
                    DataContractSerializer serializer = new(typeof(T));
                    serializer.WriteObject(writer, obj);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存できなかった\n{ex.Message}");
                return false;
                //throw new ArgumentException("保存できなかった", ex.Message);                
            }
        }
        #endregion 設定保存と読み込み

        #region その他関数

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
            foreach (var item in Fonts.SystemFontFamilies)
            {
                var typefaces = item.GetTypefaces();
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
        /// 設定ファイルをAppConfigにデシリアライズ
        /// </summary>
        /// <param name="filePath">設定ファイルのフルパス</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static AppConfig? LoadConfig(string filePath)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    DataContractSerializer serializer = new(typeof(AppConfig));
                    AppConfig? result = (AppConfig?)serializer.ReadObject(reader);
                    if (result == null)
                    {
                        MessageBox.Show("読込できんかった");
                        return null;
                    }
                    else { return result; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "読込できんかった");
                return null;
            }
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

        ///// <summary>
        ///// クリップボードから画像を取得してActiveGroupに追加
        ///// <paramref name="isPreferPNG">取得時に"PNG"形式を優先するときはtrue</paramref>
        ///// <paramref name="isBgr32">ピクセルフォーマットをBgr32に変換(アルファ値を255に)するときはtrue</paramref>
        ///// </summary>
        //private void AddImageFromClipboard(bool isPreferPNG, bool isBgr32)
        //{
        //    BitmapSource? bmp;
        //    if (isPreferPNG)
        //    {
        //        if (isBgr32)
        //        {
        //            bmp = MyClipboard.GetBgr32FromPng();
        //        }
        //        else
        //        {
        //            bmp = MyClipboard.GetClipboardImagePngWithAlphaFix();
        //        }
        //    }
        //    else
        //    {
        //        if (isBgr32)
        //        {
        //            bmp = MyClipboard.GetClipboardImageBgr32();
        //        }
        //        else
        //        {
        //            bmp = MyClipboard.GetImageFromClipboardWithAlphaFix();
        //        }
        //    }
        //    if (bmp != null)
        //    {//追加
        //        MyRoot.AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp });
        //    }
        //    else
        //    {
        //        MessageBox.Show("画像は得られなかった");
        //    }
        //}
        ///// <summary>
        ///// クリップボードから画像を取得してActiveGroupに追加
        ///// "PNG"形式優先で取得、できなければGetImageで取得
        ///// </summary>
        //private void AddImageFromClipboard()
        //{
        //    if (MyClipboard.GetImageFromClipboardPreferPNG() is BitmapSource bmp)
        //    {
        //        MyRoot.AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp });
        //    }
        //    else { MessageBox.Show("画像は得られなかった"); }
        //}

        #endregion その他関数

        #region 画像保存


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
        internal bool SaveBitmapSub(BitmapSource bitmap, string fullPath, BitmapEncoder encoder)
        {
            if (MakeMetadata() is BitmapMetadata meta)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmap, null, meta, null));
                //重複回避ファイルパス取得
                //fullPath = MakeFilePathAvoidDuplicate(fullPath);
                try
                {
                    using FileStream fs = new(fullPath, FileMode.Create, FileAccess.Write);
                    encoder.Save(fs);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存できなかった\n{ex}", "保存できなかった");
                    return false;
                }
            }
            else { return false; }
        }


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

        /// <summary>
        /// 重複回避ファイルパス作成、重複しなくなるまでファイル名末尾に_を追加して返す
        /// </summary>
        /// <returns></returns>
        private string MakeFilePathAvoidDuplicate(string fullPath)
        {
            string extension = System.IO.Path.GetExtension(fullPath);
            string name = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            string? directory = System.IO.Path.GetDirectoryName(fullPath);
            if (directory != null)
            {
                while (File.Exists(fullPath))
                {
                    name += "_";
                    fullPath = System.IO.Path.Combine(directory, name) + extension;
                }
                return fullPath;
            }
            else return string.Empty;
        }

        //メタデータ作成
        private BitmapMetadata? MakeMetadata()
        {
            BitmapMetadata? data = null;
            string software = APP_NAME + "_" + AppVersion;
            switch (ComboBoxSaveFileType.SelectedValue)
            {
                case ImageType.png:
                    data = new BitmapMetadata("png");
                    data.SetQuery("/tEXt/Software", software);
                    break;
                case ImageType.jpg:
                    data = new BitmapMetadata("jpg");
                    data.SetQuery("/app1/ifd/{ushort=305}", software);
                    break;
                case ImageType.bmp:

                    break;
                case ImageType.gif:
                    data = new BitmapMetadata("Gif");
                    //tData.SetQuery("/xmp/xmp:CreatorTool", "Pixtrim2");
                    //tData.SetQuery("/XMP/XMP:CreatorTool", "Pixtrim2");
                    data.SetQuery("/XMP/XMP:CreatorTool", software);
                    break;
                case ImageType.tiff:
                    data = new BitmapMetadata("tiff")
                    {
                        ApplicationName = software
                    };
                    break;
                default:
                    break;
            }

            return data;
        }
        private BitmapMetadata? MakeMetadata(int filterIndex)
        {
            BitmapMetadata? data = null;
            string software = APP_NAME + "_" + AppVersion;

            switch (filterIndex)
            {
                case 1:
                    data = new BitmapMetadata("png");
                    data.SetQuery("/tEXt/Software", software);
                    break;
                case 2:
                    data = new BitmapMetadata("jpg");
                    data.SetQuery("/app1/ifd/{ushort=305}", software);
                    break;
                case 3:

                    break;
                case 4:
                    data = new BitmapMetadata("Gif");
                    //tData.SetQuery("/xmp/xmp:CreatorTool", "Pixtrim2");
                    //tData.SetQuery("/XMP/XMP:CreatorTool", "Pixtrim2");
                    data.SetQuery("/XMP/XMP:CreatorTool", software);
                    break;
                case 5:
                    data = new BitmapMetadata("tiff")
                    {
                        ApplicationName = software
                    };
                    break;
                default:
                    break;
            }

            return data;
        }

        //画像ファイル形式によるEncoder取得
        private BitmapEncoder GetEncoder(int filterIndex)
        {
            //var type = MyAppConfig.ImageType;

            switch (filterIndex)
            {
                case 1:
                    return new PngBitmapEncoder();
                case 2:
                    var jpeg = new JpegBitmapEncoder
                    {
                        QualityLevel = MyAppConfig.JpegQuality
                    };
                    return jpeg;
                case 3:
                    return new BmpBitmapEncoder();
                case 4:
                    return new GifBitmapEncoder();
                case 5:
                    return new TiffBitmapEncoder();
                default:
                    throw new Exception();
            }

        }
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
                        QualityLevel = MyAppConfig.JpegQuality
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

        private bool SaveBitmap2(BitmapSource bitmap)
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


        #endregion 画像保存

        #region ファイルドロップで開く

        private void AddThumbFromFiles(string[] fileList2)
        {
            List<string> errorFiles = new();

            foreach (var item in fileList2)
            {
                //拡張子で判定、関連ファイルならDataで追加
                var ext = System.IO.Path.GetExtension(item);
                if (ext == EXTENSION_NAME_DATA || ext == EXTENSION_NAME_APP)
                {
                    var (data, appConfig) = LoadDataFromFile(item);
                    if (data == null)
                    {
                        errorFiles.Add(item); continue;
                    }
                    //DataがRootならGroupに変換して追加
                    if (data.Type == TType.Root)
                    {
                        data = ConvertDataRootToGroup(data);
                        if (data != null && MyAppConfig != null)
                        {
                            MyRoot.AddThumbDataToActiveGroup(data, MyAppConfig.IsAddUpper);
                        }
                        else { errorFiles.Add(item); continue; }
                    }
                    else if (MyAppConfig is not null)
                    {
                        MyRoot.AddThumbDataToActiveGroup(data, MyAppConfig.IsAddUpper);
                    }


                }
                //それ以外の拡張子ファイルは画像として読み込む
                else
                {
                    //試みてエラーだったらファイル名を表示
                    try
                    {
                        //using FileStream stream = new(item, FileMode.Open, FileAccess.Read);
                        //BitmapImage img = new();
                        //img.BeginInit();
                        //img.CacheOption = BitmapCacheOption.OnLoad;
                        //img.StreamSource = stream;
                        //img.EndInit();

                        using var stream = System.IO.File.OpenRead(item);
                        BitmapSource img = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        if (img.DpiX != 96.0)
                        {
                            FormatConvertedBitmap fcb = new(img, PixelFormats.Bgra32, null, 0.0);
                            int w = fcb.PixelWidth;
                            int h = fcb.PixelHeight;
                            int stride = w * 4;
                            byte[] pixels = new byte[stride * h];
                            fcb.CopyPixels(pixels, stride, 0);
                            img = BitmapSource.Create(w, h, 96.0, 96.0, fcb.Format, null, pixels, stride);

                        }

                        Data data = new(TType.Image)
                        {
                            BitmapSource = img
                        };

                        if (MyAppConfig is not null)
                        {
                            MyRoot.AddThumbDataToActiveGroup(data, MyAppConfig.IsAddUpper);
                        }
                    }
                    catch (Exception)
                    {
                        errorFiles.Add(item);
                        continue;
                    }
                }
            }
            if (errorFiles.Count > 0)
            {
                string ms = "";
                foreach (var name in errorFiles)
                {
                    ms += $"{name}\n";
                }
                MessageBox.Show(ms, "開くことができなかったファイル一覧",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //ファイル名一覧取得
                var fileList2 = ((string[])e.Data.GetData(DataFormats.FileDrop)).ToArray();
                if (MyAppConfig.IsAscendingSort)
                {
                    Array.Sort(fileList2);//昇順ソート
                }
                else
                {
                    Array.Sort(fileList2);
                    Array.Reverse(fileList2);
                }
                AddThumbFromFiles(fileList2);
            }
        }
        #endregion ファイルドロップで開く


        #region データ保存、アプリの設定保存

        /// <summary>
        /// Rootデータをアプリの設定とともにファイルに保存
        /// </summary>
        /// <param name="filePath">拡張子も含めたフルパス</param>
        /// <param name="data"></param>
        /// <param name="isWithAppConfigSave">アプリの設定も保存するときはtrue</param>
        private void SaveRootDataWithConfig(string filePath, Data data, bool isWithAppConfigSave)
        {
            try
            {
                using FileStream zipStream = File.Create(filePath);
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
                //xml形式にシリアライズして、それをzipに詰め込む
                ZipArchiveEntry entry = archive.CreateEntry(XML_FILE_NAME);
                using (Stream entryStream = entry.Open())
                {
                    using XmlWriter writer = XmlWriter.Create(entryStream, settings);
                    try { serializer.WriteObject(writer, data); }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                //アプリの設定保存
                if (isWithAppConfigSave)
                {
                    entry = archive.CreateEntry(APP_CONFIG_FILE_NAME);
                    serializer = new(typeof(AppConfig));
                    using (Stream entryStream = entry.Open())
                    {
                        using XmlWriter writer = XmlWriter.Create(entryStream, settings);
                        try { serializer.WriteObject(writer, MyAppConfig); }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                }

                //BitmapSourceの保存
                SubLoop(archive, data);

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

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







        #endregion データ保存、アプリの設定保存

        #region データ読み込み、アプリの設定読み込み

        /// <summary>
        /// ファイルからData読み込み
        /// </summary>
        /// <param name="filePath">フルパス</param>
        /// <returns></returns>
        private (Data? data, AppConfig? config) LoadDataFromFile(string filePath)
        {
            try
            {
                using FileStream zipStream = File.OpenRead(filePath);
                using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);
                ZipArchiveEntry? entry = archive.GetEntry(XML_FILE_NAME);
                if (entry != null)
                {
                    //Dataをデシリアライズ
                    using Stream entryStream = entry.Open();
                    DataContractSerializer serializer = new(typeof(Data));
                    using var reader = XmlReader.Create(entryStream);
                    Data? data = (Data?)serializer.ReadObject(reader);
                    if (data is null) return (null, null);

                    SubSetImageSource(data, archive);

                    //画像をデコードしてDataに設定する
                    SubLoop(data, archive);


                    //アプリの設定をデシリアライズ
                    entry = archive.GetEntry(APP_CONFIG_FILE_NAME);
                    if (entry != null)
                    {
                        using Stream entryAppConfig = entry.Open();
                        serializer = new(typeof(AppConfig));
                        using (var appConfigReader = XmlReader.Create(entryAppConfig))
                        {
                            AppConfig? config = (AppConfig?)serializer.ReadObject(appConfigReader);
                            return (data, config);
                        }
                    }
                    else
                    {
                        return (data, null);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return (null, null);

            void SubLoop(Data data, ZipArchive archive)
            {
                foreach (var item in data.Datas)
                {
                    //DataのTypeがImage型ならzipから画像を取り出して設定
                    if (item.Type == TType.Image) { SubSetImageSource(item, archive); }
                    //DataのTypeがGroupなら子要素も取り出す
                    else if (item.Type == TType.Group) { SubLoop(item, archive); }
                }
            }
            void SubSetImageSource(Data data, ZipArchive archive)
            {
                //Guidに一致する画像ファイルをデコードしてプロパティに設定
                ZipArchiveEntry? imageEntry = archive.GetEntry(data.Guid + ".png");
                if (imageEntry != null)
                {
                    using Stream imageStream = imageEntry.Open();
                    PngBitmapDecoder decoder =
                        new(imageStream,
                        BitmapCreateOptions.None,
                        BitmapCacheOption.Default);
                    data.BitmapSource = decoder.Frames[0];//設定
                }
            }
        }
        ////前回終了時のデータ読み込み
        //private void ButtonLoadDataPrevious_Click(object sender, RoutedEventArgs e)
        //{
        //    LoadPreviousData(true);
        //}

        //ファイルの読み込み
        //TTRootのDataとアプリの設定を取得して設定
        private void ButtonLoadData_Click(object sender, RoutedEventArgs e)
        {
            LoadP3File();
        }
        private bool LoadP3File()
        {
            if (GetLoadFilePathFromFileDialog(EXTENSION_FILTER_P3) is string filePath)
            {
                (Data? data, AppConfig? appConfig) = LoadDataFromFile(filePath);
                if (data is not null && appConfig is not null)
                {
                    MyRoot.SetRootData(data);
                    MyAppConfig = appConfig;
                    DataContext = MyAppConfig;//必要？

                    //読み込んだファイルを上書き対象として指定
                    CurrentFileFullPath = filePath;
                    return true;
                }
                return false;
            }
            return false;
        }
        private string? GetLoadFilePathFromFileDialog(string extFilter)
        {
            OpenFileDialog dialog = new();
            dialog.Filter = extFilter;
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        //Data＋アプリ設定ファイルのp3ファイルを読み込むけど、アプリ設定は無視する
        //RootDataはTTGroupに変換して追加
        //変換部分が怪しい、項目が増えた場合はここも増やす必要があるのでバグ発生源になる？
        private void ButtonLoadDataRootToGroup_Click(object sender, RoutedEventArgs e)
        {
            LoadDataRootToGroup(false);
        }
        /// <summary>
        /// p3ファイルを開く、Dataは読み込むけどアプリ設定は無視する
        /// </summary>
        /// <param name="isOverrideRoot">開いたファイルでRootを上書きする(入れ替える)</param>
        /// <returns></returns>
        private bool LoadDataRootToGroup(bool isOverrideRoot)
        {
            if (GetLoadFilePathFromFileDialog(EXTENSION_FILTER_P3) is string filePath)
            {
                (Data? data, AppConfig? appConfig) = LoadDataFromFile(filePath);
                if (data != null)
                {
                    if (isOverrideRoot)
                    {
                        MyRoot.SetRootData(data);
                        return true;
                    }
                    else if (ConvertDataRootToGroup(data) is Data groupData)
                    {
                        MyRoot.AddThumbDataToActiveGroup(groupData, MyAppConfig.IsAddUpper);
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        private void ButtonLoadDataP3WithoutConfig_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadP3FileWithoutConfig();
        }
        /// <summary>
        /// p3ファイルを開く、RootDataだけ読み込んで、設定ファイルは無視する
        /// </summary>
        /// <returns></returns>
        private bool LoadP3FileWithoutConfig()
        {
            if (GetLoadFilePathFromFileDialog(EXTENSION_FILTER_P3) is string filePath)
            {
                (Data? data, AppConfig? appConfig) = LoadDataFromFile(filePath);
                if (data != null)
                {
                    MyRoot.SetRootData(data);
                    return true;
                }
                return false;
            }
            return false;
        }
        //個別Data読み込み
        private void ButtonLoadDataThumb_Click(object sender, RoutedEventArgs e)
        {
            LoadDataThumb();
        }
        private bool LoadDataThumb()
        {
            if (GetLoadFilePathFromFileDialog(EXTENSION_FILTER_P3D) is string filePath)
            {
                (Data? data, AppConfig? config) = LoadDataFromFile(filePath);
                if (data is not null && config is not null)
                {
                    MyRoot.AddThumbDataToActiveGroup(data, config.IsAddUpper);
                    return true;
                }
                return false;
            }
            return false;
        }

        //複数ファイル
        private void ButtonLoadFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Multiselect = true;
            dialog.Filter = "対応ファイル|*.bmp;*.jpg;*.png;*.gif;*.tiff;*.p3d;*.p3|すべて|*.*";
            if (dialog.ShowDialog() == true)
            {
                string[] names = dialog.FileNames;
                Array.Sort(names);
                if (MyAppConfig.IsAscendingSort == false)
                {
                    Array.Reverse(names);
                }
                AddThumbFromFiles(names);
            }
        }

        #endregion データ読み込み、アプリの設定読み込み






        #region ボタンクリックイベント

        #region 上書き保存と読み込み

        //上書き保存
        private void ButtonSaveDefault_Click(object sender, RoutedEventArgs e)
        {
            SaveRootDataWithConfig(CurrentFileFullPath, MyRoot.Data, true);
        }
        //上書き保存を読み込み
        private void ButtonLoadDefault_Click(object sender, RoutedEventArgs e)
        {
            (Data? data, AppConfig? config) = LoadDataFromFile(CurrentFileFullPath);
            if (data is not null)
            {
                MyRoot.SetRootData(data);
            }
        }
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

        #region 保存系

        //Rootを画像ファイルとして保存
        private void ButtonSaveToImage_Click(object sender, RoutedEventArgs e)
        {
            if (MyRoot.GetBitmapRoot() is BitmapSource bitmap)
            {
                if (SaveBitmap2(bitmap))
                {

                }
                else { MessageBox.Show("保存できなかった"); }
            }
        }
        private void ButtonSaveToImageActive_Click(object sender, RoutedEventArgs e)
        {//ActiveThumb

            if (MyRoot.GetBitmapActiveThumb() is BitmapSource bitmap)
            {
                if (SaveBitmap2(bitmap))
                {

                }
                else { MessageBox.Show("保存できなかった"); }
            }
        }

        private void ButtonSaveToImageClicked_Click(object sender, RoutedEventArgs e)
        {//ClickedThumb
            if (MyRoot.GetBitmapClickedThumb() is BitmapSource bitmap)
            {
                if (SaveBitmap2(bitmap))
                {

                }
                else { MessageBox.Show("保存できなかった"); }
            }
        }
        //TTRootのDataとアプリの設定を保存
        private void ButtonSaveData_Click(object sender, RoutedEventArgs e)
        {
            if (MyRoot.Thumbs.Count == 0)
            {
                MessageBox.Show("保存対象がない");
                return;
            }
            Microsoft.Win32.SaveFileDialog dialog = new();
            dialog.Filter = EXTENSION_FILTER_P3;
            if (dialog.ShowDialog() == true)
            {
                SaveRootDataWithConfig(dialog.FileName, MyRoot.Data, true);
            }
        }
        //個別保存、ActiveThumbのDataを保存
        //アプリ終了時
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            //設定保存
            SaveConfig(System.IO.Path.Combine(
                Environment.CurrentDirectory, APP_CONFIG_FILE_NAME), MyAppConfig);
            //RootData保存
            //SaveRootDataWithConfig(System.IO.Path.Combine(
            //    Environment.CurrentDirectory, APP_ROOT_DATA_FILENAME), MyRoot.Data, true);
            SaveRootDataWithConfig(AppLastEndTimeDataFilePath, MyRoot.Data, true);
        }

        private void ButtonSaveDataThumb_Click(object sender, RoutedEventArgs e)
        {
            //ActiveThumbのDataを保存
            if (MyRoot.ActiveThumb?.Data is Data data)
            {
                Microsoft.Win32.SaveFileDialog dialog = new();
                dialog.Filter = EXTENSION_FILTER_P3D;
                if (dialog.ShowDialog() == true)
                {
                    SaveRootDataWithConfig(dialog.FileName, data, false);
                }
            }
        }

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
        private void ButtonSaveAllData_Click(object sender, RoutedEventArgs e)
        {
            SaveAll();
        }
        private void SaveAll()
        {
            if (MyRoot.Thumbs.Count == 0) { return; }
            if (GetSaveDataFilePath("Dataとアプリ設定|*.p3|Dataのみ|*.p3d") is string path)
            {
                if (System.IO.Path.GetExtension(path) == EXTENSION_NAME_APP)
                {
                    SaveRootDataWithConfig(path, MyRoot.Data, true);
                }
                else
                {
                    SaveRootDataWithConfig(path, MyRoot.Data, false);
                }
            }
        }
        //private void ButtonSaveRootThumb_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MyRoot.Thumbs.Count == 0) { return; }
        //    if (GetSaveDataFilePath(EXTENSION_FILTER_P3D) is string path)
        //    {
        //        SaveRootDataWithConfig(path, MyRoot.Data, false);
        //    }
        //}

        private void ButtonSaveCickedThumb_Click(object sender, RoutedEventArgs e)
        {
            if (MyRoot.ClickedThumb?.Data == null) { return; }
            if (GetSaveDataFilePath(EXTENSION_FILTER_P3D) is string path)
            {
                SaveRootDataWithConfig(path, MyRoot.ClickedThumb.Data, false);
            }
        }

        private void ButtonSaveActiveThumb_Click(object sender, RoutedEventArgs e)
        {
            if (MyRoot.ActiveThumb?.Data == null) { return; }
            if (GetSaveDataFilePath(EXTENSION_FILTER_P3D) is string path)
            {
                SaveRootDataWithConfig(path, MyRoot.ActiveThumb.Data, false);
            }
        }


        #endregion 保存系

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
                MyRoot.AddThumbDataToActiveGroup(new Data(TType.Image) { BitmapSource = bmp }, true);
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
                MyRoot.AddThumbDataToActiveGroup(data, true);
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

        #endregion ボタンクリックイベント

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyRoot.ClickedThumb.Data.PointCollection;
            var neko2 = MyRoot.ClickedThumb.Data.Stroke;
            //MyRoot.ClickedThumb.Data.PointCollection[0] = new Point(200,200);
            if (MyRoot.ClickedThumb.MyTemplateElement is PolyCanvas p2)
            {
                //p2.MyPoints[0] = new Point(200, 200);
                var inu = p2.MyPoints;
                var inu2 = p2.Stroke;
                if (MyRoot.ClickedThumb is TTPolyline poly)
                {
                    var uma = poly.MyPoints;
                    var uma2 = poly.Stroke;
                }
            }
        }


        #region 図形関連

        #region マウスクリックでPolyline描画
        /// <summary>
        /// マウスクリックでPolyline描画開始
        /// </summary>
        private void DrawPolylineFromClick()
        {
            MyTabControl.IsEnabled = false;
            MyDrawCanvas.Visibility = Visibility.Visible;
            MyTempPoints.Clear();

            PolylineZ polyZ = new()
            {
                Angle = (double)MyNumeArrowHeadAngle.MyValue,
                Fill = GetBrush(),
                Stroke = GetBrush(),
                StrokeThickness = (double)MyNumeStrokeThickness.MyValue,
                HeadEndType = (HeadType)MyComboBoxLineHeadEndType.SelectedValue,
                HeadBeginType = (HeadType)MyComboBoxLineHeadBeginType.SelectedValue,
                MyPoints = MyTempPoints,
            };

            MyTempShape = polyZ;
            MyDrawCanvas.Children.Add(MyTempShape);
        }
        private SolidColorBrush GetBrush()
        {
            return new SolidColorBrush(Color.FromArgb((byte)MyNumeShapeBackA.MyValue,
                (byte)MyNumeShapeBackR.MyValue, (byte)MyNumeShapeBackG.MyValue, (byte)MyNumeShapeBackB.MyValue));
        }
        //右クリックで終了
        //MyTempPointsからData作成してRootに追加
        private void MyDrawCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MyTempPoints.Count >= 2)
            {
                //    Data data = new(TType.Polyline)
                //    {
                //        HeadAngle = (double)MyNumeArrowHeadAngle.MyValue,
                //        Stroke = GetBrush(),
                //        StrokeThickness = (double)MyNumeStrokeThickness.MyValue,
                //        Fill = GetBrush(),
                //        PointCollection = new(MyTempPoints),
                //        HeadEndType = (HeadType)MyComboBoxLineHeadBeginType.SelectedValue,
                //    };
                //    FixTopLeftPointCollectionData(data);
                //    MyRoot.AddThumbDataToActiveGroup(data, MyAppConfig.IsAddUpper, false);
                AddShapePolyline(new(MyTempPoints), false);
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

        private void MyDrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MyTempPoints.Count < 2) { return; }
            Point pp = Mouse.GetPosition(MyDrawCanvas);
            MyTempPoints[^1] = pp;
        }

        /// <summary>
        /// 左クリックで頂点追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyDrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pp = Mouse.GetPosition(MyDrawCanvas);
            //最初だけは同時に2点追加
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
        #endregion マウスクリックでPolyline描画

        #region 図形のアンカーポイント編集開始、終了

        private void ContextAddAnchor_Click(object sender, RoutedEventArgs e)
        {
            //if (MyRoot.ClickedThumb is TThumb tShape)
            //{
            //    Point thumbMP = Mouse.GetPosition(tShape);
            //    Point canvasMP = Mouse.GetPosition(MyAnchorPointEditCanvas);

            //    AnchorThumb anchor = new(canvasMP);
            //    anchor.DragDelta += AnchorThumb_DragDelta;
            //    anchor.DragCompleted += AnchorThumb_DragCompleted;

            //    double beginD = TwoPointDistance(tShape.Data.PointCollection[0], thumbMP);
            //    double endD = TwoPointDistance(tShape.Data.PointCollection[^1], thumbMP);
            //    if (beginD > endD)
            //    {
            //        tShape.Data.PointCollection.Add(thumbMP);//頂点追加
            //        MyAnchoredThumbs.Add(anchor);//アンカーThumb追加
            //        MyAnchorPointEditCanvas.Children.Add(anchor);//アンカーThumb追加
            //    }
            //    else
            //    {
            //        tShape.Data.PointCollection.Insert(0, thumbMP);
            //        MyAnchoredThumbs.Insert(0, anchor);//アンカーThumb追加
            //        MyAnchorPointEditCanvas.Children.Insert(0, anchor);//アンカーThumb追加
            //    }



            //}
        }


        public double TwoPointDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p2.Y));
        }
        private void ContextAddAnchorExtend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonStartEditAnchor_Click(object sender, RoutedEventArgs e)
        {
            EditStartAnchor();
        }

        private void ButtonEndEditAnchor_Click(object sender, RoutedEventArgs e)
        {
            EditEndAnchor();
        }
        //アンカーポイントの編集開始
        private void EditStartAnchor()
        {
            if (MyRoot.ClickedThumb is TTPolyline thumb)
            {
                //MyAnchorPointEditCanvas.Cursor = Cursors.Hand;             
                thumb.MyAnchorVisible = Visibility.Visible;

                MyTabItemShape.DataContext = thumb.Data;
                MyNumeArrowHeadAngle.SetBinding(NumericUpDown.MyValueProperty, nameof(Data.HeadAngle));
                MyNumeStrokeThickness.SetBinding(NumericUpDown.MyValueProperty, nameof(Data.StrokeThickness));
                MyComboBoxLineHeadBeginType.SetBinding(ComboBox.SelectedValueProperty, nameof(Data.HeadBeginType));
                MyComboBoxLineHeadEndType.SetBinding(ComboBox.SelectedValueProperty, nameof(Data.HeadEndType));

                MyNumeShapeBackA.SetBinding(NumericUpDown.MyValueProperty, new Binding(nameof(Data.Stroke)) { ConverterParameter = thumb.Stroke, Converter = new MyConverterBrushColorA() });
                MyNumeShapeBackR.SetBinding(NumericUpDown.MyValueProperty, new Binding(nameof(Data.Stroke)) { ConverterParameter = thumb.Stroke, Converter = new MyConverterBrushColorR() });
                MyNumeShapeBackG.SetBinding(NumericUpDown.MyValueProperty, new Binding(nameof(Data.Stroke)) { ConverterParameter = thumb.Stroke, Converter = new MyConverterBrushColorG() });
                MyNumeShapeBackB.SetBinding(NumericUpDown.MyValueProperty, new Binding(nameof(Data.Stroke)) { ConverterParameter = thumb.Stroke, Converter = new MyConverterBrushColorB() });

              
            }
        }


        //アンカーポイントの編集終了
        private void EditEndAnchor()
        {
            if (MyRoot.ClickedThumb is TTPolyline thumb)
            {
                thumb.MyAnchorVisible = Visibility.Collapsed;
                MyTabItemShape.DataContext = null;
                MyNumeStrokeThickness.MyValue = (decimal)thumb.StrokeThickness;
                MyNumeArrowHeadAngle.MyValue = (decimal)thumb.Angle;
                MyComboBoxLineHeadBeginType.SelectedValue = thumb.HeadBeginType;
                MyComboBoxLineHeadEndType.SelectedValue = thumb.HeadEndType;

                //BindingOperations.ClearAllBindings(MyNumeShapeBackA);
                //BindingOperations.ClearAllBindings(MyNumeShapeBackR);
                //BindingOperations.ClearAllBindings(MyNumeShapeBackG);
                //BindingOperations.ClearAllBindings(MyNumeShapeBackB);
                SolidColorBrush brush = (SolidColorBrush)thumb.Stroke;
                MyNumeShapeBackA.MyValue = (decimal)brush.Color.A;
                MyNumeShapeBackR.MyValue = (decimal)brush.Color.R;
                MyNumeShapeBackG.MyValue = (decimal)brush.Color.G;
                MyNumeShapeBackB.MyValue = (decimal)brush.Color.B;
                //MyBorderShapeColor.Background = brush;

                //BindingOperations.ClearAllBindings(MyNumeArrowHeadAngle);
                //BindingOperations.ClearAllBindings(MyNumeStrokeThickness);
                //BindingOperations.ClearAllBindings(MyComboBoxLineHeadBeginType);
                //BindingOperations.ClearAllBindings(MyComboBoxLineHeadEndType);
            }
        }
        //private void EditEndAnchor()
        //{
        //    MyAnchorPointEditCanvas.Cursor = Cursors.Arrow;
        //    MyAnchorPointEditCanvas.Visibility = Visibility.Collapsed;
        //    MyAnchoredThumbs.Clear();
        //    MyAnchorPointEditCanvas.Children.Clear();

        //}

        //アンカーポイント移動中、対象Pointの更新
        private void AnchorThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is AnchorThumb anchor && MyRoot.ClickedThumb?.Data is Data tData)
            {
                double x = anchor.X + e.HorizontalChange;
                double y = anchor.Y + e.VerticalChange;
                if (x < 0 || y < 0)
                {
                    if (x < 0)
                    {
                        tData.X += x;
                        anchor.X = x;
                        foreach (var item in MyAnchoredThumbs)
                        {
                            item.X -= x;
                        }
                    }
                    if (y < 0)
                    {
                        tData.Y += y;
                        anchor.Y = y;
                        foreach (var item in MyAnchoredThumbs)
                        {
                            item.Y -= y;
                        }
                    }

                    //DataPointCollection全体をオフセット
                    for (int i = 0; i < tData.PointCollection.Count; i++)
                    {
                        tData.PointCollection[i] = new Point(MyAnchoredThumbs[i].X, MyAnchoredThumbs[i].Y);
                    }
                }
                else
                {
                    anchor.X = x; anchor.Y = y;
                    tData.PointCollection[MyAnchoredThumbs.IndexOf(anchor)] = new Point(x, y);
                }
                //anchor.X += e.HorizontalChange;
                //anchor.Y += e.VerticalChange;
                //tData.PointCollection[MyAnchoredThumbs.IndexOf(anchor)] = new Point(x, y);

                //if (MyRoot.ClickedThumb is TTPolylineZ pt)
                //{
                //    int ii = MyAnchoredThumbs.IndexOf(anchor);
                //    //pt.MyPoints[ii] = new Point(anchor.X - pt.TTLeft, anchor.Y - pt.TTTop);
                //    pt.MyPoints[ii] = new Point(anchor.X, anchor.Y);
                //}
            }
        }


        //アンカーポイント移動後、マイナス座標を修正
        private void AnchorThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //FixAnchorPoint(MyRoot.ClickedThumb?.Data);

            //if (sender is AnchorThumb anchor)
            //{
            //    var maiX = anchor.X;
            //    var maiY = anchor.Y;
            //    if (maiX < 0)
            //    {
            //        foreach (var item in MyAnchoredThumbs)
            //        {
            //            item.X -= maiX;
            //        }
            //    }
            //    if (maiY < 0)
            //    {
            //        foreach (var item in MyAnchoredThumbs)
            //        {
            //            item.Y -= maiY;
            //        }
            //    }
            //}
        }
        private void FixAnchorPoint(Data? thumbData)
        {
            if (thumbData == null) return;
            double x = double.MaxValue;
            double y = double.MaxValue;
            foreach (var item in thumbData.PointCollection)
            {
                if (x > item.X) { x = item.X; }
                if (y > item.Y) { y = item.Y; }
            }
            for (int i = 0; i < thumbData.PointCollection.Count; i++)
            {
                thumbData.PointCollection[i]
                    = new Point(
                        thumbData.PointCollection[i].X - x,
                        thumbData.PointCollection[i].Y - y);
            }
            thumbData.X += x;
            thumbData.Y += y;

            //for (int i = 0; i < MyAnchoredThumbs.Count; i++)
            //{
            //    MyAnchoredThumbs[i].X = thumbData.X + thumbData.PointCollection[i].X;
            //    MyAnchoredThumbs[i].Y = thumbData.Y + thumbData.PointCollection[i].Y;
            //}
        }




        #endregion 図形のアンカーポイント編集開始、終了

        #region 追加
        private void AddShapePolyline(PointCollection points, bool locateFix = true)
        {
            Data data = new(TType.Polyline)
            {
                HeadAngle = (double)MyNumeArrowHeadAngle.MyValue,
                Stroke = GetBrush(),
                StrokeThickness = (double)MyNumeStrokeThickness.MyValue,
                Fill = GetBrush(),
                PointCollection = points,
                HeadBeginType = (HeadType)MyComboBoxLineHeadBeginType.SelectedItem,
                HeadEndType = (HeadType)MyComboBoxLineHeadEndType.SelectedItem,
            };
            FixTopLeftPointCollectionData(data);
            MyRoot.AddThumbDataToActiveGroup(data, MyAppConfig.IsAddUpper, locateFix);
        }
        #endregion 追加
        #endregion 図形関連


        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {

        }
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

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            //if (MyRoot.ActiveThumb is TTTextBox textb)
            //{
            //    if (textb.IsFocused)
            //    {

            //        e.Handled = true;
            //    }
            //}
            //var isf = MytextBox.IsFocused;
            //var iskef = MytextBox.IsKeyboardFocused;
            //var iskfw = MytextBox.IsKeyboardFocusWithin;
            //if (!MytextBox.IsFocused) { e.Handled = true; }
            //e.Handled = true;
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //e.Handled = true;
        }

        private void NumericUpDown_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void ButtonRenderText_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MyTextBox.Text)) { return; }


            Data data = new(TType.TextBox)
            {
                Text = MyTextBox.Text,
                FontSize = (double)NumeFontSize.MyValue,
            };
            if (MyCombBoxFontFmilyNames.SelectedItem is KeyValuePair<string, FontFamily> kvp)
            {
                data.FontName = kvp.Key;
            }


            data.ForeColor = Color.FromArgb((byte)MyNumeFontA.MyValue,
                (byte)MyNumeFontR.MyValue, (byte)MyNumeFontG.MyValue, (byte)MyNumeFontB.MyValue);
            data.BackColor = Color.FromArgb((byte)MyNumeBackA.MyValue,
                (byte)MyNumeBackR.MyValue, (byte)MyNumeBackG.MyValue, (byte)MyNumeBackB.MyValue);
            data.BorderColor = Color.FromArgb((byte)MyNumeWakuA.MyValue,
                (byte)MyNumeWakuR.MyValue, (byte)MyNumeWakuG.MyValue, (byte)MyNumeWakuB.MyValue);
            data.BorderThickness = new Thickness((double)NumeWakuThickness.MyValue);
            if (MyCheckIsBold.IsChecked == true) { data.IsBold = true; }
            if (MyCheckIsItalic.IsChecked == true) { data.IsItalic = true; }

            MyRoot.AddThumbDataToActiveGroup(data, MyAppConfig.IsAddUpper);
        }

        //TestDrawPolyline
        private void ButtonTestDrawPolyline_Click(object sender, RoutedEventArgs e)
        {
            DrawPolylineFromClick();
        }

        private void ButtonAddShape_Click(object sender, RoutedEventArgs e)
        {
            AddShapePolyline(new PointCollection() { new Point(0, 0), new Point(100, 100) });
        }
    }

    public class MyConverterBrush : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte a = (byte)(decimal)values[0];
            byte r = (byte)(decimal)values[1];
            byte g = (byte)(decimal)values[2];
            byte b = (byte)(decimal)values[3];
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)value;
            object[] array = new object[4];
            array[0] = brush.Color.A;
            array[1] = brush.Color.R;
            array[2] = brush.Color.G;
            array[3] = brush.Color.B;
            return array;
        }
    }
    public class MyConverterBrushColorA : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            SolidColorBrush brush = (SolidColorBrush)value;
            return brush.Color.A;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)parameter;
            Color c = brush.Color;
            byte v = (byte)(decimal)value;
            return new SolidColorBrush(Color.FromArgb(v, c.R, c.G, c.B));
        }
    }
    public class MyConverterBrushColorR : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            SolidColorBrush brush = (SolidColorBrush)value;
            return brush.Color.R;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)parameter;
            Color c = brush.Color;
            byte v = (byte)(decimal)value;
            return new SolidColorBrush(Color.FromArgb(c.A, v, c.G, c.B));
        }
    }
    public class MyConverterBrushColorG : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            SolidColorBrush brush = (SolidColorBrush)value;
            return brush.Color.G;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)parameter;
            Color c = brush.Color;
            byte v = (byte)(decimal)value;
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, v, c.B));
        }
    }
    public class MyConverterBrushColorB : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            SolidColorBrush brush = (SolidColorBrush)value;
            return brush.Color.B;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)parameter;
            Color c = brush.Color;
            byte v = (byte)(decimal)value;
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, v));
        }
    }


    #region アプリの設定保存用Dataクラス

    /// <summary>
    /// アプリの設定値用クラス
    /// </summary>
    [DataContract]
    public class AppConfig : INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler? PropertyChanged;
        //private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        //private int _xShift;
        //[DataMember] public int XShift { get => _xShift; set => SetProperty(ref _xShift, value); }
        //private int _yShift;
        //[DataMember] public int YShift { get => _yShift; set => SetProperty(ref _yShift, value); }
        //private int _grid;
        //[DataMember] public int Grid { get => _grid; set => SetProperty(ref _grid, value); }

        //枠表示設定
        private WakuVisibleType _wakuVisibleType = WakuVisibleType.All;
        [DataMember]
        public WakuVisibleType WakuVisibleType
        {
            get => _wakuVisibleType;
            set => SetProperty(ref _wakuVisibleType, value);
        }
        //複数ファイル追加時の順番、昇順ソート、falseなら降順ソートになる
        private bool _isAscendingSort = true;
        [DataMember] public bool IsAscendingSort { get => _isAscendingSort; set => SetProperty(ref _isAscendingSort, value); }
        //Thumbは上側に追加する、falseなら下側に追加

        private bool _isAddUpper = true;
        [DataMember] public bool IsAddUpper { get => _isAddUpper; set => SetProperty(ref _isAddUpper, value); }





        [DataMember] public int JpegQuality { get; set; } = 94;//jpeg画質
        [DataMember] public double Top { get; set; }//アプリ
        [DataMember] public double Left { get; set; }//アプリ
                                                     //保存先リスト
        [DataMember] public ObservableCollection<string> DirList { get; set; }
        [DataMember] public string? Dir { get; set; }
        [DataMember] public int DirIndex { get; set; }

        //チェックボックス
        [DataMember] public bool? IsDrawCursor { get; set; }//マウスカーソル描画の有無


        //ホットキー
        [DataMember] public bool HotkeyAlt { get; set; }
        [DataMember] public bool HotkeyCtrl { get; set; }
        [DataMember] public bool HotkeyShift { get; set; }
        [DataMember] public bool HotkeyWin { get; set; }
        [DataMember] public Key HotKey { get; set; }//キャプチャーキー

        #region ファイルネーム

        //
        //[DataMember] public FileNameBaseType FileNameBaseType { get; set; }
        [DataMember] public bool IsFileNameDate { get; set; }
        [DataMember] public double FileNameDateOrder { get; set; }
        [DataMember] public string? FileNameDataFormat { get; set; }
        [DataMember] public ObservableCollection<string> FileNameDateFormatList { get; set; } = new();

        [DataMember] public bool IsFileNameSerial { get; set; }
        [DataMember] public decimal FileNameSerial { get; set; }
        [DataMember] public double FileNameSerialOrder { get; set; }
        [DataMember] public decimal FileNameSerialDigit { get; set; }
        [DataMember] public decimal FileNameSerialIncreace { get; set; }

        [DataMember] public bool IsFileNameText1 { get; set; }
        [DataMember] public string? FileNameText1 { get; set; }
        [DataMember] public ObservableCollection<string> FileNameText1List { get; set; } = new();

        [DataMember] public bool IsFileNameText2 { get; set; }
        [DataMember] public string? FileNameText2 { get; set; }
        [DataMember] public ObservableCollection<string> FileNameText2List { get; set; } = new();

        [DataMember] public bool IsFileNameText3 { get; set; }
        [DataMember] public string? FileNameText3 { get; set; }
        [DataMember] public ObservableCollection<string> FileNameText3List { get; set; } = new();

        [DataMember] public bool IsFileNameText4 { get; set; }
        [DataMember] public string? FileNameText4 { get; set; }
        [DataMember] public ObservableCollection<string> FileNameText4List { get; set; } = new();
        #endregion ファイルネーム

        //音
        [DataMember] public bool IsSoundPlay { get; set; }
        //[DataMember] public bool IsSoundDefault { get; set; }
        [DataMember] public ObservableCollection<string> SoundFilePathList { get; set; } = new();
        [DataMember] public string? SoundFilePath { get; set; }
        [DataMember] public MySoundPlay MySoundPlay { get; set; }


        //保存時の動作
        [DataMember] public SaveBehaviorType SaveBehaviorType { get; set; }

        //起動時の動作
        //前回終了時の編集状態を読み込む
        [DataMember] public bool IsLoadPreviewData { get; set; } = false;

        private ImageType _ImageType = ImageType.png;//保存画像形式
        [DataMember]
        public ImageType ImageType
        {
            get => _ImageType;
            set
            {
                _ImageType = value;
                //if (_ImageType == value) return;
                //_ImageType = value;
                //RaisePropertyChanged();
            }
        }

        private CaptureRectType _RectType;//切り出し範囲

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        [DataMember]
        public CaptureRectType RectType
        {
            get => _RectType;
            set
            {
                _RectType = value;
                //if (_RectType == value) return;
                //_RectType = value;
                //RaisePropertyChanged();
            }
        }




        public AppConfig()
        {
            DirList = new ObservableCollection<string>();
            JpegQuality = 94;
            FileNameSerialIncreace = 1m;
            FileNameSerialDigit = 4m;
            HotKey = Key.PrintScreen;
            IsDrawCursor = false;
            IsFileNameDate = true;
        }


        //        c# - DataContract、デフォルトのDataMember値
        //https://stackoverrun.com/ja/q/2220925

        //初期値の設定
        [OnDeserialized]
        void OnDeserialized(System.Runtime.Serialization.StreamingContext c)
        {
            DirList ??= new();
            FileNameDateFormatList ??= new();
            FileNameText1List ??= new();
            FileNameText2List ??= new();
            FileNameText3List ??= new();
            FileNameText4List ??= new();
            SoundFilePathList ??= new();
        }
    }

    #endregion アプリの設定保存用Dataクラス



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


}
