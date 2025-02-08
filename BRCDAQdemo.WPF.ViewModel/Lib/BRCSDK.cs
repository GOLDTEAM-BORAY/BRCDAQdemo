using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BRCDAQdemo.WPF.Core.Lib
{
    public static class BRCSDK
    {
#if X64
        private const string DLLPATH = "Lib/x64/brc_daq_sdk.dll";
#elif X86
        private const string DLLPATH = "Lib/x86/brc_daq_sdk.dll";
#endif


        public enum CouplingMode : int
        {
            DC = 0,
            AC = 1,
        }
        public enum SourceType : int
        {
            ONBOARD = 0,
            EXTERNAL = 1,
        }


        #region GetLastError
        [DllImport(DLLPATH, EntryPoint = "get_last_error", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int GetLastError(byte* pErr);
        private static unsafe string GetLastError()
        {
            byte[] utf8Bytes = new byte[1024];
            var handle = utf8Bytes.AsMemory().Pin();
            try
            {
                int result = GetLastError((byte*)handle.Pointer);
                if (result != 0)
                {
                    throw new Exception($"Error getting last error: {result}");
                }

                // 找到字符串的实际长度
                int length = Array.IndexOf(utf8Bytes, (byte)0);
                if (length < 0) length = 1024;
                return Encoding.UTF8.GetString(utf8Bytes, 0, length);

            }
            finally
            {
                handle.Dispose();
            }
        }
        #endregion


        /// <summary>
        /// 发现设备
        /// </summary>
        /// <returns>可用设备数</returns>
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int scan_modules();


        private static List<ModuleInfo> _moduleInfos = [];

        public static List<ModuleInfo> ScanModules()
        {
            var moduleCount = scan_modules();
            _moduleInfos = Enumerable.Range(0, moduleCount).Select(index => new ModuleInfo
            {
                DeviceId = GetModuleInfoDeviceId(index),
                ProductName = GetModuleInfoProductName(index),
                ChannelCount = GetModuleInfoChannelCount(index),
                SampleRateOptions = GetModuleInfoSampleRateOptions(index),
                CurrentOptions = GetModuleInfoSampleCurrentOptions(index),
                CouplingOptions = GetModuleInfoSampleCouplingOptions(index),
            }).ToList();
            return _moduleInfos;
        }




        #region 获取设备基础信息
        private enum ModuleInfoType : int
        {
            ProductName = 1,
            DeviceId = 2,
            ChannelCount = 3,
            SampleRateOptions = 4,
            GainOptions = 5,
            CurrentOptions = 6,
            CouplingOptions = 7,
        }

        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int get_module_info(int index, ModuleInfoType moduleInfoType, void* ptr1, void* ptr2);

        /// <summary>
        /// 获取设备名称
        /// </summary>
        /// <param name="index">索引序号</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static unsafe string GetModuleInfoProductName(int index)
        {
            byte[] bytes = new byte[1024];
            var handle = bytes.AsMemory().Pin();
            try
            {
                if (get_module_info(index, ModuleInfoType.ProductName, handle.Pointer, null) != 0)
                    throw new Exception(GetLastError());
                int length = Array.IndexOf(bytes, (byte)0);
                if (length < 0) length = 1024;
                return Encoding.Default.GetString(bytes, 0, length);
            }
            finally
            {
                handle.Dispose();
            }
        }
        public static unsafe string GetModuleInfoDeviceId(int index)
        {
            byte[] bytes = new byte[1024];
            var handle = bytes.AsMemory().Pin();
            try
            {
                if (get_module_info(index, ModuleInfoType.DeviceId, handle.Pointer, null) != 0)
                    throw new Exception(GetLastError());
                int length = Array.IndexOf(bytes, (byte)0);
                if (length < 0) length = 1024;
                return Encoding.Default.GetString(bytes, 0, length);
            }
            finally
            {
                handle.Dispose();
            }
        }
        public static unsafe int GetModuleInfoChannelCount(int index)
        {
            int channelCount = 0;
            if (get_module_info(index, ModuleInfoType.ChannelCount, &channelCount, null) != 0)
                throw new Exception(GetLastError());
            return channelCount;
        }
        public static unsafe List<double> GetModuleInfoSampleRateOptions(int index)
        {
            double[] pDouble = new double[1024];
            int length = 0;
            var handle = pDouble.AsMemory().Pin();
            try
            {
                if (get_module_info(index, ModuleInfoType.SampleRateOptions, handle.Pointer, &length) != 0)
                    throw new Exception(GetLastError());
                return new List<double>(pDouble.Take(length));
            }
            finally
            {
                handle.Dispose();
            }
        }
        public static unsafe List<double> GetModuleInfoSampleCurrentOptions(int index)
        {
            double[] pDouble = new double[1024];
            int length = 0;
            var handle = pDouble.AsMemory().Pin();
            try
            {
                if (get_module_info(index, ModuleInfoType.CurrentOptions, handle.Pointer, &length) != 0)
                    throw new Exception(GetLastError());
                return new List<double>(pDouble.Take(length));
            }
            finally
            {
                handle.Dispose();
            }
        }
        public static unsafe List<CouplingMode> GetModuleInfoSampleCouplingOptions(int index)
        {
            int[] array = new int[512];
            int length = 0;
            var handle = array.AsMemory().Pin();
            try
            {
                if (get_module_info(index, ModuleInfoType.CouplingOptions, handle.Pointer, &length) != 0)
                    throw new Exception(GetLastError());
                return array.Take(length).Select(i => (CouplingMode)i).ToList();
            }
            finally
            {
                handle.Dispose();
            }
        }
        #endregion



        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int connect_module(int index);
        private enum ModulePropertyType : int
        {
            ClockSource = 1,
            TrigerSource = 2,
            SampleRate = 3
        }
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int get_module_property(int mHandle, ModulePropertyType propertyType, void* ptr1, void* ptr2);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int set_module_property(int mHandle, ModulePropertyType propertyType, void* ptr1, void* ptr2);
        private enum ChannelPropertyType : int
        {
            Enabled = 1,
            Gain = 2,
            Current = 3,
            CouplingMode = 4
        }
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int get_channel_property(int mHandle, int channelIndex, ChannelPropertyType propertyType, void* ptr1, void* ptr2);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int set_channel_property(int mHandle, int channelIndex, ChannelPropertyType propertyType, void* ptr1, void* ptr2);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int disconnect_module(int mHandle);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int start(int mHandle, bool rawValue);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int stop(int mHandle);
        [DllImport(DLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int get_channels_data(int mHandle, double* data_array, int length, int data_array_length, int timeout);




        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="index">索引号</param>
        /// <returns>设备对象</returns>
        public static BrcDevice Connect(ModuleInfo moduleInfo)
        {
            var index = _moduleInfos.FindIndex((m) => m.DeviceId == moduleInfo.DeviceId);
            int handle = connect_module(index);
            if (handle < 0)
            {
                var error = GetLastError();
                throw new Exception($"设备连接失败 {error}");
            }
            var brcDevice = new BrcDevice(handle, moduleInfo);

            _moduleInfos.RemoveAt(index);

            return brcDevice;
        }



        public class ModuleInfo()
        {
            public string DeviceId { get; set; }
            public string ProductName { get; set; }
            public List<double> GainOptions { get; set; } = [];
            public List<double> SampleRateOptions { get; set; } = [];
            public List<double> CurrentOptions { get; set; } = [];
            public List<CouplingMode> CouplingOptions { get; set; } = [];
            public int ChannelCount { get; set; }
        }

        public class BrcDevice(int mHandle, ModuleInfo ModuleInfo) : IDisposable
        {
            #region 获取/设置设备参数
            public unsafe SourceType GetModulePropertyClockSource()
            {
                int clockSource = 0;
                if (get_module_property(mHandle, ModulePropertyType.ClockSource, &clockSource, null) != 0)
                    throw new Exception(GetLastError());
                return (SourceType)clockSource;
            }
            public unsafe SourceType GetModulePropertyTrigerSource()
            {
                int trigerSource = 0;
                if (get_module_property(mHandle, ModulePropertyType.TrigerSource, &trigerSource, null) != 0)
                    throw new Exception(GetLastError());
                return (SourceType)trigerSource;
            }
            public unsafe double GetModulePropertySampleRate()
            {
                double sampleRate = 0;
                if (get_module_property(mHandle, ModulePropertyType.SampleRate, &sampleRate, null) != 0)
                    throw new Exception(GetLastError());
                return sampleRate;
            }

            public unsafe void SetModulePropertyClockSource(SourceType sourceType)
            {
                int source = (int)sourceType;
                if (set_module_property(mHandle, ModulePropertyType.ClockSource, &source, null) != 0)
                    throw new Exception(GetLastError());
            }
            public unsafe void SetModulePropertyTrigerSource(SourceType sourceType)
            {
                int source = (int)sourceType;
                if (set_module_property(mHandle, ModulePropertyType.TrigerSource, &source, null) != 0)
                    throw new Exception(GetLastError());
            }
            public unsafe void SetModulePropertySampleRate(double sampleRate)
            {
                if (set_module_property(mHandle, ModulePropertyType.SampleRate, &sampleRate, null) != 0)
                    throw new Exception(GetLastError());
            }
            #endregion

            #region 获取/设置通道参数
            public unsafe bool GetChannelPropertyEnabled(int channelIndex)
            {
                byte enabled = 0;
                if (get_channel_property(mHandle, channelIndex, ChannelPropertyType.Enabled, &enabled, null) != 0)
                    throw new Exception(GetLastError());
                return enabled != 0;
            }
            public unsafe double GetChannelPropertyGain(int channelIndex)
            {
                double gain = 0;
                if (get_channel_property(mHandle, channelIndex, ChannelPropertyType.Gain, &gain, null) != 0)
                    throw new Exception(GetLastError());
                return gain;
            }
            public unsafe double GetChannelPropertyCurrent(int channelIndex)
            {
                double current = 0;
                if (get_channel_property(mHandle, channelIndex, ChannelPropertyType.Current, &current, null) != 0)
                    throw new Exception(GetLastError());
                return current;
            }
            public unsafe CouplingMode GetChannelPropertyCouplingMode(int channelIndex)
            {
                int couplingMode = 0;
                if (get_channel_property(mHandle, channelIndex, ChannelPropertyType.CouplingMode, &couplingMode, null) != 0)
                    throw new Exception(GetLastError());
                return (CouplingMode)couplingMode;
            }
            public unsafe void SetChannelPropertyEnabled(int channelIndex, bool enabled)
            {
                byte value = (byte)(enabled ? 1 : 0);
                if (set_channel_property(mHandle, channelIndex, ChannelPropertyType.Enabled, &value, null) != 0)
                    throw new Exception(GetLastError());
            }
            public unsafe void SetChannelPropertyGain(int channelIndex, double gain)
            {
                if (set_channel_property(mHandle, channelIndex, ChannelPropertyType.Gain, &gain, null) != 0)
                    throw new Exception(GetLastError());
            }
            public unsafe void SetChannelPropertyCurrent(int channelIndex, double current)
            {
                if (set_channel_property(mHandle, channelIndex, ChannelPropertyType.Current, &current, null) != 0)
                    throw new Exception(GetLastError());
            }
            public unsafe void SetChannelPropertyCouplingMode(int channelIndex, CouplingMode couplingMode)
            {
                int value = (int)couplingMode;
                if (set_channel_property(mHandle, channelIndex, ChannelPropertyType.CouplingMode, &value, null) != 0)
                    throw new Exception(GetLastError());
            }
            #endregion

            public void Disconnect()
            {
                if (disconnect_module(mHandle) < 0)
                    throw new Exception(GetLastError());
            }
            public void Start()
            {
                if (start(mHandle, false) != 0)
                    throw new Exception(GetLastError());
            }
            public void Stop()
            {
                if (stop(mHandle) != 0)
                    throw new Exception(GetLastError());
            }
            public unsafe void GetChannelsData(Memory<double> array, TimeSpan timeout)
            {
                if (array.Length % ModuleInfo.ChannelCount != 0)
                    throw new InvalidOperationException("数组长度必须是通道数的整数倍");
                var handle = array.Pin();
                try
                {
                    if (get_channels_data(mHandle, (double*)handle.Pointer, array.Length / ModuleInfo.ChannelCount, 0, (int)timeout.TotalMilliseconds) != 0)
                        throw new Exception(GetLastError());
                }
                finally
                {
                    handle.Dispose();
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Disconnect();
            }
        }
    }
}
