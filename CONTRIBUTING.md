# Contributing Guidelines

## Branching Strategy
The main development branch for this project is `development`.  
Do **not** commit directly to `development`.

All new work should be done on a personal feature branch derived from `development`.  
Use the following naming convention for your branch:

```
<developer_name>_development
```

Example:
```
martin_development
```

## Commit Standards
- Keep commit messages **short**, **clear**, and **descriptive**.  
- Each commit should represent a **logical unit of work**.  
- Use present-tense phrasing (e.g., *add feature*, *fix bug*, *update readme*).

## Pull Requests
Pull requests (PRs) should:
- Represent a **complete feature** or **fix**.
- Pass **all tests** and **build requirements**.
- Target the `development` branch.

Before submitting a PR:
1. Merge the latest changes from `development` into your branch.  
   Example:
   ```
   git checkout development
   git pull
   git checkout <branch_name>
   git merge development
   ```
2. Resolve **all merge conflicts locally**.  
3. Test and verify that the project builds and functions correctly.  
4. Push your updated branch:
   ```
   git push
   ```

Once the PR is approved, it will be merged into `development`.

---

Following these conventions ensures a clean, stable, and collaborative workflow for all contributors.
