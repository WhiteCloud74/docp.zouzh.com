using FrameworkCore.Instrument;
using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AppClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string _url = "https://localhost:5001/";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitDatabase_Click(object sender, RoutedEventArgs e)
        {
            string ret = HttpHelper.HttpClienPut($"{_url}Database/InitDatabase", "");
            MessageBox.Show(ret, "初始化数据库");
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string usrGateway = JsonConvert.SerializeObject(TestStub.CreateGatewayProduct());
                var response1 = HttpHelper.HttpClientPost($"{_url}Product/AddOne", usrGateway);

                string smartLamp = JsonConvert.SerializeObject(TestStub.CreateSmartLampProduct());
                var response2 = HttpHelper.HttpClientPost($"{_url}Product/AddOne", smartLamp);

                MessageBox.Show($"添加网关产品：{response1}{Environment.NewLine}添加智能台灯产品：{response2}", "添加产品");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            //Mac地址
            //
            try
            {
                var productString = HttpHelper.HttpClientGet($"{_url}Product/GetAll");
                var products = JsonConvert.DeserializeObject<List<Product>>(productString, MyDataTypeJsonConvert.Instance);


                var usrProduct = products.Where(p => p.IsGateway).ToList()[0];
                var usrDeviceString = HttpHelper.HttpClientGet($"{_url}Device/GetTemplate/{usrProduct.ProductId}");
                var usrDevice = JsonConvert.DeserializeObject<Device>(usrDeviceString, MyDataTypeJsonConvert.Instance);

                usrDevice.DeviceId = Guid.NewGuid();
                usrDevice.GatewayId = usrDevice.DeviceId;
                usrDevice.IsGateway = true;
                usrDevice.IsIndependentOnline = true;
                usrDevice.IsOnLine = true;
                usrDevice.MacAddress = "CED400010000";
                var response1 = HttpHelper.HttpClientPost($"{_url}Device/AddOne", JsonConvert.SerializeObject(usrDevice));


                var smartLampProduct = products.Where(p => !p.IsGateway).ToList()[0];
                var smartLampDevice = HttpHelper.HttpClientGet($"{_url}Device/GetTemplate/{smartLampProduct.ProductId}");
                var device = JsonConvert.DeserializeObject<Device>(smartLampDevice, MyDataTypeJsonConvert.Instance);

                device.DeviceId = Guid.NewGuid();
                device.GatewayId = usrDevice.DeviceId;
                device.IsGateway = false;
                device.IsIndependentOnline = false;
                device.IsOnLine = true;
                device.MacAddress = "CED400010001";

                device.DeviceNameplates[0].DataValue = "25";
                device.DeviceNameplates[1].DataValue = "黑色";
                device.DeviceProperties[0].DataValue = "220";
                device.DeviceProperties[1].DataValue = "8";
                device.DeviceProperties[2].DataValue = "6";
                device.DeviceProperties[3].DataValue = "开";
                var response2 = HttpHelper.HttpClientPost($"{_url}Device/AddOne", JsonConvert.SerializeObject(device));

                MessageBox.Show($"添加网关设备：{response1}{Environment.NewLine}添加智能台灯设备：{response2}", "添加产品");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void GetProperties_Click(object sender, RoutedEventArgs e)
        {
            var deviceString = HttpHelper.HttpClientGet($"{_url}Device/GetAll");
            var devices = JsonConvert.DeserializeObject<List<Device>>(deviceString, MyDataTypeJsonConvert.Instance);
            var smartLampDevice = devices.Where(d => !d.IsGateway).ToList()[0];

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("deviceId", smartLampDevice.DeviceId.ToString())
            };
            foreach (var item in smartLampDevice.DeviceProperties)
            {
                parameters.Add(new KeyValuePair<string, string>(item.DeviceFieldName, ""));
            }

            var ret = HttpHelper.HttpClientPostAsync($"{_url}Operate/GetProperties", JsonConvert.SerializeObject(parameters)).Result;
            MessageBox.Show(ret, "GetProperties");
        }
    }
}
