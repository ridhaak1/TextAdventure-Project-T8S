# Text Adventure – Testing & Security Project

## Projectstructuur

| Project | Type | Beschrijving |
|---------|------|-------------|
| `TextAdventure` | Console app | Het spelzelf |
| `MinimalApi-Auth` | Web API | Authenticatie, hashing, keyshares |
| `TestProject1` | Unit tests | Tests voor Item, Room, Inventory |
| `TextAdventure-IntgrationTests` | Integratietests | Samenwerking tussen klassen |

---

## Hoe starten

### Stap 1 – API starten
1. Rechtsklik op `MinimalApi-Auth` → **Set as Startup Project**
2. Druk op **Run** (of F5)
3. Swagger opent op `https://localhost:7298/swagger`

### Stap 2 – Account aanmaken via Swagger
1. Ga naar `POST /api/auth/register`
2. Vul een gebruikersnaam (min. 3 tekens) en wachtwoord (min. 8 tekens) in

### Stap 3 – Spel starten
1. Rechtsklik op `TextAdventure` → **Set as Startup Project**
2. Druk op **Run**
3. Log in met het aangemaakte account

---

## Spelcommando's

| Commando | Actie |
|---------|-------|
| `go n` / `go e` / `go s` / `go w` | Beweeg naar richting |
| `take sleutel` | Pak een item op |
| `fight` | Vecht tegen het monster |
| `unlock room_vault` | Ontgrendel de kluis (alle spelers) |
| `unlock room_godmode` | Ontgrendel de geheime kamer (alleen Admin) |
| `inventory` | Bekijk je items |
| `look` | Beschrijving van de kamer |
| `help` | Alle commando's |
| `quit` | Stop het spel |

---

## Kaart van het spel

```
              [De Uitgang] (vereist Sleutel)
                   N
                   |
[Dodelijke Gang] W -- [Start] -- E -- [Schatkamer] -- E -- [Kluis*]
                   |
                   S
        [Kelder (Zwaard, hint: 'draken')] -- E -- [Geheime Kamer**]
                   |
                   S
             [Monsterkamer (hint: 'geheim')]
```

`*`  Kluis = `unlock room_vault` (passphrase: in de Kelder)
`**` Geheime Kamer = `unlock room_godmode` (passphrase: in de Monsterkamer, vereist Admin-rol)

---

## Security (Deel 2)

### Authenticatie & Autorisatie
- **SHA-256 hashing** via `HashService` met `FixedTimeEquals` (timing-attack preventie)
- **JWT token** (2 uur geldig, met rol Player/Admin)
- **Lockout** na 3 mislukte inlogpogingen

### HTTPS
- API draait op `https://localhost:7298`
- Spel verbindt via HTTPS met `DangerousAcceptAnyServerCertificateValidator` (dev-certificaat)

### Encryptie (X.509/CMS)
- **Twee kamers** versleuteld via `EnvelopedCms` (X.509 certificaat, RSA 2048)
- `.pfx` = certificaat beschermd met wachtwoord, `.enc` = versleutelde inhoud
- Sleutel = **keyshare** (opgehaald via HTTPS API) + **passphrase** (gevonden in het spel)

| Kamer | RoomId | Passphrase hint | Toegang |
|-------|--------|-----------------|---------|
| Kluis | `room_vault` | inscriptie in Kelder: `draken` | Alle spelers |
| Geheime Kamer | `room_godmode` | briefje in Monsterkamer: `geheim` | Admin only |

### Keyshares (API – HTTPS)
- `GET /api/keys/keyshare/room_vault` → vereist geldig JWT
- `GET /api/keys/keyshare/room_godmode` → vereist Admin-rol (anders 403)

### Secure coding
- Alle input gevalideerd (geen crashes door lege/foute invoer)
- JWT Key staat in `appsettings.json` (voor productie: gebruik user-secrets)

---

## Tests uitvoeren

Open **Test Explorer** in Visual Studio → **Run All Tests**

| Testproject | Inhoud |
|-------------|--------|
| `TestProject1` | ItemTests (3), RoomTests (8), InventoryTests (5) |
| `TextAdventure-IntgrationTests` | MovementIntTest (7) |
