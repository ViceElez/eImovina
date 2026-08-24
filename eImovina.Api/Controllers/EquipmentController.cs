using eImovina.Api.Data;
using eImovina.Shared.Models.Equipments;
using eImovina.Shared.DTOs;
using eImovina.Shared.DTOs.Equipments;
using eImovina.Shared.DTOs.WriteOffRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public EquipmentController(eImovinaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
   // [Authorize(Roles = "Admin,InventoryManager,LocationResponsible")]
    public async Task<ActionResult<PagedResult<EquipmentDto>>> GetEquipment([FromQuery] EquipmentFilterDto filter)
    {
        var query = _context.Equipment
            .Include(equipment => equipment.Category)
            .Include(equipment => equipment.Status)
            .Include(equipment => equipment.CurrentLocation)
            .Include(equipment => equipment.Assignments)
                .ThenInclude(assignment => assignment.Employee)
            .Include(equipment => equipment.Assignments)
                .ThenInclude(assignment => assignment.Status)
            .AsQueryable();

        if (User.IsInRole("LocationResponsible") && !User.IsInRole("Admin") && !User.IsInRole("InventoryManager"))
        {
            var locationClaim = User.FindFirst("LocationId")?.Value;
            if (locationClaim is not null)
                query = query.Where(equipment => equipment.CurrentLocationId == int.Parse(locationClaim));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var term = filter.SearchText.Trim().ToLower();
            query = query.Where(equipment =>
                equipment.Name.ToLower().Contains(term) ||
                equipment.InventoryNumber.ToLower().Contains(term) ||
                (equipment.SerialNumber != null && equipment.SerialNumber.ToLower().Contains(term)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(equipment => equipment.CategoryId == filter.CategoryId);

        if (filter.StatusId.HasValue)
            query = query.Where(equipment => equipment.StatusId == filter.StatusId);

        if (filter.LocationId.HasValue)
            query = query.Where(equipment => equipment.CurrentLocationId == filter.LocationId);

        if (filter.EmployeeId.HasValue)
            query = query.Where(equipment => equipment.Assignments.Any(assignment =>
                assignment.EmployeeId == filter.EmployeeId && assignment.Status!.Name == "Aktivno"));

        query = filter.SortBy switch
        {
            "InventoryNumber" => filter.SortDescending
                ? query.OrderByDescending(equipment => equipment.InventoryNumber)
                : query.OrderBy(equipment => equipment.InventoryNumber),
            _ => filter.SortDescending
                ? query.OrderByDescending(equipment => equipment.Name)
                : query.OrderBy(equipment => equipment.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(equipment => ToDto(equipment))
            .ToListAsync();

        return Ok(new PagedResult<EquipmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    [HttpGet("lookup")]
   // [Authorize(Roles = "Admin,InventoryManager,LocationResponsible")]
    public async Task<ActionResult<List<LookUpDto>>> GetEquipmentLookup()
    {
        var equipment = await _context.Equipment
            .Where(item => item.Status!.Name != "Otpisano")
            .OrderBy(item => item.Name)
            .Select(item => new LookUpDto
            {
                Id = item.Id,
                Name = item.InventoryNumber + " - " + item.Name
            })
            .ToListAsync();

        return Ok(equipment);
    }

    [HttpGet("{id}")]
   // [Authorize(Roles = "Admin,InventoryManager,LocationResponsible")]
    public async Task<ActionResult<EquipmentDto>> GetEquipmentById(int id)
    {
        var equipment = await _context.Equipment
            .Include(item => item.Category)
            .Include(item => item.Status)
            .Include(item => item.CurrentLocation)
            .Include(item => item.Assignments)
                .ThenInclude(assignment => assignment.Employee)
            .Include(item => item.Assignments)
                .ThenInclude(assignment => assignment.Status)
            .Include(item => item.Files)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (equipment is null)
            return NotFound();

        return Ok(ToDto(equipment));
    }

    [HttpGet("{id}/profile")]
   // [Authorize(Roles = "Admin,InventoryManager,LocationResponsible")]
    public async Task<ActionResult<EquipmentProfileDto>> GetEquipmentProfile(int id)
    {
        var equipment = await _context.Equipment
            .Include(item => item.Category)
            .Include(item => item.Status)
            .Include(item => item.CurrentLocation)
            .Include(item => item.Assignments)
                .ThenInclude(assignment => assignment.Employee)
            .Include(item => item.Assignments)
                .ThenInclude(assignment => assignment.Status)
            .Include(item => item.Files)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (equipment is null)
            return NotFound();

        var equipmentDto = ToDto(equipment);

        var history = await _context.EquipmentAssignments
            .Include(assignment => assignment.Employee)
            .Include(assignment => assignment.Status)
            .Include(assignment => assignment.AssignedByUser)
            .Include(assignment => assignment.ReturnedByUser)
            .Where(assignment => assignment.EquipmentId == id)
            .OrderByDescending(assignment => assignment.AssignedAt)
            .Select(assignment => new EquipmentAssignmentDto
            {
                Id = assignment.Id,
                EquipmentId = assignment.EquipmentId,
                Equipment = equipmentDto.Name,
                InventoryNumber = equipmentDto.InventoryNumber,
                EmployeeId = assignment.EmployeeId,
                Employee = assignment.Employee!.FirstName + " " + assignment.Employee.LastName,
                StatusId = assignment.StatusId,
                Status = assignment.Status!.Name,
                AssignedAt = assignment.AssignedAt,
                ReturnedAt = assignment.ReturnedAt,
                AssignedBy = assignment.AssignedByUser!.Username,
                ReturnedBy = assignment.ReturnedByUser != null ? assignment.ReturnedByUser.Username : null,
                Note = assignment.Note
            })
            .ToListAsync();

        var files = await _context.EquipmentFiles
            .Include(file => file.UploadedByUser)
            .Where(file => file.EquipmentId == id)
            .Select(file => new EquipmentFileDto
            {
                Id = file.Id,
                EquipmentId = file.EquipmentId,
                FileType = file.FileType.ToString(),
                OriginalFileName = file.OriginalFileName,
                Url = "/uploads/equipment/" + file.StoredFileName,
                ContentType = file.ContentType,
                SizeBytes = file.SizeBytes,
                IsCoverImage = file.IsCoverImage,
                UploadedAt = file.UploadedAt,
                UploadedBy = file.UploadedByUser!.Username
            })
            .ToListAsync();

        var writeOffRequests = await _context.WriteOffRequests
            .Include(request => request.Status)
            .Include(request => request.RequestedByUser)
            .Include(request => request.DecisionByUser)
            .Where(request => request.EquipmentId == id)
            .Select(request => new WriteOffRequestDto
            {
                Id = request.Id,
                EquipmentId = request.EquipmentId,
                Equipment = equipmentDto.Name,
                InventoryNumber = equipmentDto.InventoryNumber,
                RequestedBy = request.RequestedByUser!.Username,
                StatusId = request.StatusId,
                Status = request.Status!.Name,
                Reason = request.Reason,
                RequestedAt = request.RequestedAt,
                DecisionBy = request.DecisionByUser != null ? request.DecisionByUser.Username : null,
                DecisionNote = request.DecisionNote,
                DecidedAt = request.DecidedAt
            })
            .ToListAsync();

        return Ok(new EquipmentProfileDto
        {
            Equipment = equipmentDto,
            AssignmentHistory = history,
            Files = files,
            WriteOffRequests = writeOffRequests
        });
    }

    [HttpPost]
    //[Authorize(Roles = "Admin,InventoryManager")]
    public async Task<ActionResult<EquipmentDto>> CreateEquipment(SaveEquipmentDto dto)
    {
        var duplicateInventoryNumber = await _context.Equipment
            .AnyAsync(item => item.InventoryNumber == dto.InventoryNumber);
        if (duplicateInventoryNumber)
            return BadRequest("Inventurni broj već postoji.");

        if (!string.IsNullOrWhiteSpace(dto.SerialNumber))
        {
            var duplicateSerialNumber = await _context.Equipment
                .AnyAsync(item => item.SerialNumber == dto.SerialNumber);
            if (duplicateSerialNumber)
                return BadRequest("Serijski broj već postoji.");
        }

        var equipment = new Equipment
        {
            InventoryNumber = dto.InventoryNumber,
            SerialNumber = dto.SerialNumber,
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            StatusId = dto.StatusId,
            CurrentLocationId = dto.CurrentLocationId,
            Value = dto.Value,
            PurchaseDate = dto.PurchaseDate,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEquipmentById), new { id = equipment.Id }, dto);
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> UpdateEquipment(int id, SaveEquipmentDto dto)
    {
        var equipment = await _context.Equipment.FindAsync(id);
        if (equipment is null)
            return NotFound();

        var duplicateInventoryNumber = await _context.Equipment
            .AnyAsync(item => item.InventoryNumber == dto.InventoryNumber && item.Id != id);
        if (duplicateInventoryNumber)
            return BadRequest("Inventurni broj već postoji.");

        equipment.InventoryNumber = dto.InventoryNumber;
        equipment.SerialNumber = dto.SerialNumber;
        equipment.Name = dto.Name;
        equipment.CategoryId = dto.CategoryId;
        equipment.StatusId = dto.StatusId;
        equipment.CurrentLocationId = dto.CurrentLocationId;
        equipment.Value = dto.Value;
        equipment.PurchaseDate = dto.PurchaseDate;
        equipment.Note = dto.Note;
        equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static EquipmentDto ToDto(Equipment equipment)
    {
        var activeAssignment = equipment.Assignments.FirstOrDefault(assignment => assignment.Status?.Name == "Aktivno");

        return new EquipmentDto
        {
            Id = equipment.Id,
            InventoryNumber = equipment.InventoryNumber,
            SerialNumber = equipment.SerialNumber,
            Name = equipment.Name,
            CategoryId = equipment.CategoryId,
            Category = equipment.Category?.Name ?? string.Empty,
            StatusId = equipment.StatusId,
            Status = equipment.Status?.Name ?? string.Empty,
            CurrentLocationId = equipment.CurrentLocationId,
            CurrentLocation = equipment.CurrentLocation?.Name ?? string.Empty,
            Value = equipment.Value,
            PurchaseDate = equipment.PurchaseDate,
            Note = equipment.Note,
            AssignedToEmployeeId = activeAssignment?.EmployeeId,
            AssignedTo = activeAssignment?.Employee != null
                ? activeAssignment.Employee.FirstName + " " + activeAssignment.Employee.LastName
                : null,
            CoverImageUrl = equipment.Files.FirstOrDefault(file => file.IsCoverImage) is { } cover
                ? "/uploads/equipment/" + cover.StoredFileName
                : null
        };
    }
}
