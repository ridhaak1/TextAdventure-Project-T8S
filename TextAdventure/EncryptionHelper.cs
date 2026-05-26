using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TextAdventure
{
    // Hulpklasse voor X.509/CMS versleuteling van kamers
    public static class EncryptionHelper
    {
        // Geeft het volledige pad naar een bestand naast de exe
        private static string Pad(string bestand) =>
            Path.Combine(AppContext.BaseDirectory, bestand);

        // Maak een zelfondertekend X.509 certificaat aan (RSA 2048)
        private static X509Certificate2 MaakCertificaat(string wachtwoord)
        {
            using var rsa = RSA.Create(2048);
            var aanvraag = new CertificateRequest(
                "cn=TextAdventure", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var cert = aanvraag.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
            var pfxBytes = cert.Export(X509ContentType.Pfx, wachtwoord);
            return new X509Certificate2(pfxBytes, wachtwoord, X509KeyStorageFlags.Exportable);
        }

        // Versleutel tekst via CMS EnvelopedCms met het certificaat
        private static byte[] Versleutel(string tekst, X509Certificate2 cert)
        {
            var inhoud = new ContentInfo(Encoding.UTF8.GetBytes(tekst));
            var cms = new EnvelopedCms(inhoud);
            cms.Encrypt(new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, cert));
            return cms.Encode();
        }

        // Ontsleutel een CMS EnvelopedCms bestand met het certificaat
        private static string Ontsleutel(byte[] data, X509Certificate2 cert)
        {
            var cms = new EnvelopedCms();
            cms.Decode(data);
            cms.Decrypt(new X509Certificate2Collection(cert));
            return Encoding.UTF8.GetString(cms.ContentInfo.Content);
        }

        // Genereer .pfx en .enc bestanden voor een kamer als ze nog niet bestaan
        // Wachtwoord = keyshare (van de API) + passphrase (gevonden in het spel)
        public static void InitialiseerKamer(string roomId, string keyshare, string passphrase, string inhoud)
        {
            var pfxPad = Pad($"{roomId}.pfx");
            var encPad = Pad($"{roomId}.enc");

            if (File.Exists(encPad)) return; // Al aangemaakt

            var wachtwoord = keyshare + passphrase;
            var cert = MaakCertificaat(wachtwoord);
            File.WriteAllBytes(pfxPad, cert.Export(X509ContentType.Pfx, wachtwoord));
            File.WriteAllBytes(encPad, Versleutel(inhoud, cert));
        }

        // Open een versleutelde kamer: keyshare komt van de API, passphrase van de speler
        public static string? OpenKamer(string roomId, string keyshare, string passphrase)
        {
            var pfxPad = Pad($"{roomId}.pfx");
            var encPad = Pad($"{roomId}.enc");

            if (!File.Exists(encPad)) return null;

            try
            {
                var cert = new X509Certificate2(pfxPad, keyshare + passphrase);
                return Ontsleutel(File.ReadAllBytes(encPad), cert);
            }
            catch
            {
                // Verkeerd wachtwoord of beschadigd bestand
                return null;
            }
        }
    }
}
