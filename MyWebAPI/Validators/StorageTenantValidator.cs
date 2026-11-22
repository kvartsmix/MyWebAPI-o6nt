using FluentValidation;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace MyWebAPI.Validators
{
    public class StorageTenantCreateDtoValidator : AbstractValidator<StorageTenantCreateDto>
    {
        public StorageTenantCreateDtoValidator()
        {
            RuleFor(t => t.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters long.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(t => t.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+380\d{9}$").WithMessage("Phone number must be in the format +380XXXXXXXXX.");

            RuleFor(t => t.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(t => t.Type)
                .IsInEnum().WithMessage("Invalid tenant type.");
        }
    }

    public class StorageTenantPatchDtoValidator : AbstractValidator<StorageTenantPatchDto>
    {
        public StorageTenantPatchDtoValidator()
        {
            RuleFor(t => t.FullName)
                .NotEmpty().WithMessage("Full name cannot be empty.")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters long.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.")
                .When(t => t.FullName != null);

            RuleFor(t => t.PhoneNumber)
                .Matches(@"^\+380\d{9}$").WithMessage("Phone number must be in the format +380XXXXXXXXX.")
                .When(t => !string.IsNullOrEmpty(t.PhoneNumber));

            RuleFor(t => t.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .When(t => !string.IsNullOrEmpty(t.Email));

            RuleFor(t => t.Type)
                .IsInEnum().WithMessage("Invalid tenant type.")
                .When(t => t.Type.HasValue);
        }
    }
}