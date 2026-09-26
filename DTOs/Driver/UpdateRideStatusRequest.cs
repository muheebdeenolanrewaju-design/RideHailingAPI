using System.ComponentModel.DataAnnotations;
using RideHailingAPI.Domain.Enum;

namespace RideHailingAPI.DTOs.Driver;

public class UpdateRideStatusRequest
{
    [Required]
    public RideStatus NewStatus { get; set; }
}