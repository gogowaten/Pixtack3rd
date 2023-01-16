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
    public enum TType { None = 0, TextBlock, Group, Image, Rectangle }


    [KnownType(typeof(DataImage)), KnownType(typeof(DataGroup)), KnownType(typeof(DataTextBlock))]
    public abstract class Data : IExtensibleDataObject, INotifyPropertyChanged
    {
        public ExtensionDataObject? ExtensionData { get; set; }
        private double _x;
        public double X { get => _x; set => SetProperty(ref _x, value); }

        private double _y;
        public double Y { get => _y; set => SetProperty(ref _y, value); }

        public TType Type { get; protected set; } = TType.None;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }


    public class DataGroup : Data
    {
        public ObservableCollection<Data> Datas { get; set; } = new();
        public DataGroup() { Type = TType.Group; }
    }


    public class DataTextBlock : Data
    {

        private string? _text;
        public string? Text { get => _text; set => SetProperty(ref _text, value); }

        public DataTextBlock() { Type = TType.TextBlock; }
    }

    public class DataImage : Data
    {
        //画像、それ自体は直接シリアライズしないので[IgnoreDataMember]
        //        [IgnoreDataMember] public BitmapSource? Source { get; set; }

        [IgnoreDataMember] private BitmapSource? _source;
        [IgnoreDataMember] public BitmapSource? Source { get => _source; set => SetProperty(ref _source, value); }

        //シリアライズ時の画像ファイル名に使用、Guidで一意の名前作成している
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public DataImage() { Type = TType.Image; }
    }
}
