Analyze the code changes and generate a commit message in the following format:

1. Use the Conventional Commit style: `<type>(<scope>): <short summary>`

- Use `feat`, `fix`, `refactor`, `docs`, `test`, `style`, `chore`, or `perf` as `<type>`
- Use a relevant `<scope>` based on the file(s) or feature(s) changed
- The short summary must be clear, concise, and in imperative mood (e.g., "add support", "fix bug", "refactor handler")

2. Include a detailed body below the title:

- Summarize the key changes
- Mention any reasoning behind the change (e.g., performance, bug fix, clarity, separation of concerns)
- Explain how the changes affect behavior (if applicable)
- Avoid repeating the title verbatim

3. Do not include breaking changes or footer unless explicitly stated.

Generate the full message now.

## Sample

refactor(auth): extract JWT validation logic into a separate service

- Moved JWT parsing and validation from the middleware into a dedicated AuthService
- Improves testability and adheres to single responsibility principle
- No change to existing API behavior or authentication flow
