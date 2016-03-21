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
using RateMyApp.Helpers;
using BugSense;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Phone.UserData;
using System.Threading;
using Windows.ApplicationModel.Store;

//drawing on a writeable bitmap - toolkit with extension methods for WriteableBitmap
//http://writeablebitmapex.codeplex.com/

//putting text on a bitmap
//http://code.msdn.microsoft.com/wpapps/Render-text-on-bitmap-fff2b406

//scheduling a background task
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh202941%28v=vs.105%29.aspx

//windows theme colors
//http://marcofranssen.nl/wp-content/uploads/2012/11/PhoneThemeColors.png

namespace CalendarTile
{
    public partial class MainPage : PhoneApplicationPage
    {
        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        public bool agentsAreEnabled = true;
        private bool _loading = true;
        private bool _appointmentsEnabled = false;

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - loaded");
        }

        private void GetAppointments()
        {
            if (_appointmentsEnabled)
            {
                var appts = new Appointments();

                appts.SearchCompleted += new EventHandler<AppointmentsSearchEventArgs>(Appointments_SearchCompleted);

                DateTime start = DateTime.Now;
                DateTime end = start.AddDays(31);

                appts.SearchAsync(start, end, 50, "Appointments Test #1");
            }
            else
            {
                GenerateCalendars(null);
            }
        }

        void Appointments_SearchCompleted(object sender, AppointmentsSearchEventArgs e)
        {
            GenerateCalendars(e.Results);
        }

        private void GenerateCalendars(IEnumerable<Appointment> results)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Dispatcher.BeginInvoke(async () =>
                {
                    await CreateImage(results);
                    UpdateTileData();
                    var tiles = ShellTile.ActiveTiles;
                    PlaceTileButton.IsEnabled = (tiles.Count() < 2);
                    StartPeriodicAgent();
                });
            });
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - navigated to");

#if DEBUG
            _appointmentsEnabled = true;
            await Task.FromResult(0);
#else
            try
            {
                var inAppProducts = await CurrentApp.LoadListingInformationByProductIdsAsync(new string[] { "CalendarTile_CalendarAppointments" });
                _appointmentsEnabled = inAppProducts.ProductListings.Count > 0;
            }
            catch (Exception)
            {
                //log exception
            }
