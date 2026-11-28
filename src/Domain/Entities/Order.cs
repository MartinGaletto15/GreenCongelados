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

    protected Order()
    {
        ShippingStreet = string.Empty;
    }

    private Order(int userId, string street, string? dpto, string? reference, decimal shippingCost)
    {
        IdUser = userId;
        ShippingStreet = street;
        ShippingDpto = dpto;
        ShippingReference = reference;
        ShippingCost = shippingCost;
        OrderDate = DateTime.UtcNow;
        OrderStatus = OrderStatus.LogisticsPending;
    }


    public static Order Create(
        int userId, string street, string? dpto, string? reference,
        decimal baseShippingCost,
        IEnumerable<(int IdProduct, decimal UnitPrice, int Quantity)> items,
        Promotion? promotion)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0) throw new AppValidationException("La orden debe tener al menos un producto.", "EMPTY_ORDER");
        if (string.IsNullOrWhiteSpace(street)) throw new AppValidationException("La calle de envío no puede estar vacía.", "INVALID_SHIPPING_STREET");
        if (baseShippingCost < 0) throw new AppValidationException("El costo de envío no puede ser negativo.", "INVALID_SHIPPING_COST");

        var order = new Order(userId, street, dpto, reference, baseShippingCost);

        order.Subtotal = itemList.Sum(x => x.UnitPrice * x.Quantity);
        order.ApplyPromotion(promotion);

        foreach (var (idProduct, unitPrice, quantity) in itemList)
            order._orderItems.Add(new OrderItem(idProduct, quantity, unitPrice)); // ya valida quantity

        order.CalculateTotal();
        return order;
    }

    public void Cancel()
    {
        if (OrderStatus == OrderStatus.Delivered)
            throw new AppValidationException("No se puede cancelar una orden entregada.", "INVALID_STATUS_TRANSITION");

        if (OrderStatus == OrderStatus.Cancelled)
            return;

        OrderStatus = OrderStatus.Cancelled;
    }

    public void AssignCourier(int courierId)
    {
        if (courierId <= 0)
            throw new AppValidationException("El cadete debe ser válido.", "INVALID_COURIER");

        if (OrderStatus != OrderStatus.LogisticsPending)
            throw new AppValidationException("La orden debe estar en estado pendiente de logística para asignar un cadete.", "INVALID_STATUS_TRANSITION");

        IdCourier = courierId;
        OrderStatus = OrderStatus.InDelivery;
    }

    public void MarkAsDelivered()
    {
        if (OrderStatus != OrderStatus.InDelivery)
            throw new AppValidationException("La orden debe estar en estado en camino para marcarse como entregada.", "INVALID_STATUS_TRANSITION");

        OrderStatus = OrderStatus.Delivered;
    }

    public void ReturnToLogisticsPending()
    {
        if (OrderStatus == OrderStatus.Delivered)
            throw new AppValidationException("No se puede devolver a logística una orden entregada.", "INVALID_STATUS_TRANSITION");

        if (OrderStatus == OrderStatus.Cancelled)
            throw new AppValidationException("No se puede devolver a logística una orden cancelada.", "INVALID_STATUS_TRANSITION");

        if (OrderStatus == OrderStatus.LogisticsPending)
            return;

        // The order goes back to the logistics queue and must be reassigned.
        IdCourier = null;
        OrderStatus = OrderStatus.LogisticsPending;
    }

    public void UpdateShippingDetails(string? street, string? dpto, string? reference)
    {
        if (OrderStatus == OrderStatus.Delivered || OrderStatus == OrderStatus.Cancelled)
            throw new AppValidationException("No se pueden modificar los datos de envío de una orden completada o cancelada.", "INVALID_OPERATION");

        if (street != null && string.IsNullOrWhiteSpace(street))
            throw new AppValidationException("La calle de envío no puede estar vacía.", "INVALID_SHIPPING_STREET");

        ShippingStreet = street ?? ShippingStreet;
        ShippingDpto = dpto ?? ShippingDpto;
        ShippingReference = reference ?? ShippingReference;
    }

    private void ApplyPromotion(Promotion? promotion)
    {
        if (promotion == null) return;

        var discount = promotion.CalculateDiscount(Subtotal, ShippingCost);

        IdPromotion = promotion.IdPromotion;
        GlobalDiscount = discount;
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
                    throw new AppValidationException("No se puede pasar una orden a estado en camino desde su estado actual.", "INVALID_STATUS_TRANSITION");
                if (IdCourier is null or <= 0)
                    throw new AppValidationException("La orden debe tener un cadete asignado para pasar a estado en camino.", "COURIER_NOT_ASSIGNED");
                OrderStatus = OrderStatus.InDelivery;
                break;
            case OrderStatus.LogisticsPending:
                ReturnToLogisticsPending();
                break;
            default:
                throw new AppValidationException("Estado de orden inválido.", "INVALID_ORDER_STATUS");
        }
    }
}
