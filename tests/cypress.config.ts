import { defineConfig } from "cypress";

export default defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      // implement node event listeners here
    },
    baseUrl: process.env.CYPRESS_BASE_URL,
    viewportWidth: 1600,
    viewportHeight: 1800,
    specPattern:'cypress/e2e/**/*.spec.{js,jsx,ts,tsx}'
    },
    reporter: "junit",
    reporterOptions: {
        mochaFile: "results/my-test-output-[hash].xml",
    }
});
