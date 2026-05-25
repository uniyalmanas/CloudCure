using System;
using System.Threading.Tasks;
using QRCoder;

namespace DanpheEMR.Core.Integrations
{
    public interface IPaymentGatewayService
    {
        Task<string> GenerateUPIReciptQR(double amount, string transactionId, string clinicUPIId = "curecloud@okaxis");
        Task<string> CreateRazorpayOrderAsync(double amount, string receiptId);
    }

    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly string _razorpayKeyId;
        private readonly string _razorpayKeySecret;

        public PaymentGatewayService()
        {
            _razorpayKeyId = "rzp_test_MOCK_KEY_ID_3829472";
            _razorpayKeySecret = "MOCK_SECRET_38495738927489";
        }

        public async Task<string> GenerateUPIReciptQR(double amount, string transactionId, string clinicUPIId = "curecloud@okaxis")
        {
            try
            {
                // In India, a standard UPI payment QR string follows the schema:
                // upi://pay?pa=PAYEE_ADDRESS&pn=PAYEE_NAME&am=AMOUNT&tn=TRANSACTION_NOTE
                string payeeName = Uri.EscapeDataString("CureCloud Clinic");
                string note = Uri.EscapeDataString($"Bill-{transactionId}");
                string upiUrl = $"upi://pay?pa={clinicUPIId}&pn={payeeName}&am={amount:F2}&tn={note}&cu=INR";

                Console.WriteLine($"[Payment Gateway] Generated UPI Payment URI: {upiUrl}");

                // Generate actual QR Code using QRCoder (completely cross-platform PngByteQRCode)
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(upiUrl, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
                    string realBase64Image = "data:image/png;base64," + Convert.ToBase64String(qrCodeAsPngByteArr);
                    
                    await Task.Delay(10);
                    return realBase64Image;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Payment Gateway Error] Failed to generate UPI QR: {ex.Message}");
                return null;
            }
        }

        public async Task<string> CreateRazorpayOrderAsync(double amount, string receiptId)
        {
            try
            {
                // In a production Razorpay integration, you would construct a POST request to:
                // https://api.razorpay.com/v1/orders
                // using Basic Authentication (KeyId:KeySecret) and passing the amount (in paisa, so amount * 100).

                int amountInPaisa = (int)(amount * 100);
                string orderId = $"order_MOCK_{Guid.NewGuid().ToString().Substring(0, 14)}";

                Console.WriteLine($"[Razorpay Gateway] Created Order {orderId} for Receipt {receiptId} of amount {amountInPaisa} paisa.");

                await Task.Delay(100);
                return orderId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Razorpay Error] Failed to create order: {ex.Message}");
                return null;
            }
        }
    }
}
