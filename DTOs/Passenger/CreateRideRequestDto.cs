namespace RideHailingAPI.DTOs.Passenger;

public class CreateRideRequestDto
{
    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal DistanceInKm { get; set; }
}