
describe('Admin journey search for application', () => {
    beforeEach(() => {
        cy.session("Session 5", () => {
            cy.SignInLA();
            cy.wait(1);
        });
    });

    it('Returns the correct warning message when invalid characters are used in the Child last name field', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.contains('Search all records').click();

        cy.get('#ChildLastName').type('12345');
        cy.contains('Generate results').click();
        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('.field-validation-error').should('contain.text', 'Child last name field contains an invalid character');

    });

    it('Returns the correct warning message when invalid characters are used in the Parent last name field', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.contains('Search all records').click();

        cy.get('#ParentLastName').type('12345');
        cy.contains('Generate results').click();
        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('.field-validation-error').should('contain.text', 'Parent or guardian last name field contains an invalid character');

    });

    it('Returns the correct warning message when an invalid Reference number is input', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.contains('Search all records').click();

        cy.get('#Reference').type('word');
        cy.contains('Generate results').click();
        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('li').should('contain.text', 'Reference Number field contains an invalid character');

    });

    it('Returns the correct warning message when an invalid Child date of birth is input', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.wait(1);

        cy.contains('Search all records').click();

        cy.get('#ChildDobDay').type('35');
        cy.get('#ChildDobMonth').type('35');
        cy.get('#ChildDobYear').type('2090');
        cy.contains('Generate results').click();

        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('li').should('contain.text', 'Enter a valid date');
        cy.get('#ChildDobDay').clear().type('01');
        cy.get('#ChildDobMonth').type('35');
        cy.get('#ChildDobYear').type('2090');
        cy.contains('Generate results').click();

        cy.get('li').should('contain.text', 'Enter a valid date');
        cy.get('#ChildDobDay').clear().type('01');
        cy.get('#ChildDobMonth').clear().type('01');
        cy.get('#ChildDobYear').type('2090');
        cy.contains('Generate results').click();

        cy.get('li').should('contain.text', 'Enter a date in the past');

    });

    it('Returns the correct warning message when an invalid Parent date of birth is input', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.wait(1);

        cy.contains('Search all records').click();

        cy.get('#PGDobDay').type('35');
        cy.get('#PGDobMonth').type('35');
        cy.get('#PGDobYear').type('2090');
        cy.contains('Generate results').click();

        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('li').should('include.text', 'Enter a valid date');
        cy.get('#PGDobDay').clear().type('01');
        cy.get('#PGDobMonth').type('35');
        cy.get('#PGDobYear').type('2090');
        cy.contains('Generate results').click();

        cy.get('li').should('include.text', 'Enter a valid date');
        cy.get('#PGDobDay').clear().type('01');
        cy.get('#PGDobMonth').clear().type('01');
        cy.get('#PGDobYear').type('2090');

        cy.contains('Generate results').click();

        cy.get('li').should('include.text', 'Enter a date in the past');

    });


});