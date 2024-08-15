
describe('Admin journey search for application', () => {
    beforeEach(() => {
        cy.SignInLA();
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
            .eq(0)
            .find('td')
            .eq(3)
            .contains(/Simpson/i);
    });

    it('Will allow Local Authority users to search for an application with a selected Parent or Guardian name', () => {

        cy.get('#ParentLastName').type('Smith');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(2)
            .should('include.text', /Smith/i);
    });


    it('Will allow Local Authority users to search for an application with a selected reference', () => {

        cy.get('#Reference').type('68383670');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('h1').should('contain.text', 'Search results (1)');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(1)
            .should('contain.text', '68383670');
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

    
    it('Will allow a Local Authority to view an application from the results page by selecting the reference number link', () => {
        cy.get('#ParentLastName').type('Smith');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(2)
            .should('include.text', 'Smith');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .find('a')
            .click();

        cy.url().should('include', 'ApplicationDetail');
    })
});