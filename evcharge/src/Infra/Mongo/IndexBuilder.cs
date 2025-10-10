// indexBuilder.cs
using MongoDB.Bson;
using MongoDB.Driver;
using Domain.Users;

namespace Infra.Mongo;

public static class IndexBuilder
{
    public static async Task EnsureAsync(IMongoDatabase db)
    {
        // users: unique email (non-empty), partial 
        var users = db.GetCollection<User>("users");
        const string emailField = "email";
        const string userEmailIndexName = "ux_user_email";

        // Drop existing index with that name 
        var existing = await users.Indexes.ListAsync();
        var existingList = await existing.ToListAsync();
        if (existingList.Any(ix => ix.GetValue("name", "").AsString == userEmailIndexName))
        {
            await users.Indexes.DropOneAsync(userEmailIndexName);
        }

        var partial = Builders<User>.Filter.And(
            Builders<User>.Filter.Exists(emailField, true),
            Builders<User>.Filter.Type(emailField, BsonType.String),
            Builders<User>.Filter.Gt(emailField, "") // non-empty
        );

        var userIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(emailField),
            new CreateIndexOptions<User>
            {
                Name = userEmailIndexName,
                Unique = true,
                PartialFilterExpression = partial
            });

        await users.Indexes.CreateOneAsync(userIndex);


        // ev_owners: unique nic
        var owners = db.GetCollection<dynamic>("ev_owners");
        await owners.Indexes.CreateOneAsync(new CreateIndexModel<dynamic>(
            Builders<dynamic>.IndexKeys.Ascending("nic"),
            new CreateIndexOptions { Unique = true, Name = "ux_evowner_nic" }
        ));

        var stations = db.GetCollection<dynamic>("stations");
        await stations.Indexes.CreateOneAsync(new CreateIndexModel<dynamic>(
            Builders<dynamic>.IndexKeys.Ascending("lat").Ascending("lng"),
            new CreateIndexOptions { Name = "ix_station_latlng" }
        ));

        // bookings: compound on station + start time + status
        var bookings = db.GetCollection<dynamic>("bookings");
        await bookings.Indexes.CreateOneAsync(new CreateIndexModel<dynamic>(
            Builders<dynamic>.IndexKeys
                .Ascending("stationId")
                .Ascending("startTimeUtc")
                .Ascending("status"),
            new CreateIndexOptions { Name = "ix_booking_station_start_status" }
        ));

        // schedules: unique per (stationId, slotId, startTimeUtc)
        var schedules = db.GetCollection<dynamic>("schedules");
        await schedules.Indexes.CreateOneAsync(new CreateIndexModel<dynamic>(
            Builders<dynamic>.IndexKeys
                .Ascending("stationId")
                .Ascending("slotId")
                .Ascending("startTimeUtc"),
            new CreateIndexOptions { Name = "ux_schedule_slot_time", Unique = true }
        ));

        var qr = db.GetCollection<dynamic>("qr_tokens");
        await qr.Indexes.CreateOneAsync(new CreateIndexModel<dynamic>(
            Builders<dynamic>.IndexKeys.Ascending("token"),
            new CreateIndexOptions { Unique = true, Name = "ux_qr_token" }
        ));
        
        await qr.Indexes.CreateOneAsync(new CreateIndexModel<dynamic>(
            Builders<dynamic>.IndexKeys.Ascending("expUtc"),
            new CreateIndexOptions { Name = "ttl_qr_exp", ExpireAfter = TimeSpan.Zero }
        ));

    }
}
