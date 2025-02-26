describe('Parent Date of Birth Validation Tests', () => {
    beforeEach(() => {
        cy.visit('/Check/Enter_Details');
    });

    it('displays error messages for missing date fields', () => {
        cy.get('#DateOfBirth\\.Day').clear();
        cy.get('#DateOfBirth\\.Month').clear();
        cy.get('#DateOfBirth\\.Year').clear();
        cy.contains('Save and continue').click();

        cy.get('.govuk-error-message').should('contain', 'Enter a date of birth');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');
    });

    it('displays error for single missing field', () => {
        cy.get('#DateOfBirth\\.Day').clear();
        cy.get('#DateOfBirth\\.Month').clear().type('6');
        cy.get('#DateOfBirth\\.Year').clear().type('1990');
        cy.contains('Save and continue').click();
        cy.get('.govuk-error-message').should('contain', 'Date of birth must include a day');

        cy.get('#DateOfBirth\\.Day').clear().type('15');
        cy.get('#DateOfBirth\\.Month').clear();
        cy.get('#DateOfBirth\\.Year').clear().type('1990');
        cy.contains('Save and continue').click();
        cy.get('.govuk-error-message').should('contain', 'Date of birth must include a month');
    });

    it('displays error messages for non-numeric inputs', () => {
        cy.get('#DateOfBirth\\.Day').clear().type('abc');
        cy.get('#DateOfBirth\\.Month').clear().type('xyz');
        cy.get('#DateOfBirth\\.Year').clear().type('abcd');
        cy.contains('Save and continue').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for out-of-range inputs', () => {
        cy.get('#DateOfBirth\\.Day').clear().type('50');
        cy.get('#DateOfBirth\\.Month').clear().type('13');
        cy.get('#DateOfBirth\\.Year').clear().type('1800');
        cy.contains('Save and continue').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for future dates', () => {
        cy.get('#DateOfBirth\\.Day').clear().type('01');
        cy.get('#DateOfBirth\\.Month').clear().type('01');
        cy.get('#DateOfBirth\\.Year').clear().type((new Date().getFullYear() + 1).toString());
        cy.contains('Save and continue').click();

        cy.get('.govuk-error-message').should('contain', 'Enter a date in the past');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for invalid combinations', () => {
        cy.get('#DateOfBirth\\.Day').clear().type('31');
        cy.get('#DateOfBirth\\.Month').clear().type('02');
        cy.get('#DateOfBirth\\.Year').clear().type('2020');
        cy.contains('Save and continue').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for invalid year', () => {
        cy.get('#DateOfBirth\\.Day').clear().type('1');
        cy.get('#DateOfBirth\\.Month').clear().type('1');
        cy.get('#DateOfBirth\\.Year').clear().type('1800');
        cy.contains('Save and continue').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');
    });

    it('allows valid date of birth submission', () => {
        cy.get('#DateOfBirth\\.Day').clear().type('15');
        cy.get('#DateOfBirth\\.Month').clear().type('06');
        cy.get('#DateOfBirth\\.Year').clear().type('1990');
        cy.contains('Save and continue').click();

        cy.get('#DateOfBirth\\.Day + .govuk-error-message').should('not.exist');
        cy.get('#DateOfBirth\\.Month + .govuk-error-message').should('not.exist');
        cy.get('#DateOfBirth\\.Year + .govuk-error-message').should('not.exist');
        cy.get('#DateOfBirth\\.Day').should('not.have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('not.have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('not.have.class', 'govuk-input--error');
    });
});