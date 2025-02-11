using BRCDAQdemo.WPF.Core;
using BRCDAQdemo.WPF.Core.ViewModels;
using ScottPlot;
using ScottPlot.DataSources;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BRCDAQdemo.WPF
{
    /// <summary>
    /// ScopeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScopeWindow : Window, IRenderable
    {
        //图表曲线配色方案
        private static readonly ScottPlot.Palettes.Category10 palette = new ScottPlot.Palettes.Category10();
        public ScopeWindow()
        {
            InitializeComponent();

            //设置图表背景样式
            // give the plot a dark background with light text
            WpfPlot1.Plot.FigureBackground.Color = new Color("#1c1c1e");
            WpfPlot1.Plot.Axes.Color(new Color("#888888"));

            // shade regions between major grid lines
            WpfPlot1.Plot.Grid.XAxisStyle.FillColor1 = new Color("#888888").WithAlpha(10);
            WpfPlot1.Plot.Grid.YAxisStyle.FillColor1 = new Color("#888888").WithAlpha(10);

            // set grid line colors
            WpfPlot1.Plot.Grid.XAxisStyle.MajorLineStyle.Color = Colors.White.WithAlpha(15);
            WpfPlot1.Plot.Grid.YAxisStyle.MajorLineStyle.Color = Colors.White.WithAlpha(15);
            WpfPlot1.Plot.Grid.XAxisStyle.MinorLineStyle.Color = Colors.White.WithAlpha(5);
            WpfPlot1.Plot.Grid.YAxisStyle.MinorLineStyle.Color = Colors.White.WithAlpha(5);

            // enable minor grid lines by defining a positive width
            WpfPlot1.Plot.Grid.XAxisStyle.MinorLineStyle.Width = 1;
            WpfPlot1.Plot.Grid.YAxisStyle.MinorLineStyle.Width = 1;


            WpfPlot1.Plot.Axes.MarginsX(0);
        }


        private static readonly Regex NumberRegex = new Regex("[^0-9]+");
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = NumberRegex.IsMatch(e.Text);
        }


        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        public async Task Render(IList<ScopeWindowViewModel.ChannelProperty> channelProperties, Memory<double> points, double sampleRate)
        {
            try
            {
                await _lock.WaitAsync();

                var signalList = new List<(LegendItem legendItem, Signal signal)>();

                var pointCountPerChannel = points.Length / channelProperties.Count;
                for (int i = 0; i < channelProperties.Count; i++)
                {

                    var data = new MemoryReadOnlyListAdapter<double>(points.Slice(i * pointCountPerChannel, pointCountPerChannel));

                    if (channelProperties[i].ShowSignal)
                    {
                        var preiod = 1.0 / sampleRate;
                        var signal = new Signal(new SignalSourceDouble(data, preiod));
                        signal.LineStyle.Color = palette.GetColor(i);
                        signal.MarkerStyle.IsVisible = false;

                        var legendItem = new LegendItem();
                        legendItem.LineStyle.Color = signal.LineStyle.Color;
                        legendItem.LineStyle.Width = 2;
                        legendItem.MarkerStyle.IsVisible = false;
                        legendItem.LabelText = $"ch{i + 1}  vpp: {data.Max() - data.Min():0.###}mV  ave: {data.Average():0.###}mV";

                        signalList.Add((legendItem, signal));
                    }
                }


                if (signalList.Count > 0)
                {

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WpfPlot1.Plot.Clear();

                        var legend = WpfPlot1.Plot.Legend;
                        var manualLegendItems = new List<LegendItem>();
                        legend.BackgroundFillStyle.Color = Colors.Transparent;
                        legend.ShadowOffset = PixelOffset.Zero;
                        legend.FontSize = 12;
                        //legend.Font.AntiAlias = true;
                        legend.FontColor = Colors.White;
                        legend.FontName = Fonts.Sans;
                        legend.IsVisible = true;
                        legend.ManualItems = manualLegendItems;

                        foreach (var (legendItem, signal) in signalList)
                        {
                            manualLegendItems.Add(legendItem);
                            WpfPlot1.Plot.PlottableList.Add(signal);
                        }

                        WpfPlot1.Plot.Axes.AutoScale();
                        WpfPlot1.Refresh();

                    });

                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
