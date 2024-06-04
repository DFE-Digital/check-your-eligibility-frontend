// cypress/e2e/enterYourDetails.cy.ts

import EnterYourDetailsPage from '../../support/PageObjects/EnterYourDetailsPage';
import DoYouHaveNassNumberPage from '../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../support/PageObjects/EnterDetailsPage'

describe('Parent with valid details can carry out Eligibility Check', () => {
  const enterYourDetailsPage = new EnterYourDetailsPage();
  const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
  const enterDetailsPage = new EnterDetailsPage();


  it('Complete Parent Eligibility Check using NI number', () => {
    cy.visit('/');
    cy.clickButtonByRole('Start Now');
    cy.verifyH1Text('Enter your details');
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), true);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    cy.clickButton('Save and continue');
    
  });
});


