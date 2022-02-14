using ItServiceApp.Core.Payment;

namespace ItServiceApp.Business.Services.Payment
{
    public interface IPaymentService
    {
        InstallmentModel CheckInstallments(string binNumber,decimal price);
        PaymentResponseModel Pay(PaymentModel model);
    }
}
