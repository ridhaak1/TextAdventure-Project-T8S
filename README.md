# Secure Text Adventure

## Projectstructuur

```
projectmap/
├── MinimalApi-Auth/        → Persoon 1: Minimal API met authenticatie & JWT
├── TextAdventure/          → Persoon 3: Console-game met secure coding
├── TestProject1/           → Unit tests
├── TextAdventure-IntgrationTests/ → Integratietests
└── README.md               → Dit bestand
```

---

## Hoe starten

### 1. API opstarten (persoon 1)

Stel eerst de JWT-secret in via user-secrets (zodat deze **nooit** in Git terechtkomt):

```bash
cd MinimalApi-Auth
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "a8f3c91e7d5b4c6f9a2e8d1c4b7f6a9e3d8c1b5f7a2e9d6c4b8f1a3e7d9c2"
dotnet run
```

De API draait daarna op:
- HTTP:  `http://localhost:5056`
- HTTPS: `https://localhost:7298`
- Swagger: `https://localhost:7298/swagger`

### 2. Game opstarten (persoon 3/4)

```bash
cd TextAdventure
dotnet run
```

Het spel vraagt automatisch om in te loggen voor je kan spelen.

---

## Deel 1 — Authenticatie & Minimal API (Persoon 1)

### Endpoints

| Methode | Pad                          | Auth vereist | Beschrijving                         |
|---------|------------------------------|:------------:|--------------------------------------|
| POST    | `/api/auth/register`         | Nee          | Nieuwe gebruiker registreren         |
| POST    | `/api/auth/login`            | Nee          | Inloggen, JWT-token ontvangen        |
| GET     | `/api/auth/me`               | Ja (Bearer)  | Huidig ingelogde gebruiker opvragen  |
| GET     | `/api/keys/keyshare/{roomId}`| Ja (Bearer)  | Keyshare ophalen voor versleutelde kamer |

### Beveiliging

- **SHA-256 hashing** via `HashService` — wachtwoorden worden nooit in plaintext opgeslagen.
- **Timing-safe vergelijking** via `CryptographicOperations.FixedTimeEquals` — beschermt tegen timing-aanvallen.
- **Lockout** na 3 foutieve inlogpogingen — account wordt geblokkeerd (HTTP 403).
- **JWT-tokens** verlopen na 2 uur.
- **Opmerking over salting**: SHA-256 zonder salt is kwetsbaar voor rainbow table-aanvallen. Dit is conform de lesvereisten, maar in productie zou bcrypt of Argon2 de voorkeur hebben.

---

## Deel 2 — Encryptie (Persoon 2)

- Minstens 2 kamers zijn versleuteld via X.509/CMS (`.enc` bestanden).
- Decryptie vereist twee elementen:
  1. **Keyshare** — opgehaald via `/api/keys/keyshare/{roomId}` met een geldig JWT-token.
  2. **Passphrase** — te vinden ergens in de game zelf.
- Alleen de combinatie van beide kan een kamer ontgrendelen.

---

## Deel 3 — Secure Coding (Persoon 3)

### Input validatie in de game

Alle console-invoer wordt defensief verwerkt:

- `Console.ReadLine()` wordt nooit rechtstreeks gebruikt zonder null-check.
- Lege of witruimte-invoer wordt genegeerd (geen crash).
- `Split` gebruikt `RemoveEmptyEntries` zodat dubbele spaties geen problemen geven.
- `Enum.TryParse` met `ignoreCase: true` — ongeldige richtingen geven een duidelijke foutmelding, geen exception.
- Commando's zonder verplicht argument (`go`, `take`) geven een hulpboodschap in plaats van te crashen.

### Foutafhandeling bij API-communicatie

- `HttpRequestException` wordt opgevangen → duidelijke melding zonder stacktrace.
- Alle andere uitzonderingen worden ook opgevangen → geen interne details lekken naar de gebruiker.
- HTTP 403 (account vergrendeld) geeft een specifieke melding.
- Na 3 mislukte loginpogingen in de game stopt de applicatie netjes.

### Wachtwoord invoer

- Wachtwoord wordt gemaskeerd ingevoerd (sterretjes `*`) via `Console.ReadKey(intercept: true)`.
- Backspace werkt correct tijdens invoer.
- Fallback naar gewone `ReadLine()` als de console geen `ReadKey` ondersteunt (testomgeving).

### Secrets beheer

- De JWT-secret staat **nooit** hardcoded in broncode of `appsettings.json`.
- In development: gebruik `dotnet user-secrets` (zie Hoe starten).
- In productie: gebruik een omgevingsvariabele `Jwt__Key`.
- De API valideert bij opstart of de secret correct geconfigureerd is en weigert te starten als dat niet het geval is.
- `secrets.json` en `.env` bestanden staan in `.gitignore`.
- JWT-token wordt in het geheugen bewaard (nooit op schijf of hardcoded).

---

## Deel 4 — Integratie API & Game (Persoon 4)

- Communicatie via HTTPS (`https://localhost:7298`).
- JWT-token wordt meegestuurd als `Authorization: Bearer <token>` header.
- Login is verplicht voor spelstart — geen token = geen spel.
- Dev-certificaat wordt geaccepteerd via `DangerousAcceptAnyServerCertificateValidator` (enkel development).

---

## ZIP-inhoud checklist

- [x] Volledige broncode (MinimalApi-Auth, TextAdventure, tests)
- [x] `.enc` bestanden (versleutelde kamers — persoon 2)
- [x] `secrets.json` (apart, niet in Git)
- [x] `README.md` (dit bestand)
