using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models; // Đảm bảo namespace này khớp

namespace VoTrongHung2280601119.Repositories // ĐẢM BẢO NAMESPACE NÀY CHÍNH XÁC
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}