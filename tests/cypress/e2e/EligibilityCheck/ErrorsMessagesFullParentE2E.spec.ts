
describe('After errors have been input initially a Parent with valid details can complete full Eligibility check and application', () => {

    it('Will show the correct validation errors when user leaves the fields blank', () => {

        cy.visit('/Check/Enter_Details');
        cy.get('h1').should('contain.text', 'Enter your details');

        cy.contains('Save and continue').click();

        cy.get('h2').should('contain.text', 'There is a problem');

        cy.get('li').should('contain.text', 'First Name is required');
        cy.get('li').should('contain.text', 'Last Name is required');
        cy.get('li').should('contain.text', 'Day is required');
        cy.get('li').should('contain.text', 'Month is required');
        cy.get('li').should('contain.text', 'Year is required');
        cy.get('li').should('contain.text', 'National Insurance Number is required');
    });

    it('returns the correct error message when invalid charaters are used in the first name field', () => {
        cy.visit('/Check/Enter_Details');
        cy.get('#FirstName').should('be.visible').type('123456');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'First Name field contains an invalid character');
    });

    it('returns the correct error message when invalid charaters are used in the last name field', () => {
        cy.visit('/Check/Enter_Details');
        cy.get('#LastName').should('be.visible').type('123456');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Last Name field contains an invalid character');
    });

    it('returns the correct error message when invalid dates are added to the date fields', () => {
        cy.visit('/Check/Enter_Details');
        cy.get('#Day').should('be.visible').type('32');
        cy.get('#Month').should('be.visible').type('32');
        cy.get('#Year').should('be.visible').type('4001');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'Invalid date entered');
        cy.get('li').should('contain.text', 'Invalid Day');
        cy.get('li').should('contain.text', 'Invalid Month');
        cy.get('li').should('contain.text', 'Invalid Year');
    });

    it('will show the correct error message if child details fields are left blank', () => {

        cy.visit('/Check/Enter_Child_Details');
        cy.get('h1').should('contain.text', 'Provide details of your children');

        cy.contains('Save and continue').click();

        cy.get('h2').should('contain.text', 'There is a problem');

        cy.get('li').should('contain.text', 'First name is required');
        cy.get('li').should('contain.text', 'Last name is required');
        cy.get('li').should('contain.text', 'School is required');
        cy.get('li').should('contain.text', 'Day is required');
        cy.get('li').should('contain.text', 'Month is required');
        cy.get('li').should('contain.text', 'Year is required');

    });

    it('returns the correct error message when invalid charaters are used in the input fields', () => {

        cy.visit('/Check/Enter_Child_Details');

        cy.get('[id="ChildList[0].FirstName"]').type('123456');
        cy.get('[id="ChildList[0].LastName"]').type('123456');
        cy.get('[id="ChildList[0].School.Name"]').clear();

        cy.get('[id="ChildList[0].Day"]').clear().type('32');
        cy.get('[id="ChildList[0].Month"]').clear().type('13');
        cy.get('[id="ChildList[0].Year"]').clear().type('2050');

        cy.contains('Save and continue').click();

        cy.get('li').should('contain.text', 'First Name field contains an invalid character');
        cy.get('li').should('contain.text', 'Last Name field contains an invalid character');
        cy.get('li').should('contain.text', 'Invalid date entered');
        cy.get('li').should('contain.text', 'Invalid Day');
        cy.get('li').should('contain.text', 'Invalid Month');
        cy.get('li').should('contain.text', 'Invalid Year');
    });


    it('Parent can make the full journey using correct details after correcting issues in child details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start Now').click()
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Smith');
        cy.get('#Day').should('be.visible').type('01');
        cy.get('#Month').should('be.visible').type('01');
        cy.get('#Year').should('be.visible').type('1990');

        cy.get('#IsNassSelected').click();

        cy.get('#NationalInsuranceNumber').should('be.visible').type('AB123456C');

        cy.contains('Save and continue').click();
        cy.url().should('include', '/Check/Loader');
        cy.wait(4000);

        cy.get('h1').should('include.text', 'Your children are entitled to free school meals');



        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', 'https://signin.integration.account.gov.uk/**', (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.contains('Continue to GOV.UK One Login').click();

        cy.origin('https://signin.integration.account.gov.uk', () => {
            let currentUrl = "";
            cy.url().then((url) => {
                currentUrl = url;
            });
            // cy.wait(2000);

            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

            cy.contains('Sign in').click();

            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();

            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();

            //cy.get('h1').should('include.text', 'GOV.UK One Login terms of use update');
            //cy.contains('Continue').click();

        });

        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Provide details of your children');

        cy.get('[id="ChildList[0].FirstName"]').type('Timmy');
        cy.get('[id="ChildList[0].LastName"]').type('Smith');
        cy.get('[id="ChildList[0].School.Name"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');

        cy.contains('Save and continue').click();
        cy.wait(2000);

        cy.get('h1').should('contain.text', 'Check your answers before registering');

        cy.contains('Tim Smith');
        cy.contains('01/01/1990');
        cy.contains('AB123456C');

        cy.contains('Timmy Smith');
        cy.contains('Hinde House 2-16 Academy');
        cy.contains('01/01/2007');

        cy.contains('Register details').click();


    });
});