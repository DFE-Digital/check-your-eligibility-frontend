namespace CheckYourEligibility_FrontEnd.Models
{
    public class ApplicationSearch
    {
        public string LocalAuthority { get; set; }
        public string School { get; set; }
        public string Status { get; set; }
        public string ChildLastName { get; set; }
        public string ParentName { get; set; }
        public int ReferenceNumber { get; set; }
        public string ChildDateOfBirth { get; set; }
        public string ParentDateOfBirth { get; set; }
        public int? ChildDOBDay { get; set; }
        public int? ChildDOBMonth { get; set; }
        public int? ChildDOBYear { get; set; }
        public int? PGDOBDay { get; set; }
        public int? PGDOBMonth { get; set; }
        public int? PGDOBYear { get; set; }
        

    }
}
