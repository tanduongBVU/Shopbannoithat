using Shopbannoithat.Models;

namespace Shopbannoithat.Services
{
    public interface IEmailService
    {
        Task SendOrderInvoiceAsync(Order order);
    }
}