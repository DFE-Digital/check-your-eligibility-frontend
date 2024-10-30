describe("Admin Portal pages contain recommended network security related headers", () => {
    it("Pages respond with recommended network security headers", () => {

        cy.SignInSchool()

        // check for network headers on a regular page
        cy.request({
            method: 'GET',
            url: '/'
        }).then((response) => {
            console.log(response.headers)
            expect(response.headers).to.have.property('content-security-policy', `default-src 'self'`);
            expect(response.headers).to.have.property('strict-transport-security', 'max-age=31536000; includeSubDomains');
            expect(response.headers).to.have.property('x-xss-protection', '1; mode=block');
            expect(response.headers).to.have.property('x-content-type-options', 'nosniff');
        })
    
        // check for network headers on a error page
        cy.request({
            method: 'GET',
            url: '/home/error'
        }).then((response) => {
            console.log(response.headers)
            expect(response.headers).to.have.property('content-security-policy', `default-src 'self'`);
            expect(response.headers).to.have.property('strict-transport-security', 'max-age=31536000; includeSubDomains');
            expect(response.headers).to.have.property('x-xss-protection', '1; mode=block');
            expect(response.headers).to.have.property('x-content-type-options', 'nosniff');
        })
        })
    }
)