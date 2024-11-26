describe('Date of Birth Validation Tests', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B';
    beforeEach(() => {
        
        cy.visit('/Check/Enter_Details'); // Ensure we are on the correct page
    });

    it('displays error messages and highlights fields for multiple errors', () => {
        // Scenario 1: All fields are empty
        
        cy.get('#DateOfBirth\\.Day').clear();
        cy.get('#DateOfBirth\\.Month').clear();
        cy.get('#DateOfBirth\\.Year').clear();
        cy.contains('Save and continue').click();

        // Assert that the error message for missing date of birth is shown
        cy.get('.govuk-error-message').and('contain', 'Enter a date of birth');
        // Assert fields are highlighted with error class
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');

        // Scenario 2: Invalid day, month, and year
        cy.get('#DateOfBirth\\.Day').clear().type('32'); // Invalid day
        cy.get('#DateOfBirth\\.Month').clear().type('12'); // Invalid month
        cy.get('#DateOfBirth\\.Year').clear().type('abcd'); // Invalid year
        cy.contains('Save and continue').click();

        

        // Assert fields are highlighted with error class
        cy.get('.govuk-error-message').and('contain', 'Enter a date using numbers only');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');

        // Scenario 3: Date in the future
        cy.get('#DateOfBirth\\.Day').clear().type('01');
        cy.get('#DateOfBirth\\.Month').clear().type('01');
        cy.get('#DateOfBirth\\.Year').clear().type((new Date().getFullYear() + 1).toString()); // Next year
        cy.contains('Save and continue').click();

        // Assert that the error message for a future date is shown
        cy.contains('Enter a date in the past').should('be.visible');
    });

    it('allows valid date of birth submission', () => {
        // Provide a valid date of birth
        cy.get('#DateOfBirth\\.Day').clear().type('15'); // Valid day
        cy.get('#DateOfBirth\\.Month').clear().type('06'); // Valid month
        cy.get('#DateOfBirth\\.Year').clear().type('2005'); // Valid year
        cy.contains('Save and continue').click();

        // Assert no error messages are shown
        cy.get('.govuk-error-message').and('not.contain', 'Enter a date of birth');
        // Assert that fields are not highlighted with error class
        cy.get('#DateOfBirth\\.Day').should('not.have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('not.have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('not.have.class', 'govuk-input--error');
    });
});
