using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Vchasno.Base
{
    [XmlRoot("Document-DespatchAdvice")]
    public class DespatchAdviceXml
    {
        [XmlElement("DespatchAdvice-Header")]
        public DespatchAdviceHeader DespatchAdviceHeader { get; set; }

        [XmlElement("DespatchAdvice-Parties")]
        public OrderParties DespatchAdviceParties { get; set; }

        [XmlElement("DespatchAdvice-Lines")]
        public DespatchAdviceLines DespatchAdviceLines { get; set; }

        [XmlElement("DespatchAdvice-Summary")]
        public OrderSummary OrderResponseSummary { get; set; }

        public string ToXml()
        {
            return this.ToXml<DespatchAdviceXml>();
        }
        public override string ToString()
        {
            return DespatchAdviceHeader.DespatchAdviceNumber;
        }
    }

    public class DespatchAdviceHeader
    {
        public string DespatchAdviceNumber { get; set; }
        public string DespatchAdviceDate { get; set; }
        public string BuyerOrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string EstimatedDeliveryDate { get; set; }
        public string EstimatedDeliveryTime { get; set; }
        public string Currency { get; set; }
        public string DeliveryNoteNumber { get; set; }
        public string DeliveryNoteDate { get; set; }
    }

    public class DespatchAdviceLines
    {
        [XmlElement("Line")]
        public List<LineDespatchAdvice> Line { get; set; }
    }

    public class LineDespatchAdvice
    {
        [XmlElement("Line-Item")]
        public LineItemDespatchAdvice LineItem { get; set; }
    }

    public class LineItemDespatchAdvice
    {
        //ДОРОБИТИ!!!!!!!!!!!!
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
