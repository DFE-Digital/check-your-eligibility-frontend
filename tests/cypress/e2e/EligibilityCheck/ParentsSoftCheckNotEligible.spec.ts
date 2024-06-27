// cypress/e2e/enterYourDetails.cy.ts

import EnterYourDetailsPage from '../../support/PageObjects/EnterYourDetailsPage';
import DoYouHaveNassNumberPage from '../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../support/PageObjects/EnterDetailsPage'

describe('Parent with valid details can carry out Eligibility Check', () => {
  const enterYourDetailsPage = new EnterYourDetailsPage();
  const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
  const enterDetailsPage = new EnterDetailsPage();


  it.only('Verify Ineligible Parents Redirect to Not Eligible Screen ', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1').should('have.text', 'Enter your details')
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Bloggs");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    cy.clickButton('Save and continue');
    cy.get('h1').should('have.text', 'We could not check your children’s entitlement to free school meals')

    
  });

  it('Verify Redirect to We Could Not Check Your Childrens Entitlement Screen When Parents Do Not Provide NI or NASS Number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1'.trim()).should('have.text', 'Enter your details')
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.clickButton('Save and continue');
    cy.get('h1'.trim()).should('have.text', 'Do you have an asylum support reference number?')
    cy.clickButton('Save and continue');
    cy.get('h1'.trim()).should('have.text', 'We could not check your children’s entitlement to free school meals')        
  });

});


describe('Verify validation messages on Enter your details page', () => {

    it('Verify validation error for Parents name field', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1'.trim()).should('have.text', 'Enter your details')
    cy.clickButton('Save and continue');
    cy.get('a.govuk-error-message[href="#FirstName"]').should('have.text', 'First Name is required')
    });

})