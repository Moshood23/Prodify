namespace Prodify.Application.Common.Interfaces;

// Where uploaded files (product photos) are kept. Today a folder on the API's
// own disk; can be swapped for cloud storage without touching the handlers.
public interface IFileStorage
{
    // Saves the file under a new random name and returns its public URL.
    Task<string> SaveAsync(Stream content, string folder, string extension, CancellationToken cancellationToken = default);

    // Deletes a file saved by SaveAsync. URLs this storage did not create
    // (e.g. a pasted link to another website) are ignored.
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}