using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Encryption
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("╔══════════════════════════════════════════╗");
            Console.WriteLine("║   TextAdventure — EncryptionTool         ║");
            Console.WriteLine("╚══════════════════════════════════════════╝");
            Console.WriteLine();

            // ── Config ────────────────────────────────────────────────────
            const string keyshareVault = "a3f9e2b1c4d7f0e5a8b2c6d9";
            const string passphrase = "OpenSesame";
            string outDir = AppContext.BaseDirectory;
            string pfxPath = Path.Combine(outDir, "CMS_Encryption_Key.pfx");
            string pfxPassword = Sha256Hex($"{keyshareVault}:{passphrase}");

            Console.WriteLine($"Output map : {outDir}");
            Console.WriteLine($"PFX wachtwoord (SHA256): {pfxPassword}");
            Console.WriteLine();

            // ── Stap 1: Genereer RSA sleutel + zelfondertekend certificaat ─
            Console.WriteLine("[1/3] RSA-certificaat aanmaken...");

            RSA rsa = RSA.Create(2048);

            var certReq = new CertificateRequest(
                "CN=TextAdventure_CMS_Key", rsa,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certReq.CertificateExtensions.Add(new X509KeyUsageExtension(
                X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment, true));

            using X509Certificate2 cert = certReq.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddMinutes(-5),
                DateTimeOffset.UtcNow.AddYears(10));

            // ── Stap 2: Exporteer naar PFX ────────────────────────────────
            Console.WriteLine("[2/3] PFX exporteren...");

            byte[] pfxBytes = cert.Export(X509ContentType.Pfx, pfxPassword);
            File.WriteAllBytes(pfxPath, pfxBytes);
            Console.WriteLine($"      → {pfxPath}");
            Console.WriteLine();

            // ── Stap 3: Versleutel kamerinhoud (hybride: AES-256 + RSA-OAEP)
            // RSA-2048 kan max ~190 bytes direct versleutelen.
            // Oplossing: genereer een willekeurige AES-256 sleutel, versleutel
            // de kamerinhoud daarmee, en versleutel alleen de AES-sleutel met RSA.
            // Bestandsformaat: base64(RSA-OAEP(aesKey:aesIV))::base64(AES-256-CBC(inhoud))

            Console.WriteLine("[3/3] Kamerinhoud versleutelen...");

            var kamers = new Dictionary<string, string>
            {
                ["room_vault"] =
                    "=== SECRET VAULT ===\n" +
                    "Gefeliciteerd, Speler!\n" +
                    "Je hebt de Secret Vault succesvol ontgrendeld.\n" +
                    "Binnenin vind je een gloeiende orb die zacht zoemt van energie.\n" +
                    "De inscriptie op de muur leest:\n" +
                    "  'Alleen zij die de sleutel der kennis dragen, mogen betreden.'\n" +
                    "Je pakt de Orb of Power — jouw queeste is voltooid!",

                ["room_godmode"] =
                    "=== ADMIN SANCTUM ===\n" +
                    "Welkom, Administrator.\n" +
                    "Je hebt toegang gekregen tot het verboden Admin Sanctum.\n" +
                    "De kamer is bekleed met oude servers die knipperen in het donker.\n" +
                    "Een terminal gloeit met de boodschap:\n" +
                    "  'Root access verleend. Alle systemen zijn van jou.'\n" +
                    "Je hebt de Admin Kroon verkregen — absolute macht is van jou."
            };

            using RSA rsaPub = cert.GetRSAPublicKey()!;

            foreach (var (roomId, inhoud) in kamers)
            {
                // Genereer willekeurige AES sleutel + IV
                byte[] aesKey = RandomNumberGenerator.GetBytes(32); // 256-bit
                byte[] aesIv = RandomNumberGenerator.GetBytes(16); // 128-bit

                // Versleutel inhoud met AES-256-CBC
                byte[] plainBytes = Encoding.UTF8.GetBytes(inhoud);
                byte[] cipherBytes;
                using (var aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using var ms = new MemoryStream();
                    using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    cs.Write(plainBytes);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();
                }

                // Versleutel AES-sleutel + IV met RSA-OAEP
                string keyBlob = $"{Convert.ToHexString(aesKey).ToLower()}:{Convert.ToHexString(aesIv).ToLower()}";
                byte[] encKeyBlob = rsaPub.Encrypt(Encoding.ASCII.GetBytes(keyBlob), RSAEncryptionPadding.OaepSHA256);

                // Schrijf naar bestand
                string uitvoer = $"{Convert.ToBase64String(encKeyBlob)}::{Convert.ToBase64String(cipherBytes)}";
                string uitvoerPad = Path.Combine(outDir, $"{roomId}.enc");
                File.WriteAllText(uitvoerPad, uitvoer);
                Console.WriteLine($"      → {uitvoerPad}");
            }

            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────────────────");
            Console.WriteLine("✅  Klaar! Gegenereerde bestanden:");
            Console.WriteLine($"    CMS_Encryption_Key.pfx        ← geef pad op in het spel");
            Console.WriteLine($"    room_vault_encrypted.txt       ← kopieer naar TextAdventure bin map");
            Console.WriteLine($"    room_godmode_encrypted.txt     ← kopieer naar TextAdventure bin map");
            Console.WriteLine("──────────────────────────────────────────────────────────");

            // ── Helper ────────────────────────────────────────────────────
            static string Sha256Hex(string input)
            {
                byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }
    }
}
