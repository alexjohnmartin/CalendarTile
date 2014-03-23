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
using System.IO.IsolatedStorage;
using System.Globalization;
using Microsoft.Phone.Tasks;

//drawing on a writeable bitmap - toolkit with extension methods for WriteableBitmap
//http://writeablebitmapex.codeplex.com/

//putting text on a bitmap
//http://code.msdn.microsoft.com/wpapps/Render-text-on-bitmap-fff2b406

//scheduling a background task
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh202941%28v=vs.105%29.aspx

//windows theme colors
//http://marcofranssen.nl/wp-content/uploads/2012/11/PhoneThemeColors.png

//TODO
//'about' page, links
//review system

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
            Loaded += MainPage_Loaded;

            DefaultTheColorSettings();
            UpdateColorSettingControls();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateImage();
            UpdateTileData();
            StartPeriodicAgent();

            var tiles = ShellTile.ActiveTiles;
            PlaceTileButton.Visibility = (tiles.Count() < 2) ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string a = string.Empty;
            string r = string.Empty;
            string g = string.Empty;
            string b = string.Empty;
            string update = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("a", out a) &&
                NavigationContext.QueryString.TryGetValue("r", out r) &&
                NavigationContext.QueryString.TryGetValue("g", out g) &&
                NavigationContext.QueryString.TryGetValue("b", out b) &&
                NavigationContext.QueryString.TryGetValue("update", out update))
            {
                var color = Color.FromArgb(byte.Parse(a, NumberStyles.HexNumber), 
                                           byte.Parse(r, NumberStyles.HexNumber), 
                                           byte.Parse(g, NumberStyles.HexNumber), 
                                           byte.Parse(b, NumberStyles.HexNumber));
                switch (update)
                {
                    case "primary":
                        IsolatedStorageSettings.ApplicationSettings["PrimaryColor"] = color;
                        PrimaryColorRectangle.Fill = new SolidColorBrush(color);
                        break;
                    case "secondary":
                        IsolatedStorageSettings.ApplicationSettings["SecondaryColor"] = color;
                        SecondaryColorRectangle.Fill = new SolidColorBrush(color);
                        break;
                    case "background":
                        IsolatedStorageSettings.ApplicationSettings["BackgroundColor"] = color;
                        BackgroundColorRectangle.Fill = new SolidColorBrush(color);
                        break;
                }
                IsolatedStorageSettings.ApplicationSettings.Save(); 
            }
        }

        public void TwitterButton_Click(object sender, EventArgs e)
        {
            var task = new WebBrowserTask
            {
                Uri = new Uri("https://twitter.com/AlexJohnMartin", UriKind.Absolute)
            };
            task.Show();
        }

        public void StoreButton_Click(object sender, EventArgs e)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var task = new WebBrowserTask
            {
                Uri = new Uri(string.Format("http://www.windowsphone.com/{0}/store/publishers?publisherId=nocturnal%2Btendencies&appId=63cb6767-4940-4fa1-be8c-a7f58e455c3b", currentCulture.Name), UriKind.Absolute)
            };
            task.Show();
        }

        public void ReviewButton_Click(object sender, EventArgs e)
        {
            //FeedbackHelper.Default.Reviewed();
            var marketplace = new MarketplaceReviewTask();
            marketplace.Show();
        }

        public void EmailButton_Click(object sender, EventArgs e)
        {
            var email = new EmailComposeTask();
            email.Subject = "Feedback for the Calendar Tile application";
            email.Show();
        }

        private void PlaceTileButton_Click(object sender, RoutedEventArgs e)
        {
            CreateImage();
            UpdateTileData(); 
            PutTileOnHomeScreen();
        }

        private void ChangeBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ColorPage.xaml?update=background", UriKind.Relative));
        }

        private void ChangePrimaryColorButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ColorPage.xaml?update=primary", UriKind.Relative));
        }

        private void ChangeSecondaryColorButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ColorPage.xaml?update=secondary", UriKind.Relative));
        }

        private void CreateImage()
        {
            var renderer = new CalendarRenderer();
            var primarycolor = (Color)IsolatedStorageSettings.ApplicationSettings["PrimaryColor"];
            var secondarycolor = (Color)IsolatedStorageSettings.ApplicationSettings["SecondaryColor"];
            var backgorundcolor = (Color)IsolatedStorageSettings.ApplicationSettings["BackgroundColor"];
            renderer.DrawCalendar(336, 336, primarycolor, secondarycolor, backgorundcolor, 20, "calendar.png");
            renderer.DrawCalendar(691, 336, primarycolor, secondarycolor, backgorundcolor, 20, "calendar-wide.png");
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

        private void DefaultTheColorSettings()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("BackgroundColor"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("PrimaryColor", Colors.White);
                IsolatedStorageSettings.ApplicationSettings.Add("SecondaryColor", Colors.Black);
                IsolatedStorageSettings.ApplicationSettings.Add("BackgroundColor", (Color)Application.Current.Resources["PhoneAccentColor"]);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        private void UpdateColorSettingControls()
        {
            PrimaryColorRectangle.Fill = new SolidColorBrush((Color)IsolatedStorageSettings.ApplicationSettings["PrimaryColor"]);
            SecondaryColorRectangle.Fill = new SolidColorBrush((Color)IsolatedStorageSettings.ApplicationSettings["SecondaryColor"]);
            BackgroundColorRectangle.Fill = new SolidColorBrush((Color)IsolatedStorageSettings.ApplicationSettings["BackgroundColor"]);
        }
    }
}