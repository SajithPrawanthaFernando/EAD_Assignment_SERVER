namespace Infra.Mongo;
public sealed class MongoSettings
{
    public string ConnectionString { get; init; } = "";
    public string Database { get; init; } = "";
}
