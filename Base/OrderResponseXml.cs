using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vchasno.Base
{
    [XmlRoot("Document-OrderResponse")]
    public class OrderResponseXml
    {
        [XmlElement("OrderResponse-Header")]
        public OrderResponseHeader OrderResponseHeader { get; set; }

        [XmlElement("OrderResponse-Parties")]
        public OrderParties OrderResponseParties { get; set; }

        [XmlElement("OrderResponse-Lines")]
        public OrderResponseLines OrderResponseLines { get; set; }

        [XmlElement("OrderResponse-Summary")]
        public OrderSummary OrderResponseSummary { get; set; }

        public string ToXml()
        {
            return this.ToXml<OrderResponseXml>();
        }
        public override string ToString()
        {
            return OrderResponseHeader.OrderResponseNumber;
        }
    }

    public class OrderResponseHeader
    {
        public string OrderResponseNumber { get; set; }
        public string OrderResponseDate { get; set; }
        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string ExpectedDeliveryDate { get; set; }
        public string ExpectedDeliveryTime { get; set; }
        public string Currency { get; set; }
        public int ResponseType { get; set; }
    }

    public class OrderResponseLines
    {
        [XmlElement("Line")]
        public List<LineResponse> Line { get; set; }
    }

    public class LineResponse
    {
        [XmlElement("Line-Item")]
        public LineItemResponse LineItem { get; set; }
    }

    public class LineItemResponse
    {
        public int LineNumber { get; set; }
        public string EAN { get; set; }
        public string BuyerItemCode { get; set; }
        public string SupplierItemCode { get; set; }
        public string ItemDescription { get; set; }
        public int QuantityOrdered { get; set; }
        public int QuantityToBeDelivered { get; set; }
        public int OrderedUnitPacksize { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal OrderedUnitNetPrice { get; set; }
        public decimal OrderedUnitGrossPrice { get; set; }
        public int TaxRate { get; set; }
        public string TaxCategoryCode { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
    }
}