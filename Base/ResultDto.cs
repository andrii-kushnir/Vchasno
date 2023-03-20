using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vchasno.Base
{
    public class ResultDto
    {
        public string deal_id { get; set; }
        public string document_id { get; set; }
        public object[] documents_ids { get; set; }
    }
}
