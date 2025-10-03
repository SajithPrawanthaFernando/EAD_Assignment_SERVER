namespace Domain.Stations;

public sealed class Station
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public StationType Type { get; set; }
    public bool Active { get; set; } = true;

   
    public double Lat { get; set; }
    public double Lng { get; set; }

    public List<StationSlot> Slots { get; set; } = new(); 
}

public sealed class StationSlot
{
    public string SlotId { get; set; } = default!;
    public string Label { get; set; } = default!;
}
