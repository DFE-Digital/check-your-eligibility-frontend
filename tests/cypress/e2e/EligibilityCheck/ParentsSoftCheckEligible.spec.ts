// cypress/e2e/enterYourDetails.cy.ts

import EnterYourDetailsPage from '../../support/PageObjects/EnterYourDetailsPage';
import DoYouHaveNassNumberPage from '../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../support/PageObjects/EnterDetailsPage'

describe('Parent with valid details can carry out Eligibility Check', () => {
  const enterYourDetailsPage = new EnterYourDetailsPage();
  const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
  const enterDetailsPage = new EnterDetailsPage();


  it.only('Complete Parent Eligibility Check using NI number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1').should('have.text', 'Enter your details')
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), true);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    cy.clickButton('Save and continue');
    cy.get('h1').should('have.text', 'Your children are entitled to free school meals')
    cy.get('.govuk-button').click()
  });

  it('Complete Parent Eligibility Check using Nass number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1'.trim()).should('have.text', 'Enter your details')
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Johnson");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.clickButton('Save and continue');
   // cy.get('h1'.trim()).should('have.text', 'Your children are entitled to free school meals')
    cy.selectYesNoOption(doYouHaveNassNumPage.getSelector(), false);
  //  cy.typeIntoInput(doYouHaveNassNumPage.getFieldSelector("NASS Number"), "123456789")
    cy.clickButton('Save and continue');
   // cy.get('h1').should('have.text', 'Your children are entitled to free school meals')
    cy.get('.govuk-button').click
    
  });

});


