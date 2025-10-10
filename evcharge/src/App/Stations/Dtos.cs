// StationDtos.cs
using Domain.Stations;

namespace App.Stations;

public sealed record StationCreateDto(string Name, StationType Type, double Lat, double Lng, List<StationSlotDto> Slots);
public sealed record StationUpdateDto(string Id, string Name, StationType Type, double Lat, double Lng, List<StationSlotDto> Slots);
public sealed record StationSlotDto(string SlotId, string Label, bool Available);
public sealed record StationView(string Id, string Name, string Type, bool Active, double Lat, double Lng, List<StationSlotDto> Slots);
