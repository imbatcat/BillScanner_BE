using Azure;
using Azure.AI.DocumentIntelligence;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Services;
using Infrastructure.MarkerInterfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.ImageProcessing
{
    [UsedImplicitly]
    public class AzureImageService : IImageExtractionService, ISingletonService
    {
        private readonly string _invoiceModelId;
        private readonly string _receiptModelId;
        private readonly DocumentIntelligenceClient _client;

        public AzureImageService(IOptions<AzureImageSettings> settings)
        {
            _invoiceModelId = settings.Value.InvoiceModelId;
            _receiptModelId = settings.Value.ReceiptModelId;

            var azureCredential = new AzureKeyCredential(settings.Value.ApiKey1);
            _client = new DocumentIntelligenceClient(new Uri(settings.Value.Endpoint), azureCredential);
        }

        public async Task<ImageProcessResult> ExtractImageAsync(
            string url,
            bool isInvoice)
        {
            Uri uri = new(url);

            Operation<AnalyzeResult> operation = await _client
                .AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    isInvoice ? _invoiceModelId : _receiptModelId,
                    uri);

            var result = new ImageProcessResult();

            var receipt = operation.Value.Documents[0];

            GetMerchantName(result, receipt);

            GetTransactionDate(result, receipt);

            GetItems(result, receipt);

            GetPrice(result, receipt);

            GetCurrency(result, receipt);

            return result;
        }

        private static void GetMerchantName(ImageProcessResult result, AnalyzedDocument receipt)
        {
            if (!receipt.Fields.TryGetValue("MerchantName", out var merchantNameField)) return;
            if (merchantNameField.FieldType == DocumentFieldType.String)
            {
                result.Vendor = new ExtractedVendor
                {
                    Name = new ExtractedValue<string?>
                    {
                        Value = merchantNameField.ValueString,
                        Confidence = (decimal)(merchantNameField.Confidence ?? 0)
                    }
                };
            }
        }

        private static void GetTransactionDate(ImageProcessResult result, AnalyzedDocument receipt)
        {
            if (!receipt.Fields.TryGetValue("TransactionDate", out var transactionDateField)) return;
            if (transactionDateField.FieldType == DocumentFieldType.Date)
            {
                result.BillDate = new ExtractedValue<DateTime?>
                {
                    Value = transactionDateField.ValueDate?.DateTime ?? DateTime.Now,
                    Confidence = (decimal)(transactionDateField.Confidence ?? 0)
                };
            }
        }

        private static void GetItems(ImageProcessResult result, AnalyzedDocument receipt)
        {
            if (!receipt.Fields.TryGetValue("Items", out var itemsField)) return;
            {
                if (itemsField.FieldType != DocumentFieldType.List) return;
                foreach (var itemField in itemsField.ValueList)
                {
                    if (itemField.FieldType != DocumentFieldType.Dictionary) continue;
                    var extractedItem = new ExtractedItem();
                    IReadOnlyDictionary<string, DocumentField> itemFields = itemField.ValueDictionary;

                    GetItemName(extractedItem, itemFields);

                    GetItemTotalPrice(extractedItem, itemFields);

                    result.Items.Add(extractedItem);
                }
            }

            return;

            static void GetItemName(ExtractedItem extractedItem, IReadOnlyDictionary<string, DocumentField> itemFields)
            {
                if (!itemFields.TryGetValue("Description", out var itemDescriptionField)) return;
                if (itemDescriptionField.FieldType == DocumentFieldType.String)
                {
                    extractedItem.ItemName = new ExtractedValue<string?>
                    {
                        Value = itemDescriptionField.ValueString,
                        Confidence = (decimal)(itemDescriptionField.Confidence ?? 0)
                    };
                }
            }

            static void GetItemTotalPrice(ExtractedItem extractedItem,
                IReadOnlyDictionary<string, DocumentField> itemFields)
            {
                if (!itemFields.TryGetValue("TotalPrice", out var itemTotalPriceField)) return;
                if (itemTotalPriceField.FieldType == DocumentFieldType.Currency)
                {
                    extractedItem.TotalPrice = new ExtractedValue<decimal?>
                    {
                        Value = (decimal)itemTotalPriceField.ValueCurrency.Amount,
                        Confidence = (decimal)(itemTotalPriceField.Confidence ?? 0)
                    };
                }
            }
        }


        private static void GetPrice(ImageProcessResult result, AnalyzedDocument receipt)
        {
            if (!receipt.Fields.TryGetValue("Total", out var totalField)) return;
            if (totalField.FieldType == DocumentFieldType.Currency)
            {
                result.Total = new ExtractedValue<decimal?>
                {
                    Value = (decimal)totalField.ValueCurrency.Amount,
                    Confidence = (decimal)(totalField.Confidence ?? 0)
                };
            }

            if (!receipt.Fields.TryGetValue("SubTotal", out var subTotalField)) return;
            if (subTotalField.FieldType == DocumentFieldType.Currency)
            {
                result.SubTotal = new ExtractedValue<decimal?>
                {
                    Value = (decimal)subTotalField.ValueCurrency.Amount,
                    Confidence = (decimal)(subTotalField.Confidence ?? 0)
                };
            }

            if (!receipt.Fields.TryGetValue("Tax", out var taxField)) return;
            if (taxField.FieldType == DocumentFieldType.Currency)
            {
                result.Tax = new ExtractedValue<decimal?>
                {
                    Value = (decimal)taxField.ValueCurrency.Amount,
                    Confidence = (decimal)(taxField.Confidence ?? 0)
                };
            }
        }

        private static void GetCurrency(ImageProcessResult result, AnalyzedDocument receipt)
        {
            if (!receipt.Fields.TryGetValue("Currency", out var currencyField)) return;
            if (currencyField.FieldType == DocumentFieldType.String)
            {
                result.Currency = new ExtractedValue<string?>
                {
                    Value = currencyField.ValueString,
                    Confidence = (decimal)(currencyField.Confidence ?? 0)
                };
            }
        }
    }
}