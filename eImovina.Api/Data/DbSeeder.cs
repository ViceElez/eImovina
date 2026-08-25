using eImovina.Shared.Models.Locations;
using eImovina.Shared.Models.Equipments;
using eImovina.Shared.Models.Inventories;
using eImovina.Shared.Models.WriteOffRequests;
using eImovina.Shared.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(eImovinaDbContext db)
    {
        if (!await db.LocationTypes.AnyAsync())
        {
            db.LocationTypes.AddRange(
                new LocationType { Name = "Ured" },
                new LocationType { Name = "Škola" },
                new LocationType { Name = "Skladište" },
                new LocationType { Name = "Terenska lokacija" }
            );
        }

        if (!await db.EquipmentCategories.AnyAsync())
        {
            db.EquipmentCategories.AddRange(
                new EquipmentCategory { Name = "Računalo" },
                new EquipmentCategory { Name = "Mrežna oprema" },
                new EquipmentCategory { Name = "Namještaj" },
                new EquipmentCategory { Name = "Alat" },
                new EquipmentCategory { Name = "Ostalo" }
            );
        }

        if (!await db.EquipmentStatuses.AnyAsync())
        {
            db.EquipmentStatuses.AddRange(
                new EquipmentStatus { Name = "Na skladištu" },
                new EquipmentStatus { Name = "Zaduženo" },
                new EquipmentStatus { Name = "Na servisu" },
                new EquipmentStatus { Name = "Nedostaje" },
                new EquipmentStatus { Name = "Otpisano" }
            );
        }

        if (!await db.AssignmentStatuses.AnyAsync())
        {
            db.AssignmentStatuses.AddRange(
                new AssignmentStatus { Name = "Aktivno" },
                new AssignmentStatus { Name = "Vraćeno" },
                new AssignmentStatus { Name = "Premješteno" },
                new AssignmentStatus { Name = "Stornirano" }
            );
        }

        if (!await db.InventoryStatuses.AnyAsync())
        {
            db.InventoryStatuses.AddRange(
                new InventoryStatus { Name = "Nacrt" },
                new InventoryStatus { Name = "Otvorena" },
                new InventoryStatus { Name = "U tijeku" },
                new InventoryStatus { Name = "Završena" },
                new InventoryStatus { Name = "Zaključana" }
            );
        }

        if (!await db.RequestStatuses.AnyAsync())
        {
            db.RequestStatuses.AddRange(
                new RequestStatus { Name = "Zaprimljeno" },
                new RequestStatus { Name = "U obradi" },
                new RequestStatus { Name = "Odobreno" },
                new RequestStatus { Name = "Odbijeno" },
                new RequestStatus { Name = "Realizirano" },
                new RequestStatus { Name = "Zatvoreno" }
            );
        }

        if (!await db.WriteOffRequestStatuses.AnyAsync())
        {
            db.WriteOffRequestStatuses.AddRange(
                new WriteOffRequestStatus { Name = "Zaprimljeno" },
                new WriteOffRequestStatus { Name = "U obradi" },
                new WriteOffRequestStatus { Name = "Odobreno" },
                new WriteOffRequestStatus { Name = "Odbijeno" },
                new WriteOffRequestStatus { Name = "Provedeno" }
            );
        }

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "InventoryManager" },
                new Role { Name = "LocationResponsible" },
                new Role { Name = "Employee" }
            );
        }

        await db.SaveChangesAsync();

        if (!await db.Locations.AnyAsync())
        {
            var uredTip = await db.LocationTypes.FirstAsync(x => x.Name == "Ured");

            db.Locations.AddRange(
                new Location { Name = "Sjedište Županije", LocationTypeId = uredTip.Id, Address = "Glavni trg 1" },
                new Location { Name = "Područni ured Sinj", LocationTypeId = uredTip.Id, Address = "Sinjska 5" }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Employees.AnyAsync())
        {
            var sjediste = await db.Locations.FirstAsync(x => x.Name == "Sjedište Županije");
            var podruznica = await db.Locations.FirstAsync(x => x.Name == "Područni ured Sinj");

            db.Employees.AddRange(
                new Employee { FirstName = "Ana", LastName = "Admić", LocationId = sjediste.Id, Email = "ana.admic@zupanija.hr" },
                new Employee { FirstName = "Marko", LastName = "Marić", LocationId = sjediste.Id, Email = "marko.maric@zupanija.hr" },
                new Employee { FirstName = "Petra", LastName = "Perić", LocationId = podruznica.Id, Email = "petra.peric@zupanija.hr" },
                new Employee { FirstName = "Ivo", LastName = "Ivić", LocationId = podruznica.Id, Email = "ivo.ivic@zupanija.hr" }
            );
            await db.SaveChangesAsync();
        }
    }
}
