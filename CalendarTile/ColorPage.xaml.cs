using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;

//windows theme colors
//http://marcofranssen.nl/wp-content/uploads/2012/11/PhoneThemeColors.png

namespace CalendarTile
{
    public partial class ColorPage : PhoneApplicationPage
    {
        private string _update = string.Empty; 

        public ColorPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string update = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("update", out update))
            {
                _update = update; 
            }

            var themeColor = (Color)Application.Current.Resources["PhoneAccentColor"];
            ThemeRectangle.Fill = new SolidColorBrush(themeColor);
            ThemeButton.CommandParameter = "#" +
                BitConverter.ToString(new[] { themeColor.A }) +
                BitConverter.ToString(new[] { themeColor.R }) +
                BitConverter.ToString(new[] { themeColor.G }) +
                BitConverter.ToString(new[] { themeColor.B });
        }

        //private void ThemeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/MainPage.xaml?update=" + _update + "&color=theme", UriKind.Relative));
        //}

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; 
            var color = button.CommandParameter.ToString(); 
            NavigationService.Navigate(new Uri(string.Format("/MainPage.xaml?update={0}&a={1}&r={2}&g={3}&b={4}", 
                                                             _update, 
                                                             color.Substring(1, 2), 
                                                             color.Substring(3, 2), 
                                                             color.Substring(5, 2), 
                                                             color.Substring(7, 2)),
                                           UriKind.Relative));
        }
    }
}