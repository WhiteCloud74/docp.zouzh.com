using FrameworkCore.Instrument;
using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            string content = JsonConvert.SerializeObject(TestStub.CreateTestProduct());
            var response = HttpHelper.HttpClientPost($"{_url}Product/AddOne", content);

            MessageBox.Show(response, "添加产品");
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            var productString = HttpHelper.HttpClientGet($"{_url}Product/GetAll");
            var product = JsonConvert.DeserializeObject<List<Product>>(productString, MyDataTypeJsonConvert.Instance)[0];

            var templateString = HttpHelper.HttpClientGet($"{_url}Device/GetTemplate/{product.ProductId}");
            if (templateString != null)
            {
                var device = JsonConvert.DeserializeObject<Device>(templateString, MyDataTypeJsonConvert.Instance);
                device.DeviceId = Guid.NewGuid();
                device.IsGateway = false;
                device.IsIndependentOnline = false;
                device.DeviceNameplates[0].DataValue = "25";
                device.DeviceNameplates[1].DataValue = "黑色";
                device.DeviceProperties[0].DataValue = "220";
                device.DeviceProperties[1].DataValue = "8";
                device.DeviceProperties[2].DataValue = "6";
                device.DeviceProperties[3].DataValue = "开";

                var response = HttpHelper.HttpClientPost($"{_url}Device/AddOne", JsonConvert.SerializeObject(device));
                MessageBox.Show(response, "添加设备");

            }
            else
            {
                MessageBox.Show("error", "添加设备");
            }
        }

        private void Energy_Click(object sender, RoutedEventArgs e)
        {
            var deviceString = HttpHelper.HttpClientGet($"{_url}Device/GetAll");
            var device = JsonConvert.DeserializeObject<List<Device>>(deviceString, MyDataTypeJsonConvert.Instance)[0];

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("deviceId", device.DeviceId.ToString()));
            foreach (var item in device.DeviceProperties)
            {
                parameters.Add(new KeyValuePair<string, string>(item.DeviceFieldName, ""));
            }

            var ret = HttpHelper.HttpClientPostAsync($"{_url}Operate/GetProperties", JsonConvert.SerializeObject(parameters)).Result;
            MessageBox.Show(ret, "GetEnergy");
        }
    }
}
