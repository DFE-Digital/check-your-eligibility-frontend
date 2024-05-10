
class EnterYourDetailsPage {
    elements = {
      firstNameInputField: () => cy.get('#FirstName'),
      lastNameInputField: () => cy.get('#LastName')
  
    }
      enterParentDetails() {
        cy.contains('button', 'Start now').click();
      }
    }
    
    export default EnterYourDetailsPage;
    