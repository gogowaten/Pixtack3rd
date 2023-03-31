using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;


namespace Pixtack3rd
{
    public enum TType
    {
        None = 0, Root, Group, TextBlock, TextBox, Image, Rectangle,
        //Polyline,
        Geometric
    }



    [DataContract]
    [KnownType(typeof(FontFamily)), KnownType(typeof(KeyValuePair<string, FontFamily>)), KnownType(typeof(SolidColorBrush)),
        KnownType(typeof(MatrixTransform))]
    public class Data : IExtensibleDataObject, INotifyPropertyChanged
    {
        #region 必要

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ExtensionDataObject? ExtensionData { get; set; }//Dataの互換性維持
        #endregion 必要

        #region Group専用

        [DataMember] public ObservableCollection<Data> Datas { get; set; } = new();

        private int _xShift = 32;
        [DataMember] public int XShift { get => _xShift; set => SetProperty(ref _xShift, value); }

        private int _yShift = 32;
        [DataMember] public int YShift { get => _yShift; set => SetProperty(ref _yShift, value); }

        private int _grid;
        [DataMember] public int Grid { get => _grid; set => SetProperty(ref _grid, value); }

        //グループの表示非表示
        private bool _isVisibleThumb = false;
        [DataMember] public bool IsNotVisiblle { get => _isVisibleThumb; set => SetProperty(ref _isVisibleThumb, value); }

        #endregion Group専用

        #region 共通


        //[DataMember] public WakuVisibleType WakuVisibleType { get; set; }

        //private WakuVisibleType _wakuVisibleType;
        //[DataMember] public WakuVisibleType WakuVisibleType { get => _wakuVisibleType; set => SetProperty(ref _wakuVisibleType, value); }


        [DataMember] public TType Type { get; set; }
        //シリアライズ時の画像ファイル名に使用、Guidで一意の名前作成している
        [DataMember] public string Guid { get; set; } = System.Guid.NewGuid().ToString();

        private double _x = 0.0;
        private double _y = 0.0;
        private Color _foreColor = Color.FromArgb(255, 0, 0, 0);
        private Color _backColor = Color.FromArgb(0, 0, 0, 0);

        [DataMember] public double X { get => _x; set => SetProperty(ref _x, value); }
        [DataMember] public double Y { get => _y; set => SetProperty(ref _y, value); }
        [DataMember] public Color ForeColor { get => _foreColor; set => SetProperty(ref _foreColor, value); }
        [DataMember] public Color BackColor { get => _backColor; set => SetProperty(ref _backColor, value); }

        //枠の太さ
        private Thickness _borderThickness = new(0.0);
        [DataMember] public Thickness BorderThickness { get => _borderThickness; set => SetProperty(ref _borderThickness, value); }

        private Color _borderColor = Colors.LightGray;
        [DataMember] public Color BorderColor { get => _borderColor; set => SetProperty(ref _borderColor, value); }

        ////Thumb自体の枠の太さ
        //private Thickness _thumbWakuThickness = new(0.0);
        //[DataMember] public Thickness ThumbWakuThickness { get => _thumbWakuThickness; set => SetProperty(ref _thumbWakuThickness, value); }

        ////Thumb自体の枠の色
        //private Color _thumbWakuColor = Color.FromArgb(0, 0, 0, 0);
        //[DataMember] public Color ThumbWakuColor { get => _thumbWakuColor; set => SetProperty(ref _thumbWakuColor, value); }


        #endregion 共通


        #region 固有

        #region テキスト

        private string _text = string.Empty;
        private string _fontName = SystemFonts.MessageFontFamily.Source;
        private double _fontSize = SystemFonts.MessageFontSize;
        private bool _isBold;
        private bool _isItalic;

        [DataMember] public string Text { get => _text; set => SetProperty(ref _text, value); }

        [DataMember] public string FontName { get => _fontName; set => SetProperty(ref _fontName, value); }

        //フォントファミリーはシリアル化できない
        //private FontFamily _fontFamily;
        //[DataMember] public FontFamily FontFamily { get => _fontFamily; set => SetProperty(ref _fontFamily, value); }

        [DataMember] public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }
        [DataMember] public bool IsBold { get => _isBold; set => SetProperty(ref _isBold, value); }
        [DataMember] public bool IsItalic { get => _isItalic; set => SetProperty(ref _isItalic, value); }


        #endregion テキスト

        #region 画像

        //画像、それ自体は直接シリアライズしないので[IgnoreDataMember]
        //        [IgnoreDataMember] public BitmapSource? BitmapSource { get; set; }

        [IgnoreDataMember] private BitmapSource? _bitmapSource;
        [IgnoreDataMember] public BitmapSource? BitmapSource { get => _bitmapSource; set => SetProperty(ref _bitmapSource, value); }
        #endregion 画像

