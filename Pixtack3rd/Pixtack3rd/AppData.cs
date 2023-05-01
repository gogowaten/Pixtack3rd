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


    [DataContract]
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

        [DataMember]
        public Color TextForeColor
        {
            get { return (Color)GetValue(TextForeColorProperty); }
            set { SetValue(TextForeColorProperty, value); }
        }
        public static readonly DependencyProperty TextForeColorProperty =
            DependencyProperty.Register(nameof(TextForeColor), typeof(Color), typeof(AppData),
                new FrameworkPropertyMetadata(Colors.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [DataMember]
        public Color TextBackColor
        {
            get { return (Color)GetValue(TextBackColorProperty); }
            set { SetValue(TextBackColorProperty, value); }
        }
        public static readonly DependencyProperty TextBackColorProperty =
            DependencyProperty.Register(nameof(TextBackColor), typeof(Color), typeof(AppData),
                new FrameworkPropertyMetadata(Colors.Lavender,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [DataMember]

        public Color TextBorderColor
        {
            get { return (Color)GetValue(TextBorderColorProperty); }
            set { SetValue(TextBorderColorProperty, value); }
        }
        public static readonly DependencyProperty TextBorderColorProperty =
            DependencyProperty.Register(nameof(TextBorderColor), typeof(Color), typeof(AppData),
                new FrameworkPropertyMetadata(Colors.Gray,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 色

        #region 文字列描画
        [DataMember]

        public string FontName
        {
            get { return (string)GetValue(FontNameProperty); }
            set { SetValue(FontNameProperty, value); }
        }
        public static readonly DependencyProperty FontNameProperty =
            DependencyProperty.Register(nameof(FontName), typeof(string), typeof(AppData),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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
                new FrameworkPropertyMetadata(0.0,
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
                new FrameworkPropertyMetadata(0.0,
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


       [DataMember] public bool IsThumbAddUnder
        {
            get { return (bool)GetValue(IsThumbAddUnderProperty); }
            set { SetValue(IsThumbAddUnderProperty, value); }
        }
        public static readonly DependencyProperty IsThumbAddUnderProperty =
            DependencyProperty.Register(nameof(IsThumbAddUnder), typeof(bool), typeof(AppData), new PropertyMetadata(false));

        //保存画像形式
        private ImageType _imageType;
        [DataMember] public ImageType ImageType { get => _imageType; set => SetProperty(ref _imageType, value); }


        private bool _isDecendingSort;
       [DataMember] public bool IsDecendingSort { get => _isDecendingSort; set => SetProperty(ref _isDecendingSort, value); }

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
