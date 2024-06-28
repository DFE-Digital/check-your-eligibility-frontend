import EnterDetailsPage from '../../../support/PageObjects/EnterDetailsPage'
import EnterChildDetailsPage from '../../../support/PageObjects/EnterChildDetailsPage'
import { authenticator } from 'otplib';

describe('Verify user can navigate to One Gov login ', () => {
    const enterDetailsPage = new EnterDetailsPage();
    const enterChildDetailsPage = new EnterChildDetailsPage();

    // Get the secret key from Cypress environment variables
    const secret: string | undefined = Cypress.env('AUTH_SECRET');
    //Ensure the secret key is defined
    if (!secret) {
        throw new Error('Authenticator secret key is not defined in Cypress environment variables');
    }
    // Generate the OTP code
    const otp: string = authenticator.generate(secret);

    it('Verify user can login successfully using OneGov login', () => {
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
                cy.get("input[name=email]").type(Cypress.env("CYPRESS_ONEGOV_EMAIL"));
                cy.contains("Continue").click();
                cy.get("input[name=password]").type(Cypress.env('CYPRESS_ONEGOV_PASSWORD'));
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

    it ('Verify user cannot login with incorrect OneGov login details', () => {
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

        cy.origin('https://signin.integration.account.gov.uk', { args: { otp } }, ({ otp }) => {
            cy.wait(2000);
            cy.visit('https://signin.integration.account.gov.uk/sign-in-or-create', {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });
            cy.contains("Sign in").click();
            cy.get("input[name=email]").type("ONEGOV_EMAIL");
            cy.contains("Continue").click();
          //  cy.get("input[name=password]").type('ONEGOV_PASSWORD');
           // cy.contains("Continue").click();


            cy.get('.govuk-error-summary__body > .govuk-list > li').contains('Enter an email address in the correct format, like name@example.com')
        });

    });
})
