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


        public enum CouplingOptions : int
        {
            DC = 0,
            AC = 1,
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
        [DllImport(DLLPATH, EntryPoint = "scan_modules", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ScanModules();




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

        [DllImport(DLLPATH, EntryPoint = "get_module_info", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int GetModuleInfo(int index, ModuleInfoType moduleInfoType, void* ptr1, void* ptr2);

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
                if (GetModuleInfo(index, ModuleInfoType.ProductName, handle.Pointer, null) != 0)
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
                if (GetModuleInfo(index, ModuleInfoType.DeviceId, handle.Pointer, null) != 0)
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
            if (GetModuleInfo(index, ModuleInfoType.ChannelCount, &channelCount, null) != 0)
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
                if (GetModuleInfo(index, ModuleInfoType.SampleRateOptions, handle.Pointer, &length) != 0)
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
                if (GetModuleInfo(index, ModuleInfoType.CurrentOptions, handle.Pointer, &length) != 0)
                    throw new Exception(GetLastError());
                return new List<double>(pDouble.Take(length));
            }
            finally
            {
                handle.Dispose();
            }
        }
        public static unsafe List<CouplingOptions> GetModuleInfoSampleCouplingOptions(int index)
        {
            int[] array = new int[512];
            int length = 0;
            var handle = array.AsMemory().Pin();
            try
            {
                if (GetModuleInfo(index, ModuleInfoType.CouplingOptions, handle.Pointer, &length) != 0)
                    throw new Exception(GetLastError());
                return array.Take(length).Select(i => (CouplingOptions)i).ToList();
            }
            finally
            {
                handle.Dispose();
            }
        }
        #endregion



        [DllImport(DLLPATH, EntryPoint = "connect_module", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ConnectModule(int index);



        [DllImport(DLLPATH, EntryPoint = "set_module_property", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetModuleProperty(int mHandle, int propertyType, IntPtr ptr1, IntPtr ptr2);
        [DllImport(DLLPATH, EntryPoint = "get_module_property", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetModuleProperty(int mHandle, int propertyType, IntPtr ptr1, IntPtr ptr2);
        [DllImport(DLLPATH, EntryPoint = "set_channel_property", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetChannelProperty(int mHandle, int channelIndex, int propertyType, IntPtr ptr1, IntPtr ptr2);
        [DllImport(DLLPATH, EntryPoint = "get_channel_property", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetChannelProperty(int mHandle, int channelIndex, int propertyType, IntPtr ptr1, IntPtr ptr2);



        [DllImport(DLLPATH, EntryPoint = "disconnect_module", CallingConvention = CallingConvention.Cdecl)]
        public static extern int DisconnectModule(int mHandle);

        [DllImport(DLLPATH, EntryPoint = "start", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Start(int mHandle, bool rawValue);

        [DllImport(DLLPATH, EntryPoint = "stop", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Stop(int mHandle);

        [DllImport(DLLPATH, EntryPoint = "get_channels_data", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetChannelsData(int mHandle, IntPtr data_array, int length, int data_array_length, int timeout);
    }
}
