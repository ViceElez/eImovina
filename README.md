# e-Imovina Županije

Sustav za evidenciju županijske imovine, lokacija, zaduženja, inventura i otpisa opreme.

## Sadržaj

- [Arhitektura i tehnologije](#arhitektura-i-tehnologije)
- [Struktura projekta](#struktura-projekta)
- [Pokretanje](#pokretanje)
- [Demo korisnički računi](#demo-korisnicki-racuni)
- [Uloge i ovlasti](#uloge-i-ovlasti)
- [Glavni moduli](#glavni-moduli)
- [Poslovna pravila](#poslovna-pravila)
- [Poznata ograničenja](#poznata-ogranicenja)

---

## Arhitektura i tehnologije

Projekt je podijeljen u tri odvojena .NET projekta:

| Projekt | Uloga | Tehnologije |
|---|---|---|
| **eImovina.Api** | Backend REST API | ASP.NET Core Web API, Entity Framework Core, SQLite, JWT autentikacija |
| **eImovina.App** | Frontend | Blazor Server, MudBlazor komponente |
| **eImovina.Shared** | Zajednički DTO-i | Class Library, referenciran iz oba gornja projekta |

**Baza podataka**: SQLite (`eImovina.db`), generirana iz EF Core migracija.

**Autentikacija**: JWT token izdaje API (`AuthController`), Blazor App ga sprema preko `ProtectedSessionStorage` i sam ga lijepi na svaki API poziv preko `AuthTokenProvider`. Autorizacija na Blazor stranicama ide preko `AuthorizeRouteView` + custom `CustomAuthStateProvider`, potpuno neovisno o ASP.NET Core Identity ili cookie autentikaciji — App projekt namjerno **ne** koristi `AddAuthentication`/`UseAuthentication`/`UseAuthorization`, jer cijeli auth živi na Blazor komponentnoj razini.

## Struktura projekta

```
eImovina/
├─ eImovina.Api/
│  ├─ Controllers/          - svi API endpointi
│  ├─ Data/                 - eImovinaDbContext, DbSeeder, UserSeeder, EquipmentSeeder
│  ├─ Security/             - JwtOptions, JwtTokenService, AppClaimTypes, AuthorizationPolicies
│  ├─ Migrations/
│  ├─ wwwroot/uploads/      - fizičke slike i dokumenti opreme
│  └─ Program.cs
├─ eImovina.App/
│  ├─ Components/
│  │  ├─ Layout/            - MainLayout, NavMenu, LogoutButton, RedirectToLogin
│  │  └─ Pages/              - sve Blazor stranice
│  ├─ Auth/                 - AuthTokenProvider, CustomAuthStateProvider
│  └─ Program.cs
└─ eImovina.Shared/
   └─ DTOs/                 - DTO-i grupirani po modulu (Equipments, Locations, Users...)
```

## Pokretanje

### Preduvjeti
- .NET 8/9 SDK
- (nema potrebe za lokalno instaliranim SQL Serverom — koristi se SQLite fajl)

### Koraci

1. **Klonirati repozitorij i otvoriti solution**

2. **Provjeriti `eImovina.Api/appsettings.json`** — mora sadržavati `Jwt` sekciju s `SigningKey` od minimalno 32 znaka:
   ```json
   "Jwt": {
     "Issuer": "eImovina.Api",
     "Audience": "eImovina.App",
     "SigningKey": "OVO-JE-DEMO-KLJUC-ZAMIJENI-GA-PRIJE-PREDAJE-32-znaka-min",
     "ExpirationMinutes": 60
   }
   ```

3. **Pokrenuti API prvi** (obavezno prije App-a):
   ```bash
   cd eImovina.Api
   dotnet run
   ```
   Pri prvom pokretanju automatski se:
   - primjenjuju sve EF Core migracije na praznu bazu (`db.Database.MigrateAsync()`)
   - pune šifrarnici i osnovni podaci (`DbSeeder`)
   - kreiraju 4 demo korisnička računa (`UserSeeder`)
   - dodaje demo oprema, zaduženja i primjeri zahtjeva (`EquipmentSeeder`)

   Seed je idempotentan — ponovno pokretanje ne duplicira podatke.

4. **Pokrenuti App** (u drugom terminalu):
   ```bash
   cd eImovina.App
   dotnet run
   ```
   Provjeriti da `Program.cs` u App projektu ima ispravan `BaseAddress` za API (port mora odgovarati onome na kojem stvarno radi API).

5. Otvoriti App u browseru i prijaviti se jednim od demo računa ispod.

### Migracije od nule

Ako baza (`eImovina.db`) već postoji i želiš krenuti ispočetka:
```bash
# u eImovina.Api folderu
rm eImovina.db
dotnet run
```

## Demo korisnički računi

| Korisničko ime | Lozinka | Rola | Zaposlenik |
|---|---|---|---|
| `admin` | `Admin123!` | Admin | Ana Admić |
| `manager` | `Manager123!` | InventoryManager | Marko Marić |
| `responsible` | `Responsible123!` | LocationResponsible | Petra Perić |
| `employee` | `Employee123!` | Employee | Ivo Ivić |

## Uloge i ovlasti

| Uloga | Opseg |
|---|---|
| **Admin** | Pun pristup svemu — upravljanje korisnicima, opremom, lokacijama, zaposlenicima, inventurama, zahtjevima |
| **InventoryManager** | Operativno upravljanje opremom, zaduženjima, inventurama i zahtjevima na svim lokacijama |
| **LocationResponsible** | Pristup ograničen na **vlastitu** lokaciju — vidi i obrađuje samo opremu/inventure te lokacije |
| **Employee** | Vidi samo svoju zaduženu opremu (`/my-equipment`) i svoje zahtjeve (`/my-requests`), može podnijeti novi zahtjev |

Ovlasti su provjerene **na API razini** (`[Authorize(Policy=...)]`/`[Authorize(Roles=...)]`), ne samo skrivanjem elemenata u sučelju.

## Glavni moduli

- **Oprema** (`/equipment`) — CRUD, server-side filtriranje (tekst, kategorija, status, lokacija), profil s poviješću zaduženja i uploadom slika/dokumenata
- **Zaduženja** — zaduži/vrati/prenesi drugom zaposleniku/promijeni lokaciju, s punom poviješću (ne samo trenutačnim stanjem)
- **Lokacije** i **Zaposlenici** — osnovno upravljanje matičnim podacima
- **Inventure** (`/inventories`) — statusni tok Nacrt → Otvorena → U tijeku → Završena → Zaključana, sa snapshotom očekivane opreme pri otvaranju
- **Zahtjevi za opremu** i **Zahtjevi za otpis** — s odvojenim tokovima za zaposlenika (podnosi) i managera (rješava)
- **Korisnici** (`/users`, Admin only) — kreiranje računa, dodjela više uloga, povezivanje sa zaposlenikom, aktivacija/deaktivacija
- **Dashboard** (`/`) — agregati (ukupno opreme, zaduženo, na servisu, otvoreni zahtjevi...) vidljivi samo upravljačkim ulogama; svaki prijavljeni zaposlenik vidi svoj osobni dio

## Poslovna pravila

- Inventurni broj obavezan i jedinstven; serijski broj jedinstven ako je unesen
- Oprema ne može imati dva aktivna zaduženja istovremeno (garantirano filtriranim unique indeksom u bazi, ne samo provjerom u kodu)
- Otpisana oprema se ne može zadužiti
- Deaktivirani zaposlenik se ne može zadužiti opremom
- Ne može se prenijeti oprema na istog zaposlenika koji je već zadužen
- Zaključana inventura se više ne može mijenjati
- Upload datoteka ograničen po tipu (slike: jpg/png/webp do 5 MB; dokumenti: PDF do 10 MB) i veličini, provjereno na backendu

## Poznata ograničenja

- Sortiranje tablice opreme radi **na trenutačno dohvaćenoj stranici rezultata** (server-side filtriranje + straničenje, klijentsko sortiranje unutar te stranice)
- JWT signing key u `appsettings.json` je demo placeholder — zamijeniti prije produkcijske upotrebe

## Testirani glavni workflowi

- **Zaduživanje opreme**: zaduži nezauzetu opremu → status postaje "Zaduženo" → pokušaj zadužiti istu opremu ponovno → odbijeno
- **Prijenos opreme**: prenesi zaduženu opremu drugom zaposleniku → staro zaduženje ide u "Premješteno", novo je "Aktivno" → povijest ostaje vidljiva na profilu opreme
- **Povrat opreme**: zaprimi povrat aktivnog zaduženja → status opreme vraća se na "Na skladištu"
- **Inventura od početka do kraja**: kreiraj (Nacrt) → otvori (generira stavke) → obradi sve stavke (automatski prelazi u "U tijeku") → završi (provjerava da su sve stavke obrađene) → zaključaj (postaje immutable)
- **Otpis opreme**: podnesi zahtjev za otpis → odobri → oprema dobiva status "Otpisano" → daljnji pokušaj zaduženja te opreme odbijen
- **Zahtjev zaposlenika za opremom**: zaposlenik podnosi zahtjev preko `/my-requests` → manager ga rješava preko `/equipment-requests`
- **Upload datoteke**: upload dopuštene slike/PDF-a uspijeva; upload nedopuštene vrste ili prevelike datoteke odbijen s jasnom porukom

## Autorizacijski scenariji

**Uspješan ownership scenarij**
- LocationResponsible prijavljen za lokaciju "Sjedište Županije" → otvara/obrađuje inventuru te iste lokacije → uspijeva

**Odbijeni ownership scenarij**
- Isti LocationResponsible pokušava pristupiti opremi/inventuri lokacije "Područni ured Sinj" (nije njegova) → API vraća 403, podaci se ne prikazuju

**401 vs 403**
- Zahtjev bez JWT tokena (neprijavljen) na zaštićeni endpoint → **401 Unauthorized**
- Zahtjev s valjanim tokenom, ali krivom rolom (npr. Employee pokušava `GET /api/users`) → **403 Forbidden**

**Role-based prikaz**
- Admin vidi sve module u navigaciji, uključujući Administraciju
- InventoryManager vidi sve osim Administracije
- LocationResponsible vidi Opremu i Inventure, ograničeno na svoju lokaciju
- Employee vidi samo "Moje" sekciju (Moja oprema, Moji zahtjevi); dashboard mu ne prikazuje organizacijske agregate, samo osobni dio

## DBML i dijagram baze

Konačni DBML dijagram nalazi se u `e-imovina.dbml` (root repozitorija) — može se uvesti direktno na [dbdiagram.io](https://dbdiagram.io/d) za vizualni prikaz sheme.
