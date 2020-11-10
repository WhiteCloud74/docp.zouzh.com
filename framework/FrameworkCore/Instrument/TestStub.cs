using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.ProductDefine;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.Instrument
{
    public static class TestStub
    {
        public static Product CreateTestProduct()
        {
            Guid productId = Guid.NewGuid();

            return new Product()
            {
                ProductId = Guid.NewGuid(),
                ProductName = "智能台灯",
                IsGateway = true,
                IsIndependentOnline = true,
                ProductBrands = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="商标",IsReadOnly=true, // ProductBrandsId=productId,
                        MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue="麒麟QQ"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="厂商",IsReadOnly=true,// ProductBrandsId=productId,
                        MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  } , DataValue="深圳麒麟电器科技有限公司"},
                },
                ProductNameplates = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="颜色",IsReadOnly=true, //ProductNameplatesId=productId,
                        MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue="深红色"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="功率商标",IsReadOnly=true,// ProductNameplatesId=productId,
                        MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=200, Min=20,  }, DataValue="40"  },
                },
                ProductProperties = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="亮度",IsReadOnly=false, //ProductPropertiesId=productId,
                        MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=7, Min=0,  }, DataValue="2"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="开关",IsReadOnly=false, //ProductPropertiesId=productId,
                        MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=1, MinLength=1,  }, DataValue="开"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="电压",IsReadOnly=true, //ProductPropertiesId=productId,
                        MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=240, Min=200,  }, DataValue="220"  },
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="电流",IsReadOnly=true, //ProductPropertiesId=productId,
                        MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue="QQ"  },
                },
                ProductEvents = new List<ProductEvent>() {
                    new ProductEvent(){ ProductEventId=Guid.NewGuid(), ProductEventName="电源变化",
                        ProductEventProperties=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前电压",IsReadOnly=true,//ProductEventId=productId,
                                MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=240, Min=200,  }, DataValue="220"  },
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前电流",IsReadOnly=true,//ProductEventId=productId,
                                MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=10, Min=1,  }, DataValue="3"  },
                        }
                    },
                    new ProductEvent(){ ProductEventId=Guid.NewGuid(), ProductEventName="开关状态改变",
                        ProductEventProperties=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前状态",IsReadOnly=true,//ProductEventId=productId,
                                MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                        }
                    }
                },
                ProductFunctions = new List<ProductFunction>() {
                    new ProductFunction(){ ProductFunctionId=Guid.NewGuid(), ProductFunctionName="开关",
                        ProductFunctionInputs=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="开关",IsReadOnly=true,//ProductFunctionInputsId=productId,
                                MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                            },
                        ProductFunctionOutputs = new List<ProductField>() {
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="开关状态",IsReadOnly=true,//ProductFunctionOutputsId=productId,
                                MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                        },
                    },
                    new ProductFunction(){ ProductFunctionId=Guid.NewGuid(), ProductFunctionName="调节亮度及颜色",
                        ProductFunctionInputs=new List<ProductField>(){
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="亮度",IsReadOnly=true,//ProductFunctionInputsId=productId,
                                MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="颜色",IsReadOnly=true,//ProductFunctionInputsId=productId,
                                MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                            },
                        ProductFunctionOutputs = new List<ProductField>() {
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前亮度",IsReadOnly=true,//ProductFunctionOutputsId=productId,
                                MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                            new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="当前颜色",IsReadOnly=true,//ProductFunctionOutputsId=productId,
                                MyDataType=new StringType(){ MyDataTypeId=Guid.NewGuid(), MaxLength=128, MinLength=2,  }, DataValue=""  },
                        },
                    },
                }
            };

        }
    }
}
