using FrameworkCore.Instrument;
using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.ProductDefine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            Product product = new Product()
            {
                ProductId = Guid.NewGuid(),
                ProductName = "智能台灯",
                IsGateway = true,
                IsIndependentOnline = true,
                ProductBrands = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="商标",IsReadOnly=true,
                        DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue="麒麟QQ"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="厂商",IsReadOnly=true,
                        DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  } , DataValue="深圳麒麟电器科技有限公司"},
                },
                ProductNameplates = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="颜色",IsReadOnly=true,
                        DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue="深红色"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="功率商标",IsReadOnly=true,
                        DataType=new IntType(){ DataTypeId=Guid.NewGuid(), Max=200, Min=20,  }, DataValue="40"  },
                },
                ProductProperties = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="亮度",IsReadOnly=false,
                        DataType=new IntType(){ DataTypeId=Guid.NewGuid(), Max=7, Min=0,  }, DataValue="2"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="开关",IsReadOnly=false,
                        DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=1, MinLength=1,  }, DataValue="开"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="电压",IsReadOnly=true,
                        DataType=new IntType(){ DataTypeId=Guid.NewGuid(), Max=240, Min=200,  }, DataValue="220"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="电流",IsReadOnly=true,
                        DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue="QQ"  },
                },
                ProductEvents = new List<ProductEvent>() {
                    new ProductEvent(){ ProductEventId=Guid.NewGuid(), ProductEventName="电源变化",
                        ProductEventProperties=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前电压",IsReadOnly=true,
                                DataType=new IntType(){ DataTypeId=Guid.NewGuid(), Max=240, Min=200,  }, DataValue="220"  },
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前电流",IsReadOnly=true,
                                DataType=new IntType(){ DataTypeId=Guid.NewGuid(), Max=10, Min=1,  }, DataValue="3"  },
                        }
                    },
                    new ProductEvent(){ ProductEventId=Guid.NewGuid(), ProductEventName="开关状态改变",
                        ProductEventProperties=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前状态",IsReadOnly=true,
                                DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                        }
                    }
                },
                ProductFunctions = new List<ProductFunction>() {
                    new ProductFunction(){ ProductFunctionId=Guid.NewGuid(), ProductFunctionName="开关",
                        ProductFunctionInputs=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="开关",IsReadOnly=true,
                                DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },

                            },
                        ProductFunctionOutputs = new List<ProductField>() {
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="开关状态",IsReadOnly=true,
                                DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },

                        },
                    },
                    new ProductFunction(){ ProductFunctionId=Guid.NewGuid(), ProductFunctionName="调节亮度及颜色",
                        ProductFunctionInputs=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="亮度",IsReadOnly=true,
                                DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="颜色",IsReadOnly=true,
                                DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                            },
                        ProductFunctionOutputs = new List<ProductField>() {
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前亮度",IsReadOnly=true,
                                DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前颜色",IsReadOnly=true,
                                DataType=new StringType(){ DataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                        },
                    },
                }
            };

            HttpHelper.HttpClientPost("http://192.168.1.32/api/ProductControll/post/AddAsync", JsonConvert.SerializeObject(product));

        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
