using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace Pixtack3rd
{
    class MyValueConverter
    {
    }
    public class MyConverterThickness : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)(decimal)value;
            return new Thickness(d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            return thickness.Top;
        }
    }

    //フォントの斜体
    public class MyConverterFontStyleIsItalic : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            if (b) { return FontStyles.Italic; }
            else { return FontStyles.Normal; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontStyle style = (FontStyle)value;
            return style == FontStyles.Italic;
        }
    }

    //フォントの太字
    public class MyConverterFontWeightIsBold : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            if (b) { return FontWeights.Bold; }
            else { return FontWeights.Normal; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontWeight weight = (FontWeight)value;
            return weight == FontWeights.Bold;
        }
    }

    //ARGB各値からSolidBrush作成
    public class MyConverterArgbNumericToBrush : IMultiValueConverter
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
            throw new NotImplementedException();
        }
    }

    //ColorとSolidBrushの変換＋色反転、テキストボックスのカーソルの色に使用
    public class MyConverterColorSolidBrushNegative : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            Color negative = Color.FromArgb(255, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));
            return new SolidColorBrush(negative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush b = (SolidColorBrush)value;
            Color c = b.Color;
            Color negative = Color.FromArgb(255, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));
            return negative;
        }
    }
    public class MyConverterColorSolidBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush b = (SolidColorBrush)value;
            return b.Color;
        }
    }

    public class MyConverterFontFamilyName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            FontFamilyConverter ffc = new();
            if (ffc.ConvertFromString(name) is FontFamily font)
            {
                return font;
            }
            else { return Application.Current.MainWindow.FontFamily; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontFamily font = (FontFamily)value;
            return font.Source;
        }
    }

    public class MyConverterWakuBrush : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<Brush> myBrushes = (List<Brush>)parameter;
            if (values[3] is WakuVisibleType type)
            {
                if (type == WakuVisibleType.None || type == WakuVisibleType.OnlyActiveGroup)
                {
                    return Brushes.Transparent;
                }
                else
                {
                    if ((bool)values[2]) { return myBrushes[4]; }
                    else if ((bool)values[1]) { return myBrushes[3]; }
                    else if ((bool)values[0]) { return myBrushes[0]; }
                    else return Brushes.Transparent;
                }

            }
            else return Brushes.Transparent;
            //WakuVisibleType type = (WakuVisibleType)values[3];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyConverterWakuBrushForTTGroup : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<Brush> myBrushes = (List<Brush>)parameter;
            Brush result = Brushes.Transparent;
            if (values[4] is WakuVisibleType type)
            {



                //WakuVisibleType type = (WakuVisibleType)values[4];
                switch (type)
                {
                    case WakuVisibleType.None:
                        break;
                    case WakuVisibleType.All:
                        if ((bool)values[0]) { result = myBrushes[4]; }
                        else if ((bool)values[1]) { result = myBrushes[3]; }
                        else if ((bool)values[2]) { result = myBrushes[2]; }
                        else if ((bool)values[3]) { result = myBrushes[1]; }
                        break;
                    case WakuVisibleType.OnlyActiveGroup:
                        if ((bool)values[2]) { result = myBrushes[2]; }
                        break;
                    case WakuVisibleType.NotGroup:
                        if ((bool)values[0]) { result = myBrushes[4]; }
                        else if ((bool)values[1]) { result = myBrushes[3]; }
                        else if ((bool)values[2]) { result = myBrushes[2]; }
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //列挙型の値をラジオボタンにバインドするには？［ユニバーサルWindowsアプリ開発］：WinRT／Metro TIPS - ＠IT
    //    https://atmarkit.itmedia.co.jp/ait/articles/1507/29/news019.html
    //wpf - How to bind RadioButtons to an enum? - Stack Overflow
    //    https://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum

    public class MyConverterEnumToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(true)) { return true; }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(true)) { return parameter; }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }

    //表示非表示の切り替え用
    public class MyConverterVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bb = (bool)value;
            if (bb)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vi = (Visibility)value;
            if (vi == Visibility.Visible) { return false; }
            else { return true; }
        }
    }

    public class MyConverterBoolInverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bb = (bool)value;
            if (bb is true) { return false; }
            else { return true; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? bb = (bool?)value;
            if (bb is null or true) { return false; }
            else { return true; }
        }
    }


    public class MyConverterRectWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect r = (Rect)value;
            if (r.IsEmpty) return 0;
            return r.Width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MyConverterRectHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect r = (Rect)value;
            if (r.IsEmpty) return 0;
            return r.Height;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyConverterRectX : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect r = (Rect)value;
            if (r.IsEmpty) return 0;
            return r.X;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyConverterRectY : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Rect r = (Rect)value;
            if (r.IsEmpty) return 0;
            return r.Y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class MyConverterBoolVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            if (b) return Visibility.Visible;
            else return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            if (vis == Visibility.Visible) return true;
            else return false;
        }
    }

    public class MyConverterRectRect : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Rect r1 = (Rect)values[0];
            Rect r2 = (Rect)values[1];
            Rect result = Rect.Union(r1, r2);
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
    public class MyConverterBrushByte : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte a = (byte)values[0];
            byte r = (byte)values[1];
            byte g = (byte)values[2];
            byte b = (byte)values[3];
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

    //public class MyConverterBrushColorA : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)value;
    //        return brush.Color.A;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)parameter;
    //        Color c = brush.Color;
    //        byte v = (byte)(decimal)value;
    //        return new SolidColorBrush(Color.FromArgb(v, c.R, c.G, c.B));
    //    }
    //}
    //public class MyConverterBrushColorR : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)value;
    //        return brush.Color.R;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)parameter;
    //        Color c = brush.Color;
    //        byte v = (byte)(decimal)value;
    //        return new SolidColorBrush(Color.FromArgb(c.A, v, c.G, c.B));
    //    }
    //}
    //public class MyConverterBrushColorG : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)value;
    //        return brush.Color.G;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)parameter;
    //        Color c = brush.Color;
    //        byte v = (byte)(decimal)value;
    //        return new SolidColorBrush(Color.FromArgb(c.A, c.R, v, c.B));
    //    }
    //}
    //public class MyConverterBrushColorB : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)value;
    //        return brush.Color.B;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        SolidColorBrush brush = (SolidColorBrush)parameter;
    //        Color c = brush.Color;
    //        byte v = (byte)(decimal)value;
    //        return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, v));
    //    }
    //}

}
