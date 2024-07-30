using CsvHelper.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace CheckYourEligibility_FrontEnd.Models
{
    [ExcludeFromCodeCoverage]
    public class CheckRow
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string Ni { get; set; }
        public string Nass { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CheckRowRowMap : ClassMap<CheckRow>
    {
        public CheckRowRowMap()
        {
            Map(m => m.FirstName).Index(0);
            Map(m => m.LastName).Index(1);
            Map(m => m.DOB).Index(2);
            Map(m => m.Ni).Index(3);
            Map(m => m.Nass).Index(4);
        }
    }
}
