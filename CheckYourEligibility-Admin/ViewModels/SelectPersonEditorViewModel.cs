using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility_FrontEnd.ViewModels
{
    public class SelectPersonEditorViewModel
    {
        public bool Selected { get; set; }
        public ApplicationResponse Person { get; set; }
    }

    public class PeopleSelectionViewModel
    {
        public List<SelectPersonEditorViewModel> People { get; set; }
        public PeopleSelectionViewModel()
        {
            this.People = new List<SelectPersonEditorViewModel>();
        }


        public IEnumerable<string> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected people:
            return (from p in this.People where p.Selected select p.Person.Id).ToList();
        }
    }
}
