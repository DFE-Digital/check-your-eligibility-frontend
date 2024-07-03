import DoYouHaveNassNumberPage from '../../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../../support/PageObjects/EnterDetailsPage'
import EnterChildDetailsPage from '../../../support/PageObjects/EnterChildDetailsPage'
import { authenticator } from 'otplib';

describe('Parent with valid details can carry out Eligibility Check', () => {
  const doYouHaveNassNumPage = new DoYouHaveNassNumberPage();
  const enterDetailsPage = new EnterDetailsPage();
  const enterChildDetailsPage = new EnterChildDetailsPage();

  it('Verify user can enter Child details', () => {
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

    const authorizationHeader = Cypress.env('AUTHORIZATION_HEADER');
    cy.intercept('GET', "https://signin.integration.account.gov.uk/**", (req) => {
      req.headers['Authorization'] = authorizationHeader;
    }).as('interceptForGET');

    cy.contains("Go to OneGov").click();

    var otpCode = authenticator.generate(Cypress.env('AUTH_SECRET'));

    cy.origin('https://signin.integration.account.gov.uk', { args: { otpCode } }, ({ otpCode }) => {
      let currentUrl = "";
      cy.url().then((url) => {
          currentUrl = url;
      });
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
      cy.get('input[name=code]').type(otpCode);
      cy.contains("Continue").click();
    });

    cy.verifyH1Text('Provide details of your children');
    cy.typeIntoInput(enterChildDetailsPage.getFieldSelector("Child One first name"), "Tom");
    cy.typeIntoInput(enterChildDetailsPage.getFieldSelector("Child One last name"), "Smith");
    cy.enterDate(enterChildDetailsPage.childOnedaySelector, enterChildDetailsPage.childOnemonthSelector, enterChildDetailsPage.childOneyearSelector, '01', '01', '1990');
    cy.typeIntoInput(enterChildDetailsPage.getFieldSelector("Child One school name"), "Hinde House 2-16 Academy");
    cy.clickButton('Save and continue');
    cy.get('h1').should('have.text', 'Check your answers before registering');

  });


  it('Verify user can enter multiple children details', () => {
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
    cy.wait(2000)
    cy.get('h1').should('have.text', 'Your children are entitled to free school meals');

    const authorizationHeader = Cypress.env('AUTHORIZATION_HEADER');
    cy.intercept('GET', "https://signin.integration.account.gov.uk/**", (req) => {
      req.headers['Authorization'] = authorizationHeader;
    }).as('interceptForGET');

    cy.contains("Go to OneGov").click();

    var otpCode = authenticator.generate(Cypress.env('AUTH_SECRET'));

    cy.origin('https://signin.integration.account.gov.uk', { args: { otpCode } }, ({ otpCode }) => {
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
      cy.get('input[name=code]').type(otpCode);
      cy.contains("Continue").click();
    });

    cy.verifyH1Text('Provide details of your children');
    cy.typeIntoInput(enterChildDetailsPage.getFieldSelector("Child One first name"), "Tom");
    cy.typeIntoInput(enterChildDetailsPage.getFieldSelector("Child One last name"), "Smith");
    cy.enterDate(enterChildDetailsPage.childOnedaySelector, enterChildDetailsPage.childOnemonthSelector, enterChildDetailsPage.childOneyearSelector, '01', '01', '1990');
    cy.typeIntoInput(enterChildDetailsPage.getFieldSelector("Child One school name"), "Hinde House 2-16 Academy");
    cy.clickButton('Save and continue');
    cy.get('h1').should('have.text', 'Check your answers before registering');

  });

});


