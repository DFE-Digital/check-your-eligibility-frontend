namespace CheckYourEligibility_FrontEnd.Models
{
    public class Children
    {
        public List<Child> ChildList { get; set; }

        public int ChildIndex { get; set; }

        public Children()
        {
            ChildList = new List<Child>();
        }
    }
}
