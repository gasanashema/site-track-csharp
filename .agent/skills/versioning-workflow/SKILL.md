---
name: versioning-workflow
description: >-
  Enforces project branching and versioning on every change in zconstweb. Use whenever
  starting, modifying, or completing any feature, bugfix, migration, or configuration
  change to create a new branch before major features, bump VERSION, update
  docs/CHANGELOG.md and docs/IMPLEMENTATION_PLAN.md, run verification checks, and record
  a versioned git commit and tag.
---

# Z Construction Web App — Versioning & Implementation Workflow

Every change made in `zconstweb` must be properly branched, tracked, verified, and versioned.

## Semantic Versioning Rules

Follow `MAJOR.MINOR.PATCH` in the root `VERSION` file:
- **PATCH (`0.1.1`)**: Bug fixes, text/asset tweaks, small refactors, CSS/JS adjustments.
- **MINOR (`0.2.0`)**: New phase milestones, database migrations/seeders, new public pages, or admin CRUD features.
- **MAJOR (`1.0.0`)**: Full production release or breaking architectural changes.

## Branching Rules

- **Move to a new branch before starting any new major feature** (any `MINOR` or `MAJOR` feature/milestone, multi-file subsystem, database schema change, or new public/admin module).
- **Do not include phase numbers in branch names.** Name every branch strictly using the format:
  - `branch-type(feature)/what-to-implement` (e.g., `feat(login)/reset-password`, `feat(projects)/masonry-filter-grid`, `fix(hero)/slider-dots`)
- Ensure your working tree is clean on `main`, then create and switch to the new branch before writing code:
  - `git checkout -b "branch-type(feature)/what-to-implement"`
- Keep work on the feature branch until tests and checks pass, then merge cleanly into `main` before tagging the versioned release.

## Required Steps for Every Change

1. **Consult the Plan & Branch First**:
   - Check [docs/IMPLEMENTATION_PLAN.md](../../docs/IMPLEMENTATION_PLAN.md) before implementing so new work aligns with the schema, routes, and design guardrails.
   - Before starting any new major feature, switch to a dedicated branch (`git checkout -b "feat(scope)/what-to-implement"`, with **no phase numbers**) from a clean `main` branch.
2. **Implement Cleanly**:
   - Keep public design tokens and frontend behavior identical to `../zconst/`.
   - Write PHPUnit feature/unit tests for any new routes, controllers, requests, or models.
3. **Verify**:
   - Run `php artisan test` (or relevant test suite) and check Laravel Pint / syntax before versioning.
4. **Update Version & Docs**:
   - Bump `VERSION` at the repository root.
   - Add a dated entry under a new `## [X.Y.Z] - YYYY-MM-DD` heading in [docs/CHANGELOG.md](../../docs/CHANGELOG.md) categorizing changes under `Added`, `Changed`, `Fixed`, or `Removed`.
   - Check off completed items in [docs/IMPLEMENTATION_PLAN.md](../../docs/IMPLEMENTATION_PLAN.md).
5. **Commit, Merge & Tag**:
   - Stage changes and commit with a clear message referencing the version (e.g., `chore(release): v0.2.0 - migrate static assets and base layout`).
   - If working on a feature branch, merge into `main` once verified (`git checkout main && git merge --no-ff "branch-type(feature)/what-to-implement"`).
   - Create a git tag matching the version: `git tag vX.Y.Z`.

