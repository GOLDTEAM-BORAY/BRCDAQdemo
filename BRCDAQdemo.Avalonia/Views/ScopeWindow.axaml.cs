using Avalonia.Controls;
using Avalonia.Threading;
using BRCDAQdemo.Core;
using BRCDAQdemo.Core.Lib;
using BRCDAQdemo.Core.ViewModels;
using ScottPlot;
using ScottPlot.DataSources;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BRCDAQdemo.Avalonia.Views;

public partial class ScopeWindow : Window, IRenderable
{
    private static readonly ScottPlot.Palettes.Category10 Palette = new();
    private readonly SemaphoreSlim _renderLock = new(1, 1);
    private ScopeWindowViewModel? _viewModel;

    public ScopeWindow()
    {
        InitializeComponent();
        ApplyDarkTheme();
    }

    public ScopeWindow(BRCSDK.BrcDevice device) : this()
    {
        _viewModel = new ScopeWindowViewModel
        {
            Renderer = this,
            Device = device
        };
        DataContext = _viewModel;
    }

    private void ApplyDarkTheme()
    {
        WavePlot.Plot.FigureBackground.Color = new Color("#1c1c1e");
        WavePlot.Plot.Axes.Color(new Color("#888888"));
        WavePlot.Plot.Grid.XAxisStyle.FillColor1 = new Color("#888888").WithAlpha(10);
        WavePlot.Plot.Grid.YAxisStyle.FillColor1 = new Color("#888888").WithAlpha(10);
        WavePlot.Plot.Grid.XAxisStyle.MajorLineStyle.Color = Colors.White.WithAlpha(15);
        WavePlot.Plot.Grid.YAxisStyle.MajorLineStyle.Color = Colors.White.WithAlpha(15);
        WavePlot.Plot.Grid.XAxisStyle.MinorLineStyle.Color = Colors.White.WithAlpha(5);
        WavePlot.Plot.Grid.YAxisStyle.MinorLineStyle.Color = Colors.White.WithAlpha(5);
        WavePlot.Plot.Grid.XAxisStyle.MinorLineStyle.Width = 1;
        WavePlot.Plot.Grid.YAxisStyle.MinorLineStyle.Width = 1;
        WavePlot.Plot.Axes.Margins(0, 0);
    }

    public async Task Render(IList<ScopeWindowViewModel.ChannelProperty> channelProperties, Memory<double> points, double sampleRate)
    {
        if (channelProperties.Count == 0)
            return;

        await _renderLock.WaitAsync();
        try
        {
            var signalList = new List<(LegendItem LegendItem, Signal Signal)>();
            int pointCountPerChannel = points.Length / channelProperties.Count;

            for (int i = 0; i < channelProperties.Count; i++)
            {
                if (!channelProperties[i].ShowSignal)
                    continue;

                double[] data = points.Slice(i * pointCountPerChannel, pointCountPerChannel).ToArray();
                var adapter = new MemoryReadOnlyListAdapter<double>(data);
                var signal = new Signal(new SignalSourceDouble(adapter, 1.0 / sampleRate));
                signal.LineStyle.Color = Palette.GetColor(i);
                signal.MarkerStyle.IsVisible = false;

                var legendItem = new LegendItem();
                legendItem.LineStyle.Color = signal.LineStyle.Color;
                legendItem.LineStyle.Width = 2;
                legendItem.MarkerStyle.IsVisible = false;
                legendItem.LabelText = $"ch{i + 1}  vpp: {data.Max() - data.Min():0.###}mV  ave: {data.Average():0.###}mV";
                signalList.Add((legendItem, signal));
            }

            if (signalList.Count == 0)
                return;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WavePlot.Plot.Clear();

                var legend = WavePlot.Plot.Legend;
                var manualLegendItems = new List<LegendItem>();
                legend.BackgroundFillStyle.Color = Colors.Transparent;
                legend.ShadowOffset = PixelOffset.Zero;
                legend.FontSize = 12;
                legend.FontColor = Colors.White;
                legend.FontName = Fonts.Sans;
                legend.IsVisible = true;
                legend.ManualItems = manualLegendItems;

                foreach (var (legendItem, signal) in signalList)
                {
                    manualLegendItems.Add(legendItem);
                    WavePlot.Plot.PlottableList.Add(signal);
                }

                WavePlot.Plot.Axes.AutoScale();
                WavePlot.Refresh();
            });
        }
        finally
        {
            _renderLock.Release();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel?.Dispose();
        base.OnClosed(e);
    }
}
