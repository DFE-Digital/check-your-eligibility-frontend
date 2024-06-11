it('should intercept and mock the authorization request', () => {
    cy.visit("https://oidc.integration.account.gov.uk/authorize?ui_locales=en&response_type=code&scope=openid,email&client_id=pBvq8IcdKosgOKzyQ6szmnS0_Yw&state=dolkfkfkfkflooh&nonce=qwsrkiseyullllio&redirect_uri=https://ecs-test-as-frontend.azurewebsites.net/Check/Enter_Child_Details")
    cy.window().then((win) => {
        cy.stub(win, 'alert').callsFake((msg) => {
            if (msg.includes('Username')) {

                return "user";
            }
        })

    })

})




