using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Vchasno.Base;

namespace Vchasno
{
    public class Program
    {
        private static readonly string _server_merezha = "https://edi.vchasno.ua/api/b";
        //private static readonly string _server_postach = "https://edi.vchasno.ua/api";

        private const string connectionSql4 = "Server=192.168.4.4; Database=Sk1; uid=КушнірА; pwd=зщшфтв;";
#if DEBUG
        //private const string connectionSql100 = "Server=AKUSHNIR\\MSSQLSERVER2014; Database=InetClient; uid=test; pwd=1;";
        //private static readonly string _sql_database = "[InetClient].[dbo].";
        private const string connectionSql100 = "Server=192.168.4.100; Database=InetClient; uid=NovaPoshta; pwd=NovaPoshta;";
        private static readonly string _sql_database = "[InetClient].[dbo].";
#else
        private const string connectionSql100 = "Context Connection = true;";
        private static readonly string _sql_database = "";
#endif
        private static readonly string _token = "4jR8v9siGyo-Ne4ZcFWVZXaxpK-2Mi3twRQPz7e12zIHyu5HPcQM_ZjFOMLnU2MH";  //ТОВ "ТОРГОВА ГРУПА "АРС-КЕРАМІКА"
        //private static readonly string _GLN = "9871000077344";

        //private static readonly string _token_test = "ZJNEGlfKuk87Npph-fhywk_vYBbHROSeufNxJoABE0t2IeZcBQETf5QcUfciPX0n";  //Тест(АРС-Кераміка)
        //private static readonly string _GLN_test = "9871000098226";

        public static void SendOrder(int idOrder, int proforma)
        {
            string error;
            ResultDto result = null;

            var document = GetOrderFromSQL(idOrder);
            if (document == null)
                return;
            var fileXml = CreateXmlFile<OrderXml>(document);
            if (fileXml == null)
                return;

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

            if (response == null)
            {
                SaveErrorToSQL(error);
                return;
            }

            result = response.ConvertJson<ResultDto>(ref error);
            var deal = GetDeal(result.deal_id);
            SaveErrorToSQL(error);

            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();
                using (var query = new SqlCommand($@"UPDATE {_sql_database}[VchasnoOrder] SET idDeal = '{deal.id}', fileOrder = '{fileXml.FullName}', sendResult = '{response}' WHERE id = {idOrder}", connection))
                    query.ExecuteNonQuery();

                using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoDeal] (proforma, uuid, type, status, state, companFrom, companTo, dateCreate, dateUpdate, dateChange, stateOur) VALUES ({proforma}, '{deal.id}', '{deal.type}', '{deal.status}', {deal.state}, '{deal.company_from}', '{deal.company_to}', '{deal.date_created.DateToSQL()}', '{deal.date_updated.DateToSQL()}', GETDATE(), 1)", connection))
                    query.ExecuteNonQuery();

