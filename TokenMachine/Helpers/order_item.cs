namespace VendingMachine.Helpers
{
    public class order_item
    {
        public int Sno { get; set; }
        public int product_id { get; set; }
        public int product_no { get; set; }
        public int cabin_id { get; set; }
        public int motor_no { get; set; }
        public string product_name { get; set; }
        public decimal price { get; set; }
        public int qty { get; set; }
        public decimal amt { get; set; }
        public int discount { get; set; }
        public string img_path { get; set; }
        public int vend { get; set; }
    }

    public class product_lineItem
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
        
    }
}
