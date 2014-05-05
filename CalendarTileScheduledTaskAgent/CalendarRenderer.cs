using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CalendarTileScheduledTaskAgent
{
    public class CalendarRenderer
    {
        private const int margin = 10;
        private const int marginBottom = 60;
        private const int textmargin = 5;

        public WriteableBitmap DrawCalendar(int width, int height, Color color, Color alternateColor, Color backgroundColor, int fontSize, string filename)
        {
            var now = DateTime.Now;
            
            var bitmap = new WriteableBitmap(width, height); 
            bitmap.Clear(backgroundColor);
            
            //draw grid...
            int cellWidth = ((width - (margin * 2)) / 7);
            int cellHeight = ((height - (margin + marginBottom)) / 7);

            //horizontal lines
            int maxWidth = margin + 7 * cellWidth;
            for (int i = 0; i < 8; i++)
            {
                int top = margin + i * cellHeight;
                bitmap.DrawLine(margin, top, maxWidth, top, color);
            }

            //vertical lines
            int maxHeight = margin + 7 * cellHeight;
            for (int i = 0; i < 8; i++)
            {
                int left = margin + i * cellWidth;
                bitmap.DrawLine(left, margin, left, maxHeight, color);
            }

            bitmap.Invalidate();

            //highlight current day cell
            var coords = GetDateCoords(now, cellWidth, cellHeight);
            bitmap.FillRectangle(coords.X, coords.Y, coords.X + cellWidth, coords.Y + cellHeight, color);
            bitmap.Invalidate();

            //days of the week
            var dowOffset = int.Parse(IsolatedStorageSettings.ApplicationSettings["FirstDayOfWeek"].ToString());
            for (int count = 0; count < 7; count++)
            {
                int dayNum = (count + dowOffset) % 7;
                int left = margin + textmargin + count * cellWidth;
                bitmap.DrawText(GetDayOfWeekAbbreviation(dayNum), color, fontSize, left + textmargin, margin + textmargin);
            }

            //date numbers
            for (int d = 1; d <= DateTime.DaysInMonth(now.Year, now.Month); d++)
            {
                var drawDate = new DateTime(now.Year, now.Month, d);
                coords = GetDateCoords(drawDate, cellWidth, cellHeight);
                var textColor = drawDate.Date == now.Date ? alternateColor : color;
                bitmap.DrawText(d.ToString(CultureInfo.InvariantCulture), textColor, fontSize, coords.X + textmargin, coords.Y + textmargin);
            }

            bitmap.Invalidate();

            //save image
            if (!string.IsNullOrEmpty(filename))
            {
                String strImageName = @"Shared\ShellContent\" + filename;
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(strImageName))
                        iso.DeleteFile(strImageName);
                }
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isostream = iso.CreateFile(strImageName))
                    {
                        WriteableBitmapExtensionsPng.Encode(bitmap, isostream);
                        isostream.Close();
                    }
                }
            }
            return bitmap;
        }

        private Coordinates GetDateCoords(DateTime date, int cellWidth, int cellHeight)
        {
            var dowOffset = int.Parse(IsolatedStorageSettings.ApplicationSettings["FirstDayOfWeek"].ToString());
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var cellOffset = (firstDayOfMonth.DayOfWeek.GetHashCode() - dowOffset + 7)%7 - 1;
            var dayCellNumber = date.Day + cellOffset;
            var cellX = dayCellNumber % 7;
            var cellY = dayCellNumber / 7 + 1;

            return new Coordinates(margin + cellWidth*cellX, margin + cellHeight*cellY);
        }

        private string GetDayOfWeekAbbreviation(int dayNum)
        {
            var sunday = new DateTime(2014, 3, 2);
            return sunday.AddDays(dayNum).ToString("ddd"); 
        }
    }

    internal class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    internal static class WritableBitmapExtensions
    {
        internal static void DrawText(this WriteableBitmap wbm, string text, Color color, int fontSize, int x, int y)
        {
            TextBlock tb = new TextBlock();
            tb.FontSize = fontSize;
            tb.FontWeight = FontWeights.ExtraBold;
            tb.Foreground = new SolidColorBrush(color);
            tb.Text = text;

            // TranslateTransform 
            TranslateTransform tf = new TranslateTransform();
            tf.X = x;
            tf.Y = y;
            wbm.Render(tb, tf);
        }
    }
}
