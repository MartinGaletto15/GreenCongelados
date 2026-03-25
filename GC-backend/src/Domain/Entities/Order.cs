using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Order
{
    [Key]
    public int IdOrder { get; private set; }

    [Required]
    public int IdUser { get; private set; }
    public User? User { get; private set; }

    public int? IdCourier { get; private set; }
    public User? Courier { get; private set; }

    public int? IdPromotion { get; private set; }
    public Promotion? Promotion { get; private set; }

    [Required] 
    [Column(TypeName = "decimal(18, 2)")]
    public decimal ShippingCost { get; private set; }

    [Required] 
    public string ShippingStreet { get; private set; }

    public string? ShippingDpto { get; private set; }

    public string? ShippingReference { get; private set; }

    [Required]
    public DateTime OrderDate { get; private set; }

    public decimal GlobalDiscount { get; private set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; private set; }

    [Required]
    public OrderStatus OrderStatus { get; private set; } 
    
    private readonly List<OrderItem> _orderItems = new();
    public ICollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    protected Order() {
        ShippingStreet = string.Empty;
    }

    private Order(int userId, string street, string? dpto, string? reference, decimal shippingCost)
    {
        IdUser = userId;
        ShippingStreet = street;
        ShippingDpto = dpto;
        ShippingReference = reference;
        ShippingCost = shippingCost;
        OrderDate = DateTime.Now;
        OrderStatus = OrderStatus.LogisticsPending;
    }

    public static Order Create(int userId, string street, string? dpto, string? reference, decimal baseShippingCost, IEnumerable<(Product Product, int Quantity)> items, Promotion? promotion)
    {
        if (!items.Any()) throw new AppValidationException("Order must have at least one item", "EMPTY_ORDER");

        var order = new Order(userId, street, dpto, reference, baseShippingCost);

        decimal subtotal = 0;
        foreach (var (product, quantity) in items)
        {
            product.ReduceStock(quantity);
            var orderItem = new OrderItem(product.IdProduct, quantity, product.Price);
            order._orderItems.Add(orderItem);
            subtotal += product.Price * quantity;
        }

        order.Subtotal = subtotal;
        order.ApplyPromotion(promotion);
        order.CalculateTotal();

        return order;
    }

    public void Cancel()
    {
        if (OrderStatus == OrderStatus.Delivered)
            throw new AppValidationException("Cannot cancel a delivered order", "INVALID_STATUS_TRANSITION");

        if (OrderStatus == OrderStatus.Cancelled)
            return;

        OrderStatus = OrderStatus.Cancelled;
    }

    public void AssignCourier(int courierId)
    {
        if (OrderStatus != OrderStatus.LogisticsPending)
            throw new AppValidationException("Order must be in LogisticsPending to assign a courier", "INVALID_STATUS_TRANSITION");

        IdCourier = courierId;
        OrderStatus = OrderStatus.InDelivery;
    }

    public void MarkAsDelivered()
    {
        if (OrderStatus != OrderStatus.InDelivery)
            throw new AppValidationException("Order must be InDelivery to mark as delivered", "INVALID_STATUS_TRANSITION");

        OrderStatus = OrderStatus.Delivered;
    }

    public void UpdateShippingDetails(string? street, string? dpto, string? reference)
    {
        if (OrderStatus == OrderStatus.Delivered || OrderStatus == OrderStatus.Cancelled)
            throw new AppValidationException("Cannot update shipping details of a completed or cancelled order", "INVALID_OPERATION");

        ShippingStreet = street ?? ShippingStreet;
        ShippingDpto = dpto ?? ShippingDpto;
        ShippingReference = reference ?? ShippingReference;
    }

    private void ApplyPromotion(Promotion? promotion)
    {
        if (promotion == null) return;

        var now = DateTime.Now;
        if (now < promotion.StartDate || now > promotion.EndDate)
            throw new AppValidationException("The promotion coupon has expired or is not yet valid", "PROMOTION_EXPIRED");

        if (promotion.MinAmount.HasValue && Subtotal < promotion.MinAmount.Value)
            throw new AppValidationException($"Min amount for promotion is {promotion.MinAmount.Value}", "PROMOTION_MIN_AMOUNT");

        IdPromotion = promotion.IdPromotion;

        switch (promotion.DiscountType)
        {
            case DiscountType.Percentage:
                GlobalDiscount = Subtotal * (promotion.DiscountValue / 100);
                break;
            case DiscountType.FixedAmount:
                GlobalDiscount = promotion.DiscountValue;
                break;
            case DiscountType.FreeShipping:
                GlobalDiscount = ShippingCost;
                break;
        }
    }

    private void CalculateTotal()
    {
        Total = Subtotal + ShippingCost - GlobalDiscount;
        if (Total < 0) Total = 0;
    }
    public void UpdateStatus(OrderStatus newStatus)
    {
        switch (newStatus)
        {
            case OrderStatus.Cancelled:
                Cancel();
                break;
            case OrderStatus.Delivered:
                MarkAsDelivered();
                break;
            case OrderStatus.InDelivery:
                if (OrderStatus != OrderStatus.LogisticsPending && OrderStatus != OrderStatus.InDelivery)
                    throw new AppValidationException("Invalid status transition to InDelivery", "INVALID_STATUS_TRANSITION");
                OrderStatus = OrderStatus.InDelivery;
                break;
            case OrderStatus.LogisticsPending:
                if (OrderStatus != OrderStatus.LogisticsPending)
                    throw new AppValidationException("Cannot transition back to LogisticsPending", "INVALID_STATUS_TRANSITION");
                break;
        }
    }
}
