/*
所有函数返回值均为int, -1表示发生异常
当异常发生时，可通过 get_last_error 方法获取最近一次异常文本内容，pErr由调用方申请内存，不能小于256字节
*/

#ifdef _WIN32
    #define BRC_API __declspec(dllexport)
#else
    #define BRC_API __attribute__((visibility("default")))
#endif

/*
获取最近一次异常的文本信息
*/
extern "C" BRC_API int get_last_error(char* pErr);

/*
扫描板卡，固定耗时500ms, 返回可用板卡数
*/
extern "C" BRC_API int scan_modules();

/*
获取板卡信息
index 板卡序号0 ~ n-1
propertyType 信息类型
	ProductName = 1,   char*
	DeviceId = 2,      char*
	ChannelCount = 3,  int*
	SampleRateOptions = 4,  double* (ptr2为 int* 表示数组长度)
	GainOptions = 5,    double* (ptr2为 int* 表示数组长度)
	CurrentOptions = 6,  double* (ptr2为 int* 表示数组长度)
	CouplingOptions = 7,  int* (ptr2为 int* 表示数组长度)
	InterfaceType = 20,  int* (0 UNKNOW, 1 USB, 2 ETH)
ptr1 参数1指针
ptr2 参数2指针
*/
extern "C" BRC_API int get_module_info(int index, int propertyType, void* ptr1, void* ptr2);

/*
连接板卡, 对板卡的所有操作均要在连接成功之后进行
index 板卡序号0 ~ n-1
返回值mHandle 用于操作已建立连接的板卡
*/
extern "C" BRC_API int connect_module(int index);


/*
设置板卡属性
mHandle 板卡句柄
propertyType 信息类型:
	ClockSource = 1,
	TrigerSource = 2,
	SampleRate = 3
ptr1 参数1指针 
    ClockSource或TrigerSource => ptr1为int*, 源类型, ONBOARD = 0, EXTERNAL = 1;
    SampleRate => ptr1为double* : 采样率, 任意取值, 设备将自动适配到最接近的可用采样率
ptr2 参数2指针
*/
extern "C" BRC_API int set_module_property(int mHandle, int propertyType, void* ptr1, void* ptr2);


/*
读取板卡属性
mHandle 板卡句柄
propertyType 信息类型:
	ClockSource = 1,
	TrigerSource = 2,
	SampleRate = 3
ptr1 参数1指针 
ptr2 参数2指针
*/
extern "C" BRC_API int get_module_property(int mHandle, int propertyType, void* ptr1, void* ptr2);


/*
设置通道属性
mHandle 板卡句柄
channelIndex 通道序号 0 ~ N-1
propertyType 信息类型:
	Enabled = 1,
	Gain = 2,
	Current = 3,
	CouplingMode = 4
ptr1 参数1指针
	Enabled => bool* 或 uint8_t*, true表示通道启用 false表示通道禁用, （注：暂未实现, 修改无效, 可直接传固定值 true 或 1）
	Gain => double* , 增益倍数, 任意取值, 设备将自动适配到最接近的增益, （注：暂未实现,修改无效, 可直接传固定值 1）
	Current => double* , IEPE激励电流(mA), 任意取值, 设备将自动适配到最接近的IEPE激励电流
	CouplingMode => int* , 耦合方式, DC耦合=0, AC耦合=1
ptr2 参数2指针
*/
extern "C" BRC_API int set_channel_property(int mHandle, int channelIndex, int propertyType, void* ptr1, void* ptr2);

/*
读取通道属性
mHandle 板卡句柄
channelIndex 通道序号 0 ~ N-1
propertyType 信息类型:
	Enabled = 1,
	Gain = 2,
	Current = 3,
	CouplingMode = 4
ptr1 参数1指针
ptr2 参数2指针
*/
extern "C" BRC_API int get_channel_property(int mHandle, int channelIndex, int propertyType, void* ptr1, void* ptr2);


/*
断开板卡
*/
extern "C" BRC_API int disconnect_module(int mHandle);

/*
开始采样
rawValue 是否取ADC原始值, false:正常使用,后续返回电压值;  true:定标校准时使用, 后续返回ADC原始值
*/
extern "C" BRC_API int start(int mHandle, bool rawValue);

/*
开始采样,通过UDP广播的方式同步触发多个板卡采样，
mHandle_array 指针，传入已经connect过的mHandle数组的首地址
length 数组长度
*/
extern "C" BRC_API int start_via_udp(int* mHandle_array, int length);

/*
停止采样, 会清空内存中还未提取的数据
*/
extern "C" BRC_API int stop(int mHandle);

/*
获取所有通道数据
data_array 数据指针, 调用方申请空间, sdk填充数据
length 希望获取的点数, 请注意, 该函数将在指针data_array指向的内存填充 N x length 长度的数据,N 为板卡通道数
数据以交织方式填充 例如: 1111 2222 3333 4444
data_array_length 数组总长度 N x length ,labview需要传入这个参数，但SDK用不到
timeout 超时时间(ms), 指定时间内若仍未有足够的点数将触发超时错误
*/
extern "C" BRC_API int get_channels_data(int mHandle, double* data_array, int length, int data_array_length, int timeout);

/*
设置指定通道在指定增益下的校准参数, 此函数调用后会立即生效, 但不会持久化, 重启后将复原
若要固化参数，请调用 save_calibration 方法
channel 通道号 0 - N-1
gain 增益, 注意：gain填错将不会生效
K 系数 K
B 常数 B,  注：Y=KX+B, X为ADC原始值, Y为电压值
*/
extern "C" BRC_API int set_calibration(int mHandle, int channel, double sampleRate, double gain, double K, double B);

/*
将当前的校准参数全部保存至flash 持久化
*/
extern "C" BRC_API int save_calibration(int mHandle);

/*
清除内存和flash中的所有校准参数, 复位成出厂设置
*/
extern "C" BRC_API int reset_calibration(int mHandle);

/*
获取模块温度
pChipTemperature 芯片温度（指针）
pAmbientTemperature 环境温度（指针）
*/
extern "C" BRC_API int check_temperature(int mHandle, float* pChipTemperature, float* pAmbientTemperature);