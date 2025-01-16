describe("Links on not eligible page route to the intended locations", () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B';

    beforeEach(() => {
        cy.session("Session 1", () => {
            cy.SignInSchool();
            cy.wait(1); // Ensure session/login completes
        });
    });

    it("Guidance link should route to guidance page", () => {
        cy.visit("/Check/Enter_Details");

        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.get('#EmailAddress').clear().type(parentEmailAddress);
        cy.contains('Perform check').click();
        cy.contains('a.govuk-link', 'See a complete list of acceptable evidence', { timeout: 8000 }).then(($link) => {
            const url = $link.prop('href');
            cy.visit(url);
            cy.get('h1.govuk-heading-l').should('contain.text', 'Guidance for reviewing evidence');
        });
    });

    it("Support link should route to DfE form", () => {
        cy.visit("/Check/Enter_Details");
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.get('#EmailAddress').clear().type(parentEmailAddress);
        cy.contains('Perform check').click();
        cy.contains('a.govuk-link', 'contact the Department for Education support desk', { timeout: 8000 }).then(($link) => {
            const url = $link.prop('href');
            cy.visit(url);
            cy.get('span.text-format-content').should('contain.text', "Check a Family's FSM Eligibility Query Form");
        });
    });
});

describe('Date of Birth Validation Tests', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B';

    beforeEach(() => {
        cy.SignInSchool();
        cy.wait(1); // Ensure session/login completes
        cy.visit('/Check/Enter_Details'); // Ensure we are on the correct page
    });

    it('displays error messages for missing date fields', () => {
        // Scenario 1: All fields are empty
        cy.get('#Day').clear();
        cy.get('#Month').clear();
        cy.get('#Year').clear();
        cy.contains('Perform check').click();

        // Assert that the error message for missing date of birth is shown
        cy.get('.govuk-error-message').should('contain', 'Enter a date of birth');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for non-numeric inputs', () => {
        // Scenario 2: Non-numeric inputs
        cy.get('#Day').clear().type('abc'); // Invalid day
        cy.get('#Month').clear().type('xyz'); // Invalid month
        cy.get('#Year').clear().type('abcd'); // Invalid year
        cy.contains('Perform check').click();

        // Assert error messages
        cy.get('.govuk-error-message').should('contain', 'Enter a date of birth using numbers only');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for out-of-range inputs', () => {
        // Scenario 3: Invalid day, month, and year ranges
        cy.get('#Day').clear().type('50'); // Invalid day
        cy.get('#Month').clear().type('13'); // Invalid month
        cy.get('#Year').clear().type('1800'); // Invalid year
        cy.contains('Perform check').click();

        // Assert error messages for each out-of-range field
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
    });

    it('displays error messages for future dates', () => {
        // Scenario 4: Date in the future
        cy.get('#Day').clear().type('01');
        cy.get('#Month').clear().type('01');
        cy.get('#Year').clear().type((new Date().getFullYear() + 1).toString()); // Next year
        cy.contains('Perform check').click();

        // Assert that the error message for a future date is shown
        cy.get('.govuk-error-message').should('contain', 'Enter a date in the past');
    });

    it('displays error messages for invalid combinations (e.g., 31st February)', () => {
        // Scenario 5: Invalid date combination
        cy.get('#Day').clear().type('31');
        cy.get('#Month').clear().type('02'); // February
        cy.get('#Year').clear().type('2020'); // Leap year
        cy.contains('Perform check').click();

        // Assert error message for invalid day in month
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
    });

    it('allows valid date of birth submission', () => {
        // Scenario 6: Valid date
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('2005');
        cy.contains('Perform check').click();

        // Assert no DOB-specific error messages are shown
        cy.get('#Day + .govuk-error-message').should('not.exist'); // Scoped to Day error message
        cy.get('#Month + .govuk-error-message').should('not.exist'); // Scoped to Month error message
        cy.get('#Year + .govuk-error-message').should('not.exist'); // Scoped to Year error message

        // Assert no error classes are applied to DOB fields
        cy.get('#Day').should('not.have.class', 'govuk-input--error');
        cy.get('#Month').should('not.have.class', 'govuk-input--error');
        cy.get('#Year').should('not.have.class', 'govuk-input--error');
    });

});

