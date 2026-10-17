namespace Azul.Api.Services.Images;

public interface IImageStorage
{
    Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
