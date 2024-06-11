/// <reference types="cypress" />

describe('Authorize Endpoint Tests', () => {
    beforeEach(() => {
      cy.intercept('GET', 'https://oidc.integration.account.gov.uk/authorize?ui_locales=en&response_type=code&scope=openid,email&client_id=pBvq8IcdKosgOKzyQ6szmnS0_Yw&state=dolkfkfkfkflooh&nonce=qwsrkiseyullllio&redirect_uri=https://ecs-test-as-frontend.azurewebsites.net/Check/Enter_Child_Details', (req) => {
        // Mocking the request
        req.reply((res) => {
          res.send({
            statusCode: 200,
            body: {
              message: 'Mocked authorization response',
              success: true
            }
          });
        });
      }).as('authorizeRequest');
    });
  
    it.only('should intercept and mock the authorization request', () => {
      // Trigger the network request in your application, e.g., by visiting a URL or clicking a button
      cy.visit('https://oidc.integration.account.gov.uk/authorize?ui_locales=en&response_type=code&scope=openid,email&client_id=pBvq8IcdKosgOKzyQ6szmnS0_Yw&state=dolkfkfkfkflooh&nonce=qwsrkiseyullllio&redirect_uri=https://ecs-test-as-frontend.azurewebsites.net/Check/Enter_Child_Details'); // Update with the actual page that triggers the authorization request
  
      // Wait for the intercepted request to complete
      cy.wait('@authorizeRequest').then((interception) => {
        // Check if the interception and its response are defined
        if (interception && interception.response) {
          // Perform assertions based on the intercepted request
          expect(interception.response.statusCode).to.eq(200);
          expect(interception.response.body).to.have.property('message', 'Mocked authorization response');
          expect(interception.response.body).to.have.property('success', true);
        } else {
          throw new Error('Interception or response is undefined');
        }
      });
    });
  
    it('should handle real authorization response', () => {
      cy.intercept('GET', 'https://signin.integration.account.gov.uk/authorize').as('realAuthorizeRequest');
  
      // Trigger the real network request
      cy.visit('/your-login-page'); // Update with the actual page that triggers the authorization request
  
      // Wait for the intercepted request to complete
      cy.wait('@realAuthorizeRequest').then((interception) => {
        // Check if the interception and its response are defined
        if (interception && interception.response) {
          // Perform assertions based on the real request
          expect(interception.response.statusCode).to.be.oneOf([200, 302]); // Example: Check if status code is 200 or 302
        } else {
          throw new Error('Interception or response is undefined');
        }
      });
    });
  });
  