#endif

            UpdateButtonColor();
            DefaultSettings();
            GetAppointments();
        }

        public void ReviewButton_Click(object sender, EventArgs e)
        {
            FeedbackHelper.Default.Reviewed();
            var marketplace = new MarketplaceReviewTask();
            marketplace.Show();
        }

        public void EmailButton_Click(object sender, EventArgs e)
        {
            var email = new EmailComposeTask();
            email.To = "alexmartin9999@hotmail.com";
            email.Subject = "Feedback for the Calendar Tile application";
            email.Show();
        }

        private async void PlaceTileButton_Click(object sender, RoutedEventArgs e)
        {
            //await CreateImage();
            //GetAppointments();
            UpdateTileData(); 
            PutTileOnHomeScreen();
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - other app...");
            var button = (Button)sender;
            var task = new MarketplaceDetailTask();
            task.ContentIdentifier = button.Tag.ToString();
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - ..." + button.Tag.ToString());
            task.ContentType = MarketplaceContentType.Applications;
            task.Show();
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - pin to home screen");
            var tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("MainPage.xaml"));
            if (tile == null) ShellTile.Create(new Uri("/MainPage.xaml?test=true", UriKind.Relative), GetTileData(), true);
        }

        private async void FirstDowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading)
            {
                var item = (ListPickerItem)((ListPicker)sender).SelectedItem;
                IsolatedStorageSettings.ApplicationSettings["FirstDayOfWeek"] = item.Tag.ToString();
                IsolatedStorageSettings.ApplicationSettings.Save();
                GetAppointments();
                UpdateTileData();
                //UpdateLockScreen();
            }
        }

        //private async void UpdateLockScreen()
        //{
        //    try
        //    {
        //        var isProvider = Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication;
        //        if (!isProvider)
        //        {
        //            // If you're not the provider, this call will prompt the user for permission.
        //            // Calling RequestAccessAsync from a background agent is not allowed.
        //            var op = await Windows.Phone.System.UserProfile.LockScreenManager.RequestAccessAsync();

        //            // Only do further work if the access was granted.
        //            isProvider = op == Windows.Phone.System.UserProfile.LockScreenRequestResult.Granted;
        //        }

        //        if (isProvider)
        //        {
        //            // At this stage, the app is the active lock screen background provider.

        //            // The following code example shows the new URI schema.
        //            // ms-appdata points to the root of the local app data folder.
        //            // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package.
        //            var uri = new Uri(@"isostore:/Shared/ShellContent/calendar-wide.png");

        //            // Set the lock screen background image.
        //            Windows.Phone.System.UserProfile.LockScreen.SetImageUri(uri);

        //            // Get the URI of the lock screen background image.
        //            var currentImage = Windows.Phone.System.UserProfile.LockScreen.GetImageUri();
        //            System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());
        //        }
        //        else
        //        {
        //            MessageBox.Show("You said no, so I can't update your background.");
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //    }
        //}

        private Task CreateImage(IEnumerable<Appointment> appointments = null)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - create image");
            var renderer = new CalendarRenderer();
            Color primarycolor;
            Color secondarycolor;
            Color backgroundcolor;
            GetColorsFromSettings(out primarycolor, out secondarycolor, out backgroundcolor);
            //await Task.Run(() => {
                CalendarImage.Source = renderer.DrawCalendar(730, 365, primarycolor, secondarycolor, backgroundcolor, 20, 28, null, appointments);
                renderer.DrawCalendar(691, 336, primarycolor, secondarycolor, backgroundcolor, 20, 28, "calendar-wide.png", appointments);
                renderer.DrawCalendar(336, 336, primarycolor, secondarycolor, backgroundcolor, 20, 28, "calendar.png", appointments);
            //});
            return Task.FromResult(0);
        }

        private static void GetColorsFromSettings(out Color primarycolor, out Color secondarycolor, out Color backgroundcolor)
        {
            Visibility dbgisibility = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"];
            primarycolor = dbgisibility == Visibility.Visible ? 
                new Color() { A = 255, R = 255, G = 255, B = 255 } :
                new Color() { A = 255, R = 0, G = 0, B = 0 };
            secondarycolor = (Color)Application.Current.Resources["PhoneAccentColor"];
            backgroundcolor = new Color() { A = 0, R = 0, G = 0, B = 0 };
        }

        private void UpdateTileData()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - update tile data");
            var tiles = ShellTile.ActiveTiles;
            foreach (var tile in tiles)
            {
                tile.Update(GetTileData()); 
            }
        }

        private void PutTileOnHomeScreen()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - put tile on home screen");
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
#if (DEBUG)
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

        private void UpdateButtonColor()
        {
            VersionTextBox.Text = "v" + XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;
            ReviewButton.Background = new SolidColorBrush((System.Windows.Media.Color)Application.Current.Resources["PhoneAccentColor"]);
            EmailButton.Background = new SolidColorBrush((System.Windows.Media.Color)Application.Current.Resources["PhoneAccentColor"]);
        }

        private void DefaultSettings()
        {
            var dow = "0";
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("FirstDayOfWeek"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("FirstDayOfWeek", dow);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            else
            {
                dow = IsolatedStorageSettings.ApplicationSettings["FirstDayOfWeek"].ToString();
            }

            Dispatcher.BeginInvoke(() =>
            {
                switch (dow)
                {
                    case "0": FirstDowList.SelectedIndex = 1; break;
                    case "1": FirstDowList.SelectedIndex = 2; break;
                    case "6": FirstDowList.SelectedIndex = 0; break;
                }
            });
            FirstDowList.IsEnabled = true;
            _loading = false;
        }
    }
}