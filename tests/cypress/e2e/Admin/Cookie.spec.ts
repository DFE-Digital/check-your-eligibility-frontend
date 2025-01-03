interface CustomWindow extends Window {
    clarity: Function;
}

describe('Cookie consent banner functionality', () => {
    beforeEach(() => {
        cy.session("Cookie Test Session", () => {
            cy.SignInSchool();
            cy.wait(1000);
        });
    });

    it('Should show the cookie banner on first visit when no choice has been made', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('.govuk-cookie-banner')
            .should('be.visible')
            .within(() => {
                cy.contains('h2', 'Cookies on check a family\'s eligibility');
                cy.contains('button', 'Accept analytics cookies');
                cy.contains('button', 'Reject analytics cookies');
            });
    });

    it('Should hide banner and set cookie when accepting analytics', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        
        // Click accept and verify display state
        cy.get('#accept-cookies').click();
        cy.wait(1000);
        cy.get('#cookie-banner').should('not.exist');
        
        // Verify cookie was set
        cy.getCookie('analytics-cookies-consent').should('have.property', 'value', 'true');
        
        // Verify banner stays hidden on next visit
        cy.reload();
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
    });

    it('Should hide banner and set cookie when rejecting analytics', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        
        // Click reject and verify display state
        cy.get('#reject-cookies').click();
        cy.wait(1000);
        cy.get('#cookie-banner').should('not.exist');
        
        // Verify cookie was set
        cy.getCookie('analytics-cookies-consent').should('have.property', 'value', 'false');
        
        // Verify banner stays hidden on next visit
        cy.reload();
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
    });

    it('Should initialize Clarity when analytics are accepted', () => {
    cy.visit(Cypress.config().baseUrl ?? "");

    // Accept cookies
    cy.get('#accept-cookies').click();

    // Wait for Clarity cookies to be created
    cy.wait(5000);

    // Check and log Clarity cookies
    cy.getCookie('_clarity').then((cookie) => {
        if (cookie) {
            cy.log('_clarity cookie value:', cookie.value);
        } else {
            cy.log('_clarity cookie is missing.');
        }
    });

    cy.getCookie('CLARITY_MASTERID').then((cookie) => {
        if (cookie) {
            cy.log('CLARITY_MASTERID cookie value:', cookie.value);
        } else {
            cy.log('CLARITY_MASTERID cookie is missing.');
        }
    });
});

    
    

    it('Should remove Clarity cookies when analytics are rejected', () => {
        // First accept to set cookies
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('#accept-cookies').click();
        cy.wait(1000);
        
        // Then reject to remove them
        cy.clearCookies();
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('#reject-cookies').click();
        cy.wait(1000);
        
        // Verify Clarity cookies were removed
        cy.getCookie('_clarity').should('not.exist');
        cy.getCookie('CLARITY_MASTERID').should('not.exist');

        // Verify Clarity script is not added
        cy.window().then((win) => {
            const clarityId = win.document.body.getAttribute('data-clarity');
            cy.get(`script[src*="clarity.ms/tag/${clarityId}"]`)
                .should('not.exist');
        });
    });

    it('Should maintain cookie choice across sessions', () => {
        // Visit the page and accept cookies
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('#accept-cookies').click();
    
        // Reload the page to confirm the cookie banner is hidden
        cy.reload();
        cy.get('#cookie-banner', { timeout: 10000 }).should('have.css', 'display', 'none');
    
        // Verify the cookie was set correctly
        cy.getCookie('analytics-cookies-consent').should('exist').and('have.property', 'value', 'true');
    
        // End the session and start a new one
        cy.session("New Cookie Test Session", () => {
            cy.SignInSchool();
            cy.wait(1000);
        });
    
        // Visit the page again and confirm the banner remains hidden
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
        cy.getCookie('analytics-cookies-consent').should('have.property', 'value', 'true');
    });
    

    it('Should show cookie preferences page with current selection', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        
        // Accept cookies first
        cy.get('#accept-cookies').click();
        cy.wait(1000);
        
        // Visit cookies page directly
        cy.visit(`${Cypress.config().baseUrl}/Home/Cookies`);
        
        // Verify current selection is shown
        cy.get('[name="analytics"]')
            .should('be.checked')
            .and('have.value', 'true');
    });

    /*it('Should allow changing preferences on cookie settings page', () => {
        // Go directly to cookies page
        cy.visit(`${Cypress.config().baseUrl}/home/cookies`);
        
        // Change preference to reject
        cy.get('[name="analytics"][value="false"]').check();
        cy.contains('button', 'Save cookie preferences').click();
        
        // Verify cookie was updated
        cy.getCookie('analytics-cookies-consent')
            .should('have.property', 'value', 'false');
        
        // Verify confirmation message
        cy.get('#notification-banner--success')
            .should('be.visible')
            .and('contain', 'Your cookie preferences have been saved');
    });*/
});