import EnterYourDetailsPage from '../../support/PageObjects/EnterYourDetailsPage'

describe('Parent with NI number can submit successful application - without login', () => {
  const enterYourDetailsPage = new EnterYourDetailsPage();

  it('Complete Parent Eligibility Check using NI number', () => {
    cy.visit('/');
    cy.clickButtonByRole('Start Now');
    enterYourDetailsPage.enterParentsInfo("Parent's first name", "John");
    enterYourDetailsPage.enterParentsInfo("Parent's last name", "Doe");
    enterYourDetailsPage.enterDateOfBirth('12', '08', '1985');
    enterYourDetailsPage.selectYesNoOption('Yes')
    enterYourDetailsPage.verifyField("Parent's National Insurance number");
    enterYourDetailsPage.enterParentsInfo("Parent's National Insurance number", "AB123456C")
    cy.clickButton('Save and continue');
   // 
  });
});

