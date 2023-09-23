using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine.Helpers.paytm.refund
{
    class paytm_create_refund_response
    {
        public Head head { get; set; }
        public Body body { get; set; }
    }

    public class Head
    {
        public string responseTimestamp { get; set; }
        public string signature { get; set; }
        public string version { get; set; }
    }

    public class ResultInfo
    {
        public string resultStatus { get; set; }
        public string resultCode { get; set; }
        public string resultMsg { get; set; }
    }

    public class Body
    {
        public string txnTimestamp { get; set; }
        public string orderId { get; set; }
        public string mid { get; set; }
        public string refId { get; set; }
        public ResultInfo resultInfo { get; set; }
        public string refundId { get; set; }
        public string txnId { get; set; }
        public string refundAmount { get; set; }
    }

}
