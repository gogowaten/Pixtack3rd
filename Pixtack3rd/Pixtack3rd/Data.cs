using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;


namespace Pixtack3rd
{
    public enum TType { None = 0, Root, Group, TextBlock, Image, Rectangle }



    [DataContract]
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

        //Thumb追加位置

        private bool _isAddUpper = true;
        [DataMember] public bool IsAddUpper { get => _isAddUpper; set => SetProperty(ref _isAddUpper, value); }

        #endregion Group専用

        #region 共通


        [DataMember] public TType Type { get; set; }
        //[DataMember] public WakuVisibleType WakuVisibleType { get; set; }

        //private WakuVisibleType _wakuVisibleType;
        //[DataMember] public WakuVisibleType WakuVisibleType { get => _wakuVisibleType; set => SetProperty(ref _wakuVisibleType, value); }


        private double _x = 0.0;
        [DataMember] public double X { get => _x; set => SetProperty(ref _x, value); }

        private double _y;
        [DataMember] public double Y { get => _y; set => SetProperty(ref _y, value); }

        //シリアライズ時の画像ファイル名に使用、Guidで一意の名前作成している
        [DataMember] public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        #endregion 共通


        #region 固有


        private string _text = string.Empty;
        [DataMember] public string Text { get => _text; set => SetProperty(ref _text, value); }


        //画像、それ自体は直接シリアライズしないので[IgnoreDataMember]
        //        [IgnoreDataMember] public BitmapSource? BitmapSource { get; set; }

        [IgnoreDataMember] private BitmapSource? _bitmapSource;
        [IgnoreDataMember] public BitmapSource? BitmapSource { get => _bitmapSource; set => SetProperty(ref _bitmapSource, value); }

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
