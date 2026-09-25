namespace RideHailingAPI.Domain.Enum;

public enum RideStatus
{
    Requested = 1,
    Accepted = 2,
    DriverArriving = 3,
    DriverArrived = 4,
    InProgress = 5,
    Completed = 6,
    Cancelled = 7
}