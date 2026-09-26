using System.ComponentModel.DataAnnotations;

namespace RideHailingAPI.DTOs.Ride;

public class CreateRideRequest
{
    [Required]
    public string PickupLocation { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;
}