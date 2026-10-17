namespace Azul.Api.Services.Images;

public class LocalImageStorage(IWebHostEnvironment environment) : IImageStorage
{
    private readonly string root = Path.Combine(environment.WebRootPath, "photos");

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var path = Path.Combine(root, key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, ct);
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        File.Delete(Path.Combine(root, key));
        return Task.CompletedTask;
    }
}
