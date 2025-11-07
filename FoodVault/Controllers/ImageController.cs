using FoodVault.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.Controllers;

[ApiController]
[Route("api/images")]
public sealed class ImageController : ControllerBase
{
	private static readonly string[] Allowed = new[]{"image/jpeg","image/png","image/webp"};
	private const long MaxSize = 5L * 1024 * 1024;
	private readonly AzureBlobService _blob;

	public ImageController(AzureBlobService blob)
	{
		_blob = blob;
	}

	[HttpPost("upload")]
	[Authorize]
	[RequestSizeLimit(MaxSize * 6)]
	public async Task<IActionResult> Upload([FromQuery] string container = "uploads", [FromQuery] string prefix = "", CancellationToken ct = default)
	{
		if (Request.Form?.Files == null || Request.Form.Files.Count == 0) return BadRequest("No files");
		var results = new List<object>();
		foreach (var f in Request.Form.Files)
		{
			if (f.Length == 0 || f.Length > MaxSize) return BadRequest("File too large");
			if (!Allowed.Contains(f.ContentType)) return BadRequest("Invalid type");
			var name = Guid.NewGuid().ToString("n") + Path.GetExtension(f.FileName).ToLowerInvariant();
			var path = string.IsNullOrWhiteSpace(prefix) ? name : ($"{prefix.TrimEnd('/')}/{name}");
			await using var s = f.OpenReadStream();
			var (url, savedPath) = await _blob.UploadAsync(container, path, s, f.ContentType, ct);
			results.Add(new { url, path = savedPath });
		}
		return Ok(new { files = results });
	}

	[HttpDelete]
	[Authorize]
	public async Task<IActionResult> Delete([FromQuery] string container, [FromQuery] string path, CancellationToken ct = default)
	{
		if (string.IsNullOrEmpty(container) || string.IsNullOrEmpty(path)) return BadRequest();
		var ok = await _blob.DeleteAsync(container, path, ct);
		return ok ? NoContent() : NotFound();
	}
}


