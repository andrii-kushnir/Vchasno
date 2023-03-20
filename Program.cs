using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Vchasno.Base;

namespace Vchasno
{
    public class Program
    {
        private static readonly string _server_merezha = "https://edi.vchasno.ua/api/b";
        //private static readonly string _server_postach = "https://edi.vchasno.ua/api";

        //private static readonly string _sql_database = "[192.168.4.5].[InetClient].[dbo].";
        private static readonly string _sql_database = "";

        private static readonly string _token = "4jR8v9siGyo-Ne4ZcFWVZXaxpK-2Mi3twRQPz7e12zIHyu5HPcQM_ZjFOMLnU2MH";  //ТОВ "ТОРГОВА ГРУПА "АРС-КЕРАМІКА"
        //private static readonly string _GLN = "9871000077344";

        //private static readonly string _token_test = "ZJNEGlfKuk87Npph-fhywk_vYBbHROSeufNxJoABE0t2IeZcBQETf5QcUfciPX0n";  //Тест(АРС-Кераміка)
        //private static readonly string _GLN_test = "9871000098226";

        //File.AppendAllText(@"E:\temp\log.txt", $"\r\n1. {query.CommandText}");

        static void Main(string[] args)
        {
            string error;
            var orderResp = GetDocument("0f1d52c4-37cd-0abf-cc35-ebbb82c6e0b1", out error)?.ConvertJson<DocumentsDto>(out error);

            //GetDespatchAdvice("0f1e8e8c-6897-97fc-f1b4-88fa29ce422d", out error);

            //SaveDeal(deal, 40441);

            //SendOrder(0, out error);
            //GetDocuments(new DateTime(2022,06,16), out error);
            //var sss = GetInvoice("0f0e4268-33f5-6e5f-fd29-18a2719a29f6", out error);

            Console.ReadLine();
            Console.ReadLine();
        }

        public static void SendOrder(int id, int proforma, out string error)
        {
            ResultDto result = null;
            FileInfo fileXml;
            if (id == 0)
            {
                var fileNamePDF = @"C:\Windows\ServiceProfiles\MSSQL$MSSQLSERVER2014\AppData\Local\Temp\ARC00007040441.xml";
                fileXml = new FileInfo(fileNamePDF);
            }
            else
            {
                var document = GetOrderFromSQL(id, out error);
                if (document == null)
                    return;
                fileXml = CreateXmlFile<OrderXml>(document, out error);
                if (fileXml == null)
                    return;
            }

            var keysBody = new Dictionary<string, string>
            {
                //{ "document_type", "order" }
            };

            string response = null;
            try
            {
                response = RequestData.FormDataRequest(_server_merezha + "/deals/documents?document_type=order", _token, keysBody, fileXml, "text/xml", "file", out error);
            }
            catch (Exception ex)
            {
                error = ex.Message + "  ";
            }

            if (response == null) response = "";

            if (id != 0)
            {
                result = response.ConvertJson<ResultDto>(out error);
                var deal = GetDeal(result.deal_id, out error);

                using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
                {
                    connection.Open();
                    using (var query = new SqlCommand($@"UPDATE {_sql_database}[VchasnoOrder] SET idDeal = '{deal.id}', fileOrder = '{fileXml.FullName}', sendResult = '{response}' WHERE id = {id}", connection))
                        query.ExecuteNonQuery();

                    using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoDeal] (proforma, uuid, type, status, state, companFrom, companTo, dateCreate, dateUpdate, dateChange, stateOur) VALUES ({proforma}, '{deal.id}', '{deal.type}', '{deal.status}', {deal.state}, '{deal.company_from}', '{deal.company_to}', '{deal.date_created.DateToSQL()}', '{deal.date_updated.DateToSQL()}', GETDATE(), 1)", connection))
                        query.ExecuteNonQuery();

                    connection.Close();
                }
            }
        }

        public static DealDto GetDeal(string idDeal, out string error)
        {
            var response = RequestData.SendGet(_server_merezha + $"/deals/{idDeal}/status", _token, out error);
            if (response == null)
            {
                return null;
            }
            var result = response.ConvertJson<DealDto>(out error);
            return result;
        }

        public static string GetDocument(string id, out string error)
        {
            var response = RequestData.SendGet(_server_merezha + $"/deals/documents/{id}", _token, out error);
            return response;
        }

