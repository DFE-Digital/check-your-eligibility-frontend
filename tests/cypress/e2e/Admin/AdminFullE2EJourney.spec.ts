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

        cy.contains('button', 'Perform check').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('p', { timeout: 30000 }).should('include.text', 'The children of this parent or guardian may not be entitled to free school meals');
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
                referenceNumber = text;
            });

        cy.then(() =>{
            cy.visit('/')

            cy.contains('Process appeals').click();

            cy.contains(referenceNumber).click();
        })

        cy.get('h1').should('contain',`${parentFirstName} ${parentLastName}`);
        cy.get('.govuk-button').click();

        cy.url().should('contain', 'ApplicationDetailAppealConfirmation');
        cy.get('p').should('include.text', 'Are you sure?');
        cy.contains('.govuk-button', 'Yes, send now').click();

    });

    it('Allows a user when logged into the LA portal to approve the application review', () => {
        
        cy.SignInLA();

        cy.contains('.govuk-link', 'Pending applications').click();
        cy.url().should('contain', 'Application/PendingApplications');
        
        cy.contains(referenceNumber).click();
        cy.contains('.govuk-button', 'Approve application').click();
        cy.contains('.govuk-button', 'Yes, approve now').click();

        cy.visit('/');

        cy.contains('Search all records').click();
        cy.url().should('contain', 'Application/Search');

        cy.get('#Reference').type(referenceNumber);
        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/Results');

        cy.get('h1').should('contain.text', 'Search results (1)');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(1)
            .should('contain.text', referenceNumber)
            .click();

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(5)
            .should('contain.text', 'Reviewed Entitled');
    });

    it('Allows a user when back logged into the School portal to finalise the application', () => {

        cy.SignInSchool();
        cy.contains('Finalise applications').click();
        cy.url().should('contain', 'Application/FinaliseApplications');
        cy.log(referenceNumber)

        cy.get('.govuk-table tbody tr').each(($row) => {
            cy.wrap($row).find('td').eq(1).invoke('text').then((text) => {
                if (text.trim() === referenceNumber) {
                    cy.wrap($row).find('td').eq(0).find('input[type="checkbox"]').click();
                    cy.log('found it!');
                }
            })
        })
        cy.contains('.govuk-button', 'Finalise applications').click();
        cy.contains('.govuk-button', 'Yes, finalise now').click();

    });

})  