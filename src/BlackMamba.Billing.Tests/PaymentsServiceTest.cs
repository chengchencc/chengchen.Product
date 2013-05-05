using Alipay.Class;
using BlackMamba.Billing.Domain.Common;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Framework.RedisMapper;
using BlackMamba.Billing.Domain;
using com.yeepay.cmbn;
using Moq;
using SubSonic.Oracle.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;
using Xunit;

namespace BlackMamba.Billing.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class PaymentsServiceTest
    {
        protected PaymentsService PaymentsService;
        protected PaymentsService yeePaymentsService;
        IRepository mockRepository;
        Mock<IRepository> _repository;

        Mock<IRedisService> _redisServiceMock;
        IRedisService redisService;

        RedisService redis = new RedisService();

        Mock<IYeepayService> _yeepayServiceMock;
        Mock<IRedisService> _redisService;
        Mock<IRESTfulClient> _restfulClient;
        Mock<IRepository> simpleOracleRepository;

        IYeepayService yeepayService;

        public PaymentsServiceTest()
        {
            _repository = new Mock<IRepository>();
            mockRepository = _repository.Object;

            _redisServiceMock = new Mock<IRedisService>();
            redisService = _redisServiceMock.Object;

            _yeepayServiceMock = new Mock<IYeepayService>();

            redis.FlushAll();
            _redisService = new Mock<IRedisService>();
            simpleOracleRepository = new Mock<IRepository>();

            redisService = new RedisService();
            yeepayService = new YeepayService();
            _restfulClient = new Mock<IRESTfulClient>();
            PaymentsService = new PaymentsService(_redisService.Object, _yeepayServiceMock.Object, _restfulClient.Object);
            yeePaymentsService = new PaymentsService(redisService, yeepayService, _restfulClient.Object);
        }

        CustomerOrder customerOrder = new CustomerOrder()
        {
            CustomerId = 1234,
            Amount = 100,
            CreateDate = DateTime.Now,
            OrderNo = "2012021618561111111",
            ProductName = "网游中心充值",
            ProductType = "游戏，网游",
            Status = OrderStatus.Created,
            ProductDescription = "productDescription",
            MerchantExtentionalDescription = "MerchantExtentionalDescription"

        };


        //[Fact]
        //public void GetPaymentUrlByOrderTest_with_all_neededparams()
        //{
        //    PaymentsService.Repository = mockRepository;
        //    _repository.Setup<CustomerOrder>(s => s.Single<CustomerOrder>((It.IsAny<Expression<Func<CustomerOrder, bool>>>()))).Returns(customerOrder);
        //    var url = PaymentsService.GetYeepayPaymentUrlByOrder("2012021618561111111");
        //    Assert.NotEmpty(url);
        //}

        [Fact]
        public void XmlResultParseTest()
        {
            string xml= "<?xml version=\"1.0\" encoding=\"gb2312\"?><Root><Result>0</Result><Msg>请发送指令 XXXXX 到 1066XXXXXXX按照短信提示进行支付</Msg></Root>";
            XmlDocument resultDoc = new XmlDocument();
            resultDoc.LoadXml(xml);
            var resultNode = resultDoc.SelectSingleNode("/Root/Result");
            var msgNode = resultDoc.SelectSingleNode("/Root/Msg");
            Assert.NotNull(resultNode);
            Assert.NotNull(msgNode);
        }

        [Fact]
        public void datetime_test()
        {
            DateTime initTime = new DateTime();
            var now = DateTime.Now;
            initTime.ToString();
            var res = initTime > new DateTime(1990, 1, 1) ? initTime.ToString() : "-";
            var nowRes = now > new DateTime(1990, 1, 1) ? now.ToString() : "-";
            Assert.Equal("-", res);
            Assert.Equal(now.ToString(), nowRes);
        }


        [Fact]
        public void add_balance_without_record()
        {
            PaymentsService.Repository = mockRepository;
            _repository.Setup(s => s.Single<FinancialAccount>((It.IsAny<Expression<Func<FinancialAccount, bool>>>())));//.Returns(null);
            var result = PaymentsService.ChangeBalance(200, 100);

            Assert.Equal(200, result.CustomerId);
            Assert.Equal(100, result.Balance);

        }

        [Fact]
        public void plus_balance_with_record()
        {
            FinancialAccount financialAccount = new FinancialAccount()
            {
                CreatedDate = DateTime.Now,
                CustomerId = 100,
                Balance = 50,
                Id = 11
            };

            PaymentsService.Repository = mockRepository;
            _repository.Setup(s => s.Single<FinancialAccount>(It.IsAny<Expression<Func<FinancialAccount, bool>>>())).Returns(financialAccount);
            _repository.Setup(s => s.Update<FinancialAccount>(It.IsAny<FinancialAccount>())).Returns(3);
            var result = PaymentsService.ChangeBalance(100, -30);

            Assert.Equal(20, result.Balance);

        }

        [Fact]
        public void update_customerOrder_with_partly_payments()
        {
            PaymentsService.Repository = mockRepository;
            _repository.Setup<CustomerOrder>(s => s.Single<CustomerOrder>((It.IsAny<Expression<Func<CustomerOrder, bool>>>()))).Returns(customerOrder);
            var resultOrder = PaymentsService.UpdateCustomerOrder("2012021618561111111", 50, 1234);

            Assert.Equal(50, resultOrder.Payed);
            Assert.Equal(100, resultOrder.Amount);
            Assert.Equal(OrderStatus.PartlySuccess, resultOrder.Status);
        }

        [Fact]
        public void update_customerOrder_with_complete_payment()
        {
            PaymentsService.Repository = mockRepository;
            _repository.Setup<CustomerOrder>(s => s.Single<CustomerOrder>((It.IsAny<Expression<Func<CustomerOrder, bool>>>()))).Returns(customerOrder);
            var resultOrder = PaymentsService.UpdateCustomerOrder("2012021618561111111", 100, 1234);

            Assert.Equal(100, resultOrder.Payed);
            Assert.Equal(100, resultOrder.Amount);
            Assert.Equal(OrderStatus.Successed, resultOrder.Status);
        }

        [Fact]
        public void update_customerOrder_with_over_payment()
        {
            PaymentsService.Repository = mockRepository;
            _repository.Setup<CustomerOrder>(s => s.Single<CustomerOrder>((It.IsAny<Expression<Func<CustomerOrder, bool>>>()))).Returns(customerOrder);
            var resultOrder = PaymentsService.UpdateCustomerOrder("2012021618561111111", 150, 1234);

            Assert.Equal(150, resultOrder.Payed);
            Assert.Equal(100, resultOrder.Amount);
            Assert.Equal(OrderStatus.Exceed, resultOrder.Status);
        }
        [Fact]
        public void update_customerOrder_with_no_order_record()
        {
            PaymentsService.Repository = mockRepository;
            _repository.Setup<CustomerOrder>(s => s.Single<CustomerOrder>((It.IsAny<Expression<Func<CustomerOrder, bool>>>())));
            var resultOrder = PaymentsService.UpdateCustomerOrder("2012021618561111111", 100, 123456);

            Assert.Equal("2012021618561111111", resultOrder.OrderNo);
            Assert.Equal(100, resultOrder.Payed);
            Assert.Equal(100, resultOrder.Amount);
            Assert.Equal(123456, resultOrder.CustomerId);

        }

        [Fact]
        public void ConvertUTF8ToGBK_test_input_null()
        {
            string oriString = null;
            var resultString = PaymentsService.ConvertUTF8ToGBK(oriString);
            Assert.NotNull(resultString);
        }

        [Fact]
        public void ConvertUTF8ToGBK_test_input_English_char()
        {
            string oriString = "2342323";
            var resultString = PaymentsService.ConvertUTF8ToGBK(oriString);
            Assert.NotNull(resultString);
        }

        [Fact]
        public void ConvertUTF8ToGBK_test_input_Chinese_char()
        {
            string oriString = "中文";
            var resultString = PaymentsService.ConvertUTF8ToGBK(oriString);
            Assert.NotNull(resultString);
        }

        [Fact]
        public void YeepayCardCallBack_test_input_SZXCallbackResult_Success()
        {
            PaymentsService.Repository = mockRepository;

            SZXCallbackResult result = new SZXCallbackResult(
                "ChargeCardDirect",
                 "1",
                 "123456789",
                 "987654321",
                 "100.0", //支付总金额
                 "CNY",
                 "456321987",
                 "100.0", //每张卡支付的金额，以半角逗号隔开
                 "100.0", //每张卡原有的金额
                 "0",     //成功
                 "全冲",
                 string.Empty,
                 string.Empty,
                 "1234asdf9874",
                 string.Empty ////errmsg
            );

            var orderBeforeUpdate = new Order()
            {
                Id = 1,
                CreatedDate = new DateTime(),
                OrderNo = result.P2_Order,
                OrderStatus = OrderStatus.Processing,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                Amount = float.Parse(result.P3_Amt)
            };

            _yeepayServiceMock.Setup<SZXCallbackResult>(m => m.VerifyCallback(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac)).Returns(result);

            _repository.Setup<Order>(m => m.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(orderBeforeUpdate);
            _repository.Setup(m => m.Update<Order>(It.IsAny<Order>()));
            _repository.Setup(m => m.Add<OrderLine>(It.IsAny<OrderLine>()));

            _redisServiceMock.Setup(m => m.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, It.IsAny<PaymentNotification>()));

            string responseStr = PaymentsService.YeepayCardCallBack(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac);

            Assert.Equal(responseStr, "SUCCESS<BR>非银行卡支付成功<BR>商户订单号:" + result.P2_Order + "<BR>实际扣款金额(商户收到该返回数据后,一定用自己数据库中存储的金额与该金额进行比较):" + result.P3_Amt);
        }

        [Fact]
        public void YeepayCardCallBack_test_input_SZXCallbackResult_Failed()
        {
            PaymentsService.Repository = mockRepository;

            SZXCallbackResult result = new SZXCallbackResult(
                "ChargeCardDirect",
                 "-1", //冲卡失败
                 "123456789",
                 "987654321",
                 "100.0",
                 "CNY",
                 "456321987",
                 "100.0",
                 "100.0",
                 "1007", //余额不足
                 "全冲",
                 string.Empty,
                 string.Empty,
                 "1234asdf9874",
                 string.Empty
            );

            var orderBeforeUpdate = new Order()
            {
                Id = 1,
                CreatedDate = new DateTime(),
                OrderNo = result.P2_Order,
                OrderStatus = OrderStatus.Processing,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                Amount = float.Parse(result.P3_Amt)
            };

            _yeepayServiceMock.Setup<SZXCallbackResult>(m => m.VerifyCallback(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac)).Returns(result);

            _repository.Setup<Order>(m => m.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(orderBeforeUpdate);
            _repository.Setup(m => m.Update<Order>(It.IsAny<Order>()));
            _repository.Setup(m => m.Add<OrderLine>(It.IsAny<OrderLine>()));

            _redisServiceMock.Setup(m => m.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, It.IsAny<PaymentNotification>()));

            string responseStr = PaymentsService.YeepayCardCallBack(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac);

            Assert.Equal(responseStr, "SUCCESS交易失败!");
        }

        [Fact]
        public void YeepayCardCallBack_test_input_SZXCallbackResult_Error()
        {
            SZXCallbackResult result = new SZXCallbackResult(
                "ChargeCardDirect",
                 "-1", //冲卡失败
                 "123456789",
                 "987654321",
                 "100.0",
                 "CNY",
                 "456321987",
                 "100.0",
                 "100.0",
                 "10000", //未知错误
                 "全冲",
                 string.Empty,
                 string.Empty,
                 "1234asdf9874",
                 "Error 签名无效" //签名无效
            );

            var orderBeforeUpdate = new Order()
            {
                Id = 1,
                CreatedDate = new DateTime(),
                OrderNo = result.P2_Order,
                OrderStatus = OrderStatus.Processing,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                Amount = float.Parse(result.P3_Amt)
            };

            _yeepayServiceMock.Setup<SZXCallbackResult>(m => m.VerifyCallback(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac)).Returns(result);

            string responseStr = PaymentsService.YeepayCardCallBack(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac);

            Assert.Equal(responseStr, "交易签名无效!<BR>YeePay-HMAC:" + result.Hmac + "<BR>LocalHost:" + result.ErrMsg);

        }

        [Fact]
        public void CardCallBackLogic_test_input_SZXCallbackResult()
        {
            yeePaymentsService.Repository = mockRepository;
            yeePaymentsService.YeepayService = _yeepayServiceMock.Object;
            SZXCallbackResult result = new SZXCallbackResult(
                "ChargeCardDirect",
                 "1", //1成功，非1 失败
                 "123456789",
                 "987654321",
                 "100.0", //支付总金额
                 "CNY",
                 "456321987",
                 "100.0", //每张卡支付的金额，以半角逗号隔开
                 "100.0", //每张卡原有的金额
                 "0",     //成功
                 "全冲",
                 string.Empty,
                 string.Empty,
                 "1234asdf9874",
                 string.Empty ////errmsg
            );

            var orderBeforeUpdate = new Order()
            {
                Id = 1,
                CreatedDate = new DateTime(),
                OrderNo = result.P2_Order,
                OrderStatus = OrderStatus.Processing,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                Amount = float.Parse(result.P3_Amt),
                CallBackUrl = "http://sdf.com"
            };

            _yeepayServiceMock.Setup<SZXCallbackResult>(m => m.VerifyCallback(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac)).Returns(result);

            _repository.Setup<Order>(m => m.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(orderBeforeUpdate);
            _repository.Setup(m => m.Update<Order>(It.IsAny<Order>()));
            _repository.Setup(m => m.Add<OrderLine>(It.IsAny<OrderLine>()));

            var allCallBackResultBefore = redis.GetAllItemsFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE);

            string responseStr = yeePaymentsService.YeepayCardCallBack(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac);

            //assert isUpdated
            yeePaymentsService.YeepayCardCallBack(result.R0_Cmd, result.R1_Code, result.P1_MerId, result.P2_Order, result.P3_Amt, result.P4_FrpId, result.P5_CardNo, result.P6_confirmAmount, result.P7_realAmount, result.P8_cardStatus, result.P9_MP, result.Pb_BalanceAmt, result.Pc_BalanceAct, result.Hmac);

            var allCallBackResultAfter = redis.GetAllItemsFromQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE);

            //Assert.Equal(allCallBackResultAfter.Count, allCallBackResultBefore.Count + 1);
            //Assert.Equal(allCallBackResultAfter[allCallBackResultAfter.Count - 1].OrderNo, result.P2_Order);
            //Assert.Equal(allCallBackResultAfter[allCallBackResultAfter.Count - 1].ResultCode, Int32.Parse(result.P8_cardStatus));
        }

        [Fact]
        public void UpdateOrderStatus_test_input_SZXCallbackResult_update_to_Success()
        {
            PaymentsService.Repository = mockRepository;

            SZXCallbackResult result = new SZXCallbackResult(
                "ChargeCardDirect",
                 "1", //1成功，非1 失败
                 "123456789",
                 "987654321",
                 "100.0", //支付总金额
                 "CNY",
                 "456321987",
                 "100.0", //每张卡支付的金额，以半角逗号隔开
                 "100.0", //每张卡原有的金额
                 "0",     //成功
                 "全冲",
                 string.Empty,
                 string.Empty,
                 "1234asdf9874",
                 string.Empty ////errmsg
            );

            var orderBeforeUpdate = new Order()
            {
                Id = 1,
                CreatedDate = new DateTime(),
                OrderNo = result.P2_Order,
                OrderStatus = OrderStatus.Processing,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                Amount = float.Parse(result.P3_Amt),
                CallBackUrl = "http://callback.com"
            };

            _repository.Setup<Order>(m => m.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(orderBeforeUpdate);
            _repository.Setup(m => m.Update<Order>(It.IsAny<Order>()));

            string callBackURL = string.Empty;
            var isUpdated = PaymentsService.UpdateOrderStatus(result, out callBackURL);

            Assert.Equal(isUpdated, false);
            Assert.Equal(orderBeforeUpdate.OrderStatus, OrderStatus.Successed);
        }

        [Fact]
        public void UpdateOrderStatus_test_input_SZXCallbackResult_update_to_Failed()
        {
            PaymentsService.Repository = mockRepository;

            SZXCallbackResult result = new SZXCallbackResult(
                "ChargeCardDirect",
                 "-1", //1成功，非1 失败
                 "123456789",
                 "987654321",
                 "100.0", //支付总金额
                 "CNY",
                 "456321987",
                 "100.0", //每张卡支付的金额，以半角逗号隔开
                 "100.0", //每张卡原有的金额
                 "0",     //成功
                 "全冲",
                 string.Empty,
                 string.Empty,
                 "1234asdf9874",
                 string.Empty ////errmsg
            );

            var orderBeforeUpdate = new Order()
            {
                Id = 1,
                CreatedDate = new DateTime(),
                OrderNo = result.P2_Order,
                OrderStatus = OrderStatus.Processing,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                Amount = float.Parse(result.P3_Amt),
                CallBackUrl = "http://callback.com"
            };

            _repository.Setup<Order>(m => m.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(orderBeforeUpdate);
            _repository.Setup(m => m.Update<Order>(It.IsAny<Order>()));

            string callBackURL = string.Empty;
            var isUpdated = PaymentsService.UpdateOrderStatus(result, out callBackURL);

            Assert.Equal(isUpdated, false);
            Assert.Equal(orderBeforeUpdate.OrderStatus, OrderStatus.Failed);
        }


        [Fact]
        public void UpdateOrderStatus_test_input_SZXCallbackResult_have_Updated()
        {
            PaymentsService.Repository = mockRepository;

            SZXCallbackResult result = new SZXCallbackResult(
                "ChargeCardDirect",
                 "1", //1成功，非1 失败
                 "123456789",
                 "987654321",
                 "100.0", //支付总金额
                 "CNY",
                 "456321987",
                 "100.0", //每张卡支付的金额，以半角逗号隔开
                 "100.0", //每张卡原有的金额
                 "0",     //成功
                 "全冲",
                 string.Empty,
                 string.Empty,
                 "1234asdf9874",
                 string.Empty ////errmsg
            );

            var orderBeforeUpdate = new Order()
            {
                Id = 1,
                CreatedDate = new DateTime(),
                OrderNo = result.P2_Order,
                OrderStatus = OrderStatus.Successed,
                CardPaymentRequestStatus = CardPaymentRequestStatus.Success,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                Amount = float.Parse(result.P3_Amt)
            };

            _repository.Setup<Order>(m => m.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(orderBeforeUpdate);
            _repository.Setup(m => m.Update<Order>(It.IsAny<Order>()));

            string callBackURL = string.Empty;
            var isUpdated = PaymentsService.UpdateOrderStatus(result, out callBackURL);

            Assert.Equal(isUpdated, true);
            Assert.Empty(callBackURL);
        }

        [Fact]
        public void CreateOrder_Test_Normal()
        {
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            string userId = "testuserid";
            string userName = "testUserName";
            var order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, UserId = userId, UserName = userName };

            var test = PaymentsService.CreateOrder(Amount, Desription, productDescription, productName, productType, "", userId, userName, (int)PaymentType.Yeepay);
            Assert.Equal(test.Amount, order.Amount);
            Assert.Equal(test.MerchantExtentionalDescription, order.MerchantExtentionalDescription);
            Assert.Equal(test.ProductDescription, order.ProductDescription.MakeSureUnicodeStringByteLength(30));
            Assert.Equal(test.ProductName, order.ProductName);
            Assert.Equal(test.ProductType, order.ProductType);
            Assert.Equal(test.UserId, order.UserId);
            Assert.Equal(test.UserName, order.UserName);
            Assert.Equal(test.CardPaymentCallBackStatus, CardPaymentCallBackStatus.NotCallBack);
            Assert.Equal(test.CardPaymentRequestStatus, CardPaymentRequestStatus.NotRequest);
            Assert.Equal(test.OrderStatus, OrderStatus.Created);

        }

        [Fact]
        public void CreateOrder_Test_Param_Error()
        {
            float Amount = 0.00f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            string userId = "testuserid";
            string userName = "testUserName";
            Order order = new Order();

            var test = PaymentsService.CreateOrder(Amount, Desription, productDescription, productName, productType, "", userId, userName, (int)PaymentType.Alipay);
            Assert.Equal(test.Amount, order.Amount);
            Assert.Null(test.MerchantExtentionalDescription);
            Assert.Null(test.ProductDescription);
            Assert.Null(test.ProductName);
            Assert.Null(test.ProductType);
            Assert.Equal(test.Currency, "CNY");
            Assert.Equal(test.CardPaymentCallBackStatus, CardPaymentCallBackStatus.NotCallBack);
            Assert.Equal(test.CardPaymentRequestStatus, CardPaymentRequestStatus.NotRequest);
            Assert.Equal(test.OrderStatus, OrderStatus.Initial);
        }

        [Fact]
        public void SaveOrder_Test_Normal()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName };
            long orderId = 159357;

            _repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Returns(159357);
            var test = PaymentsService.SaveOrder(order);

            Assert.Equal(test, orderId);
        }

        [Fact]
        public void SaveOrder_Test_Throw_Error()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName };

            _repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Throws(new Exception());
            Assert.Throws<Exception>(() => PaymentsService.SaveOrder(order));
        }

        [Fact]
        public void LoadCardPayment_Test_Normal()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, Id = 147258369, OrderNo = "TestOrderNo" };

            int cardType = 1;
            string cardNo = "Test";
            string cardPassword = "Test";
            float cardAmount = 0.00f;

            var test = PaymentsService.CreateCardPayment(order, cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription);

            Assert.Equal(test.Amount, Amount);
            Assert.Equal(test.CardAmount, Amount);
            Assert.Equal(test.CardNo, cardNo);
            Assert.Equal(test.CardPassword, cardPassword);
            Assert.Equal(test.OrderID, order.Id);
            Assert.Equal(test.OrderNo, order.OrderNo);

            cardAmount = 1.11f;
            test = PaymentsService.CreateCardPayment(order, cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription);
            Assert.Equal(test.CardAmount, cardAmount);

        }

        [Fact]
        public void LoadCardPayment_Test_Param_Error()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            var order = new Order();
            CardPayment cardPayment = new CardPayment();

            int cardType = 1;
            string cardNo = "Test";
            string cardPassword = "Test";
            float cardAmount = 0.00f;


            var test = PaymentsService.CreateCardPayment(order, cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription);
            Assert.Equal(test.OrderID, 0);
            Assert.Null(test.OrderNo);


            cardNo = "";
            test = PaymentsService.CreateCardPayment(order, cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription);
            Assert.Equal(test.OrderNo, cardPayment.OrderNo);
            Assert.Null(test.OrderNo);
        }

        [Fact]
        public void SaveCardPayment_Test_Normal()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, Id = 147258369 };

            int cardType = 1;
            string cardNo = "Test";
            string cardPassword = "Test";
            float cardAmount = 0.00f;


            var cardPayment = PaymentsService.CreateCardPayment(order, cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription);

            _redisService.Setup(s => s.AddItemToQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_PROCESSING_QUEUE, cardPayment));
            PaymentsService.SaveCardPayment(cardPayment);
            _redisService.Verify(s => s.AddItemToQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_PROCESSING_QUEUE, cardPayment));

        }

        //[RedisFact]
        public void SaveCardPayment_RedisTest_Normal()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, Id = 147258369 };

            int cardType = 1;
            string cardNo = "Test";
            string cardPassword = "Test";
            float cardAmount = 0.00f;


            var cardPayment = PaymentsService.CreateCardPayment(order, cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription);

            yeePaymentsService.SaveCardPayment(cardPayment);
            var test = redisService.RetrieveItemFromQueue<CardPayment>(BillingConsts.KEY_CARD_PAYMENT_REQUEST_PROCESSING_QUEUE);
            Assert.NotNull(test);
        }

        [Fact]
        public void RequestYeepay_Test_Normal()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            int cardType = 1;
            string cardNo = "Test";
            string cardPassword = "Test";
            float cardAmount = 0.00f;
            string userId = "TestUserId";
            string userName = "TestUserName";

            //Mobile.Infrastructure.Models.Order order = new Mobile.Infrastructure.Models.Order();
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "Test", CallBackUrl = "http://" };

            _repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Returns(159357);
            var test = PaymentsService.RequestCardPayment(cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription, "", userId, userName);

            Assert.NotNull(test.OrderNo);
        }

        [Fact]
        public void RequestYeepay_Test_Order_Is_Error()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 0.00f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            int cardType = 1;
            string cardNo = "Test";
            string cardPassword = "Test";
            float cardAmount = 0.00f;
            Order order = new Order();
            //Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName };
            string userId = "TestUserId";
            string userName = "TestUserName";

            _repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Returns(159357);
            var test = PaymentsService.RequestCardPayment(cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription, "", userId, userName);

            Assert.Null(test.OrderNo);
        }

        [Fact]
        public void RequestYeepay_Test_CardPayment_Is_Error()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.00f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            int cardType = 1;
            string cardNo = "";
            string cardPassword = "";
            float cardAmount = 0.00f;
            string userId = "TestUserId";
            string userName = "TestUserName";
            //Order order = new Order();
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "Test" };

            _repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Returns(159357);
            var test = PaymentsService.RequestCardPayment(cardType, cardNo, cardPassword, cardAmount, Amount, productName, productType, productDescription, Desription, "", userId, userName);

            Assert.Null(test.OrderNo);
        }

        public void SaveOrder_Test_No_Mock_Normal()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesription";
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            //long orderId = 159357;
            Order order = new Order
            {
                Amount = Amount,
                ProductDescription = productDescription,
                MerchantExtentionalDescription = Desription,
                ProductType = productType,
                ProductName = productName,
                OrderNo = "TestOrderNo",
                CreatedDate = DateTime.Now,
                PayedAmount = 0,
                CardPaymentCallBackStatus = CardPaymentCallBackStatus.NotCallBack,
                CardPaymentRequestStatus = CardPaymentRequestStatus.NotRequest,
                Currency = "CNY",
                OrderStatus = OrderStatus.Failed,
                CallBackUrl = "http://test.test.test"
            };


            //_repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Returns(orderId);
            var test = yeePaymentsService.SaveOrder(order);
        }

        public void SaveOrder_Test_No_Mock_Critical()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesriptionTestDesriptionbb";//30
            string productDescription = "TestproductDescription";
            string productName = "TestproductName";
            string productType = "TestproductType";
            //long orderId = 159357;
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "TestOrderNo", CreatedDate = DateTime.Now, PayedAmount = 0 };

            //_repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Returns(orderId);
            var test = yeePaymentsService.SaveOrder(order);
            Assert.NotEqual(test, 0);

            productDescription = "蔡胖子的测试蔡胖子的测试蔡胖子";//30 china
            order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "TestOrderNo", CreatedDate = DateTime.Now, PayedAmount = 0 };
            test = yeePaymentsService.SaveOrder(order);
            Assert.NotEqual(test, 0);

            productDescription = "蔡胖子的测试abcdef蔡胖子的测试";//30 china
            order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "TestOrderNo", CreatedDate = DateTime.Now, PayedAmount = 0 };
            test = yeePaymentsService.SaveOrder(order);
            Assert.NotEqual(test, 0);

            productDescription = "，，，，，，，，，，，，，，，";//30 china
            order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "TestOrderNo", CreatedDate = DateTime.Now, PayedAmount = 0 };
            test = yeePaymentsService.SaveOrder(order);
            Assert.NotEqual(test, 0);
        }

        public void SaveOrder_Test_No_Mock_Critical_Overflow()
        {
            PaymentsService.Repository = mockRepository;
            float Amount = 9.99f;
            string Desription = "TestDesriptionTestDesriptionccc";//31
            string productDescription = "TestproductDescriptionTestproductDescription";//42
            string productName = "TestproductName";
            string productType = "TestproductType";
            //long orderId = 159357;
            Order order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "TestOrderNo", CreatedDate = DateTime.Now, PayedAmount = 0 };

            //_repository.Setup(s => s.NewAdd<Order>(It.IsAny<Order>())).Returns(orderId);
            var test = yeePaymentsService.SaveOrder(order);


            productDescription = "蔡胖子的测试蔡胖子的测试蔡胖子的测试蔡胖子的测试蔡胖子的测试";//30 china
            order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "TestOrderNo", CreatedDate = DateTime.Now, PayedAmount = 0 };
            var testSecond = yeePaymentsService.SaveOrder(order);


            productDescription = "蔡胖子的测试蔡胖子的测试蔡胖子的测试蔡胖子的测试蔡胖子的测试多那么一点点";//36 china
            order = new Order { Amount = Amount, ProductDescription = productDescription, MerchantExtentionalDescription = Desription, ProductType = productType, ProductName = productName, OrderNo = "TestOrderNo", CreatedDate = DateTime.Now, PayedAmount = 0 };
            testSecond = yeePaymentsService.SaveOrder(order);

        }

        //[Fact]
        //public void AlipaySDKCallBack_should_add_order_and_orderLine_and_Q2()
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml("<notify><partner>2088701092044335</partner><discount>0.00</discount><payment_type>1</payment_type><subject>商品名</subject><trade_no>2012081708875322</trade_no><buyer_email>iike22@gmail.com</buyer_email><gmt_create>2012-08-17 16:43:50</gmt_create><quantity>1</quantity><out_trade_no>0817164340-2383</out_trade_no><seller_id>2088701092044335</seller_id><trade_status>TRADE_FINISHED</trade_status><is_total_fee_adjust>N</is_total_fee_adjust><total_fee>0.01</total_fee><gmt_payment>2012-08-17 16:43:51</gmt_payment><seller_email>tianfei@youleonline.com</seller_email><gmt_close>2012-08-17 16:43:50</gmt_close><price>0.01</price><buyer_id>2088102984178220</buyer_id><use_coupon>N</use_coupon></notify>");
        //    string callBackUrl = "http://test/callback/url";

        //    _repository.Setup(s => s.Exists<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(false);

        //    PaymentsService.Repository = _repository.Object;
        //    PaymentsService.AlipaySDKCallback(xmlDoc, callBackUrl);


        //    _repository.Verify(s => s.Add<Order>(It.IsAny<Order>()));
        //    _repository.Verify(s => s.Add<OrderLine>(It.IsAny<OrderLine>()));
        //    _redisService.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, It.IsAny<PaymentNotification>()));
        //}

        [Fact]
        public void alipay_wap_pay_init_should_return_redirect_url()
        {
            PaymentsService.Repository = _repository.Object;
            string url = PaymentsService.InitAlipayWapPay("pName", "0.01", "13", "http://callback");

            _repository.Verify(s => s.Add<Order>(It.IsAny<Order>()));

            Assert.NotEmpty(url);
        }

        [Fact]
        public void Verify_Alipay_Wap_Notify_with_correct_sign()
        {
            string notifyData = PaymentsService.VerifyAlipayWapNotify("nThYjFljFc6AM9k4MbWK1vnAKUbjI5qBU9Q56MbYqSNcul7zp5LX4uyZqx74UCgsgl6Uxnk62CIyYpGhsybIKrYDeOk+hPk+4HY+uUhfCxDowMS9wJaXOin9+5FFL20VBuNgDZzd1hDuCe9RKVJ88rCuk0sReyXmx5rb2vcbMWqu7tvLgqOwpL5rTmAkUiYeFmpefWZ35r/1qLDsw7ndJWozuGy/r0lxbCm/1QI3iKoTI/XylhDQ56QyWsAzXyPZO3QN0jJX9/JvHiJ9AC+9Vkk2iqjl8FlXfzhrbqjpX82DGPq6TGehBxi3U6M4D+FB5EgxZh2T9nbF76EqH5rq5iNi7EQLqWGwx+97DyvQegr+w7DZytCJOfrw3ekB4dDlasmH3yrv2D5+h9viJos2386mNoQdM4NplVazHd59DUcKbs3ssKoQTCHmouXNWBBuf/bIV7RuHpvBb3t/CeUda9+3j72ppyR248CMwJgs0VtQq85cSI+6AkUR207ypMzlm17i/kiCNp2lopfTQWtH7JjDA2LWFhYDDcyDOEKe02DK6rI1CN/kf8Y6TecEq5fLl9tRzKpJVfp2g4jzUddnYu28KdWZPCHazM+o3z2vSPEXNUNKoQ3XnBnLbbT0aIdRAzOCOvMgRcchCFX/PbYPePKtEdvXJ0tb3MJzesxlNLM2u+pRMnXjsWc7jAyduo3guxYpbjhZvd7azVGJR8MMeW7NvdAdplux2NGM3PTVV+n8UdDg+h9p1lPGhCYqdFt86jaYdoFh5+MxFz1RzjyD21H+sMAAniUIVl45IBRN7z3pQgrSlbRG5/jaNT81p20shXQuLjoVcb+OlfDSvlfeLVISJTIqVGS2Nfm1ZjS2+kNNfPxfK03Z08Oi49Tuw9wLIq79En4/DJgBGT3m5hu3rX3GWLfdgMW1Ef2PvNArumHGaTeUDVc8b1A2dRnaBJzUV0nBvgGwfocNDidUzwS2U+TdhWeb3PvUHFk8ti3wvMRBLZvTm4a4eRGA+bXWl0J7Wmp4Um66nGbmZpXB6YDZzuJK+AzsdQSyMxdKSf3GLsw5CbfWQiaNQyCnWujH+Sk2IcgU7WRkAl0Ut0V+WF3C7ssCpfF7Qpyse8RREyBn3X/54KNs/HuwEugX7N4x69BA8RHhA2UU5f7wwmTVX98b2BYZSumnfzRS7Lkvwn0q7OxdOwvb9uQG5Ad8s9dKqtWtE1b2cDSYhtYvzxHqpk1jATFWtBs+m2wxgqouW6XXrutFOI20DpYZGC4Io4jtCYxoNxOphTZAav0Vc37K4ddL7l/0KpXq4hkqNdVh8D6M84sUK9Q/n+Vk0PhaRu4rCRQB+PJdfFFi8NTLT8zopB69HA==", "NnxNAIxbTc1SKKPGqNdwAh9YtO35h/c9u7ddjODRWNztOYhRkcjlyPMfAQX+30ZVbb1/nu0uWIJ++qyzgXB8lcitvsrtzZCOxk+Y582zQIX25dbGR6kmGQ57M6OtvqgW7uws1cGspu3QDgYiLkBofjBvM6HqjymdSW0MkboS6qA=", "alipay.wap.trade.create.direct", "1.0", "0001");

            Assert.NotEmpty(notifyData);

            Console.WriteLine(notifyData);
        }

        [Fact]
        public void Verify_Alipay_Wap_Notify_with_wrong_sign()
        {
            string notifyData = PaymentsService.VerifyAlipayWapNotify("nThYjFljFc6AM9k4MbWK1vnAKUbjI5qBU9Q56MbYqSNcul7zp5LX4uyZqx74UCgsgl6Uxnk62CIyYpGhsybIKrYDeOk+hPk+4HY+uUhfCxDowMS9wJaXOin9+5FFL20VBuNgDZzd1hDuCe9RKVJ88rCuk0sReyXmx5rb2vcbMWqu7tvLgqOwpL5rTmAkUiYeFmpefWZ35r/1qLDsw7ndJWozuGy/r0lxbCm/1QI3iKoTI/XylhDQ56QyWsAzXyPZO3QN0jJX9/JvHiJ9AC+9Vkk2iqjl8FlXfzhrbqjpX82DGPq6TGehBxi3U6M4D+FB5EgxZh2T9nbF76EqH5rq5iNi7EQLqWGwx+97DyvQegr+w7DZytCJOfrw3ekB4dDlasmH3yrv2D5+h9viJos2386mNoQdM4NplVazHd59DUcKbs3ssKoQTCHmouXNWBBuf/bIV7RuHpvBb3t/CeUda9+3j72ppyR248CMwJgs0VtQq85cSI+6AkUR207ypMzlm17i/kiCNp2lopfTQWtH7JjDA2LWFhYDDcyDOEKe02DK6rI1CN/kf8Y6TecEq5fLl9tRzKpJVfp2g4jzUddnYu28KdWZPCHazM+o3z2vSPEXNUNKoQ3XnBnLbbT0aIdRAzOCOvMgRcchCFX/PbYPePKtEdvXJ0tb3MJzesxlNLM2u+pRMnXjsWc7jAyduo3guxYpbjhZvd7azVGJR8MMeW7NvdAdplux2NGM3PTVV+n8UdDg+h9p1lPGhCYqdFt86jaYdoFh5+MxFz1RzjyD21H+sMAAniUIVl45IBRN7z3pQgrSlbRG5/jaNT81p20shXQuLjoVcb+OlfDSvlfeLVISJTIqVGS2Nfm1ZjS2+kNNfPxfK03Z08Oi49Tuw9wLIq79En4/DJgBGT3m5hu3rX3GWLfdgMW1Ef2PvNArumHGaTeUDVc8b1A2dRnaBJzUV0nBvgGwfocNDidUzwS2U+TdhWeb3PvUHFk8ti3wvMRBLZvTm4a4eRGA+bXWl0J7Wmp4Um66nGbmZpXB6YDZzuJK+AzsdQSyMxdKSf3GLsw5CbfWQiaNQyCnWujH+Sk2IcgU7WRkAl0Ut0V+WF3C7ssCpfF7Qpyse8RREyBn3X/54KNs/HuwEugX7N4x69BA8RHhA2UU5f7wwmTVX98b2BYZSumnfzRS7Lkvwn0q7OxdOwvb9uQG5Ad8s9dKqtWtE1b2cDSYhtYvzxHqpk1jATFWtBs+m2wxgqouW6XXrutFOI20DpYZGC4Io4jtCYxoNxOphTZAav0Vc37K4ddL7l/0KpXq4hkqNdVh8D6M84sUK9Q/n+Vk0PhaRu4rCRQB+PJdfFFi8NTLT8zopB69HA==", "NnxNAIxbTc1SKKPGqNdwAh9YtO35h/c9u7ddjODRWNztOYhRkcjlyPMfAQX+30ZVbb1/nu0uWIJ++qyzgXB8lcitvsrtzZCOxk+Y582zQIX25dbGR6kmGQ57M6OtvqgW7uws1cGspu3QDgYiLkBofjBvM6HqjymdSW0MkboS6qA=", "alipay.wap.trade.create.direct", "1.1", "0001");
            Assert.Empty(notifyData);
        }

        [Fact]
        public void AlipayWapCallback_with_orderno_should_update_order()
        {
            PaymentsService.Repository = _repository.Object;
            SortedDictionary<string, string> sArrary = new SortedDictionary<string, string>();
            sArrary.Add("out_trade_no", "201208301041260043349");
            sArrary.Add("request_token", "requestToken");
            sArrary.Add("result", "success");
            sArrary.Add("sign", "ALxK9zUkJSivLLrkXGcZP1aWuSXKmo9JfENDNrrHur5mgFPp/OczmCgRXJE2pj8iWwOIcgte2/gWhW9aJZT6pr6z0ICk3HOs2eV/HdPtO1T9mM93GGf2Ggl+mkF1QyOGTcIrFTrp1O1chbmfn4kfVfP0JKlXOc5Hrd46sULdzqM=");
            sArrary.Add("sign_type", "0001");
            sArrary.Add("trade_no", "2012083042678022");
            var order = new Order { OrderStatus = OrderStatus.Created };
            _repository.Setup(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);

            PaymentsService.AlipayWapCallback("success", "ALxK9zUkJSivLLrkXGcZP1aWuSXKmo9JfENDNrrHur5mgFPp/OczmCgRXJE2pj8iWwOIcgte2/gWhW9aJZT6pr6z0ICk3HOs2eV/HdPtO1T9mM93GGf2Ggl+mkF1QyOGTcIrFTrp1O1chbmfn4kfVfP0JKlXOc5Hrd46sULdzqM=", sArrary);
            _repository.Verify(s => s.Update<Order>(It.IsAny<Order>()));
        }

        [Fact]
        public void AlipayWapCallback_with_empty_orderno_should_not_update_order()
        {
            PaymentsService.Repository = _repository.Object;
            SortedDictionary<string, string> sArrary = new SortedDictionary<string, string>();
            sArrary.Add("out_trade_no", "");
            sArrary.Add("request_token", "requestToken");
            sArrary.Add("result", "success");
            sArrary.Add("sign", "ALxK9zUkJSivLLrkXGcZP1aWuSXKmo9JfENDNrrHur5mgFPp/OczmCgRXJE2pj8iWwOIcgte2/gWhW9aJZT6pr6z0ICk3HOs2eV/HdPtO1T9mM93GGf2Ggl+mkF1QyOGTcIrFTrp1O1chbmfn4kfVfP0JKlXOc5Hrd46sULdzqM=");
            sArrary.Add("sign_type", "0001");
            sArrary.Add("trade_no", "2012083042678022");
            _repository.Setup(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(new Order());

            PaymentsService.AlipayWapCallback("success", "ALxK9zUkJSivLLrkXGcZP1aWuSXKmo9JfENDNrrHur5mgFPp/OczmCgRXJE2pj8iWwOIcgte2/gWhW9aJZT6pr6z0ICk3HOs2eV/HdPtO1T9mM93GGf2Ggl+mkF1QyOGTcIrFTrp1O1chbmfn4kfVfP0JKlXOc5Hrd46sULdzqM=", sArrary);
        }

        [Fact]
        public void sign_check_test()
        {
            SortedDictionary<string, string> sArrary = new SortedDictionary<string, string>();
            sArrary.Add("out_trade_no", "201208301041260043349");
            sArrary.Add("request_token", "requestToken");
            sArrary.Add("result", "success");
            sArrary.Add("sign", "ALxK9zUkJSivLLrkXGcZP1aWuSXKmo9JfENDNrrHur5mgFPp/OczmCgRXJE2pj8iWwOIcgte2/gWhW9aJZT6pr6z0ICk3HOs2eV/HdPtO1T9mM93GGf2Ggl+mkF1QyOGTcIrFTrp1O1chbmfn4kfVfP0JKlXOc5Hrd46sULdzqM=");
            sArrary.Add("sign_type", "0001");
            sArrary.Add("trade_no", "2012083042678022");
            bool isVerify = Function.Verify(sArrary, sArrary["sign"], Config.Alipaypublick, Config.Input_charset_UTF8);

            Assert.True(isVerify);
        }

        //[Fact]
        //public void AlipayWapNotify_should_update_order_and_add_orderline_and_add_Q2()
        //{
        //    var order = new Order { OrderStatus = OrderStatus.Created };
        //    _repository.Setup(s => s.Single<Order>(It.IsAny<Expression<Func<Order, bool>>>())).Returns(order);
        //    PaymentsService.Repository = _repository.Object;
        //    var notify_data = PaymentsService.VerifyAlipayWapNotify("nThYjFljFc6AM9k4MbWK1vnAKUbjI5qBU9Q56MbYqSNcul7zp5LX4uyZqx74UCgsgl6Uxnk62CIyYpGhsybIKrYDeOk+hPk+4HY+uUhfCxDowMS9wJaXOin9+5FFL20VBuNgDZzd1hDuCe9RKVJ88rCuk0sReyXmx5rb2vcbMWqu7tvLgqOwpL5rTmAkUiYeFmpefWZ35r/1qLDsw7ndJWozuGy/r0lxbCm/1QI3iKoTI/XylhDQ56QyWsAzXyPZO3QN0jJX9/JvHiJ9AC+9Vkk2iqjl8FlXfzhrbqjpX82DGPq6TGehBxi3U6M4D+FB5EgxZh2T9nbF76EqH5rq5iNi7EQLqWGwx+97DyvQegr+w7DZytCJOfrw3ekB4dDlasmH3yrv2D5+h9viJos2386mNoQdM4NplVazHd59DUcKbs3ssKoQTCHmouXNWBBuf/bIV7RuHpvBb3t/CeUda9+3j72ppyR248CMwJgs0VtQq85cSI+6AkUR207ypMzlm17i/kiCNp2lopfTQWtH7JjDA2LWFhYDDcyDOEKe02DK6rI1CN/kf8Y6TecEq5fLl9tRzKpJVfp2g4jzUddnYu28KdWZPCHazM+o3z2vSPEXNUNKoQ3XnBnLbbT0aIdRAzOCOvMgRcchCFX/PbYPePKtEdvXJ0tb3MJzesxlNLM2u+pRMnXjsWc7jAyduo3guxYpbjhZvd7azVGJR8MMeW7NvdAdplux2NGM3PTVV+n8UdDg+h9p1lPGhCYqdFt86jaYdoFh5+MxFz1RzjyD21H+sMAAniUIVl45IBRN7z3pQgrSlbRG5/jaNT81p20shXQuLjoVcb+OlfDSvlfeLVISJTIqVGS2Nfm1ZjS2+kNNfPxfK03Z08Oi49Tuw9wLIq79En4/DJgBGT3m5hu3rX3GWLfdgMW1Ef2PvNArumHGaTeUDVc8b1A2dRnaBJzUV0nBvgGwfocNDidUzwS2U+TdhWeb3PvUHFk8ti3wvMRBLZvTm4a4eRGA+bXWl0J7Wmp4Um66nGbmZpXB6YDZzuJK+AzsdQSyMxdKSf3GLsw5CbfWQiaNQyCnWujH+Sk2IcgU7WRkAl0Ut0V+WF3C7ssCpfF7Qpyse8RREyBn3X/54KNs/HuwEugX7N4x69BA8RHhA2UU5f7wwmTVX98b2BYZSumnfzRS7Lkvwn0q7OxdOwvb9uQG5Ad8s9dKqtWtE1b2cDSYhtYvzxHqpk1jATFWtBs+m2wxgqouW6XXrutFOI20DpYZGC4Io4jtCYxoNxOphTZAav0Vc37K4ddL7l/0KpXq4hkqNdVh8D6M84sUK9Q/n+Vk0PhaRu4rCRQB+PJdfFFi8NTLT8zopB69HA==", "NnxNAIxbTc1SKKPGqNdwAh9YtO35h/c9u7ddjODRWNztOYhRkcjlyPMfAQX+30ZVbb1/nu0uWIJ++qyzgXB8lcitvsrtzZCOxk+Y582zQIX25dbGR6kmGQ57M6OtvqgW7uws1cGspu3QDgYiLkBofjBvM6HqjymdSW0MkboS6qA=", "alipay.wap.trade.create.direct", "1.0", "0001");
        //    PaymentsService.AlipayWapNotify(notify_data);


        //    _repository.Verify(s => s.Update<Order>(It.IsAny<Order>()));
        //    _repository.Verify(s => s.Add<OrderLine>(It.IsAny<OrderLine>()));
        //    _redisService.Verify(s => s.AddItemToQueue<PaymentNotification>(BillingConsts.KEY_CARD_PAYMENT_CALLBACK_PROCESSING_QUEUE, It.IsAny<PaymentNotification>()));

        //}
    }
}
