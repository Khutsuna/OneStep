using OneStep.Application.Common;
using OneStep.Application.Features.Checkout;

namespace OneStep.Application.Features.Orders;

public interface IOrderService
{
    Task<OrderTotals> QuoteAsync(QuoteRequest request, CancellationToken cancellationToken);
    Task<OrderDetailsDto> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken);
    Task<PagedResult<OrderSummaryDto>> GetOrdersAsync(OrderListRequest request, CancellationToken cancellationToken);
    Task<OrderDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OrderDetailsDto> ChangeStatusAsync(int id, ChangeOrderStatusRequest request, CancellationToken cancellationToken);
    Task<OrderDetailsDto> CancelAsync(int id, CancellationToken cancellationToken);
    Task<OrderDetailsDto> PayAsync(int id, CancellationToken cancellationToken);
}
