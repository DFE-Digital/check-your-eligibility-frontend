
// Custom commands

Cypress.Commands.add('SignInLA', () => {
  cy.visit('/');
  cy.get('#username').type(Cypress.env('DFE_ADMIN_EMAIL_ADDRESS'));
  cy.get('button[type="submit"]').click()

  cy.get('#password').type(Cypress.env('DFE_ADMIN_PASSWORD'));
  cy.get('button[type="submit"]').click()

  cy.contains('Telford and Wrekin Council')
  .parent()
  .find('input[type="radio"]')
  .check();

  cy.contains('Continue',{ timeout: 15000 }).click();
});

Cypress.Commands.add('SignInSchool', () => {
  cy.visit('/');
  cy.get('#username').type(Cypress.env('DFE_ADMIN_EMAIL_ADDRESS'));
  cy.get('button[type="submit"]').click()

  cy.get('#password').type(Cypress.env('DFE_ADMIN_PASSWORD'));
  cy.get('button[type="submit"]').click()

  cy.contains('The Telford Park School')
    .parent()
    .find('input[type="radio"]')
    .check();

  cy.contains('Continue').click();
});

Cypress.Commands.add('CheckValuesInSummaryCard', (sectionTitle: string, key: string, expectedValue: string) => {
  cy.contains('.govuk-summary-card__title', sectionTitle)
  .parents('.govuk-summary-card')
  .within(() => {
    cy.contains('.govuk-summary-list__key', key)
    .siblings('.govuk-summary-list__value')
    .should('include.text', expectedValue)
  });
});

Cypress.Commands.add('scanPagesForValue', (value: string) => {
  cy.get('body').then((body) => {
    if (body.find(`td a:contains("${value}")`).length > 0) {
      cy.get(`td a:contains("${value}")`).click();
    }
    else {
      cy.contains('.govuk-link', 'Next').click();
      cy.scanPagesForValue(value);
    }
  });
});

Cypress.Commands.add('scanPagesForNewValue', (value) => {
  // Function to check for the value on the current page
  const checkForValue = () => {
    cy.get('body').then((body) => {
      if (body.find(`td a:contains("${value}")`).length > 0) {
        cy.get(`td a:contains("${value}")`).click();
      } else {
        // If 'Previous' button is present, click it and continue scanning
        if (body.find('.govuk-pagination__prev a').length > 0) {
          cy.get('.govuk-pagination__prev a').click().then(() => {
            checkForValue();
          });
        }
      }
    });
  };

  // Start by navigating to the last page
  cy.get('.govuk-pagination__list')
    .find('a[href*="PageNumber"]')
    .not('[rel="next"]')
    .last()
    .click()
    .then(() => {
      checkForValue();
    });
});

Cypress.Commands.add('scanPagesForStatusAndClick', (value: string) => {

  cy.get('body').then(($body) => {
    if ($body.text().includes(value)){
      cy.get('tr').contains('strong', value).parents('tr').within(() =>{
        cy.get('a.govuk-link').click();
      });
    } else {
      cy.get('nav.govuk-pagination').contains('a.govuk-pagination__link', 'Next').click().then(() => {
        cy.wait(2000);
        cy.scanPagesForStatusAndClick(value);
    }
  )};
  });
})

Cypress.Commands.add('findApplicationFinalise', (value: string) => {
  let referenceFound = false;
  function searchOnPage() {
    cy.get('.govuk-table tbody tr').each(($row) => {
      cy.wrap($row).find('td').eq(1).invoke('text').then((text) => {
          if (text.trim() === value) {
              referenceFound = true;
              cy.wrap($row).find('td').eq(0).find('input[type="checkbox"]').click();
              return false;
          }
      });
    }).then(() => {
      if (!referenceFound){
        cy.get('.govuk-link').contains('Next').then(($nextButton) => {
          if($nextButton.length > 0){
            cy.wrap($nextButton).click({ force: true }).then(() => {
              cy.wait(500);
              searchOnPage();
            });
          } else {
            cy.log('Reference number could not be found');
          }
        })
      }
    });
  }
  searchOnPage();
});

Cypress.Commands.add('findNewApplicationFinalise', (value: string) => {
  let referenceFound = false;
  function searchOnPage() {
    cy.get('.govuk-table tbody tr').each(($row) => {
      cy.wrap($row).find('td').eq(1).invoke('text').then((text) => {
          if (text.trim() === value) {
              referenceFound = true;
              cy.wrap($row).find('td').eq(0).find('input[type="checkbox"]').click();
              return false;
          }
      });
    }).then(() => {
      if (!referenceFound){
        cy.get('.govuk-link').contains('Previous').then(($previousButton) => {
          if($previousButton.length > 0){
            cy.wrap($previousButton).click({ force: true }).then(() => {
              cy.wait(500);
              searchOnPage();
            });
          } else {
            cy.log('Reference number could not be found');
          }
        })
      }
    });
  }
    // Start by navigating to the last page
    cy.get('.govuk-pagination__list')
    .find('a[href*="PageNumber"]')
    .not('[rel="next"]')
    .last()
    .click()
    .then(() => {
      searchOnPage();
    });
});

Cypress.Commands.add('verifyFieldVisibility', (selector: string, isVisible: boolean) => {
  if (isVisible) {
    cy.get(selector).should('be.visible');
  } else {
    cy.get(selector).should('not.be.visible');
  }
});


Cypress.Commands.add('verifyH1Text', (expectedText: string) => {
  cy.contains('h1', expectedText).should('be.visible');
  cy.get('h1').invoke('text').then((actualText: string) => {
    expect(actualText.trim()).to.eq(expectedText); 
  });
});

Cypress.Commands.add('selectYesNoOption', (baseSelector: string, isYes: boolean) => {
  const finalSelector = isYes ? `${baseSelector}[value="true"]` : `${baseSelector}[value="false"]`;
  cy.log(`selector being used: ${finalSelector}`)
  cy.get(finalSelector).click();
});

Cypress.Commands.add('retainAuthOnRedirect', (initialUrl, authHeader, alias) => {
  let redirectUrl: string;

  cy.intercept(initialUrl, (req) => {
    req.continue((res) => {
      const locationHeader = res.headers['location'];
      if (Array.isArray(locationHeader)) {
        redirectUrl = locationHeader[0];
      } else {
        redirectUrl = locationHeader;
      }
    });
  }).as('initialRequest');

  cy.request({
    url: initialUrl,
    headers: {
      'Authorization': authHeader,
    },
    followRedirect: false,
  }).then(() => {
    expect(redirectUrl).to.exist;

    cy.request({
      url: redirectUrl,
      headers: {
        'Authorization': authHeader,
      }
    }).as(alias);
  });
});

