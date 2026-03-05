namespace Domain.Enums.Entities;

public enum OrderStatus
{
    Cart,
    LogisticsPending,
    AwaitingPayment,
    PaymentApproved,
    PaymentRejected,
    Delivered,
    Cancelled
}