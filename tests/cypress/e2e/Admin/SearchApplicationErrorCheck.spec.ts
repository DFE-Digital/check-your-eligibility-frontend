
describe('Admin journey search for application', () => {
    beforeEach(() => {
        cy.SignIn();
        cy.contains('Search all records').click();

    });

    // it('Returns the correct warning message when invalid characters are used in the Child last name field', () => {
        
    //     cy.get('#ChildLastName').type('12345');
    //     cy.contains('Generate results').click();
    //     cy.get('h2').should('include.text', 'There is a problem');

    //     cy.get('li').should('contain.text', 'Child last name field contains an invalid character');

    // });

    it('Returns the correct warning message when invalid characters are used in the Parent last name field', () => {
        
        cy.get('#ParentLastName').type('12345');
        cy.contains('Generate results').click();
        cy.get('h2').should('include.text', 'There is a problem');

        cy.get('.field-validation-error').should('contain.text', 'Parent  or Guardian last name field contains an invalid character');

    });

    // it('Returns the correct warning message when an invalid Parent last name is input', () => {
        
    //     cy.get('#ParentLastName').type('12345');
    //     cy.contains('Generate results').click();
    //     cy.get('h2').should('include.text', 'There is a problem');

    //     cy.get('li').should('contain.text', 'Parent or Guardian last name field contains an invalid character');

    // });


});