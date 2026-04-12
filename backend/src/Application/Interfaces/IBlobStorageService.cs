namespace Application.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken cancellationToken = default);
        Task<string> GetUrlAsync(string blobPath, CancellationToken cancellationToken = default);
        Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default);
    }
}
