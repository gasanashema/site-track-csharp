# Workspace Agent Customizations (`.agent/`)

This directory holds project-specific AI agent **skills** and **rules** for `zconstweb`.

## Adding a New Skill

Place each skill inside its own lowercase, hyphenated folder under `.agent/skills/`:

```text
.agent/
├── README.md
└── skills/
    └── <skill-name>/
        ├── SKILL.md       # Required: YAML frontmatter (name, description) + instructions
        ├── scripts/       # Optional: helper scripts
        ├── references/    # Optional: detailed docs
        └── resources/     # Optional: templates/assets
```

### Required `SKILL.md` Header

```markdown
---
name: your-skill-name
description: >-
  Describe clearly what the skill does and when the agent should activate it.
---
```

## Included Skills
- [`versioning-workflow`](./skills/versioning-workflow/SKILL.md): Enforces branching before major features, semantic versioning (`VERSION`), `docs/CHANGELOG.md` updates, implementation plan tracking, test execution, and git commit/tag creation on every change.
- [`grill-me`](./skills/grill-me/SKILL.md): Stress-tests the project plan through interactive Q&A to surface gaps and decisions before writing implementation code.
- [`phase-by-phase`](./skills/phase-by-phase/SKILL.md): Breaks the plan into small phases in `docs/phase-by-phase-plan.md`, implements one phase at a time, and pauses for review after each phase.

