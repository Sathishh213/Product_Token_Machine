using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenMachine.Helpers.paytm.transaction_status
{
    class paytm_transaction_status_response
    {
        public Head head { get; set; }
        public Body body { get; set; }
    }

    public class Head
    {
        public string responseTimestamp { get; set; }
        public string version { get; set; }
        public string clientId { get; set; }
        public string signature { get; set; }
    }

    public class ResultInfo
    {
        public string resultStatus { get; set; }
        public string resultCode { get; set; }
        public string resultMsg { get; set; }
    }

    public class Body
    {
        public ResultInfo resultInfo { get; set; }
        public string txnId { get; set; }
        public string bankTxnId { get; set; }
        public string orderId { get; set; }
        public string txnAmount { get; set; }
        public string txnType { get; set; }
        public string gatewayName { get; set; }
        public string bankName { get; set; }
        public string mid { get; set; }
        public string paymentMode { get; set; }
        public string refundAmt { get; set; }
        public string txnDate { get; set; }
    }

}
