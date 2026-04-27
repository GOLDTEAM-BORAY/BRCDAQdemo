using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using ScottPlot;
using ScottPlot.Interactivity;
using SkiaSharp;
using PixelRect = ScottPlot.PixelRect;
using SpCursor = ScottPlot.Cursor;

namespace BRCDAQdemo.Avalonia.Controls;

public class AvaPlot : Control, IPlotControl
{
    public Plot Plot { get; internal set; }
    public IMultiplot Multiplot { get; set; }
    public IPlotMenu? Menu { get; set; }
    public UserInputProcessor UserInputProcessor { get; }
    public GRContext? GRContext => null;
    public float DisplayScale { get; set; }

    public AvaPlot()
    {
        Plot = new Plot { PlotControl = this };
        Multiplot = new Multiplot(Plot);
        ClipToBounds = true;
        DisplayScale = DetectDisplayScale();
        UserInputProcessor = new UserInputProcessor(this);
        Focusable = true;
        Refresh();
    }

    private sealed class CustomDrawOp : ICustomDrawOperation
    {
        private readonly IMultiplot _multiplot;
        private readonly float _scale;

        public Rect Bounds { get; }
        public bool HitTest(Point p) => true;
        public bool Equals(ICustomDrawOperation? other) => false;
        public void Dispose() { }

        public CustomDrawOp(Rect bounds, IMultiplot multiplot, float scale)
        {
            Bounds = bounds;
            _multiplot = multiplot;
            _scale = scale;
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
                return;

            using var lease = leaseFeature.Lease();
            using SKAutoCanvasRestore _ = new(lease.SkCanvas, true);
            lease.SkCanvas.Scale(1.0f / _scale);

            PixelRect rect = new(0, (float)Bounds.Width * _scale, (float)Bounds.Height * _scale, 0);
            _multiplot.Render(lease.SkCanvas, rect);
        }
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new CustomDrawOp(new Rect(Bounds.Size), Multiplot, DisplayScale));
    }

    public void Reset()
    {
        Reset(new Plot { PlotControl = this });
    }

    public void Reset(Plot plot)
    {
        Plot oldPlot = Plot;
        Plot = plot;
        oldPlot.Dispose();
        Multiplot.Reset(plot);
    }

    public void Refresh()
    {
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }

    public void ShowContextMenu(Pixel position)
    {
        Menu?.ShowContextMenu(position);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Pixel pixel = e.ToPixel(this);
        PointerUpdateKind kind = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
        UserInputProcessor.ProcessMouseDown(pixel, kind);
        e.Pointer.Capture(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Pixel pixel = e.ToPixel(this);
        PointerUpdateKind kind = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
        UserInputProcessor.ProcessMouseUp(pixel, kind);
        e.Pointer.Capture(null);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        UserInputProcessor.ProcessMouseMove(e.ToPixel(this));
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        UserInputProcessor.ProcessMouseWheel(e.ToPixel(this), e.Delta.Y);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        UserInputProcessor.ProcessKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        UserInputProcessor.ProcessKeyUp(e);
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        UserInputProcessor.ProcessLostFocus();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DisplayScale = DetectDisplayScale();
    }

    public float DetectDisplayScale()
    {
        return (float)(TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0);
    }

    public void SetCursor(SpCursor cursor)
    {
        Cursor = cursor.GetCursor();
    }
}
