﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.Specialized;

namespace Pixtack3rd
{
    [DebuggerDisplay(nameof(Type))]
    public abstract class TThumb : Thumb
    {
        //依存プロパティは主にデザイナー画面で、要素を追加して表示の確認する用
        #region 依存プロパティ

        public double TTLeft
        {
            get { return (double)GetValue(TTLeftProperty); }
            set { SetValue(TTLeftProperty, value); }
        }
        public static readonly DependencyProperty TTLeftProperty =
            DependencyProperty.Register(nameof(TTLeft), typeof(double), typeof(TThumb),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double TTTop
        {
            get { return (double)GetValue(TTTopProperty); }
            set { SetValue(TTTopProperty, value); }
        }
        public static readonly DependencyProperty TTTopProperty =
            DependencyProperty.Register(nameof(TTTop), typeof(double), typeof(TThumb),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存プロパティ
        protected readonly string TEMPLATE_NAME = "NEMO";
        public TTGroup? TTParent { get; set; } = null;//親Group
        public TType Type { get; set; }
        public TThumb()
        {
            SetBinding(Canvas.LeftProperty, new Binding()
            {
                Path = new PropertyPath(TTLeftProperty),
                Source = this
            });
            SetBinding(Canvas.TopProperty, new Binding()
            {
                Path = new PropertyPath(TTTopProperty),
                Source = this
            });


        }
        protected T? MakeTemplate<T>()
        {
            FrameworkElementFactory factory = new(typeof(T), TEMPLATE_NAME);
            this.Template = new() { VisualTree = factory };
            this.ApplyTemplate();
            if (this.Template.FindName(TEMPLATE_NAME, this) is T element)
            {
                return element;
            }
            else return default;
        }
        protected void MySetXYBinging(Data data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            Type = data.Type;
            SetBinding(Canvas.LeftProperty, nameof(data.X));
            SetBinding(Canvas.TopProperty, nameof(data.Y));
        }

    }


    //[DebuggerDisplay("Name = {" + nameof(Name) + "}")]
    [ContentProperty(nameof(Thumbs))]
    public class TTGroup : TThumb
    {
        public DataGroup Data { get; set; }
        private ItemsControl MyTemplateElement;
        public ObservableCollection<TThumb> Thumbs { get; private set; } = new();



        public TTGroup() : this(new DataGroup())
        {

        }
        public TTGroup(DataGroup data)
        {
            Thumbs.CollectionChanged += Thumbs_CollectionChanged;
            Data = data;
            MyTemplateElement = MyInitializeBinding();
            MyTemplateElement.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Thumbs)) { Source = this });
        }

