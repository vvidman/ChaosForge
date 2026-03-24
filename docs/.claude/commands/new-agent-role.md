# /new-agent-role

When asked to add a new AgentRole, follow this checklist in order:

1. Add the value to the `AgentRole` enum in Domain
2. Determine if the role is a singleton — if yes, add it to the `SingletonRoles` collection
3. Create a prompt template: `Infrastructure/LLM/Prompts/{RoleName}PromptTemplate.cs`
   implementing `IPromptTemplate`
4. Register the template in `PromptTemplateRepository`
5. Add the default provider mapping in `ILLMProviderFactory` configuration
6. Add default provider config entry in `appsettings.json` under `AgentProviders`
7. Update `AgentWorkerService` if the new role requires unique polling or dispatch logic
8. Add the role label and display color in the React frontend constants file
9. Write a unit test for the prompt template with a representative task input

Do NOT run any git commands. Report all created and modified files when done.
