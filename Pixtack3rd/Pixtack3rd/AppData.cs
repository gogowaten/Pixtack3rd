using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pixtack3rd
{
    [DataContract]
    public class AppDatas : DependencyObject
    {
        [DataMember] public ObservableCollection<AppData> MyDatas { get; set; } = new();
        public AppDatas() { }
    }


    [DataContract]
    public class AppData : DependencyObject, IExtensibleDataObject
    {
        public AppData()
        {
            
        }

        public ExtensionDataObject? ExtensionData { get; set; }// => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name { get; set; } = "nemo";

        [DataMember]
        public double AppLeft
        {
            get { return (double)GetValue(AppLeftProperty); }
            set { SetValue(AppLeftProperty, value); }
        }
        public static readonly DependencyProperty AppLeftProperty =
            DependencyProperty.Register(nameof(AppLeft), typeof(double), typeof(AppData),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



    }


}