        #region 図形
        [DataMember] public PointCollection PointCollection { get; set; } = new();

        private double _strokeThickness = 1.0;
        [DataMember] public double StrokeThickness { get => _strokeThickness; set => SetProperty(ref _strokeThickness, value); }

        //private Brush _stroke = Brushes.Red;
        //[DataMember] public Brush Stroke { get => _stroke; set => SetProperty(ref _stroke, value); }

        private byte _strokeA;
        [DataMember] public byte StrokeA { get => _strokeA; set => SetProperty(ref _strokeA, value); }
        private byte _strokeR;
        [DataMember] public byte StrokeR { get => _strokeR; set => SetProperty(ref _strokeR, value); }
        private byte _strokeG;
        [DataMember] public byte StrokeG { get => _strokeG; set => SetProperty(ref _strokeG, value); }
        private byte _strokeB;
        [DataMember] public byte StrokeB { get => _strokeB; set => SetProperty(ref _strokeB, value); }


        private Brush _fill = Brushes.DarkOrange;
        [DataMember] public Brush Fill { get => _fill; set => SetProperty(ref _fill, value); }

        private HeadType _beginHeadType;
        [DataMember] public HeadType HeadBeginType { get => _beginHeadType; set => SetProperty(ref _beginHeadType, value); }

        private HeadType _endHeadType;
        [DataMember] public HeadType HeadEndType { get => _endHeadType; set => SetProperty(ref _endHeadType, value); }

        private double _headAngle = 30.0;
        [DataMember] public double HeadAngle { get => _headAngle; set => SetProperty(ref _headAngle, value); }

        private bool _isBezier;
        [DataMember] public bool IsBezier { get => _isBezier; set => SetProperty(ref _isBezier, value); }

        private ShapeType _shapeType;
        [DataMember] public ShapeType ShapeType { get => _shapeType; set => SetProperty(ref _shapeType, value); }

        private bool _isSmoothJoin;
        [DataMember] public bool IsSmoothJoin { get => _isSmoothJoin; set => SetProperty(ref _isSmoothJoin, value); }

        private bool _isLineClose;
        [DataMember] public bool IsLineClose { get => _isLineClose; set => SetProperty(ref _isLineClose, value); }


        #endregion 図形

        #endregion 固有

        #region コンストラクタ
        public Data(TType type)
        {

            Type = type;
            switch (type)
            {
                case TType.None:
                    break;
                case TType.Root:
                    Datas = new ObservableCollection<Data>();
                    break;
                case TType.Group:
                    Datas = new ObservableCollection<Data>();
                    break;
                case TType.TextBlock:
                    break;
                case TType.Image:
                    break;
                case TType.Rectangle:
                    break;
                case TType.TextBox:
                    break;
                case TType.Geometric:
                    break;
                default:
                    break;
            }
        }
        #endregion コンストラクタ

        #region ディープコピー


        /// <summary>
        /// ディープコピー、シリアライズを使ってディープコピー
        /// </summary>
        /// <returns></returns>
        public Data? DeepCopy()
        {
            try
            {
                using System.IO.MemoryStream stream = new();
                DataContractSerializer serializer = new(typeof(Data));
                serializer.WriteObject(stream, this);
                stream.Position = 0;
                if (serializer.ReadObject(stream) is Data data)
                {
                    //Guidはコピーしないで更新する
                    data.Guid = System.Guid.NewGuid().ToString();
                    //BitmapSourceはシリアライズできないので、BitmapFrameで複製
                    if (data.Type == TType.Image)
                    {
                        data.BitmapSource = BitmapFrame.Create(this.BitmapSource);
                    }

                    //Groupだった場合はDatasの要素もディープコピーする
                    else if (data.Type == TType.Root || data.Type == TType.Group)
                    {
                        DatasDeepCopy(this.Datas, data.Datas);
                    }
                    return data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            return null;
        }

        /// <summary>
        /// Datasの要素をディープコピーする
        /// </summary>
        /// <param name="moto"></param>
        /// <returns></returns>
        private void DatasDeepCopy(ObservableCollection<Data> moto, ObservableCollection<Data> saki)
        {
            for (int i = 0; i < moto.Count; i++)
            {
                Data motoItem = moto[i];
                Data sakiItem = saki[i];
                sakiItem.Guid = System.Guid.NewGuid().ToString();
                if (motoItem.Type == TType.Image)
                {
                    sakiItem.BitmapSource = BitmapFrame.Create(motoItem.BitmapSource);
                }
                else if (motoItem.Type == TType.Group)
                {
                    DatasDeepCopy(motoItem.Datas, sakiItem.Datas);
                }
            }
        }
        #endregion ディープコピー
    }

}
