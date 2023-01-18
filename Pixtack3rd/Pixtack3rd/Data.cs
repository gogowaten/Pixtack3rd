using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace Pixtack3rd
{
    public enum TType { None = 0, Root, Group, TextBlock, Image, Rectangle }


    //[KnownType(typeof(DataImage)), KnownType(typeof(DataGroup)), KnownType(typeof(DataTextBlock))]
    public class Data : IExtensibleDataObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<Data>? Datas { get; set; }

        private int _xShift;
        public int XShift { get => _xShift; set => SetProperty(ref _xShift, value); }

        private int _yShift;
        public int YShift { get => _yShift; set => SetProperty(ref _yShift, value); }

        private int _grid;
        public int Grid { get => _grid; set => SetProperty(ref _grid, value); }




        public TType Type { get; protected set; }
        public ExtensionDataObject? ExtensionData { get; set; }



        private double _x=0.0;
        public double X { get => _x; set => SetProperty(ref _x, value); }

        private double _y;
        public double Y { get => _y; set => SetProperty(ref _y, value); }

        private string? _text;
        public string? Text { get => _text; set => SetProperty(ref _text, value); }


        //画像、それ自体は直接シリアライズしないので[IgnoreDataMember]
        //        [IgnoreDataMember] public BitmapSource? Source { get; set; }

        [IgnoreDataMember] private BitmapSource? _source;
        [IgnoreDataMember] public BitmapSource? Source { get => _source; set => SetProperty(ref _source, value); }

        //シリアライズ時の画像ファイル名に使用、Guidで一意の名前作成している
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();


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


    }
}
