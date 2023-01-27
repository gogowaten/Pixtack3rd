using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Pixtack3rd
{
    internal class TTManager : Canvas, INotifyPropertyChanged
    {
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
        private TTGroup _activeGroup;
        public TTGroup ActiveGroup
        {
            get => _activeGroup;
            set
            {
                //ChildrenDragEventDesoption(_activeGroup, value);
                SetProperty(ref _activeGroup, value);
            }
        }

        #endregion 通知プロパティ

        public TTGroup Root { get; set; } = new TTGroup();
        public TTManager()
        {
            _activeGroup = Root;
        }











    }
}
