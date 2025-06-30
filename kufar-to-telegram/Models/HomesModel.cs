using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kufar_to_telegram.Models
{
    internal class HomesModel
    {
        public class Rootobject
        {
            public int account_id { get; set; }
            public Account_Parameters[] account_parameters { get; set; }
            public int ad_id { get; set; }
            public string ad_link { get; set; }
            public Ad_Parameters[] ad_parameters { get; set; }
            public object body { get; set; }
            public string body_short { get; set; }
            public string category { get; set; }
            public bool company_ad { get; set; }
            public string currency { get; set; }
            public Image[] images { get; set; }
            public int list_id { get; set; }
            public DateTime list_time { get; set; }
            public string message_id { get; set; }
            public Paid_Services paid_services { get; set; }
            public bool phone_hidden { get; set; }
            public string price_byn { get; set; }
            public string price_usd { get; set; }
            public string remuneration_type { get; set; }
            public Show_Parameters show_parameters { get; set; }
            public string subject { get; set; }
            public string type { get; set; }
        }

        public class Paid_Services
        {
            public bool halva { get; set; }
            public bool highlight { get; set; }
            public bool polepos { get; set; }
            public object ribbons { get; set; }
        }

        public class Show_Parameters
        {
            public bool show_call { get; set; }
            public bool show_chat { get; set; }
            public bool show_import_link { get; set; }
            public bool show_web_shop_link { get; set; }
        }

        public class Account_Parameters
        {
            public string pl { get; set; }
            public string vl { get; set; }
            public string p { get; set; }
            public string v { get; set; }
            public string pu { get; set; }
            public G[] g { get; set; }
        }

        public class G
        {
            public int gi { get; set; }
            public string gl { get; set; }
            public int go { get; set; }
            public int po { get; set; }
        }

        public class Ad_Parameters
        {
            public string pl { get; set; }
            public object vl { get; set; }
            public string p { get; set; }
            public object v { get; set; }
            public string pu { get; set; }
            public G1[] g { get; set; }
        }

        public class G1
        {
            public int gi { get; set; }
            public string gl { get; set; }
            public int go { get; set; }
            public int po { get; set; }
        }

        public class Image
        {
            public string id { get; set; }
            public string media_storage { get; set; }
            public string path { get; set; }
            public bool yams_storage { get; set; }
        }

    }
}
