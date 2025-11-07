using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace FoodVault.Services;

public sealed class AzureBlobService
{
	private readonly BlobServiceClient _client;
	private readonly string _cdnBase;

	public AzureBlobService(IConfiguration cfg)
	{
		_client = new BlobServiceClient(cfg.GetConnectionString("AzureBlobStorage") ?? cfg["Azure:Blob:ConnectionString"]);
		_cdnBase = cfg["Azure:Blob:CdnBase"] ?? string.Empty;
	}

	public async Task<(string Url, string Path)> UploadAsync(string container, string path, Stream stream, string contentType, CancellationToken ct = default)
	{
		var cont = _client.GetBlobContainerClient(container);
		await cont.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
		var blob = cont.GetBlobClient(path);
		await blob.UploadAsync(stream, new BlobHttpHeaders{ ContentType = contentType }, cancellationToken: ct);
		var url = string.IsNullOrEmpty(_cdnBase) ? blob.Uri.ToString() : _cdnBase.TrimEnd('/') + "/" + container + "/" + path;
		return (url, path);
	}

	public async Task<bool> DeleteAsync(string container, string path, CancellationToken ct = default)
	{
		var cont = _client.GetBlobContainerClient(container);
		var blob = cont.GetBlobClient(path);
		var resp = await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
		return resp.Value;
	}

	public string GetSignedUrl(string container, string path, TimeSpan ttl)
	{
		var cont = _client.GetBlobContainerClient(container);
		var blob = cont.GetBlobClient(path);
		if (!blob.CanGenerateSasUri) return blob.Uri.ToString();
		var sas = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(ttl))
		{ BlobContainerName = container, BlobName = path };
		return blob.GenerateSasUri(sas).ToString();
	}
}


