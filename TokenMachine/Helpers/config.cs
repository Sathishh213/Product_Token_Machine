using System;
using System.Collections.Generic;

namespace TokenMachine.Helpers
{
    public static class config
    {
        public static int machine_id = 0;
        public static string helpline = "";

        public static List<string> videos = new List<string>();
        public static int paly_index = 0;

        public static bool IsCoupon = false;


        public static int cus_id = 0;
        public static string cus_name = "";
        public static string cus_code = "";
        public static int cus_bal = 0;
        public static int cus_limit = 0;
        public static string cus_mobile_no = "";
        public static string cus_email = "";
        public static string idcardnumber = "";
        public static DateTime cus_dob = new DateTime();

        public static List<order_item> ordered = new List<order_item>();
        public static List<Products> lstProducts = new List<Products>();

        public static decimal tot_amt = 0;
        public static int tot_item = 0;
        public static decimal in_amt = 0;
        public static string inmode = "";
        public static string sales_code = "";

        public static int emp_id = 0;
        public static string emp_name = "";
        public static int isadmin = 0;
        public static int login_session_id = 0;

        public static string thanks_msg = "";

       // public static string localBillValidator = "server=127.0.0.1;user id=betaCreative;password=MsiSCB#02;database=billvalidator";

        public static string paytm_upi_order_id = "";
        public static string paytm_upi_txnId = "";
    }

}