        private void Thumbs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems?[0] is TThumb thumb)
                    {
                        thumb.TTParent = this;
                    }
                    break;
                default: break;
            }
        }

        private ItemsControl MyInitializeBinding()
        {
            this.DataContext = Data;
            SetBinding(TTLeftProperty, nameof(Data.X));
            SetBinding(TTTopProperty, nameof(Data.Y));
            MySetXYBinging(this.Data);
            return MakeTemplate();

        }
        private ItemsControl MakeTemplate()
        {
            FrameworkElementFactory factory = new(typeof(ItemsControl), TEMPLATE_NAME);
            factory.SetValue(ItemsControl.ItemsPanelProperty,
                new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas))));
            this.Template = new() { VisualTree = factory };
            this.ApplyTemplate();
            if (this.Template.FindName(TEMPLATE_NAME, this) is ItemsControl element)
            {
                return element;
            }
            else { throw new ArgumentException("テンプレート作成できんかった"); }
        }
        public void AddItem(TThumb thumb, Data data)
        {
            Thumbs.Add(thumb);
            Data.Datas.Add(data);
        }
        public void RemoveItem(TThumb thumb, Data data)
        {
            Thumbs.Remove(thumb);
            Data.Datas.Remove(data);
        }
        public void AddItem(Data data)
        {
            switch (data.Type)
            {
                case TType.None:
                    break;
                case TType.TextBlock:
                    AddItem(new TTTextBlock((DataTextBlock)data), data);
                    break;
                case TType.Group:
                    AddItem(new TTGroup((DataGroup)data), data);
                    break;
                case TType.Image:
                    AddItem(new TTImage((DataImage)data), data);
                    break;
                case TType.Rectangle:
                    throw new NotImplementedException();
            }
        }

        //TTGroupのRect取得
        public static (double x, double y, double w, double h) GetRect(TTGroup? group)
        {
            if (group == null) { return (0, 0, 0, 0); }
            return GetRect(group.Thumbs);
        }
        public static (double x, double y, double w, double h) GetRect(IEnumerable<TThumb> thumbs)
        {
            double x = double.MaxValue, y = double.MaxValue;
            double w = 0, h = 0;
            if (thumbs != null)
            {
                foreach (var item in thumbs)
                {
                    var left = item.TTLeft; if (x > left) x = left;
                    var top = item.TTTop; if (y > top) y = top;
                    var width = left + item.ActualWidth;
                    var height = top + item.ActualHeight;
                    if (w < width) w = width;
                    if (h < height) h = height;
                }
            }
            return (x, y, w, h);
        }

        /// <summary>
        /// サイズと位置の更新
        /// </summary>
        public void TTGroupUpdateLayout()
        {
            //Rect取得
            (double x, double y, double w, double h) = GetRect(this);

            //子要素位置修正
            foreach (var item in Thumbs)
            {
                item.TTLeft -= x;
                item.TTTop -= y;
            }
            //自身がRoot以外なら自身の位置を更新
            if (this.GetType() != typeof(TTRoot))
            {
                TTLeft += x;
                TTTop += y;
            }

            //自身のサイズ更新
            w -= x; h -= y;
            if (w < 0) w = 0;
            if (h < 0) h = 0;
            if (w >= 0) Width = w;
            if (h >= 0) Height = h;

            //必要、これがないと見た目が変化しない、実行直後にSizeChangedが発生
            UpdateLayout();

            //親要素Groupがあれば遡って更新
            if (TTParent is TTGroup parent)
            {
                parent.TTGroupUpdateLayout();
            }
        }
    }


    public class TTRoot : TTGroup, INotifyPropertyChanged
    {
        #region 通知プロパティ

        protected void SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private TThumb? _clickedThumb;
        public TThumb? ClickedThumb { get => _clickedThumb; set => SetProperty(ref _clickedThumb, value); }
        private TThumb? _activeThumb;
        public TThumb? ActiveThumb { get => _activeThumb; set => SetProperty(ref _activeThumb, value); }

        private TTGroup _activeGroup;
        public TTGroup ActiveGroup
        {
            get => _activeGroup;
            set
            {
                ChildrenDragEventDesoption(_activeGroup, value);
                SetProperty(ref _activeGroup, value);
            }
        }
        #endregion 通知プロパティ

        //選択状態の要素を保持
        public ObservableCollection<TThumb> SelectedThumbs { get; private set; } = new();

        //クリック前の選択状態、クリックUp時の削除に使う
        private bool IsSelectedPreviewMouseDown { get; set; }

        public TTRoot()
        {
            _activeGroup ??= this;
        }

        #region ドラッグ移動
        //ActiveGroup用、ドラッグ移動イベント脱着
        private void ChildrenDragEventDesoption(TTGroup removeTarget, TTGroup addTarget)
        {
            foreach (var item in removeTarget.Thumbs)
            {
                item.DragDelta -= Thumb_DragDelta;
                item.DragCompleted -= Thumb_DragCompleted;
            }
            foreach (var item in addTarget.Thumbs)
            {
                item.DragDelta += Thumb_DragDelta;
                item.DragCompleted += Thumb_DragCompleted;
            }
        }
        private void Thumb_DragDelta(object seneer, DragDeltaEventArgs e)
        {
            //複数選択時は全てを移動
            foreach (TThumb item in SelectedThumbs)
            {
                item.TTLeft += e.HorizontalChange;
                item.TTTop += e.VerticalChange;
            }
        }
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is TThumb thumb) { thumb.TTParent?.TTGroupUpdateLayout(); }
        }
        #endregion ドラッグ移動

        #region オーバーライド関連

        //起動直後、自身がActiveGroupならChildrenにドラッグ移動登録
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (ActiveGroup == this)
            {
                foreach (var item in Thumbs)
                {
                    item.DragDelta += Thumb_DragDelta;
                    item.DragCompleted += Thumb_DragCompleted;
                }
            }
        }

        //クリックしたとき、ClickedThumbの更新とActiveThumbの更新、SelectedThumbsの更新
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);//要る？

            //OriginalSourceにテンプレートに使っている要素が入っているので、
            //そのTemplateParentプロパティから目的のThumbが取得できる
            if (e.OriginalSource is FrameworkElement el && el.TemplatedParent is TThumb clicked)
            {
                ClickedThumb = clicked;
                TThumb? active = GetActiveThumb(clicked);
                if (active != ActiveThumb)
                {
                    ActiveThumb = active;
                }
                //SelectedThumbsの更新
                if (active != null)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        if (SelectedThumbs.Contains(active) == false)
                        {
                            SelectedThumbs.Add(active);
                            IsSelectedPreviewMouseDown = false;
                        }
                        else
                        {
                            //フラグ
                            //ctrl+クリックされたものがもともと選択状態だったら
                            //マウスアップ時に削除するためのフラグ
                            IsSelectedPreviewMouseDown = true;
                        }
                    }
                    else
                    {
                        if (SelectedThumbs.Contains(active) == false)
                        {
                            SelectedThumbs.Clear();
                            SelectedThumbs.Add(active);
                            IsSelectedPreviewMouseDown = false;
                        }
                    }
                }
                else { IsSelectedPreviewMouseDown = false; }
            }
        }

        //マウスレフトアップ、フラグがあればSelectedThumbsから削除する
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //
            if (SelectedThumbs.Count > 1 && IsSelectedPreviewMouseDown && ActiveThumb != null)
            {
                SelectedThumbs.Remove(ActiveThumb);//削除
                IsSelectedPreviewMouseDown = false;//フラグ切り替え
                ActiveThumb = null;
                ClickedThumb = null;
            }

        }

        #endregion オーバーライド関連

        #region その他関数

        private bool CheckIsActive(TThumb thumb)
        {
            if (thumb.TTParent is TTGroup ttg && ttg == ActiveGroup)
            {
                return true;
            }
            return false;

        }
        //起点からActiveThumbをサーチ
        //ActiveはActiveThumbのChildrenの中で起点に連なるもの
        private TThumb? GetActiveThumb(TThumb? start)
        {
            if (start == null) { return null; }
            if (CheckIsActive(start))
            {
                return start;
            }
            else if (start.TTParent is TTGroup ttg)
            {
                return GetActiveThumb(ttg);
            }
            return null;
        }
        /// <summary>
        /// SelectedThumbsを並べ替えたList作成、基準はActiveGroupのChildren
        /// </summary>
        /// <param name="selected">SelectedThumbs</param>
        /// <param name="group">並べ替えの基準にするGroup</param>
        /// <returns></returns>
        private List<TThumb> MakeSortedList(IEnumerable<TThumb> selected, TTGroup group)
        {
            List<TThumb> tempList = new();
            foreach (var item in group.Thumbs)
            {
                if (selected.Contains(item)) { tempList.Add(item); }
            }
            return tempList;
        }
        /// <summary>
        /// 要素すべてがGroupのChildrenに存在するか判定
        /// </summary>
        /// <param name="thums">要素群</param>
        /// <param name="group">ParentGroup</param>
        /// <returns></returns>
        private bool IsAllContains(IEnumerable<TThumb> thums, TTGroup group)
        {
            if (!thums.Any()) { return false; }//要素が一つもなければfalse
            foreach (var item in thums)
            {
                if (group.Thumbs.Contains(item) == false)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion その他関数

        #region 追加と削除
        //基本的にActiveThumbのChildrenに対して行う
        //削除対象はActiveThumbになる
        //ドラッグ移動イベントの着脱も行う
        public void AddThumb(TThumb thumb)
        {
            AddThumb(thumb, ActiveGroup);
        }
        /// <summary>
        /// 追加先Groupを指定して追加
        /// </summary>
        /// <param name="thumb">追加する子要素</param>
        /// <param name="destGroup">追加先Group</param>
        public void AddThumb(TThumb thumb, TTGroup destGroup)
        {
            destGroup.Thumbs.Add(thumb);
            //ドラッグ移動イベント付加
            thumb.DragDelta += Thumb_DragDelta;
            thumb.DragCompleted += Thumb_DragCompleted;
        }


        /// <summary>
        /// 選択Thumbすべてを削除
        /// </summary>
        /// <returns></returns>
        public bool RemoveThumb()
        {
            if (SelectedThumbs == null) return false;
            bool flag = true;
            foreach (var item in SelectedThumbs.ToArray())
            {
                if (RemoveThumb(item, ActiveGroup) == false)
                {
                    flag = false;
                }
                else
                {
                    SelectedThumbs.Remove(item);
                }
            }
            ClickedThumb = null;
            ActiveThumb = null;
            return flag;
        }
        /// <summary>
        /// 指定Thumbだけを指定Groupから削除
        /// </summary>
        /// <param name="thumb"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool RemoveThumb(TThumb thumb, TTGroup group)
        {
            if (group.Thumbs.Remove(thumb))
            {
                thumb.DragCompleted -= Thumb_DragCompleted;
                thumb.DragDelta -= Thumb_DragDelta;
                group.TTGroupUpdateLayout();
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion 追加と削除

        #region グループ化

        //基本的にSelectedThumbsの要素群でグループ化、それをActiveGroupに追加する
        public void AddGroup()
        {
            //選択要素群をActiveGroupを基準に並べ替え
            var tempList = MakeSortedList(SelectedThumbs, ActiveGroup);

            TTGroup? group = MakeAndAddGroup(tempList, ActiveGroup);
            if (group != null)
            {
                SelectedThumbs.Clear();
                SelectedThumbs.Add(group);
                ActiveThumb = group;
            }
        }
        /// <summary>
        /// グループ化
        /// </summary>
        /// <param name="thumbs">グループ化する要素群</param>
        /// <param name="destGroup">新グループの追加先</param>
        private TTGroup? MakeAndAddGroup(IEnumerable<TThumb> thumbs, TTGroup destGroup)
        {
            if (CheckAddGroup(thumbs, destGroup) == false) { return null; }
            var (x, y, w, h) = GetRect(thumbs);
            TTGroup group = new() { Name = "new_group", TTLeft = x, TTTop = y };
            //各要素のドラッグイベントを外す、新グループに追加
            foreach (var item in thumbs)
            {
                destGroup.Thumbs.Remove(item);
                item.DragDelta -= Thumb_DragDelta;
                item.DragCompleted -= Thumb_DragCompleted;

                group.Thumbs.Add(item);
                item.TTLeft -= x;
                item.TTTop -= y;
            }
            AddThumb(group, destGroup);

            group.Arrange(new(0, 0, w, h));//再配置？このタイミングで必須、Actualサイズに値が入る
            group.TTGroupUpdateLayout();//必須、サイズと位置の更新

            return group;
        }
        private static bool CheckAddGroup(IEnumerable<TThumb> thumbs, TTGroup destGroup)
        {
            if (thumbs.Count() < 2) { return false; }
            if (thumbs.Count() == destGroup.Thumbs.Count) { return false; }
            foreach (TThumb thumb in thumbs)
            {
                if (destGroup.Thumbs.Contains(thumb) == false) { return false; }
            }
            return true;
        }
        #endregion グループ化
        #region グループ解除
        /// <summary>
        /// ActiveThumbのグループ解除
        /// </summary>
        public void UnGroup()
        {
            if (ActiveThumb is TTGroup group)
            {
                UnGroup(group, ActiveGroup);
                SelectedThumbs.Clear();
                ActiveThumb = GetActiveThumb(ClickedThumb);
            }
        }
        /// <summary>
        /// 指定グループのグループ解除
        /// </summary>
        /// <param name="group">解除するグループを指定</param>
        /// <param name="destGroup">指定グループの親Group</param>
        public void UnGroup(TTGroup group, TTGroup destGroup)
        {
            //解除対象のグループの要素群に対して
            //ドラッグイベント解除＋解除対象から削除してから
            //その親Groupに追加＋ドラッグイベント追加
            //位置修正
            foreach (var item in group.Thumbs.ToArray())
            {
                group.Thumbs.Remove(item);
                item.DragDelta -= Thumb_DragDelta;
                item.DragCompleted -= Thumb_DragCompleted;

                destGroup.Thumbs.Add(item);
                item.DragDelta += Thumb_DragDelta;
                item.DragCompleted += Thumb_DragCompleted;
                item.TTLeft += group.TTLeft;
                item.TTTop += group.TTTop;
            }
            //抜け殻になった元のグループ要素削除
            destGroup.Thumbs.Remove(group);
            group.DragCompleted -= Thumb_DragCompleted;//いる？
            group.DragDelta -= Thumb_DragDelta;
        }
        #endregion グループ解除

        #region InOut、ActiveThumbの切り替え
        //ActiveThumbを内側(ActiveThumbの親)へ切り替える
        public void ActiveGroupInside()
        {
            if (ActiveThumb is TTGroup group)
            {
                ActiveGroup = group;
                ActiveThumb = GetActiveThumb(ClickedThumb);
                SelectedThumbs.Clear();
            }
        }

        //ActiveThumbを外側(親)へ切り替える
        public void ActiveGroupOutside()
        {
            if (ActiveGroup.TTParent is TTGroup parent)
            {
                ActiveGroup = parent;
                ActiveThumb = GetActiveThumb(ClickedThumb);
                SelectedThumbs.Clear();
            }
        }
        #endregion InOut、ActiveThumbの切り替え

        #region ZIndex
        //ZIndexが同じ場合はThumbsIndexが前後関係になるのを利用して
        //Thumbs要素の入れ替えによって前面、背面移動させる
        #region 背面に移動


        /// <summary>
        /// 選択Thumbを最背面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZDownBackMost()
        {
            return ZDownBackMost(SelectedThumbs, ActiveGroup);
        }
        /// <summary>
        /// 指定Thumbを最背面へ移動
        /// </summary>
        /// <param name="thumbs">移動させるThumb群</param>
        /// <param name="group">Thumb群の親Group</param>
        /// <returns></returns>
        public bool ZDownBackMost(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            if (IsAllContains(thumbs, group) == false) { return false; }
            //下側にある要素から処理したいので、並べ替えたListを作成
            List<TThumb> tempList = MakeSortedList(thumbs, group);
            //削除してから先頭から挿入
            for (int i = 0; i < tempList.Count; i++)
            {
                if (group.Thumbs.Remove(tempList[i]))
                {
                    group.Thumbs.Insert(i, tempList[i]);
                }
                else { return false; }
            }
            return true;
        }
        /// <summary>
        /// 選択Thumbを背面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZDown()
        {
            return ZDown(SelectedThumbs, ActiveGroup);
        }
        /// <summary>
        /// 指定Thumbを背面へ移動
        /// </summary>
        /// <param name="thumbs"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool ZDown(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            if (IsAllContains(thumbs, group) == false) { return false; }
            //順番を揃えたリスト作成
            List<TThumb> tempList = MakeSortedList(thumbs, group);

            //一番下の要素がもともと一番下だった場合は処理しない
            if (group.Thumbs[0] == tempList[0]) { return false; }

            //順番に処理、削除してから挿入、挿入箇所は元のインデックス-1
            foreach (var item in tempList)
            {
                int ii = group.Thumbs.IndexOf(item);
                ii--;
                if (group.Thumbs.Remove(item))
                {
                    group.Thumbs.Insert(ii, item);
                }
                else
                {
                    return false;
                    throw new ArgumentException("対象要素が親要素から見つからなかった");
                }
            }

            return true;
        }
        #endregion 背面に移動

        #region 前面に移動

        /// <summary>
        /// 指定Thumbを最前面へ移動
        /// </summary>
        /// <param name="thumbs">移動させるThumb群</param>
        /// <param name="group">親Group</param>
        /// <returns></returns>
        public bool ZUpFrontMost(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            //要素すべてがGroupのChildrenに存在するか判定、存在しない要素があれば処理しない            
            if (IsAllContains(thumbs, group) == false) { return false; }

            //下側にある要素から処理したいので、並べ替えたListを作成
            List<TThumb> tempList = MakeSortedList(thumbs, group);
            //削除してから追加(末尾に追加)
            foreach (var item in tempList)
            {
                if (group.Thumbs.Remove(item))
                {
                    group.Thumbs.Add(item);
                }
                else { return false; }
            }
            return true;
        }
        /// <summary>
        /// 選択Thumbを最前面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZUpFrontMost()
        {
            return ZUpFrontMost(SelectedThumbs, ActiveGroup);
        }

        /// <summary>
        /// 指定Thumbを前面へ移動
        /// </summary>
        /// <param name="thumbs"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool ZUp(IEnumerable<TThumb> thumbs, TTGroup group)
        {
            if (IsAllContains(thumbs, group) == false) { return false; }
            //順番を揃えてから削除して追加
            List<TThumb> tempList = MakeSortedList(thumbs, group);

            //一番上の要素がもともと一番上だった場合は処理しない
            if (group.Thumbs[^1] == tempList[^1])
            {
                return false;
            }
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                int ii = group.Thumbs.IndexOf(tempList[i]);
                ii++;
                if (group.Thumbs.Remove(tempList[i]))
                {
                    group.Thumbs.Insert(ii, tempList[i]);
                }
                else { return false; }
            }
            return true;

        }
        /// <summary>
        /// 選択Thumbを前面へ移動
        /// </summary>
        /// <returns></returns>
        public bool ZUp()
        {
            return ZUp(SelectedThumbs, ActiveGroup);
        }
        #endregion 前面に移動


        //backmost 最背面 back to back最背面にする
        //frontmsot 一番前
        //front 最前面
        //put one back  一つ後ろにする
        #endregion ZIndex


    }




    public class TTTextBlock : TThumb
    {
        #region 依存プロパティ

        public string TTText
        {
            get { return (string)GetValue(TTTextProperty); }
            set { SetValue(TTTextProperty, value); }
        }
        public static readonly DependencyProperty TTTextProperty =
            DependencyProperty.Register(nameof(TTText), typeof(string), typeof(TTTextBlock),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion 依存プロパティ

        public DataTextBlock Data { get; set; }
        private readonly TextBlock MyTemplateElement;

        public TTTextBlock() : this(new DataTextBlock()) { }
        public TTTextBlock(DataTextBlock data)
        {
            Data = data;
            this.DataContext = Data;
            if (MakeTemplate<TextBlock>() is TextBlock element)
            {
                MyTemplateElement = element;
            }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            SetBinding(TTTextProperty, nameof(Data.Text));
            MyTemplateElement.SetBinding(TextBlock.TextProperty, nameof(Data.Text));
            MySetXYBinging(this.Data);
        }
    }


    public class TTImage : TThumb
    {

        //public BitmapSource TTSource
        //{
        //    get { return (BitmapSource)GetValue(TTSourceProperty); }
        //    set { SetValue(TTSourceProperty, value); }
        //}
        //public static readonly DependencyProperty TTSourceProperty =
        //    DependencyProperty.Register(nameof(TTSource), typeof(BitmapSource), typeof(TTImage),
        //        new FrameworkPropertyMetadata(null,
        //            FrameworkPropertyMetadataOptions.AffectsRender |
        //            FrameworkPropertyMetadataOptions.AffectsMeasure));

        public string TTSourcePath
        {
            get { return (string)GetValue(TTSourcePathProperty); }
            set { SetValue(TTSourcePathProperty, value); }
        }
        public static readonly DependencyProperty TTSourcePathProperty =
            DependencyProperty.Register(nameof(TTSourcePath), typeof(string), typeof(TTImage),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTTUriChanged)));

        private static void OnTTUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TTImage obj)
            {
                obj.Data.Source = new BitmapImage(new Uri((string)e.NewValue));
            }
        }

        public DataImage Data { get; set; }
        private readonly Image MyTemplateElement;


        public TTImage() : this(new DataImage()) { }
        public TTImage(DataImage data)
        {
            Data = data;
            this.DataContext = Data;
            if (MakeTemplate<Image>() is Image element) { MyTemplateElement = element; }
            else { throw new ArgumentException("テンプレート作成できんかった"); }

            //SetBinding(TTSourceProperty, nameof(Data.Source));
            MyTemplateElement.SetBinding(Image.SourceProperty, nameof(Data.Source));
            MySetXYBinging(this.Data);
        }

    }

}
