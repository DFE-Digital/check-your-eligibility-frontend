// cypress/e2e/enterYourDetails.cy.ts
import DoYouHaveNassNumberPage from '../../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../../support/PageObjects/EnterDetailsPage'
import { authenticator } from 'otplib';

describe('Parent with valid details can carry out Eligibility Check', () => {
  const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
  const enterDetailsPage = new EnterDetailsPage();


  // Get the secret key from Cypress environment variables
  const secret: string | undefined = Cypress.env('AUTH_SECRET');
  //Ensure the secret key is defined
  if (!secret) {
    throw new Error('Authenticator secret key is not defined in Cypress environment variables');
  }
  // Generate the OTP code
  const otp: string = authenticator.generate(secret);

  it.only('Verify Parent Eligibility Check using eligible NI number', () => {
    cy.visit('/');
    cy.verifyH1Text('Check if your children can get free school meals');
    cy.clickButtonByRole('Start Now');
    cy.get('h1').should('have.text', 'Enter your details');
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    cy.selectYesNoOption(enterDetailsPage.getRadioSelector(), false);
    cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    cy.clickButton('Save and continue');
    cy.get('h1').should('have.text', 'Your children are entitled to free school meals');

    const authorizationHeader = 'Basic aW50ZWdyYXRpb24tdXNlcjp3aW50ZXIyMDIx';
    cy.intercept('GET', "https://signin.integration.account.gov.uk/**", (req) => {
      req.headers['Authorization'] = authorizationHeader;
    }).as('interceptForGET');

    cy.contains("Go to OneGov").click();
    // Use the custom command to generate the OTP
    cy.generateOtp().then((otp) => {
      cy.origin('https://signin.integration.account.gov.uk', { args: { otp } }, ({ otp }) => {
        cy.wait(2000);
        cy.visit('https://signin.integration.account.gov.uk/sign-in-or-create', {
          auth: {
            username: Cypress.env('AUTH_USERNAME'),
            password: Cypress.env('AUTH_PASSWORD')
          },
        });
        cy.contains("Sign in").click();
        cy.get("input[name=email]").type(Cypress.env("ONEGOV_EMAIL"));
        cy.contains("Continue").click();
        cy.get("input[name=password]").type(Cypress.env('ONEGOV_PASSWORD'));
        cy.contains("Continue").click();
        cy.get('h1').invoke('text').then((actualText: string) => {
          expect(actualText.trim()).to.eq('Enter the 6 digit security code shown in your authenticator app');
        });
        cy.get('input[name=code]').type(otp);
        cy.contains("Continue").click();
      });

      cy.verifyH1Text('Provide details of your children');
    });

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


