
describe('Admin journey search for application', () => {
    beforeEach(() => {
        cy.SignInLA();
        cy.contains('Search all records').click();

    });

    it('Returns the correct warning message when invalid characters are used in the Child last name field', () => {
        
        cy.get('#ChildLastName').type('12345');
        cy.contains('Generate results').click();
        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('li').should('contain.text', 'Child last name field contains an invalid character');

    });

    it('Returns the correct warning message when invalid characters are used in the Parent last name field', () => {
        
        cy.get('#ParentLastName').type('12345');
        cy.contains('Generate results').click();
        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('.field-validation-error').should('contain.text', 'Parent or Guardian last name field contains an invalid character');

    });

    it('Returns the correct warning message when an invalid Reference number is input', () => {
        
        cy.get('#Reference').type('word');
        cy.contains('Generate results').click();
        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('li').should('contain.text', 'Reference Number field contains an invalid character');

    });

    it('Returns the correct warning message when an invalid Child data of birth is input', () => {
        
        cy.get('#Child-DOB-Day').type('35');
        cy.get('#Child-DOB-Month').type('35');
        cy.get('#Child-DOB-Year').type('2090');
        cy.contains('Generate results').click();

        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('li').should('contain.text', 'Invalid Day');
        cy.get('li').should('contain.text', 'Invalid Month');
        cy.get('li').should('contain.text', 'Invalid Year');

    });

    it('Returns the correct warning message when an invalid Parent data of birth is input', () => {
        
        cy.get('#PG-DOB-Day').type('35');
        cy.get('#PG-DOB-Month').type('35');
        cy.get('#PG-DOB-Year').type('2090');
        cy.contains('Generate results').click();

        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('li').should('contain.text', 'Invalid Day');
        cy.get('li').should('contain.text', 'Invalid Month');
        cy.get('li').should('contain.text', 'Invalid Year');

    });


});