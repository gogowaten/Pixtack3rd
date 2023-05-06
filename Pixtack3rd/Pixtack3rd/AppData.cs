using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;

namespace Pixtack3rd
{
    //[DataContract]
    //public class AppDatas : DependencyObject
    //{
    //    [DataMember] public ObservableCollection<AppData> MyDatas { get; set; } = new();
    //    public AppDatas() { }
    //}


    //[DataContract]
    public class AppData : DependencyObject, IExtensibleDataObject, INotifyPropertyChanged
    {
        public AppData()
        {

        }

        #region 必要

        public ExtensionDataObject? ExtensionData { get; set; }// => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion 必要

        #region Window
        [DataMember]
        public double AppLeft
        {
            get { return (double)GetValue(AppLeftProperty); }
            set { SetValue(AppLeftProperty, value); }
        }
        public static readonly DependencyProperty AppLeftProperty =
            DependencyProperty.Register(nameof(AppLeft), typeof(double), typeof(AppData), new PropertyMetadata(100.0));

        [DataMember]
        public double AppTop
        {
            get { return (double)GetValue(AppTopProperty); }
            set { SetValue(AppTopProperty, value); }
        }
        public static readonly DependencyProperty AppTopProperty =
            DependencyProperty.Register(nameof(AppTop), typeof(double), typeof(AppData), new PropertyMetadata(100.0));

