using eImovina.Api.Data;
using eImovina.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;
[ApiController]
[Route("api/lookups")]
//[Authorize]
public class LookupsController : ControllerBase
{
    private readonly eImovinaDbContext _db;

    public LookupsController(eImovinaDbContext db) => _db = db;

    [HttpGet("equipment-categories")]
    public async Task<List<LookUpDto>> EquipmentCategories() =>
        await _db.EquipmentCategories.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("equipment-statuses")]
    public async Task<List<LookUpDto>> EquipmentStatuses() =>
        await _db.EquipmentStatuses.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("assignment-statuses")]
    public async Task<List<LookUpDto>> AssignmentStatuses() =>
        await _db.AssignmentStatuses.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("inventory-statuses")]
    public async Task<List<LookUpDto>> InventoryStatuses() =>
        await _db.InventoryStatuses.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("request-statuses")]
    public async Task<List<LookUpDto>> RequestStatuses() =>
        await _db.RequestStatuses.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("write-off-request-statuses")]
    public async Task<List<LookUpDto>> WriteOffRequestStatuses() =>
        await _db.WriteOffRequestStatuses.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("location-types")]
    public async Task<List<LookUpDto>> LocationTypes() =>
        await _db.LocationTypes.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("locations")]
    public async Task<List<LookUpDto>> Locations() =>
        await _db.Locations.Where(x => x.IsActive).Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();

    [HttpGet("employees")]
    public async Task<List<LookUpDto>> Employees() =>
        await _db.Employees.Where(x => x.IsActive)
            .Select(x => new LookUpDto { Id = x.Id, Name = x.FirstName + " " + x.LastName })
            .ToListAsync();

    [HttpGet("roles")]
    //[Authorize(Roles = "Admin")]
    public async Task<List<LookUpDto>> Roles() =>
        await _db.Roles.Select(x => new LookUpDto { Id = x.Id, Name = x.Name }).ToListAsync();
}
