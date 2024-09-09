using CsvHelper.Configuration.Attributes;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class BatchFSMExport
    {
        [Name("Parent First Name")]
        public string FirstName { get; set; }

        [Name("Parent Last Name")]
        public string LastName { get; set; }

        [Name("Parent Date of Birth")]
        public string DOB { get; set; }

        [Name("Parent NI Number")]
        public string NI { get; set; }

        [Name("Parent Asylum Seeker Reference Number")]
        public string NASS { get; set; }

        [Name("Outcome")]
        public string Outcome { get; set; }
    }
}
