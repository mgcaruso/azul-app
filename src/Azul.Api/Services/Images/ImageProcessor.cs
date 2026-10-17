using Azul.Api.Common.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Azul.Api.Services.Images;

public record ProcessedPhoto(MemoryStream Normal, MemoryStream Thumb);

public class ImageProcessor
{
    private static readonly string[] AllowedFormats = ["JPEG", "PNG", "WEBP"];
    private const int MaxSide = 6000;
    private const int NormalSize = 512;
    private const int ThumbSize = 128;

    public async Task<ProcessedPhoto> ProcessAsync(Stream input, CancellationToken ct)
    {
        ImageInfo info;
        try
        {
            info = await Image.IdentifyAsync(input, ct);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
        {
            throw InvalidPhoto("La foto tiene que ser JPG, PNG o WebP.");
        }

        if (!AllowedFormats.Contains(info.Metadata.DecodedImageFormat?.Name))
        {
            throw InvalidPhoto("La foto tiene que ser JPG, PNG o WebP.");
        }

        if (info.Width > MaxSide || info.Height > MaxSide)
        {
            throw InvalidPhoto("La foto es demasiado grande.");
        }

        input.Position = 0;
        using var image = await LoadAsync(input, ct);
        image.Mutate(x => x.AutoOrient());
        image.Metadata.ExifProfile = null;

        return new ProcessedPhoto(
            await ToWebpAsync(image, NormalSize, ct),
            await ToWebpAsync(image, ThumbSize, ct));
    }

    private static async Task<Image> LoadAsync(Stream input, CancellationToken ct)
    {
        try
        {
            return await Image.LoadAsync(input, ct);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
        {
            throw InvalidPhoto("La foto está dañada o no se puede leer.");
        }
    }

    private static async Task<MemoryStream> ToWebpAsync(Image image, int size, CancellationToken ct)
    {
        using var copy = image.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Crop
        }));
        var stream = new MemoryStream();
        await copy.SaveAsWebpAsync(stream, new WebpEncoder { Quality = 80 }, ct);
        stream.Position = 0;
        return stream;
    }

    private static ValidationFailedException InvalidPhoto(string message)
        => ValidationFailedException.ForField("photo", message);
}
