describe('Parent or Guardian without an NI or NASS will be redirected to correct page', () => {

    it('Will redirect the parent or guardian to the correct dropout page if no NI or NASS is given', () => {

        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start Now').click()

        cy.url().should('include', '/Check/Enter_Details');

        cy.get('h1').should('include.text', 'Enter your details');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('GRIFFIN');
        cy.get('#Day').should('be.visible').type('31');
        cy.get('#Month').should('be.visible').type('12');
        cy.get('#Year').should('be.visible').type('2000');

        cy.get('input[type="radio"][value="false"]').click();
        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('input[type="radio"][value="false"]').click();
        cy.contains('Save and continue').click();

        cy.get('.govuk-grid-column-full').find('h1').should('include.text', "We could not check your children’s entitlement to free school meals")
    })
})