describe("Conditional content on ApplicationDetail page", () => {
    const testData = {
        parent: {
            firstName: 'Tim',
            lastName: Cypress.env('lastName'),
            email: 'TimJones@Example.com',
            nin: 'PN668767B',
            dob: {
                day: '01',
                month: '01',
                year: '1990'
            }
        },
        child: {
            firstName: 'Timmy',
            lastName: 'Smith',
            dob: {
                day: '01',
                month: '01',
                year: '2007'
            }
        }
    };

    beforeEach(() => {
        // Setup and verify initial state
        cy.session("school-session", () => {
            cy.SignInSchool();
            // Verify successful login
            cy.get('h1', { timeout: 10000 })
                .should('be.visible')
                .and('include.text', 'The Telford Park School');
        });
    });

    const fillParentDetails = () => {
        cy.get('#FirstName').should('be.visible').type(testData.parent.firstName);
        cy.get('#LastName').should('be.visible').type(testData.parent.lastName);
        cy.get('#EmailAddress').should('be.visible').type(testData.parent.email);
        cy.get('#Day').should('be.visible').type(testData.parent.dob.day);
        cy.get('#Month').should('be.visible').type(testData.parent.dob.month);
        cy.get('#Year').should('be.visible').type(testData.parent.dob.year);
        cy.get('#NinAsrSelection').should('be.visible').click();
        cy.get('#NationalInsuranceNumber').should('be.visible').type(testData.parent.nin);
    };

    const fillChildDetails = () => {
        cy.get('[id="ChildList[0].FirstName"]').should('be.visible').type(testData.child.firstName);
        cy.get('[id="ChildList[0].LastName"]').should('be.visible').type(testData.child.lastName);
        cy.get('[id="ChildList[0].Day"]').should('be.visible').type(testData.child.dob.day);
        cy.get('[id="ChildList[0].Month"]').should('be.visible').type(testData.child.dob.month);
        cy.get('[id="ChildList[0].Year"]').should('be.visible').type(testData.child.dob.year);
    };

    it("shows and hides conditional content based on application status", () => {
        // Start the check process
        cy.visit("/");
        cy.contains('Run a check for one parent or guardian')
            .should('be.visible')
            .click();

        // Verify navigation to details page
        cy.url().should('include', '/Check/Enter_Details');

        // Fill parent details
        fillParentDetails();
        cy.contains('button', 'Perform check')
            .should('be.visible')
            .click();

        // Wait for and verify loader page
        cy.url().should('include', 'Check/Loader');
        
        // Wait for eligibility result with appropriate timeout
        cy.get('p.govuk-notification-banner__heading', { timeout: 30000 })
            .should('be.visible')
            .and('include.text', 'The children of this parent or guardian may not be eligible for free school meals');

        // Click appeal button
        cy.contains('.govuk-button', 'Appeal now')
            .should('be.visible')
            .click();

        // Verify navigation to child details page
        cy.url().should('include', '/Check/Enter_Child_Details');

        // Fill child details
        fillChildDetails();
        cy.contains('button', 'Save and continue')
            .should('be.visible')
            .click();

        // Verify answers page and submit
        cy.get('h1')
            .should('be.visible')
            .and('include.text', 'Check your answers before submitting');
        
        cy.contains('button', 'Add details')
            .should('be.visible')
            .click();

        // Get reference number and verify application status changes
        cy.get('.govuk-table__cell')
            .eq(1)
            .invoke('text')
            .then((referenceNumber) => {
                const refNumber = referenceNumber.trim();

                // Check initial status (Evidence Needed)
                cy.visit("/Application/Search");
                cy.get('button.govuk-button')
                    .should('be.visible')
                    .click();

                // Custom command to find reference number (implement with proper retry logic)
                cy.scanPagesForValue(refNumber);

                // Verify conditional content is present
                cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:")
                    .should('be.visible');

                // Change application status
                cy.get('a.govuk-button')
                    .should('be.visible')
                    .click();
                cy.get('a.govuk-button--primary')
                    .should('be.visible')
                    .click();

                // Verify status change
                cy.visit("/Application/Search");
                cy.get('button.govuk-button')
                    .should('be.visible')
                    .click();

                // Find reference number again
                cy.scanPagesForValue(refNumber);

                // Verify conditional content is no longer present
                cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:")
                    .should('not.exist');
            });
    });
});


