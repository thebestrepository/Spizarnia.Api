namespace Spizarnia.Domain.Entities;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}

public enum OrderStatus
{
    Pending,
    Created,
    PickedUp,
    Delivered,
    Canceled,
    Failed
}
