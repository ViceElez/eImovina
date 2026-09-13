using System.Security.Claims;
using eImovina.Api.Data;
using eImovina.Shared.DTOs.Equipments;
using eImovina.Shared.Models.Equipments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eImovina.Shared.Models;

namespace eImovina.Api.Controllers;

public class UploadEquipmentFileRequest
{
    public IFormFile File { get; set; } = null!;
    public string FileType { get; set; } = string.Empty;
    public bool IsCoverImage { get; set; }
}

[ApiController]
[Route("api/equipment/{equipmentId}/files")]
[Authorize]
public class EquipmentFilesController : ControllerBase
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedImageContentTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; 

    private static readonly string[] AllowedDocumentExtensions = { ".pdf" };
    private static readonly string[] AllowedDocumentContentTypes = { "application/pdf" };
    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024; 

    private readonly eImovinaDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EquipmentFilesController(eImovinaDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,InventoryManager")]
    [RequestSizeLimit(MaxDocumentSizeBytes)]
    public async Task<ActionResult<EquipmentFileDto>> UploadFile(int equipmentId, [FromForm] UploadEquipmentFileRequest request)
    {
        var equipment = await _context.Equipment.FirstOrDefaultAsync(item => item.Id == equipmentId);
        if (equipment is null)
            return NotFound("Oprema nije pronađena.");

        if (!Enum.TryParse<EquipmentFileType>(request.FileType, ignoreCase: true, out var fileType))
            return BadRequest("Nepoznat tip datoteke.");

        if (request.File is null || request.File.Length == 0)
            return BadRequest("Datoteka je obavezna.");

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var isImage = fileType == EquipmentFileType.Image;

        var allowedExtensions = isImage ? AllowedImageExtensions : AllowedDocumentExtensions;
        var allowedContentTypes = isImage ? AllowedImageContentTypes : AllowedDocumentContentTypes;
        var maxSize = isImage ? MaxImageSizeBytes : MaxDocumentSizeBytes;

        if (!allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(request.File.ContentType))
        {
            return BadRequest(isImage
                ? "Dopuštene su samo slike (jpg, png, webp)."
                : "Dopušteni su samo PDF dokumenti.");
        }

        if (request.File.Length > maxSize)
            return BadRequest($"Datoteka je prevelika. Maksimalno {maxSize / 1024 / 1024} MB.");

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "equipment");
        Directory.CreateDirectory(uploadsFolder);
        var physicalPath = Path.Combine(uploadsFolder, storedFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        if (isImage && request.IsCoverImage)
        {
            var existingCovers = await _context.EquipmentFiles
                .Where(item => item.EquipmentId == equipmentId && item.IsCoverImage)
                .ToListAsync();

            foreach (var cover in existingCovers)
                cover.IsCoverImage = false;
        }

        var equipmentFile = new EquipmentFile
        {
            EquipmentId = equipmentId,
            FileType = fileType,
            OriginalFileName = request.File.FileName,
            StoredFileName = storedFileName,
            ContentType = request.File.ContentType,
            SizeBytes = (int)request.File.Length,
            IsCoverImage = isImage && request.IsCoverImage,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = CurrentUserId
        };

        _context.EquipmentFiles.Add(equipmentFile);
        await _context.SaveChangesAsync();

        return Ok(new EquipmentFileDto
        {
            Id = equipmentFile.Id,
            EquipmentId = equipmentId,
            FileType = equipmentFile.FileType.ToString(),
            OriginalFileName = equipmentFile.OriginalFileName,
            Url = $"/uploads/equipment/{storedFileName}",
            ContentType = equipmentFile.ContentType,
            SizeBytes = equipmentFile.SizeBytes,
            IsCoverImage = equipmentFile.IsCoverImage,
            UploadedAt = equipmentFile.UploadedAt,
            UploadedBy = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty
        });
    }

    [HttpDelete("{fileId}")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> DeleteFile(int equipmentId, int fileId)
    {
        var file = await _context.EquipmentFiles
            .FirstOrDefaultAsync(item => item.Id == fileId && item.EquipmentId == equipmentId);

        if (file is null)
            return NotFound();

        var physicalPath = Path.Combine(_environment.WebRootPath, "uploads", "equipment", file.StoredFileName);
        if (System.IO.File.Exists(physicalPath))
            System.IO.File.Delete(physicalPath);

        _context.EquipmentFiles.Remove(file);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