        //public static bool IsDeal(string idDeal)
        //{
        //    bool result;
        //    using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
        //    {
        //        connection.Open();
        //        var query = new SqlCommand($@"SELECT * FROM {_sql_database}[VchasnoDeal] WHERE uuid = '{idDeal}'", connection);
        //        var reader = query.ExecuteReader();
        //        result = reader.HasRows;
        //        connection.Close();
        //    }
        //    return result;
        //}

        //public static void SaveDeal(DealDto deal, int proforma)
        //{
        //    if (IsDeal(deal.id)) return;
        //    using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
        //    {
        //        connection.Open();
        //        using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoDeal] (proforma, uuid, type, status, state, companFrom, companTo, dateCreate, dateUpdate, dateChange) VALUES ({proforma}, '{deal.id}', '{deal.type}', '{deal.status}', {deal.state}, '{deal.company_from}', '{deal.company_to}', '{deal.date_created.DateToSQL()}', '{(deal.date_updated.DateToSQL()}', GETDATE())", connection))
        //        {
        //            query.ExecuteNonQuery();
        //        }
        //        connection.Close();
        //    }
        //}

        public static void UpdateDeal(string idDeal, out string error)
        {
            var deal = GetDeal(idDeal, out error);
            using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
            {
                connection.Open();
                using (var query = new SqlCommand($@"SELECT * FROM {_sql_database}[VchasnoDeal] WHERE uuid = '{deal.id}'", connection))
                using (var reader = query.ExecuteReader())
                    if (!reader.HasRows)
                    {
                        connection.Close();
                        return;
                    }
                using (var query = new SqlCommand($@"UPDATE {_sql_database}[VchasnoDeal] SET type = '{deal.type}', status = '{deal.status}', state = {deal.state}, dateUpdate = '{deal.date_updated.DateToSQL()}', dateChange = GETDATE() WHERE uuid = '{deal.id}'", connection))
                    query.ExecuteNonQuery();

                foreach (var document in deal.documents)
                {
                    using (var query = new SqlCommand($@"SELECT * FROM {_sql_database}[VchasnoDocument] WHERE uuid = '{document.id}'", connection))
                    {
                        bool isDocument;
                        using (var reader = query.ExecuteReader())
                            isDocument = reader.HasRows;

                        if (isDocument)
                            using (var query1 = new SqlCommand($@"UPDATE {_sql_database}[VchasnoDocument] SET status = '{document.status}', dateChange = GETDATE(), is_vchasno = {(document.is_vchasno ? 1 : 0)} WHERE uuid = '{document.id}'", connection))
                                query1.ExecuteNonQuery();
                        else
                            using (var query1 = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoDocument] (uuid, idDeal, number, title, type, status, dateCreate, dateDocum, dateExpDel, dateChange, is_vchasno) VALUES ('{document.id}', '{deal.id}', '{document.number}', '{document.title}', {document.type}, {document.status}, '{document.date_created.DateToSQL()}', '{document.date_document.DateToSQL()}', '{document.date_expected_delivery.DateToSQL()}', GETDATE(), {(document.is_vchasno ? 1 : 0)})", connection))
                                query1.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }

        public static void GetOrderItems(string idOrder, out string error)
        {
            var order = GetDocument(idOrder, out error)?.ConvertJson<DocumentsDto>(out error);
            if (order == null)
                return;

            if (order.type != 1)
            {
                error = "Це не Order";
                return;
            }

            using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
            {
                connection.Open();
                using (var query = new SqlCommand($@"DELETE FROM {_sql_database}[VchasnoItem] WHERE idDeal = '{order.deal_id}'", connection))
                    query.ExecuteNonQuery();

                foreach (var item in order.as_json.items)
                {
                    using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoItem] (idDeal, position, title, buyerCode, supplrCode, productCod, measure, quantityOr, quantityIU, taxRate, price, priceWhTax, netAmount, netAmWhTax) VALUES ('{order.deal_id}', {item.position}, '{item.title}', '{item.buyer_code}', '{item.supplier_code}', '{item.product_code}', '{item.measure}', {item.quantity}, {item.quantity_in_unit}, {item.tax_rate}, {item.price}, {item.price_with_tax}, {item.net_amount}, {item.net_amount_with_tax})", connection))
                        query.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public static void GetOrderResp(string idOrderResp, out string error)
        {
            var orderResp = GetDocument(idOrderResp, out error)?.ConvertJson<DocumentsDto>(out error);
            if (orderResp == null)
                return;

            if (orderResp.type != 2)
            {
                error = "Це не OrderResponse";
                return;
            }

            using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
            {
                connection.Open();
                using (var query = new SqlCommand($@"DELETE FROM {_sql_database}[VchasnoOrderResp] WHERE idDeal = '{orderResp.deal_id}'", connection))
                    query.ExecuteNonQuery();

                using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoOrderResp] (uuid, idDeal, orderRespN, orderRespD, expDelDate, currency, buyerILN, buyerDep, sellerILN, dlPointILN, invoiceILN, totalLine) VALUES ('{orderResp.id}', '{orderResp.deal_id}', '{orderResp.as_json.number}', '{orderResp.as_json.date.DateToSQL()}', '{orderResp.as_json.date_expected_delivery.DateToSQL()}', '{orderResp.as_json.currency}', '{orderResp.as_json.buyer_gln}', '{orderResp.as_json.buyer_department}', '{orderResp.as_json.seller_gln}', '{orderResp.as_json.delivery_gln}', '{orderResp.as_json.invoicee_gln}', '{orderResp.as_json.items.Length}')", connection))
                    query.ExecuteNonQuery();

                using (var query = new SqlCommand($@"DELETE FROM {_sql_database}[VchasnoItem] WHERE idDeal = '{orderResp.deal_id}'", connection))
                    query.ExecuteNonQuery();

                foreach (var item in orderResp.as_json.items)
                {
                    using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoItem] (idDeal, position, title, buyerCode, supplrCode, productCod, measure, quantity, quantityOr, quantityIU, taxRate, price, priceWhTax, netAmount, netAmWhTax) VALUES ('{orderResp.deal_id}', {item.position}, '{item.title}', '{item.buyer_code}', '{item.supplier_code}', '{item.product_code}', '{item.measure}', {item.quantity}, {item.quantity_ordered}, {item.quantity_in_unit.NullCheck()}, {item.tax_rate}, {item.price}, {item.price_with_tax}, {item.net_amount.NullCheck()}, {item.net_amount_with_tax.NullCheck()})", connection))
                        query.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public static void SendStatus(string idDeal, out string error)
        {
            var deal = GetDeal(idDeal, out error);
            if (deal == null) return;

            var idOrder = deal.documents.FirstOrDefault(d => d.type == 1)?.id;
            if (idOrder == null) return;

            var order = GetDocument(idOrder, out error)?.ConvertJson<DocumentsDto>(out error);
            if (order == null) return;

            var idOrderResp = deal.documents.FirstOrDefault(d => d.type == 2)?.id;
            if (idOrderResp == null) return;

            var orderResp = GetDocument(idOrderResp, out error)?.ConvertJson<DocumentsDto>(out error);
            if (orderResp == null) return;

            var status = new StatusXml()
            {
                ReportNumber = $"STATUS_{order.number}",
                ReportSenderILN = $"{order.as_json.buyer_gln}",
                ReportReceiverILN = $"{order.as_json.seller_gln}",
                ReportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ReportItem = new ReportItem()
                {
                    ItemNumber = 1,
                    ItemDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    DocumentSenderILN = $"{order.as_json.buyer_gln}",
                    DocumentReceiverILN = $"{order.as_json.seller_gln}",
                    DocumentType = "ORDER_RESP",
                    DocumentNumber = $"{orderResp.number}",
                    ItemStatus = new ItemStatus()
                    {
                        Type = "APL",
                        Stage = "CONFIRM",
                        State = "OK",
                        Code = "000",
                        Description = "Замовлення підтверджуєм."
                    }
                }
            };

            var fileXml = CreateXmlFile<StatusXml>(status, out error);
            if (fileXml == null) return;

            var keysBody = new Dictionary<string, string>
            {
                //{ "document_type", "order" }
            };

            string response = null;
            try
            {
                response = RequestData.FormDataRequest(_server_merezha + "/deals/documents?document_type=status", _token, keysBody, fileXml, "text/xml", "file", out error);
            }
            catch (Exception ex)
            {
                error = ex.Message + "  ";
            }

            if (response == null)
            {
                response = "";
            }

            //response = "{\"deal_id\": \"0f1d52b0-098c-57bc-710e-bb44b2d63640\", \"document_id\": \"0f1e8e5c-55b1-eca7-eba8-591e4185c9e3\", \"documents_ids\": []}";

            using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
            {
                connection.Open();
                using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoStatus] (idDeal, senderILN, receivrILN, date, docType, docNumber, type, stage, state, descriptio, fileStatus, sendResult) VALUES ('{deal.id}', '{status.ReportSenderILN}', '{status.ReportReceiverILN}', '{status.ReportDate}', '{status.ReportItem.DocumentType}', '{status.ReportItem.DocumentNumber}', '{status.ReportItem.ItemStatus.Type}', '{status.ReportItem.ItemStatus.Stage}', '{status.ReportItem.ItemStatus.State}', '{status.ReportItem.ItemStatus.Description}', '{fileXml.FullName}', '{response}')", connection))
                    query.ExecuteNonQuery();
                var statusResult = response.ConvertJson<ResultDto>(out error);
                if (statusResult != null)
                    using (var query = new SqlCommand($@"UPDATE {_sql_database}[VchasnoStatus] SET uuid = '{statusResult.document_id}' WHERE idDeal = '{deal.id}'", connection))
                        query.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void GetDespatchAdvice(string idDespatchAdvice, out string error)
        {
            var despatchAdvice = GetDocument(idDespatchAdvice, out error)?.ConvertJson<DocumentsDto>(out error);
            if (despatchAdvice == null)
                return;

            if (despatchAdvice.type != 4)
            {
                error = "Це не DespatchAdvice";
                return;
            }

            using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
            {
                connection.Open();
                using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoDespatchAdvice] (uuid, idDeal, desadvNumb, desadvDate, expDelDate, currency, buyerILN, buyerDep, sellerILN, dlPointILN, totalLine, totalOrdAm, totalNetAm, totalGroAm) VALUES ('{despatchAdvice.id}', '{despatchAdvice.deal_id}', '{despatchAdvice.as_json.number}', '{despatchAdvice.as_json.date.DateToSQL()}', '{despatchAdvice.as_json.date_expected_delivery.DateToSQL()}', '{despatchAdvice.as_json.currency}', '{despatchAdvice.as_json.buyer_gln}', '{despatchAdvice.as_json.buyer_department}', '{despatchAdvice.as_json.seller_gln}', '{despatchAdvice.as_json.delivery_gln}', {despatchAdvice.as_json.summary.items_length}, {despatchAdvice.as_json.summary.items_quantity}, {despatchAdvice.as_json.summary.items_price}, {despatchAdvice.as_json.summary.items_price_with_tax})", connection))
                    query.ExecuteNonQuery();

                //!!! десь зберегти список товарів(ліній) !!!

                connection.Close();
            }
        }

        //public static DocumentsDto IsOrderResponse(string idDeal, out string error)
        //{
        //    var deal = GetDeal(idDeal, out error);
        //    if (deal == null)
        //        return null;
        //    return deal.documents.FirstOrDefault(d => d.type == 2);
        //}

        public static List<DocumentsDto> GetDocuments(DateTime from, out string error)
        {
            var response = RequestData.SendGet(_server_merezha + $"/deals/documents?date_from={from.ToString("yyyy-MM-dd")}", _token, out error);
            var result = response.ConvertJson<List<DocumentsDto>>(out error);
            return result;
        }

        private static OrderXml GetOrderVFromSQL_test(int id, out string error)
        {
            error = null;
            var test = new OrderXml()
            {
                OrderHeader = new OrderHeader() { OrderNumber = "Name" },
                OrderParties = new OrderParties()
                {
                    Buyer = new Buyer() { ILN = "1234567890123" },
                    Seller = new Seller() { ILN = "1234567890123" },
                    DeliveryPoint = new DeliveryPoint(),
                    Invoicee = new Invoicee()
                },
                OrderLines = new OrderLines()
                {
                    Line = new List<Line>()
                    {
                        new Line() { LineItem = new LineItem() { LineNumber = 1, EAN = "dsfdsfdsfds1111"} },
                        new Line() { LineItem = new LineItem() { LineNumber = 2, EAN = "dsfdsfdsfds2222"} }
                    }
                },
                OrderSummary = new OrderSummary()
                {
                    TotelLines = 2,
                    TotalOrderedAmount = 20
                }
            };
            return test;
        }

        private static OrderXml GetOrderFromSQL(int id, out string error)
        {
            error = null;
            var document = new OrderXml()
            {
                OrderHeader = new OrderHeader(),
                OrderParties = new OrderParties()
                {
                    Buyer = new Buyer(),
                    Seller = new Seller(),
                    DeliveryPoint = new DeliveryPoint(),
                    Invoicee = new Invoicee()
                },
                OrderLines = new OrderLines()
                {
                    Line = new List<Line>()
                },
                OrderSummary = new OrderSummary()
            };

            using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
            {
                connection.Open();
                using (var query = new SqlCommand($@"SELECT * FROM {_sql_database}[VchasnoOrder] WHERE id = {id}", connection))
                using (var reader = query.ExecuteReader())
                    if (reader.HasRows)
                    {
                        reader.Read();
                        document.OrderHeader.OrderNumber = Convert.ToString(reader["orderNumbr"]);
                        document.OrderHeader.OrderDate = Convert.ToString(reader["orderDate"]);
                        document.OrderHeader.ExpectedDeliveryDate = Convert.ToString(reader["expDelDate"]);
                        document.OrderHeader.ExpectedDeliveryTime = Convert.ToString(reader["expDelTime"]);
                        document.OrderHeader.Currency = Convert.ToString(reader["currency"]);
                        document.OrderHeader.DocumentFunctionCode = Convert.ToString(reader["funcCode"]);
                        document.OrderHeader.ContractNumber = Convert.ToString(reader["contrNumbr"]);
                        document.OrderHeader.Remarks = Convert.ToString(reader["remarks"]);
                        document.OrderParties.Buyer.ILN = Convert.ToString(reader["buyerILN"]);
                        document.OrderParties.Buyer.Department = Convert.ToString(reader["buyerDep"]);
                        document.OrderParties.Seller.ILN = Convert.ToString(reader["sellerILN"]);
                        document.OrderParties.DeliveryPoint.ILN = Convert.ToString(reader["dlPointILN"]);
                        document.OrderParties.Invoicee.ILN = Convert.ToString(reader["invoiceILN"]);
                        document.OrderSummary.TotelLines = Convert.ToInt32(reader["totalLine"]);
                        document.OrderSummary.TotalOrderedAmount = Convert.ToInt32(reader["totalOrdAm"]);
                        document.OrderSummary.TotalNetAmount = Convert.ToDecimal(reader["totalNetAm"]);
                        document.OrderSummary.TotalGrossAmount = Convert.ToDecimal(reader["totalGroAm"]);
                    }
                    else
                    {
                        error = $"Не знайшли документ: {id}";
                        connection.Close();
                        return null;
                    }

                using (var query = new SqlCommand($@"SELECT * FROM {_sql_database}[VchasnoLine] WHERE idOrder = {id}", connection))
                using (var reader = query.ExecuteReader())
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var item = new Line() { LineItem = new LineItem() };
                            item.LineItem.LineNumber = Convert.ToInt32(reader["lineNumber"]);
                            item.LineItem.EAN = Convert.ToString(reader["EAN"]);
                            item.LineItem.BuyerItemCode = Convert.ToString(reader["buyItemCod"]);
                            item.LineItem.SupplierItemCode = Convert.ToString(reader["selItemCod"]);
                            item.LineItem.ItemDescription = Convert.ToString(reader["itemDescrp"]);
                            item.LineItem.OrderedQuantity = Convert.ToDecimal(reader["orderQuant"]);
                            item.LineItem.OrderedUnitPacksize = Convert.ToInt32(reader["ordUnitPac"]);
                            item.LineItem.UnitOfMeasure = Convert.ToString(reader["unitOfMeas"]);
                            item.LineItem.OrderedUnitNetPrice = Convert.ToDecimal(reader["ordUnitNet"]);
                            item.LineItem.OrderedUnitGrossPrice = Convert.ToDecimal(reader["ordUnitGro"]);
                            item.LineItem.TaxRate = Convert.ToInt32(reader["taxRate"]);
                            item.LineItem.TaxCategoryCode = Convert.ToString(reader["taxCategor"]);
                            item.LineItem.NetAmount = Convert.ToDecimal(reader["netAmount"]);
                            item.LineItem.GrossAmount = Convert.ToDecimal(reader["grosAmount"]);

                            document.OrderLines.Line.Add(item);
                        }
                    }
                    else
                    {
                        error = $"Не знайшли жодного товару в документі: {id}";
                        connection.Close();
                        return null;
                    }
            }

            return document;
        }

        private static FileInfo CreateXmlFile<T>(T document, out string error)
        {
            error = null;
            var text = document.ToXml();
            if (text == String.Empty)
            {
                error = "Не зміг непервести документ в Xml-формат(стрічку)";
                return null;
            }
            var fileNamePDF = Path.GetTempPath() + document.ToString() + ".xml";
            try
            {
                File.WriteAllText(fileNamePDF, text);
            }
            catch
            {
                error = "Не зміг створити/записати файл .xml";
                return null;
            }

            var fileInf = new FileInfo(fileNamePDF);
            return fileInf;
        }
    }
}