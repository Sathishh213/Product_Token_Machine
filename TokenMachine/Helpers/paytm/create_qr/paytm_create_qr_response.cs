using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine.Helpers.paytm.create_qr
{
    public class paytm_create_qr_response
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
        public string qrCodeId { get; set; }
        public string qrData { get; set; }
        public string image { get; set; }
    }
}
