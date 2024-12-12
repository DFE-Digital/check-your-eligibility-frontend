describe('Radio buttons on the SchoolList page should route to correct location', () => {
    
    beforeEach(() => {
        // Ensure the test starts on the correct page
        cy.visit('/');
        cy.contains('Start now').click();

    });
    
    it('Routes the user to Checl/Enter_Details when yes', () => {
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.get('button.govuk-button').click();

        cy.url().should('include', '/Check/Enter_Details');
    });

    it('Routes the user to Gov.uk site when no', () => {
        cy.get('input.govuk-radios__input[value="false"]').check();
        cy.get('button.govuk-button').click();

        cy.url().should('include', '/apply-free-school-meals');
    });
    
    it('Should provide an error when neither selected',() => {
        cy.get('button.govuk-button').click();
        cy.get('.govuk-error-message').should('contain', 'Select yes if any of your children go to these schools');
    });
});
