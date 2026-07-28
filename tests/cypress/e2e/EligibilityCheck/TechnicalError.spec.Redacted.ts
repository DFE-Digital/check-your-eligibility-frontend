//Test set as Redacted due to the length of time a tech error response takes to return, so we do not want to
//  include this in regular test runs. The timeout is set to how long it will keep checking rather than a 
// fixed wait, but our intention is to see if we can generate the response faster before including
// this test more permanently.

import { GOV_UK_ONE_LOGIN_SITE, GOV_UK_ONE_LOGIN_URL } from "../../support/constants";

let schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
let schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";

describe('TechnicalError outcome should display Error Code and CorrelationID', () => {

    let lastName = Cypress.env('lastName');

    it('TechnicalError outcome should display Error Code', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click()

        cy.get('[id="SelectedSchoolURN"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolListResults', {timeout: 5000})
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})
        cy.contains('Continue').click();

        cy.url().should('include', '/Home/SchoolInPrivateBeta');
        cy.get('h1').should('include.text', 'You can use this test service');
        cy.contains('Check your eligibility').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('TESTER');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1980');
        cy.get('input[type="radio"][value="true"]').click();
        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').type('XX123456C')
        cy.contains('Save and continue').click();
        cy.get('h1',{ timeout: 120000 }).should('include.text', 'Check failed');
        cy.get('body').should('include.text', 'Error code: STE50');
        cy.get('body').should('include.text', 'Correlation ID:'); //Only shown if Guid was available from the check
    });
});