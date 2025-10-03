using FluentValidation;

namespace App.EvOwners;

public sealed class EvOwnerUpsertValidator : AbstractValidator<EvOwnerUpsertDto>
{
    public EvOwnerUpsertValidator()
    {
        RuleFor(x => x.Nic).NotEmpty().Length(10, 12);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
    }
}
