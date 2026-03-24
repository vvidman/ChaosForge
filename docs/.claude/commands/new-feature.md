# /new-feature

When asked to implement a new feature, follow this workflow:

1. Check if a spec exists at `docs/specs/`. If yes, read it before writing any code.
   If no spec exists and the feature touches multiple layers, ask:
   "Should I create a spec draft first, or proceed directly?"

2. Ask clarifying questions if the scope is ambiguous before writing any code.

3. Implement in layer order:
   - Domain: entities, events, interfaces
   - Application: Command/Query + Handler + Validator
   - Infrastructure: EF config, repository, provider changes if needed
   - API: thin controller action + SignalR event if needed

4. Write unit tests for Domain and Application layers.
   Cover the happy path and the most important edge cases.

5. Run all tests. Do not report completion if any test is failing.

6. Summarize:
   - Files created
   - Files modified
   - Tests added
   - Any manual steps required (migrations, configuration, environment variables)

7. Do NOT run any git commands. The human handles all commits.
