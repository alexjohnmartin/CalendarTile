using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CalendarTile.Resources;
using System.Windows.Media;

namespace CalendarTile
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            //Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //CreateImage();
            //UpdateTileData(); 
        }

        private void PlaceTileButton_Click(object sender, RoutedEventArgs e)
        {
            CreateImage();
            UpdateTileData(); 
            PutTileOnHomeScreen(); 
        }

        private void CreateImage()
        {
            var renderer = new CalendarRenderer();
            renderer.DrawCalendar(336, 336, Colors.White, (Color)Application.Current.Resources["PhoneAccentColor"], 20, "calendar.png");
            renderer.DrawCalendar(691, 336, Colors.White, (Color)Application.Current.Resources["PhoneAccentColor"], 20, "calendar-wide.png");
        }

        private void UpdateTileData()
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            //var data = new CycleTileData(); 
            //data.CycleImages = new []{new Uri(@"/Assets/Calendars/" + DateTime.Now.ToString("yyyy-MM") + ".png")};
            //data.Title = DateTime.Now.ToString("MMMM");
            var data = new FlipTileData();
            data.BackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar.png");
            data.WideBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar-wide.png");
            tile.Update(data); 
        }

        private void PutTileOnHomeScreen()
        {
            //TODO
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}