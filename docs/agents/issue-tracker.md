# Issue tracker: GitHub

Issues and specs for this repo live as GitHub Issues. Use the `gh` CLI for all operations. This repository is local-only until the user adds a GitHub remote; do not run issue operations before then.

## Conventions

- Create an issue with `gh issue create --title "..." --body "..."`.
- Read an issue with `gh issue view <number> --json number,title,body,labels,comments`.
- List issues with `gh issue list --state open --json number,title,body,labels,comments` and suitable filters.
- Comment with `gh issue comment <number> --body "..."`.
- Apply or remove labels with `gh issue edit <number> --add-label "..."` or `--remove-label "..."`.
- Close with `gh issue close <number> --comment "..."`.
- Infer the repository from `git remote -v`; `gh` resolves it inside a connected clone.

## Pull requests as a request surface

**PRs as a request surface: no.** Pull requests are not feature requests for triage.

## When a skill says "publish to the issue tracker"

Create a GitHub issue.

## When a skill says "fetch the relevant ticket"

Read it with `gh issue view`.
