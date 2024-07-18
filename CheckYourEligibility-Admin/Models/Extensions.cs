using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility_DfeSignIn.Models;
using System.Numerics;

namespace CheckYourEligibility_FrontEnd.Models
{
    public static class Extensions
    {
        public static string GetFsmStatusDescription(this string status)
        {
            Enum.TryParse(status, out CheckEligibilityStatus statusEnum);

            switch (statusEnum)
            {
                case CheckEligibilityStatus.parentNotFound:
                    return "May not be entitled";
                case CheckEligibilityStatus.eligible:
                    return "Entitled";
                case CheckEligibilityStatus.notEligible:
                    return "Not Entitled";
                case CheckEligibilityStatus.DwpError:
                    return "Error";
                default:
                    return status.ToString();
            }
        }
    }

}
