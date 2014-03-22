﻿using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System.Windows.Media;

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
            var renderer = new CalendarRenderer();
            renderer.DrawCalendar(336, 336, Colors.White, Colors.Black, (Color)Application.Current.Resources["PhoneAccentColor"], 20, "calendar.png");
            renderer.DrawCalendar(691, 336, Colors.White, Colors.Black, (Color)Application.Current.Resources["PhoneAccentColor"], 20, "calendar-wide.png");

            NotifyComplete();
        }
    }
}