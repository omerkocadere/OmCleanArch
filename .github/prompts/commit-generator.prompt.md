# Guide to Using the VSCode Commit Generator with Conventional Commits

## Introduction

The Conventional Commits specification provides a structured format for commit messages, making them both human-readable and machine-parsable. This enhances project maintainability by enabling automation for tasks like changelog generation, semantic version bumps, and release processes. The VSCode Commit Generator (e.g., an extension like *Git Commit Message* or *Conventional Commits*) simplifies crafting commit messages that adhere to this specification. This guide details the process, ensuring every commit includes a comprehensive body with details in a list format using hyphens.

## Structure of a Conventional Commit Message

A Conventional Commit message follows this structure:

<type>[optional scope]: <description></description></type>
[body]
[optional footer(s)]


- **Type**: Specifies the change type (e.g., `feat`, `fix`).
- **Scope**: Optional, denotes the affected codebase section (e.g., `api`).
- **Description**: A concise summary of the changes (under 50 characters).
- **Body**: A detailed explanation, **required** in this guide, using a list with hyphens.
- **Footer(s)**: Optional metadata, such as issue references or breaking change notes.

## Step-by-Step Process for Using the VSCode Commit Generator

Here’s how to use the VSCode Commit Generator to create a Conventional Commit with a detailed body:

- **Install a Commit Generator Extension**  
  - Install a VSCode extension like *Conventional Commits* or *Git Commit Message* from the VSCode Marketplace.
  - Configure the extension to enforce Conventional Commits structure, if necessary.

- **Stage Your Changes**  
  - Use VSCode’s Source Control panel or `git add` to stage files for commit.

- **Open the Commit Generator**  
  - Access the commit generator via:
    - Command Palette (`Ctrl+Shift+P` or `Cmd+Shift+P`): Search for “Commit” or “Generate Commit Message.”
    - Extension-specific shortcut or UI element (e.g., a button in the Source Control panel).
    - Typing directly in the commit message input if the extension enhances it.

- **Select the Commit Type**  
  - Choose from a dropdown or type one of the following standard types:
    - `feat`: Adds a new feature (maps to SemVer MINOR).
    - `fix`: Resolves a bug (maps to SemVer PATCH).
    - `docs`: Updates documentation.
    - `style`: Modifies code formatting (e.g., whitespace, linting).
    - `refactor`: Changes code structure without altering behavior.
    - `perf`: Improves performance.
    - `test`: Adds or updates tests.
    - `chore`: Handles maintenance tasks (e.g., updating dependencies).
    - `ci`: Modifies CI/CD configurations.
    - `build`: Affects build systems or external dependencies.

- **Specify the Scope (Optional)**  
  - Enter a scope in parentheses (e.g., `feat(api)`), such as:
    - `api`: Backend API changes.
    - `ui`: User interface updates.
    - `db`: Database-related changes.
    - `auth`: Authentication system modifications.
  - Use a noun that clearly identifies the affected module or component.

- **Write the Description**  
  - Provide a short, imperative summary (e.g., “add user login endpoint”).
  - Keep it under 50 characters for readability in logs.
  - Avoid vague terms like “update” or “change”; be specific.

- **Craft the Body**  
  - Add a detailed explanation in a list format with hyphens.
  - Include at least three points addressing:
    - What the change accomplishes.
    - Why the change was necessary.
    - Any technical or user-facing implications.
  - Example body:
    - Implemented user login endpoint for secure access.
    - Addresses issue with missing authentication in v2 API.
    - Requires updated client-side token handling.

- **Indicate Breaking Changes (If Applicable)**  
  - For breaking changes, use one of these methods:
    - Append `!` after the type/scope (e.g., `feat(api)!`).
    - Add a footer: `BREAKING CHANGE: <description>`.
  - Example:
    - `BREAKING CHANGE: API now requires JWT tokens.`

- **Add Footers (Optional)**  
  - Include metadata like:
    - `Refs: #123` to reference an issue or pull request.
    - `Reviewed-by: Jane Doe` for peer review acknowledgment.
    - `Closes: #456` to close an issue.
  - Use hyphens in tokens (e.g., `Acked-by`) to distinguish from body text.

- **Review and Commit**  
  - Preview the full commit message in the generator’s interface.
  - Ensure the body is detailed and uses hyphens for list items.
  - Confirm the commit via the extension or VSCode’s commit button.

## Best Practices

- **Be Consistent**: Use consistent type and scope naming across the project.
- **Use Imperative Mood**: Write descriptions like commands (e.g., “add feature” not “added feature”).
- **Detail the “Why”**: Focus the body on why the change was made, not how it was coded.
- **Keep Scopes Granular**: Use specific scopes (e.g., `auth` instead of `backend`).
- **Avoid Overloading Commits**: If a commit covers multiple types, split it into separate commits.
- **Test the Extension**: Ensure the generator enforces the required body field.

## Example Commit Messages

### Example 1: Feature with Scope
feat(auth): implement two-factor authentication

- Added 2FA support using TOTP for enhanced security.
- Required due to user feedback on weak account protection.
- Updates login flow, requiring client-side TOTP integration.

Refs: #789

### Example 2: Bug Fix
fix(ui): correct button alignment on mobile view

- Fixed misaligned buttons in the mobile navigation bar.
- Issue caused by incorrect CSS media query breakpoints.
- Ensures consistent UI across all device sizes.

Closes: #234

### Example 3: Breaking Change
feat(db)!: migrate to PostgreSQL from MySQL

- Replaced MySQL with PostgreSQL for better performance.
- Necessary due to scaling issues with large datasets.
- Requires schema migration for existing deployments.

BREAKING CHANGE: Database schema now uses PostgreSQL-specific syntax.


## Troubleshooting

- **Extension Not Prompting for Body**  
  - Check extension settings to enforce a mandatory body.
  - Manually add a body if the extension allows skipping it.

- **Incorrect Type or Scope**  
  - Use `git rebase -i` to edit the commit message before pushing.
  - Example: `git rebase -i HEAD~1`, then select “reword.”

- **Breaking Change Not Recognized**  
  - Ensure `!` or `BREAKING CHANGE:` is used correctly.
  - Verify tools like changelog generators are configured to detect breaking changes.

## Conclusion

The VSCode Commit Generator streamlines creating Conventional Commit messages, ensuring clarity and consistency. By requiring a detailed body with hyphenated lists, you provide valuable context for teammates and automated tools. For further details, consult [https://www.conventionalcommits.org/en/v1.0.0/](https://www.conventionalcommits.org/en/v1.0.0/).

## Additional Notes

- **Custom Types**: If your project uses custom types (e.g., `security`), configure the extension to include them.
- **Team Guidelines**: Align with your team’s scope and type conventions for consistency.
- **Squash Workflow**: For pull requests, maintainers can squash and rewrite commit messages to comply with this guide.