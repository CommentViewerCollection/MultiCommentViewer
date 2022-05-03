using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Mcv.PluginV2;

public static class Tools
{
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
