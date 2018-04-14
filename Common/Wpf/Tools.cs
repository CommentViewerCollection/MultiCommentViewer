using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Common.Wpf
{
    public static class Tools
    {
        public static string ToText(this IEnumerable<IMessagePart> messageParts)
        {
            var s = "";
            foreach (var part in messageParts)
            {
                if (part is IMessageText text)
                {
                    s += text.Text;
                }
                else if (part is IMessageLink link)
                {
                    s += link.Url;
                }
            }
            return s;
        }
        /// <summary>
        /// マウス位置を取得
        /// </summary>
        /// <returns></returns>
        public static Point GetMousePos()
        {
            var element = Application.Current.MainWindow;
            Point mousePositionInApp = Mouse.GetPosition(element);
            Point mousePositionInScreenCoordinates = element.PointToScreen(mousePositionInApp);
            return mousePositionInScreenCoordinates;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Point GetShowPos(Point mousePoint, Window view)
        {
            var left = mousePoint.X - view.Width / 2;
            if (left < 0)
                left = 0;
            var top = mousePoint.Y = view.Height / 2;
            if (top < 0)
                top = 0;
            return new Point(left, top);
        }
    }
}
