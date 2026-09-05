using eImovina.Shared.Models.Equipments;
using eImovina.Shared.Models.WriteOffRequests;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Data;

public static class EquipmentSeeder
{
    public static async Task SeedAsync(eImovinaDbContext db)
    {
        if (await db.Equipment.AnyAsync())
            return;

        var sjediste = await db.Locations.FirstAsync(x => x.Name == "Sjedište Županije");
        var podruznica = await db.Locations.FirstAsync(x => x.Name == "Područni ured Sinj");

        var racunalo = await db.EquipmentCategories.FirstAsync(x => x.Name == "Računalo");
        var mreznaOprema = await db.EquipmentCategories.FirstAsync(x => x.Name == "Mrežna oprema");
        var namjestaj = await db.EquipmentCategories.FirstAsync(x => x.Name == "Namještaj");
        var alat = await db.EquipmentCategories.FirstAsync(x => x.Name == "Alat");

        var naSkladistu = await db.EquipmentStatuses.FirstAsync(x => x.Name == "Na skladištu");
        var zaduzeno = await db.EquipmentStatuses.FirstAsync(x => x.Name == "Zaduženo");
        var naServisu = await db.EquipmentStatuses.FirstAsync(x => x.Name == "Na servisu");

        var now = DateTime.UtcNow;

        var laptop1 = new Equipment
        {
            InventoryNumber = "INV-0001",
            SerialNumber = "SN-LAP-0001",
            Name = "Laptop Dell Latitude 5440",
            CategoryId = racunalo.Id,
            StatusId = zaduzeno.Id,
            CurrentLocationId = sjediste.Id,
            Value = 1250.00m,
            PurchaseDate = new DateTime(2024, 3, 15),
            Note = "Standardna oprema za urede.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var laptop2 = new Equipment
        {
            InventoryNumber = "INV-0002",
            SerialNumber = "SN-LAP-0002",
            Name = "Laptop HP EliteBook 840",
            CategoryId = racunalo.Id,
            StatusId = naSkladistu.Id,
            CurrentLocationId = sjediste.Id,
            Value = 1100.00m,
            PurchaseDate = new DateTime(2024, 6, 1),
            CreatedAt = now,
            UpdatedAt = now
        };

        var monitor1 = new Equipment
        {
            InventoryNumber = "INV-0003",
            SerialNumber = "SN-MON-0001",
            Name = "Monitor Dell 24\"",
            CategoryId = racunalo.Id,
            StatusId = zaduzeno.Id,
            CurrentLocationId = sjediste.Id,
            Value = 180.00m,
            PurchaseDate = new DateTime(2024, 3, 15),
            CreatedAt = now,
            UpdatedAt = now
        };

        var router1 = new Equipment
        {
            InventoryNumber = "INV-0004",
            SerialNumber = "SN-RTR-0001",
            Name = "Mrežni router Cisco RV340",
            CategoryId = mreznaOprema.Id,
            StatusId = naSkladistu.Id,
            CurrentLocationId = sjediste.Id,
            Value = 420.00m,
            PurchaseDate = new DateTime(2023, 11, 20),
            CreatedAt = now,
            UpdatedAt = now
        };

        var stol1 = new Equipment
        {
            InventoryNumber = "INV-0005",
            SerialNumber = null,
            Name = "Uredski stol",
            CategoryId = namjestaj.Id,
            StatusId = naSkladistu.Id,
            CurrentLocationId = sjediste.Id,
            Value = 210.00m,
            PurchaseDate = new DateTime(2022, 5, 10),
            CreatedAt = now,
            UpdatedAt = now
        };

        var busilica1 = new Equipment
        {
            InventoryNumber = "INV-0006",
            SerialNumber = "SN-DRL-0001",
            Name = "Bušilica Bosch",
            CategoryId = alat.Id,
            StatusId = naServisu.Id,
            CurrentLocationId = sjediste.Id,
            Value = 95.00m,
            PurchaseDate = new DateTime(2021, 9, 5),
            Note = "Na servisu zbog kvara motora.",
            CreatedAt = now,
            UpdatedAt = now
        };

        var laptop3 = new Equipment
        {
            InventoryNumber = "INV-0007",
            SerialNumber = "SN-LAP-0003",
            Name = "Laptop Lenovo ThinkPad E14",
            CategoryId = racunalo.Id,
            StatusId = zaduzeno.Id,
            CurrentLocationId = podruznica.Id,
            Value = 980.00m,
            PurchaseDate = new DateTime(2024, 1, 10),
            CreatedAt = now,
            UpdatedAt = now
        };

        var telefon1 = new Equipment
        {
            InventoryNumber = "INV-0008",
            SerialNumber = "SN-PHN-0001",
            Name = "Telefon Cisco IP 7841",
            CategoryId = mreznaOprema.Id,
            StatusId = naSkladistu.Id,
            CurrentLocationId = podruznica.Id,
            Value = 130.00m,
            PurchaseDate = new DateTime(2023, 4, 2),
            CreatedAt = now,
            UpdatedAt = now
        };

        var ormar1 = new Equipment
        {
            InventoryNumber = "INV-0009",
            SerialNumber = null,
            Name = "Uredski ormar",
            CategoryId = namjestaj.Id,
            StatusId = naSkladistu.Id,
            CurrentLocationId = podruznica.Id,
            Value = 150.00m,
            PurchaseDate = new DateTime(2022, 8, 18),
            CreatedAt = now,
            UpdatedAt = now
        };

        var stolica1 = new Equipment
        {
            InventoryNumber = "INV-0010",
            SerialNumber = null,
            Name = "Uredska stolica",
            CategoryId = namjestaj.Id,
            StatusId = naSkladistu.Id,
            CurrentLocationId = podruznica.Id,
            Value = 85.00m,
            PurchaseDate = new DateTime(2022, 8, 18),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Equipment.AddRange(
            laptop1, laptop2, monitor1, router1, stol1,
            busilica1, laptop3, telefon1, ormar1, stolica1
        );
        await db.SaveChangesAsync();

        // --- Aktivna zaduženja za opremu koja je označena kao "Zaduženo" ---
        var marko = await db.Employees.FirstAsync(x => x.Email == "marko.maric@zupanija.hr");
        var ivo = await db.Employees.FirstAsync(x => x.Email == "ivo.ivic@zupanija.hr");
        var admin = await db.Users.FirstAsync(x => x.Username == "admin");

        var aktivno = await db.AssignmentStatuses.FirstAsync(x => x.Name == "Aktivno");

        db.EquipmentAssignments.AddRange(
            new EquipmentAssignment
            {
                EquipmentId = laptop1.Id,
                EmployeeId = marko.Id,
                StatusId = aktivno.Id,
                AssignedAt = now.AddDays(-30),
                AssignedByUserId = admin.Id,
                Note = "Standardna dodjela pri zapošljavanju."
            },
            new EquipmentAssignment
            {
                EquipmentId = monitor1.Id,
                EmployeeId = marko.Id,
                StatusId = aktivno.Id,
                AssignedAt = now.AddDays(-30),
                AssignedByUserId = admin.Id
            },
            new EquipmentAssignment
            {
                EquipmentId = laptop3.Id,
                EmployeeId = ivo.Id,
                StatusId = aktivno.Id,
                AssignedAt = now.AddDays(-10),
                AssignedByUserId = admin.Id
            }
        );

        // --- Primjer zahtjeva za opremom (zaposlenik traži nešto novo) ---
        var zaprimljeno = await db.RequestStatuses.FirstAsync(x => x.Name == "Zaprimljeno");

        db.EquipmentRequests.Add(new eImovina.Shared.Models.Equipments.EquipmentRequest
        {
            EmployeeId = ivo.Id,
            CategoryId = racunalo.Id,
            StatusId = zaprimljeno.Id,
            Description = "Treba mi dodatni monitor za rad od kuće.",
            RequestedAt = now.AddDays(-2)
        });

        // --- Primjer zahtjeva za otpis (bušilica je na servisu, možda za otpis) ---
        var writeOffZaprimljeno = await db.WriteOffRequestStatuses.FirstAsync(x => x.Name == "Zaprimljeno");

        db.WriteOffRequests.Add(new WriteOffRequest
        {
            EquipmentId = busilica1.Id,
            RequestedByUserId = admin.Id,
            StatusId = writeOffZaprimljeno.Id,
            Reason = "Kvar motora, popravak nije ekonomski isplativ.",
            RequestedAt = now.AddDays(-1)
        });

        await db.SaveChangesAsync();
    }
}
