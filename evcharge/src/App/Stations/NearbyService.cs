using Infra.Stations;

namespace App.Stations;

public interface INearbyStations
{
    Task<List<StationView>> FindAsync(double lat, double lng, double radiusKm);
}

public sealed class NearbyStations : INearbyStations
{
    private readonly IStationRepository _repo;
    public NearbyStations(IStationRepository repo) => _repo = repo;

    private static double HaversineKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371.0; // km
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLng = (lng2 - lng1) * Math.PI / 180.0;
        double a = Math.Sin(dLat/2)*Math.Sin(dLat/2) +
                   Math.Cos(lat1*Math.PI/180.0)*Math.Cos(lat2*Math.PI/180.0) *
                   Math.Sin(dLng/2)*Math.Sin(dLng/2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
    }

    public async Task<List<StationView>> FindAsync(double lat, double lng, double radiusKm)
    {
        var all = await _repo.GetAllAsync();
        var nearby = all
            .Where(s => s.Active)
            .Select(s => new
            {
                S = s,
                D = HaversineKm(lat, lng, s.Lat, s.Lng)
            })
            .Where(x => x.D <= radiusKm)
            .OrderBy(x => x.D)
            .Select(x => new StationView(
                x.S.Id, x.S.Name, x.S.Type.ToString(), x.S.Active, x.S.Lat, x.S.Lng,
                x.S.Slots.Select(sl => new StationSlotDto(sl.SlotId, sl.Label)).ToList()
            ))
            .ToList();

        return nearby;
    }
}
