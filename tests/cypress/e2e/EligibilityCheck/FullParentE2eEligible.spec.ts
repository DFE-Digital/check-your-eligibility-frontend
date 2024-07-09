import DoYouHaveNassNumberPage from '../../support/PageObjects/DoYouHaveNassNumberPage';
import EnterDetailsPage from '../../support/PageObjects/EnterDetailsPage'
import  StartNowPage from '../../support/PageObjects/StartNowPage';
import EnterChildDetailsPage from '../../support/PageObjects/EnterChildDetailsPage'
import { authenticator } from 'otplib';


describe('Parent with valid details can complete full Eligibility check and application', () => {
    const startNowPage = new StartNowPage();
    const enterDetailsPage = new EnterDetailsPage();

    it('Parent can make the full journey using correct details', () => {
        cy.visit('/');
        cy.verifyH1Text('Check if your children can get free school meals');
        // startNowPage.clickStartNowBtn();
        cy.contains('Start Now').click()
        cy.url().should('include', '/Check/Enter_Details');
        // cy.verifyH1Text('Enter your details');
        cy.get('h1').should('include.text', 'Enter your details');
        // cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
        // cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
        // cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Smith');
        cy.get('#Day').should('be.visible').type('01');
        cy.get('#Month').should('be.visible').type('01');
        cy.get('#Year').should('be.visible').type('1990');
        // cy.selectYesNoOption(enterDetailsPage.getRadioSelector(),false);
        cy.get('#IsNassSelected').click();
        // cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
        cy.get('#NationalInsuranceNumber').should('be.visible').type('AB123456C');
        // cy.clickButton('Save and continue');
        cy.contains('Save and continue').click();
        cy.url().should('include', '/Check/Loader');
        // cy.verifyH1Text('Enter your details');
        cy.get('h1').should('include.text', 'Your children are entitled to free school meals');

        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', 'https://signin.integration.account.gov.uk/**', (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.contains('Go to OneGov').click();

        var otpCode = authenticator.generate(Cypress.env('AUTH_SECRET'));

        cy.origin('https://signin.integration.account.gov.uk', { args: { otpCode }}, ({ otpCode }) => {
            let currentUrl = "";
            cy.url().then((url) => {
                currentUrl = url;
            });
            cy.wait(2000);

            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

            cy.contains('Sign in').click();

            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();

            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();

            cy.wait(2000);

            // cy.get('h1').invoke('text').then((actualText: string) => {
            //     expect(actualText.trim()).to.eq('Enter the 6 digit security code shown in your authenticator app');
            // });

            cy.get('h1').should('include.text', 'Enter the 6 digit security code shown in your authenticator app' );

            cy.get('input[name=code]').type(otpCode);
            cy.contains('Continue').click();
        });

        // cy.get('h1').should('include.text', 'Provide details of your children');



    });

    // it('Parent can enter their details successfully', () => {
    //     cy.visit('/Check/Enter_Details');
    //     cy.verifyH1Text('Enter your details');
    //     cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's first name"), "Tim");
    //     cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's last name"), "Smith");
    //     cy.enterDate(enterDetailsPage.daySelector, enterDetailsPage.monthSelector, enterDetailsPage.yearSelector, '01', '01', '1990');
    //     cy.selectYesNoOption(enterDetailsPage.getRadioSelector(),false); // fix false issue
    //     cy.typeIntoInput(enterDetailsPage.getFieldSelector("Parent's National Insurance number"), "AB123456C");
    //     cy.clickButton('Save and continue');
    //     cy.url().should('include', '/Check/Loader');
    //     cy.verifyH1Text('Your children are entitled to free school meals');
    // })

    // it('Parents can go to OneGov and sign in', () => {
    //     cy.visit('/Check/Loader');
    //     cy.verifyH1Text('Your children are entitled to free school meals');
    // })
})