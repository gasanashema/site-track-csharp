---
name: grill-me
description: Interview the user about their project using plan.md as the source, then update plan.md with every new feature, change, or decision that comes up. Use when the user says "grill me", wants their plan stress-tested, or wants to refine plan.md before implementation.
---

# Grill Me

## Goal
Find gaps, ambiguities, and unstated assumptions in plan.md by questioning the user, and keep plan.md updated as the answers come in. Do NOT write implementation code in this skill.

## Steps
1. Read plan.md fully (look in the project root, then docs/). If it does not exist, ask the user to point to it or offer to create a first draft from what they describe.
2. Silently list what is unclear or missing: scope, users and roles, data and entities, edge cases, error handling, permissions, integrations, non-functional needs (performance, security), what is out of scope, and what "done" means for the MVP.
3. Ask ONE question at a time, most important first. Where useful, give 2-3 concrete options with a recommended default so the user can answer quickly. Prefer questions grounded in specific lines of plan.md over generic ones.
4. After each answer, immediately edit plan.md:
   - New feature or change: add or modify it in the right section.
   - Decision: record it under a "## Decisions" section with a one-line reason.
   - Unanswered or deferred item: add it under "## Open questions".
   - Something the user says is out of scope: add it under "## Out of scope".
   Then tell the user in one line what you changed, and ask the next question.
5. If an answer contradicts something already in plan.md, point out the conflict and ask which one wins before editing.
6. Stop when the user says they are done, or when no meaningful gaps remain. Then give a short summary of what changed in plan.md and list any remaining open questions.

## Rules
- Never silently drop or rewrite existing content in plan.md; change only what the conversation justifies.
- Do not ask questions whose answers are already in plan.md or the codebase.
- Do not start planning phases or implementing. Suggest the phase-by-phase skill as the next step once the user is happy with plan.md.
- If you need to understand the existing codebase to ask a good question, use a subagent to explore it and return a short summary, instead of reading many files yourself. Never use subagents to answer the questions for the user.