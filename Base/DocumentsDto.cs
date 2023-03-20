using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vchasno.Base
{
    public class DocumentsDto
    {
        public string id { get; set; }
        public string title { get; set; }
        public string number { get; set; }
        public int type { get; set; }
        public int status { get; set; }
        public string deal_id { get; set; }
        public string company_from { get; set; }
        public string company_to { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_document { get; set; }
        public DateTime date_expected_delivery { get; set; }
        public bool is_vchasno { get; set; }
        public string deal_status { get; set; }
        public string vchasno_id { get; set; }
        public string vchasno_status { get; set; }
        public string[] company_to_glns { get; set; }
        public ItemsList as_json { get; set; }
    }

    public class ItemsList
    {
        public DateTime date { get; set; } //string
        public object type { get; set; }
        public Item[] items { get; set; }
        public object action { get; set; }
        public string number { get; set; }
        public string remarks { get; set; }
        public Summary summary { get; set; }
        public string currency { get; set; }
        public string buyer_gln { get; set; }
        public DateTime order_date { get; set; } //string
        public string seller_gln { get; set; }
        public object sender_gln { get; set; }
        public string invoicee_gln { get; set; }
        public object tax_number { get; set; }
        public string delivery_gln { get; set; }
        public string order_number { get; set; }
        public string consignee_gln { get; set; }
        public object contract_type { get; set; }
        public object seller_tax_id { get; set; }
        public object delivery_notes { get; set; }
        public object contract_number { get; set; }
        public string buyer_department { get; set; }
        public object edi_intetchange_id { get; set; }
        public object invoicepartner_gln { get; set; }
        public object final_recipient_gln { get; set; }
        public object seller_code_by_buyer { get; set; }
        public object document_function_code { get; set; }
        public object contract_date { get; set; }
        public string function_code { get; set; }
        public object despatch_number { get; set; }
        public object delivery_note_date { get; set; }
        public object delivery_note_number { get; set; }
        public object partial_total_number { get; set; }
        public DateTime date_expected_delivery { get; set; } //string
        public DateTime time_expected_delivery { get; set; } //string
        public object partial_sequence_number { get; set; }
        public string delivery_address { get; set; }
    }

    public class Summary
    {
        public string items_length { get; set; }
        public string items_price { get; set; }
        public string items_quantity { get; set; }
        public string items_price_with_tax { get; set; }
    }

    public class Item
    {
        public string price { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string deleted { get; set; }
        public string measure { get; set; }
        public string position { get; set; }
        public string quantity { get; set; }
        public string tax_rate { get; set; }
        public string buyer_code { get; set; }
        public string net_amount { get; set; }
        public string tax_amount { get; set; }
        public string product_code { get; set; }
        public string product_type { get; set; }
        public string external_code { get; set; }
        public string supplier_code { get; set; }
        public string price_with_tax { get; set; }
        public string quantity_in_unit { get; set; }
        public string quantity_minimum { get; set; }
        public string quantity_ordered { get; set; }
        public string quantity_confirmed { get; set; }
        public string tax_category_code { get; set; }
        public string net_amount_with_tax { get; set; }
        public string quantity_despatched { get; set; }
    }

    //old:
    //public class Item
    //{
    //    public string price { get; set; }
    //    public string title { get; set; }
    //    public string measure { get; set; }
    //    public int position { get; set; } //string
    //    public string quantity { get; set; }
    //    public string tax_rate { get; set; }
    //    public string buyer_code { get; set; }
    //    public string product_code { get; set; }
    //    public object supplier_code { get; set; }
    //    public string price_with_tax { get; set; }
    //    public string net_amount { get; set; }
    //    public string quantity_in_unit { get; set; }
    //    public object quantity_minimum { get; set; }
    //    public object tax_category_code { get; set; }
    //    public string net_amount_with_tax { get; set; }
    //}
}

