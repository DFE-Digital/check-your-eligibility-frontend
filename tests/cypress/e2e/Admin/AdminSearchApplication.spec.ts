
describe('Admin journey search for application', () => {

    const parentFirstName = 'Timothy';
    const parentLastName = 'Smith';
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'NN668767B'
    const childFirstName = 'Tom';
    const childLastName = 'Jones';
    var referenceNumber: string;

    beforeEach(() => {
        cy.SignInSchool();

    });

    it('Will create an application that can be used for searches', () => {

        cy.contains('Check free school meals - single student').click();
        
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.contains('button', 'Perform check').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('h1', { timeout: 20000 }).should('include.text', 'The children of this parent or guardian are entitled to free school meals');
        cy.contains('button', "Add children's details").click();

        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        cy.get('h1').should('include.text', 'Check your answers before registering');

        cy.CheckValuesInSummaryCard('Parent or guardian name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian date of birth', '1990-01-01');
        cy.CheckValuesInSummaryCard('National insurance number', NIN);
        cy.CheckValuesInSummaryCard('Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard("Child's name", childFirstName + " " + childLastName);
        cy.contains('button', 'Submit application').click();

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

        cy.contains('Search all records').click();
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');
        cy.get('h1').should('include.text', 'Search results');

    });

    it('Will allow School users to search for an application with a selected status', () => {

        cy.contains('Search all records').click();
        cy.get('#Status').select('Entitled');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');
        cy.get('h1').should('include.text', 'Search results');

    });

    it('Will allow School users to search for an application with a selected Child name', () => {

        cy.contains('Search all records').click();
        cy.get('#ChildLastName').type(childLastName);
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(3)
            .contains(childLastName);
    });

    it('Will allow School users to search for an application with a selected Parent or Guardian name', () => {

        cy.contains('Search all records').click();
        cy.get('#ParentLastName').type(parentLastName);
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(2)
            .should('include.text', parentLastName);
    });


    it('Will allow School users to search for an application with a selected reference', () => {

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

        cy.contains('Search all records').click();
        cy.get('#PGDobDay').type('01');
        cy.get('#PGDobMonth').type('01');
        cy.get('#PGDobYear').type('1990');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');
    });

    
    it('Will allow a School to view an application from the results page by selecting the reference number link', () => {

        cy.contains('Search all records').click();
        cy.get('#ParentLastName').type('Smith');
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

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