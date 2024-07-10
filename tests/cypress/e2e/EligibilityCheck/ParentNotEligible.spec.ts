
describe('Parents journey when not eligible', () => {

    it('Will not return the correct responses if the Parent is not eligible for free school meals', () => {

        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start Now').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Jones');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#IsNassSelected').click();
        cy.get('#NationalInsuranceNumber').type('AB123456C');

        cy.contains('Save and continue').click();

        cy.url().should('include', '/Check/Loader');
        cy.get('h1').should('include.text', 'Your children may not be entitled to free school meals');
    });

    it('Will return the correct response if we cannot find the user', () => {

        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start Now').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Stevens');

        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#IsNassSelected').click();
        cy.get('#NationalInsuranceNumber').type('AB123456C');

        cy.contains('Save and continue').click();

        cy.url().should('include', '/Check/Loader');
        cy.get('h1').should('include.text', "We could not check your children’s entitlement to free school meals");
    });
    
});