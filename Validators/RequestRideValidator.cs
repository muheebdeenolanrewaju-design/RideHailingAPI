using FluentValidation;
using RideHailingAPI.DTOs.Passenger;

namespace RideHailingAPI.Validators;

public class RequestRideValidator : AbstractValidator<CreateRideRequestDto>
{
    public RequestRideValidator()
    {
        RuleFor(x => x.PickupLocation)
            .NotEmpty().WithMessage("Pickup location is required.")
            .MaximumLength(250).WithMessage("Pickup location cannot exceed 250 characters.");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.")
            .MaximumLength(250).WithMessage("Destination cannot exceed 250 characters.");

        RuleFor(x => x.DistanceInKm)
            .GreaterThan(0).WithMessage("Distance must be greater than zero kilometers.");
    }
}