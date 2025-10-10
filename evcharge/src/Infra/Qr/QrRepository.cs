// QrRepository.cs
using Domain.Qr;
using MongoDB.Driver;
using Infra.Mongo;

namespace Infra.Qr;

public interface IQrRepository
{
    Task InsertAsync(QrToken t);
    Task<QrToken?> FindByTokenAsync(string token);
    Task DeleteByIdAsync(string id);
}

public sealed class QrRepository : IQrRepository
{
    private readonly IMongoCollection<QrToken> _col;
    public QrRepository(IMongoContext ctx) => _col = ctx.GetCollection<QrToken>("qr_tokens");

    public Task InsertAsync(QrToken t) => _col.InsertOneAsync(t);

    public async Task<QrToken?> FindByTokenAsync(string token) =>
        await _col.Find(x => x.Token == token).FirstOrDefaultAsync();

    public Task DeleteByIdAsync(string id) =>
        _col.DeleteOneAsync(x => x.Id == id);
}
