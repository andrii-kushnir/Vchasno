using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vchasno.Base
{
    public class DealDto
    {
        public string id { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public int state { get; set; }
        public string company_from { get; set; }
        public string company_to { get; set; }
        public bool is_partial { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }
        public string edrpou { get; set; }
        public string flow_name { get; set; }
        public DocumentsDto[] documents { get; set; }
        public object[] supplier_upload_errors { get; set; }
    }
}
