namespace WebMatcha.Services;

public interface IPhotoService
{
    Task<string> UploadPhotoAsync(Stream photoStream, string fileName, int userId);
    Task<bool> DeletePhotoAsync(string photoUrl, int userId);
    bool ValidatePhoto(Stream photoStream, string fileName);
}

public class PhotoService : IPhotoService
{
    private readonly string _uploadPath;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    // Magic numbers (file signatures) for image validation
    private static readonly Dictionary<string, byte[][]> ImageSignatures = new()
    {
        { "image/jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { "image/png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { "image/gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } } },
        { "image/webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }
    };

    public PhotoService(IWebHostEnvironment env)
    {
        _uploadPath = Path.Combine(env.WebRootPath, "uploads", "photos");
        Directory.CreateDirectory(_uploadPath);
    }
    
    public async Task<string> UploadPhotoAsync(Stream photoStream, string fileName, int userId)
    {
        if (!ValidatePhoto(photoStream, fileName))
        {
            throw new InvalidOperationException("Invalid photo file");
        }
        
        // Generate unique filename
        var extension = Path.GetExtension(fileName).ToLower();
        var uniqueFileName = $"{userId}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);
        
        // Reset stream position
        photoStream.Position = 0;
        
        // Save file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await photoStream.CopyToAsync(fileStream);
        }
        
        // Return relative URL
        return $"/uploads/photos/{uniqueFileName}";
    }
    
    public Task<bool> DeletePhotoAsync(string photoUrl, int userId)
    {
        if (string.IsNullOrEmpty(photoUrl) || !photoUrl.StartsWith("/uploads/photos/"))
        {
            return Task.FromResult(false);
        }
        
        var fileName = Path.GetFileName(photoUrl);
        if (!fileName.StartsWith($"{userId}_"))
        {
            return Task.FromResult(false); // Security check - user can only delete their own photos
        }
        
        var filePath = Path.Combine(_uploadPath, fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }
    
    public bool ValidatePhoto(Stream photoStream, string fileName)
    {
        // Check file extension
        var extension = Path.GetExtension(fileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
        {
            return false;
        }

        // Check file size
        if (photoStream.Length == 0 || photoStream.Length > _maxFileSize)
        {
            return false;
        }

        // Validate file signature (magic numbers) to prevent fake extensions
        if (!ValidateImageSignature(photoStream))
        {
            return false;
        }

        return true;
    }

    private bool ValidateImageSignature(Stream stream)
    {
        try
        {
            stream.Position = 0;
            var headerBytes = new byte[8];
            var bytesRead = stream.Read(headerBytes, 0, headerBytes.Length);
            stream.Position = 0;

            if (bytesRead == 0)
                return false;

            // Check against known image signatures
            foreach (var signatures in ImageSignatures.Values)
            {
                foreach (var signature in signatures)
                {
                    if (headerBytes.Take(signature.Length).SequenceEqual(signature))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}