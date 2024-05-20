namespace CheckYourEligibility_FrontEnd.Models
{
    public class FsmApplication
    {
        private ParentGuardian _parent { get; set; }
        private Children _children { get; set; }

        public FsmApplication(ParentGuardian parent, Children children)
        {
            _parent = parent;
            _children = children;
        }
    }
}
