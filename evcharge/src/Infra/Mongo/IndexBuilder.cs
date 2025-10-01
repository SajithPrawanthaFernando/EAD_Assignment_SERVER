using MongoDB.Bson;
using MongoDB.Driver;
using Domain.Users;

namespace Infra.Mongo;

public static class IndexBuilder
{
    public static async Task EnsureAsync(IMongoDatabase db)
    {
        // ----- users: unique email (non-empty), partial -----
        var users = db.GetCollection<User>("users");
        const string emailField = "email";
        const string userEmailIndexName = "ux_user_email";

        // Drop existing index with that name (definition may differ)
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

        // ----- stations: 2dsphere on geo -----
        var stations = db.GetCollection<BsonDocument>("stations");
        await stations.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Geo2DSphere("geo"),
                new CreateIndexOptions<BsonDocument> { Name = "ix_station_geo" }
            )
        );

        // ----- ev_owners: unique nic -----
        var owners = db.GetCollection<BsonDocument>("ev_owners");
        await owners.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("nic"),
                new CreateIndexOptions<BsonDocument> { Name = "ux_evowner_nic", Unique = true }
            )
        );
    }
}
