namespace CheckYourEligibility_FrontEnd.Models
{
    public class FsmApplication
    {
        private Parent _parent { get; set; }
        private Children _children { get; set; }

        public FsmApplication(Parent parent, Children children)
        {
            _parent = parent;
            _children = children;
        }
    }
}
