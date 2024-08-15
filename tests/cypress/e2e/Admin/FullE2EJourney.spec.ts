describe('Full journey of creating an application through school portal through to approving in LA portal', () => {
    const parentFirstName = 'Tim';
    const parentLastName = 'Jones';
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'AB123456C'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';
    var referenceNumber: string;

    it('Will allow a school user to create an application that may not be elligible and send it for appeal', () => {

        cy.SignInSchool();
        cy.contains('Check free school meals - single student').click();
        
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.contains('Perform check').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('p').should('include.text', 'The children of this parent or guardian may not be entitled to free school meals', { timeout: 10000 });
        cy.contains('button', 'Appeal now').click();

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
                cy.log('Text retrieved', text)
                referenceNumber = text;
                cy.log(referenceNumber)
            });
        cy.log(referenceNumber);

        cy.then(() =>{
            cy.visit('/')

            cy.contains('Process appeals').click();

            cy.contains(referenceNumber).click();
            // cy.get('.govuk-table')
            // .find('tbody tr')
            // .find(referenceNumber)
            // .find('a')
            // .click();
        })

    });

})  