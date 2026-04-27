using Avalonia;
using Avalonia.Input;
using ScottPlot;
using ScottPlot.Interactivity;
using ScottPlot.Interactivity.UserActions;
using AvaCursor = Avalonia.Input.Cursor;
using AvaKey = Avalonia.Input.Key;
using SpCursor = ScottPlot.Cursor;
using SpKey = ScottPlot.Interactivity.Key;

namespace BRCDAQdemo.Avalonia.Controls;

internal static class AvaPlotExtensions
{
    internal static Pixel ToPixel(this PointerEventArgs e, Visual visual)
    {
        return new Pixel((float)e.GetPosition(visual).X, (float)e.GetPosition(visual).Y);
    }

    internal static void ProcessMouseDown(this UserInputProcessor processor, Pixel pixel, PointerUpdateKind kind)
    {
        IUserAction action = kind switch
        {
            PointerUpdateKind.LeftButtonPressed => new LeftMouseDown(pixel),
            PointerUpdateKind.MiddleButtonPressed => new MiddleMouseDown(pixel),
            PointerUpdateKind.RightButtonPressed => new RightMouseDown(pixel),
            _ => new Unknown("mouse down", kind.ToString()),
        };

        processor.Process(action);
    }

    internal static void ProcessMouseUp(this UserInputProcessor processor, Pixel pixel, PointerUpdateKind kind)
    {
        IUserAction action = kind switch
        {
            PointerUpdateKind.LeftButtonReleased => new LeftMouseUp(pixel),
            PointerUpdateKind.MiddleButtonReleased => new MiddleMouseUp(pixel),
            PointerUpdateKind.RightButtonReleased => new RightMouseUp(pixel),
            _ => new Unknown("mouse up", kind.ToString()),
        };

        processor.Process(action);
    }

    internal static void ProcessMouseMove(this UserInputProcessor processor, Pixel pixel)
    {
        processor.Process(new MouseMove(pixel));
    }

    internal static void ProcessMouseWheel(this UserInputProcessor processor, Pixel pixel, double delta)
    {
        processor.Process(delta > 0 ? new MouseWheelUp(pixel) : new MouseWheelDown(pixel));
    }

    internal static void ProcessKeyDown(this UserInputProcessor processor, KeyEventArgs e)
    {
        processor.Process(new KeyDown(GetKey(e.Key)));
    }

    internal static void ProcessKeyUp(this UserInputProcessor processor, KeyEventArgs e)
    {
        processor.Process(new KeyUp(GetKey(e.Key)));
    }

    private static SpKey GetKey(AvaKey key) => key switch
    {
        AvaKey.LeftAlt or AvaKey.RightAlt => StandardKeys.Alt,
        AvaKey.LeftShift or AvaKey.RightShift => StandardKeys.Shift,
        AvaKey.LeftCtrl or AvaKey.RightCtrl => StandardKeys.Control,
        _ => new SpKey(key.ToString()),
    };

    public static AvaCursor GetCursor(this SpCursor cursor) => cursor switch
    {
        SpCursor.Arrow => new AvaCursor(StandardCursorType.Arrow),
        SpCursor.No => new AvaCursor(StandardCursorType.No),
        SpCursor.Wait => new AvaCursor(StandardCursorType.Wait),
        SpCursor.Hand => new AvaCursor(StandardCursorType.Hand),
        SpCursor.Cross => new AvaCursor(StandardCursorType.Cross),
        SpCursor.SizeAll => new AvaCursor(StandardCursorType.SizeAll),
        SpCursor.SizeNorthSouth => new AvaCursor(StandardCursorType.SizeNorthSouth),
        SpCursor.SizeWestEast => new AvaCursor(StandardCursorType.SizeWestEast),
        _ => new AvaCursor(StandardCursorType.Arrow),
    };
}