describe("Conditional content on ApplicationDetail page", () => {
    const testData = {
        parent: {
            firstName: 'Tim',
            lastName: Cypress.env('lastName'),
            email: 'TimJones@Example.com',
            nin: 'PN668767B',
            dob: {
                day: '01',
                month: '01',
                year: '1990'
            }
        },
        child: {
            firstName: 'Timmy',
            lastName: 'Smith',
            dob: {
                day: '01',
                month: '01',
                year: '2007'
            }
        }
    };

    beforeEach(() => {
        // Setup and verify initial state
        cy.session("school-session", () => {
            cy.SignInSchool();
            // Verify successful login
            cy.get('h1')
                .should('be.visible')
                .and('include.text', 'The Telford Park School');
        });
    });

    const fillParentDetails = () => {
        cy.get('#FirstName').should('be.visible').clear().type(testData.parent.firstName);
        cy.get('#LastName').should('be.visible').clear().type(testData.parent.lastName);
        cy.get('#EmailAddress').should('be.visible').clear().type(testData.parent.email);
        cy.get('#Day').should('be.visible').clear().type(testData.parent.dob.day);
        cy.get('#Month').should('be.visible').clear().type(testData.parent.dob.month);
        cy.get('#Year').should('be.visible').clear().type(testData.parent.dob.year);
        cy.get('#NinAsrSelection').should('be.visible').click();
        cy.get('#NationalInsuranceNumber').should('be.visible').clear().type(testData.parent.nin);
    };

    const fillChildDetails = () => {
        cy.get('[id="ChildList[0].FirstName"]').should('be.visible').clear().type(testData.child.firstName);
        cy.get('[id="ChildList[0].LastName"]').should('be.visible').clear().type(testData.child.lastName);
        cy.get('[id="ChildList[0].Day"]').should('be.visible').clear().type(testData.child.dob.day);
        cy.get('[id="ChildList[0].Month"]').should('be.visible').clear().type(testData.child.dob.month);
        cy.get('[id="ChildList[0].Year"]').should('be.visible').clear().type(testData.child.dob.year);
    };

    it("shows and hides conditional content based on application status", () => {
        // Start the check process
        cy.visit("/");
        cy.contains('Run a check for one parent or guardian')
            .should('be.visible')
            .click();

        // Verify navigation and fill parent details
        cy.url().should('include', '/Check/Enter_Details');
        fillParentDetails();
        
        cy.contains('button', 'Perform check')
            .should('be.visible')
            .click();

        // Wait for and verify loader page
        cy.url().should('include', 'Check/Loader');
        
        // Wait for eligibility result
        cy.get('p.govuk-notification-banner__heading', { timeout: 30000 })
            .should('be.visible')
            .and('include.text', 'The children of this parent or guardian may not be eligible for free school meals');

        // Navigate through appeal process
        cy.contains('.govuk-button', 'Appeal now')
            .should('be.visible')
            .click();

        cy.url().should('include', '/Check/Enter_Child_Details');
        fillChildDetails();
        
        cy.contains('button', 'Save and continue')
            .should('be.visible')
            .click();

        // Verify and submit
        cy.get('h1')
            .should('be.visible')
            .and('include.text', 'Check your answers before submitting');
        
        cy.contains('button', 'Add details')
            .should('be.visible')
            .click();

        // Get reference number and verify status changes
        cy.get('.govuk-table__cell')
            .eq(1)
            .invoke('text')
            .then((referenceNumber) => {
                const refNumber = referenceNumber.trim();

                // Check initial status
                cy.visit("/Application/Search");
                cy.get('button.govuk-button')
                    .should('be.visible')
                    .click();

                cy.scanPagesForValue(refNumber);

                // Verify initial content
                cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:")
                    .should('be.visible');

                // Change status
                cy.get('a.govuk-button')
                    .should('be.visible')
                    .click();
                cy.get('a.govuk-button--primary')
                    .should('be.visible')
                    .click();

                // Verify status change
                cy.visit("/Application/Search")
                    .get('button.govuk-button')
                    .should('be.visible')
                    .click();

                cy.scanPagesForValue(refNumber);

                // Verify content is removed
                cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:")
                    .should('not.exist');
            });
    });
});

describe("Feedback link in header", () => {

    it("Should route a School user to a qualtrics survey", () => {
        cy.SignInSchool();
        cy.get('span.govuk-phase-banner__text > a.govuk-link')
            .invoke('removeAttr', 'target')
            .click();
        cy.url()
            .should('include', 'https://dferesearch.fra1.qualtrics.com/jfe/form/SV_bjB0MQiSJtvhyZw');
        cy.contains("Thank you for participating in this survey")
    });
    it("Should route an LA user to a qualtrics survey", () => {
        cy.SignInLA();
        cy.get('span.govuk-phase-banner__text > a.govuk-link')
            .invoke('removeAttr', 'target')
            .click();
        cy.url()
            .should('include', 'https://dferesearch.fra1.qualtrics.com/jfe/form/SV_bjB0MQiSJtvhyZw');
        cy.contains("Thank you for participating in this survey")
    });
});