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
using CalendarTileScheduledTaskAgent;
using Microsoft.Phone.Scheduler;

//drawing on a writeable bitmap - toolkit with extension methods for WriteableBitmap
//http://writeablebitmapex.codeplex.com/

//putting text on a bitmap
//http://code.msdn.microsoft.com/wpapps/Render-text-on-bitmap-fff2b406

//scheduling a background task
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh202941%28v=vs.105%29.aspx

//TODO
//settings - set colors
//'about' page, links
//review system
//scheduled update

namespace CalendarTile
{
    public partial class MainPage : PhoneApplicationPage
    {
        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        public bool agentsAreEnabled = true;

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
            StartPeriodicAgent();
        }

        private void CreateImage()
        {
            var renderer = new CalendarRenderer();
            renderer.DrawCalendar(336, 336, Colors.White, Colors.Black, (Color)Application.Current.Resources["PhoneAccentColor"], 20, "calendar.png");
            renderer.DrawCalendar(691, 336, Colors.White, Colors.Black, (Color)Application.Current.Resources["PhoneAccentColor"], 20, "calendar-wide.png");
        }

        private void UpdateTileData()
        {
            var tiles = ShellTile.ActiveTiles;
            foreach (var tile in tiles)
            {
                tile.Update(GetTileData()); 
            }
        }

        private void PutTileOnHomeScreen()
        {
            ShellTile.Create(new Uri("/MainPage.xaml", UriKind.Relative), GetTileData(), true); 
        }

        private ShellTileData GetTileData()
        {
            var data = new FlipTileData();
            data.BackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar.png");
            data.WideBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar-wide.png");
            data.Title = DateTime.Now.ToString("MMMM");
            return data; 
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

        private void StartPeriodicAgent()
        {
            // Variable for tracking enabled status of background agents for this app.
            agentsAreEnabled = true;

            // Obtain a reference to the period task, if one exists
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (periodicTask != null)
            {
                RemoveAgent(periodicTaskName);
            }

            periodicTask = new PeriodicTask(periodicTaskName);
            periodicTask.Description = "Update the calendar live-tile with today's calendar";

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);

                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG)
                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(60));
#endif
            }
            catch (InvalidOperationException exception)
            {

            }
            catch (SchedulerServiceException)
            {

            }
        }

        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }
    }
}