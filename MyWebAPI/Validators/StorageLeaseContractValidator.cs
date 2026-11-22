using FluentValidation;
using MyWebAPI.Dtos;

namespace MyWebAPI.Validators
{
    public class StorageLeaseContractCreateDtoValidator : AbstractValidator<StorageLeaseContractCreateDto>
    {
        public StorageLeaseContractCreateDtoValidator()
        {
            RuleFor(c => c.StorageBoxId)
                .NotEmpty().WithMessage("StorageBoxId is required.")
                .Matches("^[a-fA-F0-9]{24}$").WithMessage("StorageBoxId must be a valid 24-character hex ID.");

            RuleFor(c => c.StorageTenantId)
                .NotEmpty().WithMessage("StorageTenantId is required.")
                .Matches("^[a-fA-F0-9]{24}$").WithMessage("StorageTenantId must be a valid 24-character hex ID.");

            RuleFor(c => c.DurationDays)
                .NotEmpty().WithMessage("Duration is required.")
                .GreaterThanOrEqualTo(30).WithMessage("Minimum lease duration is 30 calendar days.")
                .LessThanOrEqualTo(3650).WithMessage("Lease duration cannot exceed 10 years (3650 days).");
        }
    }

    public class StorageLeaseContractPatchDtoValidator : AbstractValidator<StorageLeaseContractPatchDto>
    {
        public StorageLeaseContractPatchDtoValidator()
        {
            RuleFor(c => c.StorageBoxId)
                .NotEmpty().WithMessage("StorageBoxId cannot be empty.")
                .Matches("^[a-fA-F0-9]{24}$").WithMessage("StorageBoxId must be a valid 24-character hex ID.")
                .When(c => !string.IsNullOrEmpty(c.StorageBoxId));

            RuleFor(c => c.StorageTenantId)
                .NotEmpty().WithMessage("StorageTenantId cannot be empty.")
                .Matches("^[a-fA-F0-9]{24}$").WithMessage("StorageTenantId must be a valid 24-character hex ID.")
                .When(c => !string.IsNullOrEmpty(c.StorageTenantId));

            RuleFor(c => c.DurationDays!.Value)
                .GreaterThanOrEqualTo(30).WithMessage("Minimum lease duration is 30 calendar days.")
                .LessThanOrEqualTo(3650).WithMessage("Lease duration cannot exceed 10 years (3650 days).")
                .When(c => c.DurationDays.HasValue);
        }
    }
}