
describe('Admin journey search for application', () => {
    beforeEach(() => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Department for Education Sign-in');
        cy.get('#username').type(Cypress.env('DFE_ADMIN_EMAIL_ADDRESS'));
        cy.get('#password').type(Cypress.env('DFE_ADMIN_PASSWORD'));
        cy.contains('Sign in').click();

        cy.url().should('include', 'select-organisation');
        cy.get('h1').should('include.text', 'Select your organisation');
        cy.get('#3A984661-4986-4866-8A3E-C5935FE20821').click();
        cy.contains('Continue').click();

        cy.get('h1').should('include.text', 'Hollinswood Primary School');

        cy.contains('Search all records').click();

    });

    it('Will allow Local Authority users to search for an application with any status', () => {
  
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');
        cy.get('h1').should('include.text', 'Search results');

    });

    it('Will allow Local Authority users to search for an application with a selected status', () => {

        cy.get('#Status').select('Entitled');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');
        cy.get('h1').should('include.text', 'Search results');

    });

    it('Will allow Local Authority users to search for an application with a selected Child name', () => {

        cy.get('#ChildLastName').type('Simpson');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(1)
            .find('td')
            .eq(3)
            .should('include.text', 'Simpson');
    });

    it('Will allow Local Authority users to search for an application with a selected Parent or Guardian name', () => {

        cy.get('#ParentLastName').type('Simpson');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(1)
            .find('td')
            .eq(2)
            .should('include.text', 'Simpson');
    });


    it('Will allow Local Authority users to search for an application with a selected Parent or Guardian name', () => {

        cy.get('#Reference').type('53579731');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('h1').should('contain.text', 'Search results (1)');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(1)
            .should('contain.text', '53579731');
    });

    it('Will allow Local Authority users to search for an application with a selected Child DOB', () => {

        cy.get('#Child-DOB-Day').type('01');
        cy.get('#Child-DOB-Month').type('01');
        cy.get('#Child-DOB-Year').type('2007');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(4)
            .should('contain.text', '01 Jan 2007');
    });

    it('Will allow Local Authority users to search for an application with a selected Parent of Guardian DOB', () => {

        cy.get('#PG-DOB-Day').type('01');
        cy.get('#PG-DOB-Month').type('01');
        cy.get('#PG-DOB-Year').type('1990');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');
    });


});