                connection.Close();
            }
        }

        private static DealDto GetDeal(string idDeal)
        {
            var response = RequestData.SendGet(_server_merezha + $"/deals/{idDeal}/status", _token, out string error);
            if (response == null)
            {
                SaveErrorToSQL(error);
                return null;
            }
            var result = response.ConvertJson<DealDto>(ref error);
            SaveErrorToSQL(error);
            return result;
        }

        private static DocumentsDto GetDocument(string idDocument)
        {
            var response = RequestData.SendGet(_server_merezha + $"/deals/documents/{idDocument}", _token, out string error);
            if (response == null)
            {
                SaveErrorToSQL(error);
                return null;
            }
            var result = response.ConvertJson<DocumentsDto>(ref error);
            SaveErrorToSQL(error);
            return result;
        }

        public static List<DocumentsDto> GetDocuments(DateTime from)
        {
            //Не використовується зараз
            var response = RequestData.SendGet(_server_merezha + $"/deals/documents?date_from={from.ToString("yyyy-MM-dd")}", _token, out string error);
            if (response == null)
            {
                SaveErrorToSQL(error);
                return null;
            }
            var result = response.ConvertJson<List<DocumentsDto>>(ref error);
            SaveErrorToSQL(error);
            return result;
        }

        public static void UpdateDeal(string idDeal)
        {
            var deal = GetDeal(idDeal);
            using (SqlConnection connection = new SqlConnection(connectionSql100))
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

        public static void GetOrderItems(string idOrder)
        {
            //Цей метод не використовується бо немає потреби бачити товари, тотожні тим що ми вже послали у Вчасно
            //Ці товари фактично є в VchasnoLine
            var order = GetDocument(idOrder);
            if (order == null) 
            {
                SaveErrorToSQL($"Не вдалось отримати Order: {idOrder}");
                return; 
            }

            if (order.type != 1)
            {
                SaveErrorToSQL($"Це не Order: {idOrder}");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionSql100))
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

        public static void GetOrderResp(string idOrderResp)
        {
            var orderResp = GetDocument(idOrderResp);
            if (orderResp == null)
            {
                SaveErrorToSQL($"Не вдалось отримати OrderResp: {idOrderResp}");
                return;
            }

            if (orderResp.type != 2)
            {
                SaveErrorToSQL($"Це не OrderResponse: {idOrderResp}");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();
                using (var query = new SqlCommand($@"DELETE FROM {_sql_database}[VchasnoOrderResp] WHERE idDeal = '{orderResp.deal_id}'", connection))
                    query.ExecuteNonQuery();

                using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoOrderResp] (uuid, idDeal, orderRespN, orderRespD, expDelDate, currency, buyerILN, buyerDep, sellerILN, dlPointILN, invoiceILN, totalLine) VALUES ('{orderResp.id}', '{orderResp.deal_id}', '{orderResp.as_json.number}', '{orderResp.as_json.date.DateToSQL()}', '{orderResp.as_json.date_expected_delivery.DateToSQL()}', '{orderResp.as_json.currency}', '{orderResp.as_json.buyer_gln}', '{orderResp.as_json.buyer_department}', '{orderResp.as_json.seller_gln}', '{orderResp.as_json.delivery_gln}', '{orderResp.as_json.invoicee_gln}', '{orderResp.as_json.items.Count}')", connection))
                    query.ExecuteNonQuery();

                using (var query = new SqlCommand($@"DELETE FROM {_sql_database}[VchasnoItem] WHERE idDeal = '{orderResp.deal_id}'", connection))
                    query.ExecuteNonQuery();

                foreach (var item in orderResp.as_json.items)
                {
                    var title = Regex.Replace(item.title, @"'", @"''");
                    using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoItem] (idDeal, position, title, buyerCode, supplrCode, productCod, measure, quantity, quantityOr, quantityIU, taxRate, price, priceWhTax, netAmount, netAmWhTax) VALUES ('{orderResp.deal_id}', {item.position}, '{title}', '{item.buyer_code}', '{item.supplier_code}', '{item.product_code}', '{item.measure}', {item.quantity.NullCheck()}, {item.quantity_ordered.NullCheck()}, {item.quantity_in_unit.NullCheck()}, {item.tax_rate.NullCheck()}, {item.price.NullCheck()}, {item.price_with_tax.NullCheck()}, {item.net_amount.NullCheck()}, {item.net_amount_with_tax.NullCheck()})", connection))
                        query.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public static void SendStatus(string idDeal)
        {
            var deal = GetDeal(idDeal);
            if (deal == null) return;

            var idOrder = deal.documents.FirstOrDefault(d => d.type == 1)?.id;
            if (idOrder == null) return;

            var order = GetDocument(idOrder);
            if (order == null) return;

            var idOrderResp = deal.documents.FirstOrDefault(d => d.type == 2)?.id;
            if (idOrderResp == null) return;

            var orderResp = GetDocument(idOrderResp);
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

            var fileXml = CreateXmlFile<StatusXml>(status);
            if (fileXml == null) return;

            var keysBody = new Dictionary<string, string>
            {
                //{ "document_type", "order" }
            };
            string error;
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
                SaveErrorToSQL(error);
                return;
            }
            var statusResult = response.ConvertJson<ResultDto>(ref error);
            SaveErrorToSQL(error);

            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();
                using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoStatus] (idDeal, senderILN, receivrILN, date, docType, docNumber, type, stage, state, descriptio, fileStatus, sendResult) VALUES ('{deal.id}', '{status.ReportSenderILN}', '{status.ReportReceiverILN}', '{status.ReportDate}', '{status.ReportItem.DocumentType}', '{status.ReportItem.DocumentNumber}', '{status.ReportItem.ItemStatus.Type}', '{status.ReportItem.ItemStatus.Stage}', '{status.ReportItem.ItemStatus.State}', '{status.ReportItem.ItemStatus.Description}', '{fileXml.FullName}', '{response}')", connection))
                    query.ExecuteNonQuery();
                if (statusResult != null)
                    using (var query = new SqlCommand($@"UPDATE {_sql_database}[VchasnoStatus] SET uuid = '{statusResult.document_id}' WHERE idDeal = '{deal.id}'", connection))
                        query.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void GetDespatchAdvice(string idDespatchAdvice)
        {
            var despatchAdvice = GetDocument(idDespatchAdvice);
            if (despatchAdvice == null)
            {
                SaveErrorToSQL($"Не вдалось отримати DespatchAdvice: {idDespatchAdvice}");
                return;
            }

            if (despatchAdvice.type != 4)
            {
                SaveErrorToSQL($"Це не DespatchAdvice: {idDespatchAdvice}");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();

                using (var query = new SqlCommand($@"DELETE FROM {_sql_database}[VchasnoDespatchAdvice] WHERE idDeal = '{despatchAdvice.deal_id}'", connection))
                    query.ExecuteNonQuery();

                using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoDespatchAdvice] (uuid, idDeal, desadvNumb, desadvDate, expDelDate, currency, buyerILN, buyerDep, sellerILN, dlPointILN, totalLine, totalOrdAm, totalNetAm, totalGroAm) VALUES ('{despatchAdvice.id}', '{despatchAdvice.deal_id}', '{despatchAdvice.as_json.number}', '{despatchAdvice.as_json.date.DateToSQL()}', '{despatchAdvice.as_json.date_expected_delivery.DateToSQL()}', '{despatchAdvice.as_json.currency}', '{despatchAdvice.as_json.buyer_gln}', '{despatchAdvice.as_json.buyer_department}', '{despatchAdvice.as_json.seller_gln}', '{despatchAdvice.as_json.delivery_gln}', {despatchAdvice.as_json.summary.items_length}, {despatchAdvice.as_json.summary.items_quantity}, {despatchAdvice.as_json.summary.items_price}, {despatchAdvice.as_json.summary.items_price_with_tax})", connection))
                    query.ExecuteNonQuery();

                using (var query = new SqlCommand($@"DELETE FROM {_sql_database}[VchasnoItem] WHERE idDeal = '{despatchAdvice.deal_id}'", connection))
                    query.ExecuteNonQuery();

                foreach (var item in despatchAdvice.as_json.items)
                {
                    var title = Regex.Replace(item.title, @"'", @"''");
                    using (var query = new SqlCommand($@"INSERT INTO {_sql_database}[VchasnoItem] (idDeal, position, title, buyerCode, supplrCode, productCod, measure, quantity, quantityOr, quantityIU, taxRate, price, priceWhTax, netAmount, netAmWhTax) VALUES ('{despatchAdvice.deal_id}', {item.position}, '{title}', '{item.buyer_code}', '{item.supplier_code}', '{item.product_code}', '{item.measure}', {item.quantity.NullCheck()}, {item.quantity_ordered.NullCheck()}, {item.quantity_in_unit.NullCheck()}, {item.tax_rate.NullCheck()}, {item.price.NullCheck()}, {item.price_with_tax.NullCheck()}, {item.net_amount.NullCheck()}, {item.net_amount_with_tax.NullCheck()})", connection))
                        query.ExecuteNonQuery();
                }
                connection.Close();
            }

            CreatePrihSQL(despatchAdvice);
        }

        private static void CreatePrihSQL(DocumentsDto despatchAdvice)
        {
            int proforma;
            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();
                var query = new SqlCommand($@"SELECT TOP 1 proforma FROM VchasnoDeal WHERE uuid = '{despatchAdvice.deal_id}'", connection);
                proforma = (int)(query.ExecuteScalar());
                connection.Close();
            }
            int codep;
            string namep;
            using (SqlConnection connection = new SqlConnection(connectionSql4))
            {
                connection.Open();
                var query = new SqlCommand($@"SELECT codep, namep FROM [Proforma] WHERE proforma = {proforma}", connection);
                var reader = query.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    SaveErrorToSQL($"Помилка в отриманні проформи({despatchAdvice.deal_id}): немає даних");
                    return;
                }
                reader.Read();
                codep = Convert.ToInt32(reader["codep"]);
                namep = Convert.ToString(reader["namep"]);
                reader.Close();
                connection.Close();
            }

            using (SqlConnection connection = new SqlConnection(connectionSql4))
            {
                connection.Open();
                var queryMain = new SqlCommand($@"us_PrihF_add", connection) { CommandType = CommandType.StoredProcedure};
                queryMain.Parameters.AddWithValue("@ttype", 2);
                queryMain.Parameters.AddWithValue("@type", 1);

                var reader = queryMain.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    SaveErrorToSQL($"Помилка створення прихідної({despatchAdvice.deal_id})");
                    return;
                }
                reader.Read();
                var coden = Convert.ToInt32(reader["coden"]);
                var codeskto = Convert.ToInt32(reader["codeskto"]);
                var nomn = Convert.ToInt32(reader["nomn"]);
                var daten = Convert.ToDateTime(reader["daten"]);
                var rah = Convert.ToInt32(reader["rah"]);
                var codepdt = Convert.ToInt32(reader["codepdt"]);
                var namepdt = Convert.ToString(reader["namepdt"]);
                var codeworker = Convert.ToInt32(reader["codeworker"]);
                reader.Close();

                using (var query = new SqlCommand($@"EXECUTE [us_PrihF_unlock] {coden}", connection))
                    query.ExecuteNonQuery();

                var prompt = "Ця накладна створена автоматично через Вчасно.";

                using (var query = new SqlCommand($@"DECLARE @daten smalldatetime=GETDATE(); EXECUTE [us_PrihF_edit] {coden}, {codeskto}, {nomn}, @daten, {codep}, '{namep}', '{"Невідомий"}', {rah}, {codepdt}, '{namepdt}', '{prompt}', 2, 4, {proforma}, {codeworker}", connection))
                    query.ExecuteNonQuery();

                foreach (var item in despatchAdvice.as_json.items)
                {
                    var codetvun = item.buyer_code.StringToInt();
                    string nametv = null;
                    string ov = null;
                    if (codetvun != 0)
                    {
                        var query = new SqlCommand($@"SELECT TOP 1 nametv, ov FROM [Tovar].[dbo].[Tovar] WHERE codetvun = {codetvun}", connection);
                        reader = query.ExecuteReader();
                        if (!reader.HasRows)
                        {
                            reader.Close();
                            SaveErrorToSQL($"Помилка в товарі({item.buyer_code}): немає даних");
                            continue;
                        }
                        reader.Read();
                        nametv = Convert.ToString(reader["nametv"]);
                        ov = Convert.ToString(reader["ov"]);
                        reader.Close();
                    }

                    using (var query = new SqlCommand($@"EXECUTE [us_PrihD_edit] 0, {coden}, 281, {item.quantity.NullCheck()}, {item.price_with_tax.NullCheck()}, 1, 0, {codetvun}, '{nametv ?? item.title}', '{ov}', 0, 0", connection))
                        query.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public static void TestPrih()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionSql4))
                {
                    connection.Open();
                    var queryMain = new SqlCommand($@"us_PrihF_add", connection) { CommandType = CommandType.StoredProcedure };
                    queryMain.Parameters.AddWithValue("@ttype", 2);
                    queryMain.Parameters.AddWithValue("@type", 1);

                    var reader = queryMain.ExecuteReader();

                    while (!reader.IsClosed)
                    {
                        SaveErrorToSQL($"Кількість колонок {reader.FieldCount}. Є дані {reader.HasRows}.");
                        while (reader.Read())
                        {
                            SaveErrorToSQL($"Дані: {reader[0]}");
                        }
                        if (!reader.NextResult())
                        {
                            reader.Close();
                        }
                    }

                    connection.Close();
                }
            }
            catch (SqlException se)
            {
                SaveErrorToSQL($"SqlException {se}");
            }
        }

        public static void TestPrihTables()
        {
            using (SqlConnection connection = new SqlConnection(connectionSql4))
            {
                connection.Open();
                var query = new SqlCommand($@"SELECT HOST_NAME() as host", connection);
                var reader = query.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    SaveErrorToSQL($"{(reader["host"])}");
                }

                reader.Close();
                connection.Close();
            }
        }

        public static void TestDelete()
        {
            using (SqlConnection connection = new SqlConnection(connectionSql4))
            {
                connection.Open();
                var queryMain = new SqlCommand($@"DELETE [Firm].[dbo].[KadrMsg] WHERE codep = 11111", connection);
                queryMain.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void TestSQL()
        {
            //Проиклад отримання декількох курсорів
            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();
                using (var query = new SqlCommand($@"test1", connection) { CommandType = CommandType.StoredProcedure })
                using (var reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //MessageBox.Show(Convert.ToString(reader["data"]));
                        SaveErrorToSQL(Convert.ToString(reader["data"]));
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            //MessageBox.Show(Convert.ToString(reader["data"]));
                            SaveErrorToSQL(Convert.ToString(reader["data"]));
                        }
                    }
                }

                connection.Close();
            }
        }

        public static void TestScalar()
        {
            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();
                var query = new SqlCommand($@"SELECT proforma FROM VchasnoDeal WHERE uuid = '0f7334cb-131e-2bb2-083d-1f0f58d034fa'", connection);
                var proforma = (int)(query.ExecuteScalar());
                connection.Close();
            }
        }

        private static OrderXml GetOrderFromSQL(int idOrder)
        {
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

            using (SqlConnection connection = new SqlConnection(connectionSql100))
            {
                connection.Open();
                using (var query = new SqlCommand($@"SELECT * FROM {_sql_database}[VchasnoOrder] WHERE id = {idOrder}", connection))
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
                        SaveErrorToSQL($"Не знайшли документ: {idOrder}");
                        connection.Close();
                        return null;
                    }

                using (var query = new SqlCommand($@"SELECT * FROM {_sql_database}[VchasnoLine] WHERE idOrder = {idOrder}", connection))
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
                        SaveErrorToSQL($"Не знайшли жодного товару в документі: {idOrder}");
                        connection.Close();
                        return null;
                    }
            }
            return document;
        }

        private static FileInfo CreateXmlFile<T>(T document)
        {
            var text = document.ToXml();
            if (text == String.Empty)
            {
                SaveErrorToSQL("Не зміг непервести документ в Xml-формат(стрічку)");
                return null;
            }
            var fileNamePDF = Path.GetTempPath() + document.ToString() + ".xml";
            try
            {
                File.WriteAllText(fileNamePDF, text);
            }
            catch
            {
                SaveErrorToSQL("Не зміг створити/записати файл .xml");
                return null;
            }

            var fileInf = new FileInfo(fileNamePDF);
            return fileInf;
        }

        private static void SaveErrorToSQL(string error)
        {
            if (!String.IsNullOrWhiteSpace(error))
            {
                var methodName = new StackTrace(1).GetFrame(0).GetMethod().Name;
                using (SqlConnection connection = new SqlConnection(connectionSql100))
                {
                    connection.Open();
                    var sql = $@"INSERT INTO {_sql_database}[VchasnoErrorLog] (error, date) VALUES ('{methodName} error: {error}', '{DateTime.Now.DateToSQL()}')";
                    using (var query = new SqlCommand(sql, connection))
                        query.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static void TestToSQL(string text)
        {
            using (SqlConnection connection = new SqlConnection("Context Connection = true;"))
            {
                connection.Open();
                var sql = $@"INSERT INTO [erp].[dbo].[FirstTable] VALUES ('Отримані дані: {text}')";
                using (var query = new SqlCommand(sql, connection))
                    query.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}