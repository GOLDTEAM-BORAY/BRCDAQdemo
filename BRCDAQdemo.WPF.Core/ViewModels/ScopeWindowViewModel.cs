using BRCDAQdemo.WPF.Core.Lib;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BRCDAQdemo.WPF.Core.Lib.BRCSDK;

namespace BRCDAQdemo.WPF.Core.ViewModels
{
    public interface IRenderable
    {
        Task Render(IList<ScopeWindowViewModel.ChannelProperty> channelProperties, Memory<double> points, double sampleRate);
    }

    public partial class ScopeWindowViewModel : ObservableObject, IDisposable
    {
        public IRenderable Renderer { get; set; }
        private BrcDevice _devce;
        public BrcDevice Device
        {
            get
            {
                return _devce;
            }
            set
            {
                SelectedSampleRate = value.ModuleInfo.SampleRateOptions.FirstOrDefault();
                PointCount = (int)SelectedSampleRate;

                for (int i = 0; i < value.ModuleInfo.ChannelCount; i++)
                {
                    //var color = palette.GetColor(i);
                    var channelProperty = new ChannelProperty()
                    {
                        Id = i + 1,
                        //Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue)),

                        ShowSignal = true,
                        SelectedCoupling = value.ModuleInfo.CouplingOptions.FirstOrDefault(),
                        SelectedCurrent = value.ModuleInfo.CurrentOptions.FirstOrDefault()
                    };
                    ChannelProperties.Add(channelProperty);
                }
                _devce = value;
            }
        }
        public string Title => $"BRC示波器 {Device.ModuleInfo.ProductName}";


        [ObservableProperty]
        private ObservableCollection<ChannelProperty> channelProperties = new ObservableCollection<ChannelProperty>();



        [ObservableProperty]
        private double selectedSampleRate;
        partial void OnSelectedSampleRateChanged(double value) => PointCount = (int)value;



        [ObservableProperty]
        private int pointCount;


        public partial class ChannelProperty : ObservableObject
        {
            public int Id { get; set; }
            //public Color Color { get; set; }
            public bool ShowSignal { get; set; }
            [ObservableProperty]
            private CouplingMode selectedCoupling = CouplingMode.DC;
            partial void OnSelectedCouplingChanged(CouplingMode value)
            {
                if (value == CouplingMode.DC)
                    SelectedCurrent = 0;
            }

            [ObservableProperty]
            private double selectedCurrent;
            partial void OnSelectedCurrentChanged(double value)
            {
                if (value > 0)
                    SelectedCoupling = CouplingMode.AC;
            }
        }


        [ObservableProperty]
        private bool started = false;



        CancellationTokenSource startCts;
        [RelayCommand]
        private async Task StartAsync()
        {
            //设置时钟源、触发源、采样率
            _devce.SetModulePropertyClockSource(SourceType.ONBOARD);
            _devce.SetModulePropertyTrigerSource(SourceType.ONBOARD);
            _devce.SetModulePropertySampleRate(SelectedSampleRate);
            //采样率回读
            SelectedSampleRate = _devce.GetModulePropertySampleRate();


            for (int i = 0; i < ChannelProperties.Count; i++)
            {
                if (ChannelProperties[i].SelectedCoupling == CouplingMode.AC)
                {
                    //先设置耦合方式
                    _devce.SetChannelPropertyCouplingMode(i, ChannelProperties[i].SelectedCoupling);
                    //再设置IEPE电流
                    _devce.SetChannelPropertyCurrent(i, ChannelProperties[i].SelectedCurrent);
                }
                else
                {
                    //先设置IEPE电流
                    _devce.SetChannelPropertyCurrent(i, ChannelProperties[i].SelectedCurrent);
                    //再设置耦合方式
                    _devce.SetChannelPropertyCouplingMode(i, ChannelProperties[i].SelectedCoupling);
                }
            }

            //启动采样
            Device.Start();

            startCts = new CancellationTokenSource();

            Started = true;

            await Task.Run(async () =>
                {
                    var totalPointCount = PointCount * _devce.ModuleInfo.ChannelCount;
                    while (!startCts.Token.IsCancellationRequested)
                    {
                        var points = ArrayPool<double>.Shared.Rent(totalPointCount);
                        try
                        {
                            var buffer = points.AsMemory().Slice(0, totalPointCount);
                            _devce.GetChannelsData(buffer, TimeSpan.FromSeconds(2));
                            await Renderer.Render(ChannelProperties, buffer, SelectedSampleRate);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.Contains("task was canceled"))
                                throw;  //throw or do some log
                        }
                        finally
                        {
                            ArrayPool<double>.Shared.Return(points);
                        }
                    }
                }, startCts.Token);
            //}
        }


        [RelayCommand]
        private void Stop()
        {
            Device.Stop();
            startCts?.Cancel();
            Started = false;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Stop();
            Device.Dispose();
        }
    }
}
