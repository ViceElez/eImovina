using eImovina.Api.Data;
using eImovina.Api.Security;
using eImovina.Shared.DTOs.Locations;
using eImovina.Shared.Models.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class LocationsController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public LocationsController(eImovinaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<ActionResult<List<LocationDto>>> GetLocations()
    {
        var locations = await _context.Locations
            .Include(location => location.LocationType)
            .OrderBy(location => location.Name)
            .Select(location => new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                LocationTypeId = location.LocationTypeId,
                LocationType = location.LocationType!.Name,
                Address = location.Address,
                IsActive = location.IsActive
            })
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("{id}")]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<ActionResult<LocationDto>> GetLocationById(int id)
    {
        var location = await _context.Locations
            .Include(item => item.LocationType)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (location is null)
            return NotFound();

        return Ok(new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            LocationTypeId = location.LocationTypeId,
            LocationType = location.LocationType!.Name,
            Address = location.Address,
            IsActive = location.IsActive
        });
    }

    [HttpPost]
    //[Authorize(Policy = ClassAuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<LocationDto>> CreateLocation(SaveLocationDto dto)
    {
        var location = new Location
        {
            Name = dto.Name,
            LocationTypeId = dto.LocationTypeId,
            Address = dto.Address,
            IsActive = dto.IsActive
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, dto);
    }

    [HttpPut("{id}")]
    //[Authorize(Policy = ClassAuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> UpdateLocation(int id, SaveLocationDto dto)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location is null)
            return NotFound();

        location.Name = dto.Name;
        location.LocationTypeId = dto.LocationTypeId;
        location.Address = dto.Address;
        location.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}