---
name: phase-by-phase
description: Turn plan.md into small implementation phases in docs/phase-by-phase-plan.md, implement one phase at a time, and stop for the user's review after each. Use when the user wants phased implementation, says "phase by phase", or says "proceed" while a phase plan exists.
---

# Phase-by-Phase

## Core rules
1. Implement ONE phase at a time. After finishing a phase, STOP and wait for the user's review. Never start the next phase on your own, even if it looks trivial.
2. docs/phase-by-phase-plan.md is the single source of truth. Read it at the start of every session.
3. Mark a phase as done ONLY when the user says "proceed" (or clearly equivalent).

## Setup (first run)
1. Read plan.md. If it does not exist, STOP and tell the user; suggest running the grill-me skill first.
2. Create docs/phase-by-phase-plan.md with this structure:

# Phase-by-Phase Plan: <project name>

Source: plan.md

## Progress
- [ ] Phase 1: <name>
- [ ] Phase 2: <name>
(one line per phase)

## Phases

### Phase 1: <name>
**Goal:** <one sentence>
**Tasks:**
- <small concrete task>
**Acceptance criteria:**
- <checkable statement>
**Changelog:**
- (empty until review)

(repeat for each phase)

3. Phase guidelines: each phase is small (about 3-8 tasks), leaves the project in a working state, and is ordered so earlier phases unblock later ones (foundation, core features, extras, polish). Cover everything in plan.md across the phases and respect its "Out of scope" section.
4. Immediately start Phase 1 (the user asked for phases; they review per phase, not the split). Do not wait for plan approval unless the user said otherwise.

## Implementing a phase
1. Implement only that phase's tasks.
2. Run or test the project and check each acceptance criterion.
3. Tell the user: what you built, files changed, how to run and verify it, and any deviations. Then STOP and wait.

## Using subagents
Use subagents to speed up a phase, but you stay responsible for the result.

When to use them:
- Parallel work: tasks in the current phase that are independent (different files or modules, no shared state), such as a migration, a model, and a UI component. Launch these subagents in a single message so they run concurrently.
- Exploration: before implementing, send a subagent to search the codebase for existing patterns, conventions, or files to reuse, and have it return only a short summary.
- Verification: after implementing, a subagent can run tests, the linter, or the build and report only failures.

When NOT to use them:
- Tasks that depend on each other or edit the same files. Do those yourself, in order.
- Very small tasks where spawning costs more than doing.

How to delegate:
1. Give each subagent one clearly scoped task: the goal, the exact files it may touch, the conventions to follow, and what to return.
2. Tell each subagent: work ONLY on this task, do not start other tasks, do not touch other phases, and do not edit docs/phase-by-phase-plan.md or plan.md.
3. When they finish, review their output yourself. Check for conflicts, duplicated code, and missed acceptance criteria, and fix problems before reporting.
4. Only you (the main agent) update the plan file and talk to the user. Subagents never mark a phase as done or begin the next phase.

The review stop still applies: after all subagents finish and you have verified the result, STOP and wait for the user's review as usual.

## Handling the user's reply
- Change requests: apply them. Add each to that phase's Changelog as a dated one-line entry (what was requested, what was done). If a change adds or alters scope, also update plan.md. Then report and wait again.
- "proceed": first apply any changes mentioned in the same message and log them. Then mark the phase as done: change its line in Progress to [x]. Then start the next phase.
- After the final phase is done, say the plan is complete and summarize.

## Changing the plan
If a later phase must change (missing task, wrong order, new phase), propose the change, get the user's OK, update the file, and add a Changelog entry.

## Do not
- Do not mark a phase done before the user says proceed.
- Do not do work from a later phase early.
- Do not add features beyond the current phase's tasks.