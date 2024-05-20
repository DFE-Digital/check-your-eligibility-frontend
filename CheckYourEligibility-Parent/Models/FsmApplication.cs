namespace CheckYourEligibility_FrontEnd.Models
{
    public class FsmApplication
    {
        public string ParentFirstName { get; set; }
        public string ParentLastName { get; set; }
        public string ParentDateOfBirth { get; set; }
        public string ParentNass { get; set; }
        public string ParentNino {  get; set; }

        public Children Children { get; set; }

        public FsmApplication()
        {
               
        }
    }
}
