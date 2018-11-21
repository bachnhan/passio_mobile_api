using Microsoft.AspNet.Identity;
using System.Web.Configuration;
using System.Web.Security;

namespace Wisky.SkyAdmin.Manage.Helpers
{
    public class MP5Hasher:PasswordHasher
    {
        public MP5Hasher(FormsAuthPasswordFormat format)
        {
            Format = format;
        }
        public FormsAuthPasswordFormat Format { get; set; }
        public override string HashPassword(string password)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(password, Format.ToString());
        }

        public override PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            var testHash = FormsAuthentication.HashPasswordForStoringInConfigFile(providedPassword, Format.ToString());
            return testHash.Equals(hashedPassword) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }

    }
}