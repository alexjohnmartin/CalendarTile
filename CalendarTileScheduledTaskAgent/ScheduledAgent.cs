﻿using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using Microsoft.Phone.UserData;
using System.Threading;

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
            GetAppointments();
        }

        private static void GetColorsFromSettings(out Color primarycolor, out Color secondarycolor, out Color backgroundcolor)
        {
            Visibility dbgisibility = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"];
            primarycolor = dbgisibility == Visibility.Visible ?
                new Color() { A = 255, R = 255, G = 255, B = 255 } :
                new Color() { A = 255, R = 0, G = 0, B = 0 };
            secondarycolor = (System.Windows.Media.Color)Application.Current.Resources["PhoneAccentColor"];
            backgroundcolor = new Color() { A = 0, R = 0, G = 0, B = 0 };
        }

        private ShellTileData GetTileData()
        {
            var data = new FlipTileData();
            data.BackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar.png");
            data.WideBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/calendar-wide.png");
            data.Title = DateTime.Now.ToString("MMMM");
            return data;
        }

        private void GetAppointments()
        {
            var appts = new Appointments();

            appts.SearchCompleted += new EventHandler<AppointmentsSearchEventArgs>(Appointments_SearchCompleted);

            DateTime start = DateTime.Now;
            DateTime end = start.AddDays(31);

            appts.SearchAsync(start, end, 50, "Appointments Test #1");
        }

        void Appointments_SearchCompleted(object sender, AppointmentsSearchEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o => {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        var renderer = new CalendarRenderer();
                        Color primarycolor;
                        Color secondarycolor;
                        Color backgroundcolor;
                        GetColorsFromSettings(out primarycolor, out secondarycolor, out backgroundcolor);
                        renderer.DrawCalendar(336, 336, primarycolor, secondarycolor, backgroundcolor, 20, 28, "calendar.png", e.Results);
                        renderer.DrawCalendar(691, 336, primarycolor, secondarycolor, backgroundcolor, 20, 28, "calendar-wide.png", e.Results);
                        var tiles = ShellTile.ActiveTiles;
                        foreach (var tile in tiles)
                        {
                            tile.Update(GetTileData());
                        }
                    }
                    catch (Exception)
                    {
                        //TODO:log exception

                    }
                    NotifyComplete();
                });
            });
        }
    }
}