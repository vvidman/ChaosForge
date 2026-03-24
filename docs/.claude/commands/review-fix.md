# /review-fix

When given code with review feedback from a Desktop session:

1. Read the code and feedback carefully before touching anything.

2. Identify the root cause:
   - Architecture issue → flag it explicitly, propose a fix aligned with CLAUDE.md rules
   - Logic issue → fix it, explain why, add or update a unit test to cover the case
   - Style issue → apply fix silently

3. Make surgical fixes only. Do not refactor unrelated code in the same pass.

4. Run the affected tests after applying the fix.

5. Summarize:
   - What was changed and why
   - Which test now covers the fixed case
   - If the fix revealed a deeper architectural problem, flag it for Desktop review

6. Do NOT run any git commands. The human handles all commits.