        [DataMember]
        public double AppWidth
        {
            get { return (double)GetValue(AppWidthProperty); }
            set { SetValue(AppWidthProperty, value); }
        }
        public static readonly DependencyProperty AppWidthProperty =
            DependencyProperty.Register(nameof(AppWidth), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(800.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [DataMember]
        public double AppHeight
        {
            get { return (double)GetValue(AppHeightProperty); }
            set { SetValue(AppHeightProperty, value); }
        }
        public static readonly DependencyProperty AppHeightProperty =
            DependencyProperty.Register(nameof(AppHeight), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(950.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [DataMember]

        public WindowState AppWindowState
        {
            get { return (WindowState)GetValue(AppWindowStateProperty); }
            set { SetValue(AppWindowStateProperty, value); }
        }
        public static readonly DependencyProperty AppWindowStateProperty =
            DependencyProperty.Register(nameof(AppWindowState), typeof(WindowState), typeof(AppData),
                new FrameworkPropertyMetadata(WindowState.Normal,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion Window

        #region 文字列色

        //[DataMember]
        //public Color TextForeColor
        //{
        //    get { return (Color)GetValue(TextForeColorProperty); }
        //    set { SetValue(TextForeColorProperty, value); }
        //}
        //public static readonly DependencyProperty TextForeColorProperty =
        //    DependencyProperty.Register(nameof(TextForeColor), typeof(Color), typeof(AppData),
        //        new FrameworkPropertyMetadata(Colors.Black,
        //            FrameworkPropertyMetadataOptions.AffectsRender |
        //            FrameworkPropertyMetadataOptions.AffectsMeasure |
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //[DataMember]
        //public Color TextBackColor
        //{
        //    get { return (Color)GetValue(TextBackColorProperty); }
        //    set { SetValue(TextBackColorProperty, value); }
        //}
        //public static readonly DependencyProperty TextBackColorProperty =
        //    DependencyProperty.Register(nameof(TextBackColor), typeof(Color), typeof(AppData),
        //        new FrameworkPropertyMetadata(Colors.Lavender,
        //            FrameworkPropertyMetadataOptions.AffectsRender |
        //            FrameworkPropertyMetadataOptions.AffectsMeasure |
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //[DataMember]

        //public Color TextBorderColor
        //{
        //    get { return (Color)GetValue(TextBorderColorProperty); }
        //    set { SetValue(TextBorderColorProperty, value); }
        //}
        //public static readonly DependencyProperty TextBorderColorProperty =
        //    DependencyProperty.Register(nameof(TextBorderColor), typeof(Color), typeof(AppData),
        //        new FrameworkPropertyMetadata(Colors.Gray,
        //            FrameworkPropertyMetadataOptions.AffectsRender |
        //            FrameworkPropertyMetadataOptions.AffectsMeasure |
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        //private byte _textForeColorA = 255;
        //[DataMember] public byte TextForeColorA { get => _textForeColorA; set => SetProperty(ref _textForeColorA, value); }
        //private byte _textForeColorR = 0;
        //[DataMember] public byte TextForeColorR { get => _textForeColorR; set => SetProperty(ref _textForeColorR, value); }
        //private byte _textForeColorG = 0;
        //[DataMember] public byte TextForeColorG { get => _textForeColorG; set => SetProperty(ref _textForeColorG, value); }
        //private byte _textForeColorB = 0;
        //[DataMember] public byte TextForeColorB { get => _textForeColorB; set => SetProperty(ref _textForeColorB, value); }

        [DataMember]
        public byte TextForeColorA
        {
            get { return (byte)GetValue(TextForeColorAProperty); }
            set { SetValue(TextForeColorAProperty, value); }
        }
        public static readonly DependencyProperty TextForeColorAProperty =
            DependencyProperty.Register(nameof(TextForeColorA), typeof(byte), typeof(AppData),
                new FrameworkPropertyMetadata((byte)250,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        [DataMember]
        public byte TextForeColorR
        {
            get { return (byte)GetValue(TextForeColorRProperty); }
            set { SetValue(TextForeColorRProperty, value); }
        }
        public static readonly DependencyProperty TextForeColorRProperty =
            DependencyProperty.Register(nameof(TextForeColorR), typeof(byte), typeof(AppData),
                new FrameworkPropertyMetadata((byte)210,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        [DataMember]
        public byte TextForeColorG
        {
            get { return (byte)GetValue(TextForeColorGProperty); }
            set { SetValue(TextForeColorGProperty, value); }
        }
        public static readonly DependencyProperty TextForeColorGProperty =
            DependencyProperty.Register(nameof(TextForeColorG), typeof(byte), typeof(AppData),
                new FrameworkPropertyMetadata((byte)0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [DataMember]
        public byte TextForeColorB
        {
            get { return (byte)GetValue(TextForeColorBProperty); }
            set { SetValue(TextForeColorBProperty, value); }
        }
        public static readonly DependencyProperty TextForeColorBProperty =
            DependencyProperty.Register(nameof(TextForeColorB), typeof(byte), typeof(AppData),
                new FrameworkPropertyMetadata((byte)0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        private byte _textBackColorA = 255;
        [DataMember] public byte TextBackColorA { get => _textBackColorA; set => SetProperty(ref _textBackColorA, value); }
        private byte _textBackColorR = 200;
        [DataMember] public byte TextBackColorR { get => _textBackColorR; set => SetProperty(ref _textBackColorR, value); }
        private byte _textBackColorG = 200;
        [DataMember] public byte TextBackColorG { get => _textBackColorG; set => SetProperty(ref _textBackColorG, value); }
        private byte _textBackColorB = 200;
        [DataMember] public byte TextBackColorB { get => _textBackColorB; set => SetProperty(ref _textBackColorB, value); }


        private byte _textBorderColorA = 255;
        [DataMember] public byte TextBorderColorA { get => _textBorderColorA; set => SetProperty(ref _textBorderColorA, value); }
        private byte _textBorderColorR = 128;
        [DataMember] public byte TextBorderColorR { get => _textBorderColorR; set => SetProperty(ref _textBorderColorR, value); }
        private byte _textBorderColorG = 128;
        [DataMember] public byte TextBorderColorG { get => _textBorderColorG; set => SetProperty(ref _textBorderColorG, value); }
        private byte _textBorderColorB = 128;
        [DataMember] public byte TextBorderColorB { get => _textBorderColorB; set => SetProperty(ref _textBorderColorB, value); }


        #endregion 文字列色

        #region 文字列描画
        //依存関係プロパティがあるけど、通知プロパティでも問題なさそう
        [DataMember]

        public string FontName
        {
            get { return (string)GetValue(FontNameProperty); }
            set { SetValue(FontNameProperty, value); }
        }
        public static readonly DependencyProperty FontNameProperty =
            DependencyProperty.Register(nameof(FontName), typeof(string), typeof(AppData),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        //private double _fontSize = 50.0;
        //[DataMember] public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(50.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        private double _textBoxBorderWidth = 1.0;
        [DataMember] public double TextBoxBorderWidth { get => _textBoxBorderWidth; set => SetProperty(ref _textBoxBorderWidth, value); }


        private bool _isTextBold = false;
        [DataMember] public bool IsTextBold { get => _isTextBold; set => SetProperty(ref _isTextBold, value); }
        private bool _isTextItalic = false;
        [DataMember] public bool IsTextItalic { get => _isTextItalic; set => SetProperty(ref _isTextItalic, value); }

        #endregion 文字列描画

        #region 図形描画
        [DataMember]

        public double StrokeWidth
        {
            get { return (double)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }
        public static readonly DependencyProperty StrokeWidthProperty =
            DependencyProperty.Register(nameof(StrokeWidth), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(10.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        private byte _strokeColorA;
        public byte StrokeColorA { get => _strokeColorA; set => SetProperty(ref _strokeColorA, value); }

        #endregion 図形描画

        #region 範囲選択Thumb
        [DataMember]

        public double AreaLeft
        {
            get { return (double)GetValue(AreaLeftProperty); }
            set { SetValue(AreaLeftProperty, value); }
        }
        public static readonly DependencyProperty AreaLeftProperty =
            DependencyProperty.Register(nameof(AreaLeft), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(10.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        [DataMember]

        public double AreaTop
        {
            get { return (double)GetValue(AreaTopProperty); }
            set { SetValue(AreaTopProperty, value); }
        }
        public static readonly DependencyProperty AreaTopProperty =
            DependencyProperty.Register(nameof(AreaTop), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(10.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [DataMember]

        public double AreaWidth
        {
            get { return (double)GetValue(AreaWidthProperty); }
            set { SetValue(AreaWidthProperty, value); }
        }
        public static readonly DependencyProperty AreaWidthProperty =
            DependencyProperty.Register(nameof(AreaWidth), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(100.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [DataMember]

        public double AreaHeight
        {
            get { return (double)GetValue(AreaHeightProperty); }
            set { SetValue(AreaHeightProperty, value); }
        }
        public static readonly DependencyProperty AreaHeightProperty =
            DependencyProperty.Register(nameof(AreaHeight), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(100.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 範囲選択Thumb


        [DataMember]
        public bool IsThumbAddUnder
        {
            get { return (bool)GetValue(IsThumbAddUnderProperty); }
            set { SetValue(IsThumbAddUnderProperty, value); }
        }
        public static readonly DependencyProperty IsThumbAddUnderProperty =
            DependencyProperty.Register(nameof(IsThumbAddUnder), typeof(bool), typeof(AppData), new PropertyMetadata(false));


        #region ファイル保存、読み込み

        //保存画像形式
        private ImageType _imageType;
        [DataMember] public ImageType ImageType { get => _imageType; set => SetProperty(ref _imageType, value); }

        //Jepg形式で保存時の品質
        private int _jpegQuality;
        public int JpegQuality { get => _jpegQuality; set => SetProperty(ref _jpegQuality, value); }

        //複数ファイルを開いたとき、ファイル名で降順ソート
        private bool _isDecendingSortFileName;
        [DataMember] public bool IsDecendingSortFileName { get => _isDecendingSortFileName; set => SetProperty(ref _isDecendingSortFileName, value); }


        #endregion ファイル保存、読み込み

        //枠表示設定
        private WakuVisibleType _wakuVisibleType = WakuVisibleType.All;
        [DataMember]
        public WakuVisibleType WakuVisibleType
        {
            get => _wakuVisibleType;
            set => SetProperty(ref _wakuVisibleType, value);
        }

    }


}
