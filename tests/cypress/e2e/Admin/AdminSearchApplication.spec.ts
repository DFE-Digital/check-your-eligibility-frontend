

describe('Admin journey search for application', () => {

    const parentFirstName = Cypress.env('lastName');
    const parentLastName = 'Smith';
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'NN668767B'
    const childFirstName = 'Tom';
    const childLastName = 'Jones';
    var referenceNumber: string;

    beforeEach(() => {
        cy.session("Session 4", () => {
            cy.SignInSchool();

            cy.wait(1);
        });
    });

    it('Will create an application that can be used for searches', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        
        cy.contains('Run a check for one parent or guardian').click();
        
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NIN);

        cy.contains('button', 'Perform check').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('h3.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian are eligible for free school meals.');
        cy.contains('a.govuk-button', "Add children's details").click();

        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        cy.get('h1').should('include.text', 'Check your answers before submitting');

        cy.CheckValuesInSummaryCard('Parent or guardian details','Name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian details','Date of birth', '1990-01-01');
        cy.CheckValuesInSummaryCard('Parent or guardian details','National Insurance number', NIN);
        cy.CheckValuesInSummaryCard('Parent or guardian details','Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard('Child 1 details',"Name", childFirstName + " " + childLastName);
        cy.contains('button', 'Add details').click();

        cy.url().should('include', '/Check/ApplicationsRegistered');
        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(2)
            .invoke('text')
            .then((text) => {
                referenceNumber = text;
            });
    });

    it('Will allow School users to search for an application with any status', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');
        cy.get('h1').should('include.text', 'Search results');

    });

    it('Will allow School users to search for an application with a selected status', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.get('#Status').select('Entitled');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');
        cy.get('h1').should('include.text', 'Search results');

    });

    it('Will allow School users to search for an application with a selected Child name', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.get('#ChildLastName').type(childLastName);
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Search');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(3)
            .contains(childLastName);
    });

    it('Will allow School users to search for an application with a selected Parent or Guardian name', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.get('#ParentLastName').type(parentLastName);
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(2)
            .should('include.text', parentFirstName + " " + parentLastName);
    });


    it('Will allow School users to search for an application with a selected reference', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.get('#Reference').type(referenceNumber);
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

        cy.get('h1').should('contain.text', 'Search results (1)');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(1)
            .should('contain.text', referenceNumber);
    });

    it('Will allow School users to search for an application with a selected Child DOB', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.get('#ChildDobDay').type('01');
        cy.get('#ChildDobMonth').type('01');
        cy.get('#ChildDobYear').type('2007');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(4)
            .should('contain.text', '01 Jan 2007');
    });

    it('Will allow School users to search for an application with a selected Parent of Guardian DOB', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.get('#PGDobDay').type('01');
        cy.get('#PGDobMonth').type('01');
        cy.get('#PGDobYear').type('1990');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');
    });

    
    it('Will allow a School to view an application from the results page by selecting the reference number link', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Search all records').click();
        cy.get('#ParentLastName').type('Smith');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(2)
            .should('include.text', parentFirstName + " " + parentLastName);

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .find('a')
            .click();

        cy.url().should('include', 'ApplicationDetail');
    })
});