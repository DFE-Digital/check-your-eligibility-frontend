describe("Links on not eligible page route to the intended locations", () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B';

    beforeEach(() => {
        cy.session("Session 1", () => {
            cy.SignInSchool();
            cy.wait(1); // Ensure session/login completes
        });
    });

    it("Guidance link should route to guidance page", () => {
        cy.visit("/Check/Enter_Details");
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.get('#EmailAddress').clear().type(parentEmailAddress);
        cy.contains('Perform check').click();
        cy.contains('a.govuk-link', 'See a complete list of acceptable evidence', { timeout: 8000 }).then(($link) => {
            const url = $link.prop('href');
            cy.visit(url);
            cy.get('h1.govuk-heading-l').should('contain.text', 'Guidance for reviewing evidence');
        });
    });

    it("Support link should route to DfE form", () => {
        cy.visit("/Check/Enter_Details");
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.get('#EmailAddress').clear().type(parentEmailAddress);
        cy.contains('Perform check').click();
        cy.get('a', { timeout: 80000 }).contains("ecs.admin@education.gov.uk");
    });
});

describe('Date of Birth Validation Tests', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B';
    beforeEach(() => {
        cy.SignInSchool();
        cy.wait(1); // Ensure session/login completes
        cy.visit('/Check/Enter_Details'); // Ensure we are on the correct page
    });

    it('displays error messages and highlights fields for multiple errors', () => {
        // Scenario 1: All fields are empty
        
        cy.get('#Day').clear();
        cy.get('#Month').clear();
        cy.get('#Year').clear();
        cy.contains('Perform check').click();

        // Assert that the error message for missing date of birth is shown
        cy.get('.govuk-error-message').and('contain', 'Enter a date of birth');
        // Assert fields are highlighted with error class
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');

        // Scenario 2: Invalid day, month, and year
        cy.get('#Day').clear().type('32'); // Invalid day
        cy.get('#Month').clear().type('12'); // Invalid month
        cy.get('#Year').clear().type('abcd'); // Invalid year
        cy.contains('Perform check').click();

        

        // Assert fields are highlighted with error class
        cy.get('.govuk-error-message').and('contain', 'Enter a date using numbers only');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');

        // Scenario 3: Date in the future
        cy.get('#Day').clear().type('01');
        cy.get('#Month').clear().type('01');
        cy.get('#Year').clear().type((new Date().getFullYear() + 1).toString()); // Next year
        cy.contains('Perform check').click();

        // Assert that the error message for a future date is shown
        cy.contains('Enter a date in the past').should('be.visible');
    });

    it('allows valid date of birth submission', () => {
        // Provide a valid date of birth
        cy.get('#Day').clear().type('15'); // Valid day
        cy.get('#Month').clear().type('06'); // Valid month
        cy.get('#Year').clear().type('2005'); // Valid year
        cy.contains('Perform check').click();

        // Assert no error messages are shown
        cy.get('.govuk-error-message').and('not.contain', 'Enter a date of birth');
        // Assert that fields are not highlighted with error class
        cy.get('#Day').should('not.have.class', 'govuk-input--error');
        cy.get('#Month').should('not.have.class', 'govuk-input--error');
        cy.get('#Year').should('not.have.class', 'govuk-input--error');
    });
});
