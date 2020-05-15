using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniformGridLayoutInitialMeasure
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var items = new int[100];
            for (int i = 0; i < items.Length; i++)
                items[i] = i;
            itemsRepeater.ItemsSource = items;
        }

        private void dataTemplateBox_Checked(object sender, RoutedEventArgs e)
        {
            itemsRepeater.ItemTemplate = aspectRatioConstrained;
        }

        private void dataTemplateBox_Unchecked(object sender, RoutedEventArgs e)
        {
            itemsRepeater.ItemTemplate = notAspectRatioConstrained;
        }
    }
}
