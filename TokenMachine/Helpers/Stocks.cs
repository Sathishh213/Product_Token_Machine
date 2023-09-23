using System.Windows.Media;

namespace TokenMachine.Helpers
{
    class Stocks
    {
        public string product_no { get; set; }
        public string product_id { get; set; }
        public string product_name { get; set; }
        public int motor_no { get; set; }
        public int stock { get; set; }
        public int capacity { get; set; }
        public int Exp { get; set; }
        public int Wanted { get; set; }
        public int valid_hours { get; set; }
        public string exp_color { get; set; }
        public string txt_visibility { get; set; }
        public string btn_visibility { get; set; }
        public string btnSoldout_visibility { get; set; }
        public string img_path { get; set; }
        public Brush background { get; set; }
    }
}
