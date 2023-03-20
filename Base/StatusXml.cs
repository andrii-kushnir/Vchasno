using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Vchasno.Base
{
    [XmlRoot("ECOD-Acknowledgement-Report")]
    public class StatusXml
    {
        [XmlElement("Report-Number")]
        public string ReportNumber { get; set; }

        [XmlElement("Report-SenderILN")]
        public string ReportSenderILN { get; set; }

        [XmlElement("Report-ReceiverILN")]
        public string ReportReceiverILN { get; set; }

        [XmlElement("Report-Date")]
        public string ReportDate { get; set; }

        [XmlElement("Report-Item")]
        public ReportItem ReportItem { get; set; }

        public string ToXml()
        {
            return this.ToXml<StatusXml>();
        }
        public override string ToString()
        {
            return ReportNumber;
        }
    }

    public class ReportItem
    {
        public int ItemNumber { get; set; }
        public string ItemDate { get; set; }
        public string DocumentSenderILN { get; set; }
        public string DocumentReceiverILN { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }

        [XmlElement("Report-Item")]
        public ItemStatus ItemStatus { get; set; }
    }

    public class ItemStatus
    {
        public string Type { get; set; }
        public string Stage { get; set; }
        public string State { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DocumentNumber { get; set; }
        public string FileName { get; set; }
    }
}
