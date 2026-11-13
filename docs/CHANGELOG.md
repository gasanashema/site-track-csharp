# Changelog

All notable changes to the **SiteTrack Construction Manager** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-11-13

### Added
- **Solution & Project Layering**: Created `SiteTrack.sln` containing `SiteTrack.Core`, `SiteTrack.Data`, `SiteTrack.Controllers`, `SiteTrack.UI`, and `SiteTrack.Tests`.
- **Database DDL Schema (16 Tables)**: Implemented complete relational schema script `docs/001_initial_schema.sql` (and embedded copy in `SiteTrack.Data/Scripts/001_initial_schema.sql`) covering all 16 tables: `users`, `projects`, `project_managers`, `material_categories`, `materials`, `material_purchases`, `project_material_stock`, `material_stock_movements`, `material_usage`, `project_activities`, `worker_types`, `site_workers`, `worker_attendance`, `worker_payments`, `otp_verifications`, and `notification_logs`.
- **Domain Entity POCOs**: Implemented all 16 C# entity models in `SiteTrack.Core.Models` with navigation helper properties, business calculations (e.g. `IsLowStock`, `TotalStockValue`, `BalanceDue`, `IsFullyPaid`), and default values.
- **Validation & Flow Types**: Implemented `Enums.cs` for role/status types, `Result.cs` & `Result<T>` operation response wrappers, and `ValidationException.cs`.
- **Base MVC Controller**: Implemented `BaseController` with static application session state tracking (`CurrentUser`, `IsAuthenticated`, `IsAdmin`).
- **Automated Domain Test Suite**: Implemented `SiteTrack.Tests` with 8 automated unit tests verifying default states, low stock triggers, weighted average formulas, attendance gating, and payment balance logic. All 8 tests pass with 100% success rate.
- **Entry Point UI Shell**: Created WinForms project `SiteTrack.UI` with initial `Program.cs`.

## [0.1.0] - 2026-11-13

### Added
- **Project Foundation & Requirements Analysis**: Synthesized system requirements from AUCA DotNet Lab 3 specification (`docs/requirements.md` and `docs/todo.md`) into core project documentation.
- **Root Version File**: Created `VERSION` file tracking semantic version `0.1.0`.
- **Master Project Plan**: Created `plan.md` defining system goals, MVC architecture on .NET Framework WinForms, in-scope/out-of-scope modules, all 16 database entities, 5 business validation rules, 5 technical validation rules, role matrix, and decisions.
- **Detailed Implementation Plan**: Created `docs/IMPLEMENTATION_PLAN.md` with multi-tier solution layout (`SiteTrack.Core`, `SiteTrack.Data`, `SiteTrack.Controllers`, `SiteTrack.UI`, `SiteTrack.Tests`), database DDL roadmap, milestone checklists, and UX guardrails.
- **Phase-by-Phase Plan**: Created `docs/phase-by-phase-plan.md` breaking implementation down into 10 structured, verifiable phases with concrete goals, tasks, acceptance criteria, and changelog sections.
- **Agent Skill Integration**: Integrated workspace skills (`versioning-workflow`, `grill-me`, and `phase-by-phase`) for continuous tracking, interactive review, and strict phase-driven execution.

