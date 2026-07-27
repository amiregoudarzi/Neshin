namespace Neshin.Domain.Ordering;

public enum OrderStatus
{
    Draft = 1,
    AwaitingPayment = 2,
    Paid = 3,
    Accepted = 4,
    Preparing = 5,
    Ready = 6,
    Completed = 7,
    PaymentFailed = 8,
    Cancelled = 9,
    Submitted = 10,
    Rejected = 11
}
