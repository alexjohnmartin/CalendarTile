using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;

namespace CalendarTileScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var renderer = new CalendarRenderer(); 
                var primarycolor = (Color)IsolatedStorageSettings.ApplicationSettings["PrimaryColor"];
                var secondarycolor = (Color)IsolatedStorageSettings.ApplicationSettings["SecondaryColor"];
                var backgorundcolor = (Color)IsolatedStorageSettings.ApplicationSettings["BackgroundColor"];
                renderer.DrawCalendar(336, 336, primarycolor, secondarycolor, backgorundcolor, 20, "calendar.png");
                renderer.DrawCalendar(691, 336, primarycolor, secondarycolor, backgorundcolor, 20, "calendar-wide.png");
                var tiles = ShellTile.ActiveTiles;
                foreach (var tile in tiles)
                {
                    tile.Update(GetTileData());
                }
                NotifyComplete();
            });
        }

        private ShellTileData GetTileData()
        {
            var data = new FlipTileData();
            data.BackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar.png");
            data.WideBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar-wide.png");
            data.Title = DateTime.Now.ToString("MMMM");
            return data;
        }
    }
}