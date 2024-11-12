import { defineConfig } from "cypress";
import { faker } from "@faker-js/faker";

export default defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      // implement node event listeners here

      const generatedLastName = faker.person.lastName().toUpperCase();
      config.env.lastName = generatedLastName;
      return config;
    },
    baseUrl: process.env.CYPRESS_BASE_URL,
    chromeWebSecurity: false,
    viewportWidth: 1600,
    viewportHeight: 1800,
    specPattern: 'cypress/e2e/**/*.spec.{js,jsx,ts,tsx}',
    experimentalOriginDependencies: true,  // Add this line to enable the flag
    projectId: 'cv64me'
  },
  reporter: "junit",
  reporterOptions: {
    mochaFile: "results/my-test-output-[hash].xml",
  },
  // retries: {
  //   "runMode": 2,
  //   "openMode": 2
  // }
});
