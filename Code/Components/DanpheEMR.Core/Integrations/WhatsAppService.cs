using System;
using System.Threading.Tasks;

namespace DanpheEMR.Core.Integrations
{
    public interface IWhatsAppService
    {
        Task<bool> SendPrescriptionWhatsAppAsync(string patientName, string phoneNumber, string prescriptionDetails, string pdfBase64 = null);
        Task<bool> SendReportWhatsAppAsync(string patientName, string phoneNumber, string testName, string pdfUrl = null);
        Task<bool> SendBillingReceiptWhatsAppAsync(string patientName, string phoneNumber, double billAmount, string receiptNo, string pdfBase64 = null);
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly string _apiKey;
        private readonly string _whatsappNumber;
        private readonly string _gatewayUrl;

        public WhatsAppService()
        {
            // PROD: Read from appsettings.json dynamically or config tables.
            _apiKey = "MOCK_GUPSHUP_API_KEY_394857329485";
            _whatsappNumber = "+919999988888";
            _gatewayUrl = "https://api.gupshup.io/sm/api/v1/msg";
        }

        public async Task<bool> SendPrescriptionWhatsAppAsync(string patientName, string phoneNumber, string prescriptionDetails, string pdfBase64 = null)
        {
            try
            {
                // In production, you would construct a POST request to your Indian WhatsApp gateway (e.g. Gupshup / SMS-Alert / Twilio)
                // string url = $"{_gatewayUrl}?apikey={_apiKey}&to={phoneNumber}&message=Dear {patientName}, your prescription is attached.";
                
                Console.WriteLine($"[WhatsApp Integration] Dispatching Prescription to {phoneNumber} ({patientName})");
                Console.WriteLine($"[WhatsApp Details] {prescriptionDetails}");
                if (!string.IsNullOrEmpty(pdfBase64))
                {
                    Console.WriteLine("[WhatsApp Attachment] Attachment PDF stream transmitted successfully.");
                }

                // Simulate brief network delay
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp Error] Failed to send prescription to {phoneNumber}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendReportWhatsAppAsync(string patientName, string phoneNumber, string testName, string pdfUrl = null)
        {
            try
            {
                Console.WriteLine($"[WhatsApp Integration] Dispatching Lab Report ({testName}) to {phoneNumber} ({patientName})");
                if (!string.IsNullOrEmpty(pdfUrl))
                {
                    Console.WriteLine($"[WhatsApp Attachment] Download Link: {pdfUrl}");
                }

                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp Error] Failed to send lab report to {phoneNumber}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendBillingReceiptWhatsAppAsync(string patientName, string phoneNumber, double billAmount, string receiptNo, string pdfBase64 = null)
        {
            try
            {
                Console.WriteLine($"[WhatsApp Integration] Dispatching Invoicing Receipt {receiptNo} to {phoneNumber} ({patientName})");
                Console.WriteLine($"[WhatsApp Amount] Total Billed Amount: INR {billAmount:N2}");

                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsApp Error] Failed to send invoice receipt to {phoneNumber}: {ex.Message}");
                return false;
            }
        }
    }
}
