namespace Domain.Schedules;


public sealed class Schedule
{
    public string Id { get; set; } = default!;
    public string StationId { get; set; } = default!;
    public string SlotId { get; set; } = default!;
    public DateTime StartTimeUtc { get; set; }
    public bool IsAvailable { get; set; } = true;
}
