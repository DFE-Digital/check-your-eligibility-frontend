describe('Parent with valid NASS number can complete full Eligibility check and application', () => {

    it('Parent can make the full journey using correct details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start Now').click()

        cy.url().should('include', '/Check/Enter_Details');

        cy.get('h1').should('include.text', 'Enter your details');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Simpson');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('input[type="radio"][value="false"]').click();

        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('#IsNassSelected').click();
        cy.get('#NationalAsylumSeekerServiceNumber').type('240712349')

        cy.contains('Save and continue').click();

        cy.url().should('include', '/Check/Loader');



        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', 'https://signin.integration.account.gov.uk/**', (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.contains('Continue to GOV.UK One Login', { timeout: 60000 }).click();
        
        cy.wait(3);
            let currentUrl = "";

            cy.url().then((url) => {
                currentUrl = url;
            });
            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

        cy.origin('https://signin.integration.account.gov.uk', () => {
                let currentUrl = "";
                cy.url().then((url) => {
                    currentUrl = url;
                });
                cy.wait(2000);

                cy.visit(currentUrl, {
                    auth: {
                        username: Cypress.env('AUTH_USERNAME'),
                        password: Cypress.env('AUTH_PASSWORD')
                    },
                });

                cy.wait(2000);

                cy.contains('Sign in').click();

                cy.log(":)");
                
                cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
                cy.contains('Continue').click();
                
                cy.log(":(");

                cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
                cy.contains('Continue').click();
            });

        cy.wait(2000);
        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Provide details of your children');


        cy.get('[id="ChildList[0].FirstName"]').type('Tim');
        cy.get('[id="ChildList[0].LastName"]').type('Simpson');
        cy.get('[id="school-search-0"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');

        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 15000 }).should('contain.text', 'Check your answers before registering');

        cy.get('h2').should('contain.text', 'Parent or guardian details')
        cy.contains('dt', 'Parent or guardian name')
            .next('dd')
            .contains('Tim Simpson');

        cy.contains('dt', 'Parent or guardian date of birth')
            .next('dd')
            .contains('01/01/1990');

        cy.contains('dt', 'Asylum support reference number')
            .next('dd')
            .contains('240712349');

        cy.contains('dt', 'Email address')
            .next('dd')
            .contains(Cypress.env('ONEGOV_EMAIL'));


        cy.get('h2').should('contain.text', 'Child 1')
        cy.contains('dt', "Child's name")
            .next('dd')
            .contains('Tim Simpson');

        cy.contains('dt', 'School')
            .next('dd')
            .contains('Hinde House 2-16 Academy');

        cy.contains('dt', "Child's date of birth")
            .next('dd')
            .contains('01/01/2007');

        cy.contains('Submit application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('contain.text', 'Application complete');

        cy.get('.govuk-table__header').should('contain.text', 'Simpson');
        
        cy.get('.govuk-table__cell').should('contain.text', 'Hinde House 2-16 Academy');

    });
});