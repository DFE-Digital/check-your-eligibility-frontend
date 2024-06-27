// cypress/e2e/enterYourDetails.cy.ts
import DoYouHaveNassNumberPage from '../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../support/PageObjects/EnterDetailsPage'
const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
const enterDetailsPage = new EnterDetailsPage();

describe('Parent with valid details can carry out Eligibility Check', () => {
  it('Verify Parent Eligibility Check using eligible NI number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals');
    cy.clickButtonByRole('Start Now');
    cy.verifyH1Text('Enter your details')
    cy.get('h1').should('have.text', 'Enter your details');
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    cy.clickButton('Save and continue');
    cy.verifyH1Text('Your children are entitled to free school meals')
    cy.get('h1').should('have.text', 'Your children are entitled to free school meals');
  });


  it('Complete Parent Eligibility Check using NASS number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1'.trim()).should('have.text', 'Enter your details')
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Johnson");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), true);
    cy.clickButton('Save and continue');
    cy.verifyH1Text('Do you have an asylum support reference number?')
    cy.selectYesNoOption(doYouHaveNassNumPage.getSelector(), true);
    cy.typeIntoInput(doYouHaveNassNumPage.getFieldSelector("NASS Number"), "123456789")
    cy.clickButton('Save and continue');
    cy.verifyH1Text('Your children are entitled to free school meals')
  });
});


describe('Parent with ineligble information', () => {

  it('Verify Parent Eligibility Check using ineligible NI number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals');
    cy.clickButtonByRole('Start Now');
    cy.verifyH1Text('Enter your details')
    cy.get('h1').should('have.text', 'Enter your details');
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB152326C");
    cy.clickButton('Save and continue');
    cy.waitForElementToDisappear('h1');
    cy.verifyH1Text('Your children are entitled to free school meals')
    cy.get('h1').should('have.text', 'Your children are entitled to free school meals');
  });


  it('Verify Parent Eligibility Check using ineligible NASS number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals')
    cy.clickButtonByRole('Start Now');
    cy.get('h1'.trim()).should('have.text', 'Enter your details')
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Johnson");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), true);
    cy.clickButton('Save and continue');
    cy.verifyH1Text('Do you have an asylum support reference number?')
    cy.selectYesNoOption(doYouHaveNassNumPage.getSelector(), true);
    cy.typeIntoInput(doYouHaveNassNumPage.getFieldSelector("NASS Number"), "123456777")
    cy.clickButton('Save and continue');
    cy.verifyH1Text('Your children are entitled to free school meals')
  });

  it.only('Verify Parent Eligibility Check using valid NASS number and invalid date of birth', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals');
    cy.clickButtonByRole('Start Now');
    cy.verifyH1Text('Enter your details')
    cy.get('h1').should('have.text', 'Enter your details');
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '03', '2000');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    cy.clickButton('Save and continue');
    cy.verifyH1Text('Your children are entitled to free school meals')
    cy.get('h1').should('have.text', 'Your children are entitled to free school meals');
  });
})


describe('Check validation', () => {

  it.only('Verify Parent Eligibility Check using valid NASS number and invalid date of birth', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals');
    cy.clickButtonByRole('Start Now');
    cy.verifyH1Text('Enter your details')
    cy.clickButton('Save and continue');
  })
})
