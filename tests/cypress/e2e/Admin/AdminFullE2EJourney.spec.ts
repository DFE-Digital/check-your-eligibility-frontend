describe('Full journey of creating an application through school portal through to approving in LA portal', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';
    let referenceNumber: string;

    
    it('Will allow a school user to create an application that may not be elligible and send it for appeal', () => {
        cy.SignInSchool();
        cy.wait(1);
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.wait(1);
        cy.get('h1').should('include.text', 'The Telford Park School');

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
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
        cy.contains('a.govuk-button', 'Appeal now').click();

        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        cy.get('h1').should('include.text', 'Check your answers before submitting');

        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Date of birth', '1 January 1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'National Insurance number', NIN);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard('Child 1 details', "Name", childFirstName + " " + childLastName);
        cy.contains('button', 'Add details').click();

        cy.url().should('include', '/Check/AppealsRegistered');
        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(1)
            .invoke('text')
            .then((text) => {
                referenceNumber = text.trim().toString();
                cy.wrap('referenceNumber').as(referenceNumber);
            });

        cy.then(() => {
            cy.visit('/')

            cy.contains('Process appeals').click();

            cy.scanPagesForValue(referenceNumber);

            cy.contains(referenceNumber).click();
        })

        cy.get('h1').should('contain', `${parentFirstName} ${parentLastName}`);
        cy.get('.govuk-button').click();

        cy.url().should('contain', 'ApplicationDetailAppealConfirmation');
        cy.get('p').should('include.text', 'Send this record to the local authority?');
        cy.contains('.govuk-button', 'Yes, send now').click();

    });
    it('Will allow a school user to create an application is eligible and submit an application', () => {
        cy.SignInSchool();
        cy.wait(1);
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.wait(1);
        cy.get('h1').should('include.text', 'The Telford Park School');

        cy.contains('Run a check for one parent or guardian').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');

        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type("nn123456c");

        cy.contains('button', 'Perform check').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian are eligible for free school meals');
        cy.contains('a.govuk-button', "Add children's details").click();

        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        cy.get('h1').should('include.text', 'Check your answers before submitting');

        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Date of birth', '1 Jan 1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'National Insurance number', "NN123456C");
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard('Child 1 details', "Name", childFirstName + " " + childLastName);
        cy.contains('button', 'Add details').click();

        cy.url().should('include', '/Check/ApplicationsRegistered');
    });

    it('Allows a user when logged into the LA portal to approve the application review', () => {
        cy.SignInLA();
        cy.wait(1);
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.get('h1').should('include.text', 'Telford and Wrekin Council');

        cy.contains('.govuk-link', 'Pending applications').click();
        cy.url().should('contain', 'Application/PendingApplications');
        cy.log(referenceNumber);
        cy.scanPagesForValue(referenceNumber);
        cy.contains('.govuk-button', 'Approve application').click();
        cy.contains('.govuk-button', 'Yes, approve now').click();

        cy.visit('/');

        cy.contains('Search all records').click();
        cy.url().should('contain', 'Application/Search');

        cy.get('#Reference').type(referenceNumber);

        cy.contains('Generate results').click();
        cy.url().should('include', 'Application/SearchResults');

        cy.get('h1').should('contain.text', 'Search results (1)');

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(0)
            .should('contain.text', referenceNumber);

        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(4)
            .should('contain.text', 'Reviewed Entitled');

    });

    it('Allows a user when back logged into the School portal to finalise the application', () => {
        cy.SignInSchool();
        cy.wait(1);
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.contains('Finalise applications').click();
        cy.url().should('contain', 'Application/FinaliseApplications');

        cy.findApplicationFinalise(referenceNumber).then(() => {
            cy.contains('.govuk-button', 'Finalise applications').click();
            cy.contains('.govuk-button', 'Yes, finalise now').click();
        });

    });


});
