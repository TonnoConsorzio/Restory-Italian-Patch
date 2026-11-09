## Agent skills

### Issue tracker

Issues and specs live in GitHub Issues and use the `gh` CLI after this local repository has a GitHub remote. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context: root `GLOSSARY.md` and `docs/adr/`. Read relevant docs before changing the project. See `docs/agents/domain.md`.

### Game data handling

Keep temporary game extracts, downloaded loaders, disposable game copies, and build outputs in ignored folders. Do not commit the game's original text corpus; commit patch-owned Italian translation data and extraction tooling only.
