using FluentValidation;

namespace App.Bookings;

public sealed class BookingCreateValidator : AbstractValidator<BookingCreateDto>
{
    public BookingCreateValidator()
    {
        RuleFor(x => x.OwnerNic).NotEmpty().Length(10, 12);
        RuleFor(x => x.StationId).NotEmpty();
        RuleFor(x => x.SlotId).NotEmpty();
        RuleFor(x => x.StartTimeUtc).NotEmpty();
    }
}

public sealed class BookingUpdateValidator : AbstractValidator<BookingUpdateDto>
{
    public BookingUpdateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.StationId).NotEmpty();
        RuleFor(x => x.SlotId).NotEmpty();
        RuleFor(x => x.StartTimeUtc).NotEmpty();
    }
}
