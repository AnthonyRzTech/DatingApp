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
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    
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
        if (photoStream.Length > _maxFileSize)
        {
            return false;
        }
        
        // Basic validation passed
        return true;
    }
}