using FluentValidation;

namespace BirthdayReminder.Application.DTOs.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Birthday)
            .NotEmpty()
            .WithMessage("Birthday is required.")
            .Must(x => x <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birthday cannot be greater than today.");

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("Time zone is required.")
            .Must(x => TimeZoneInfo.FindSystemTimeZoneById(x) != null)
            .WithMessage("Invalid time zone.");
    }
}