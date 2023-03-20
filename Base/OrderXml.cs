using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Vchasno.Base
{
    [XmlRoot("Document-Order")]
    public class OrderXml
    {
        [XmlElement("Order-Header")]
        public OrderHeader OrderHeader { get; set; }

        [XmlElement("Order-Parties")]
        public OrderParties OrderParties { get; set; }

        [XmlElement("Order-Lines")]
        public OrderLines OrderLines { get; set; }

        [XmlElement("Order-Summary")]
        public OrderSummary OrderSummary { get; set; }

        public string ToXml()
        {
            return this.ToXml<OrderXml>();
        }
        public override string ToString()
        {
            return OrderHeader.OrderNumber;
        }
    }

    public class OrderHeader
    {
        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string ExpectedDeliveryDate { get; set; }
        public string ExpectedDeliveryTime { get; set; }
        public string Currency { get; set; }
        public string DocumentFunctionCode { get; set; }
        public string ContractNumber { get; set; }
        public string Remarks { get; set; }
    }

    public class OrderParties
    {
        [XmlElement("Buyer")]
        public Buyer Buyer { get; set; }

        [XmlElement("Seller")]
        public Seller Seller { get; set; }

        [XmlElement("DeliveryPoint")]
        public DeliveryPoint DeliveryPoint { get; set; }

        [XmlElement("Invoicee")]
        public Invoicee Invoicee { get; set; }
    }

    public class Buyer
    {
        public string ILN { get; set; }
        public string Department { get; set; }
    }

    public class Seller
    {
        public string ILN { get; set; }
    }

    public class DeliveryPoint
    {
        public string ILN { get; set; }
    }

    public class Invoicee
    {
        public string ILN { get; set; }
    }

    public class OrderLines
    {
        [XmlElement("Line")]
        public List<Line> Line { get; set; }
    }

    public class Line
    {
        [XmlElement("Line-Item")]
        public LineItem LineItem { get; set; }
    }

    public class LineItem
    {
        public int LineNumber { get; set; }
        public string EAN { get; set; }
        public string BuyerItemCode { get; set; }
        public string SupplierItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal OrderedQuantity { get; set; }
        public int OrderedUnitPacksize { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal OrderedUnitNetPrice { get; set; }
        public decimal OrderedUnitGrossPrice { get; set; }
        public int TaxRate { get; set; }
        public string TaxCategoryCode { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
    }

    public class OrderSummary
    {
        public int TotelLines { get; set; }
        public int TotalOrderedAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalGrossAmount { get; set; }
    }
}
