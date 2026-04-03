---
name: team-release
description: "Orchestrates a full release process across all departments. Coordinates qa-lead, release-manager, devops-engineer, and community-manager for a coordinated launch."
argument-hint: "[version number]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, Task
---

When this skill is invoked:

1. **Read** milestone docs, open bugs, and release checklist.

2. **Launch parallel pre-release checks**:
   - **qa-lead**: Final QA pass — all P1/P2 resolved?
   - **release-manager**: Build configuration review, version number, store assets
   - **devops-engineer**: CI build passing? Build artifacts ready?
   - **security-engineer**: Final security audit (credentials, anti-cheat)

3. **Synthesize results** — any blockers?

4. **If clear to proceed**:
   - **release-manager**: Trigger final release build
   - **community-manager**: Prepare launch communications (patch notes, social)
   - **analytics-engineer**: Verify analytics events active in release build

5. **Post-launch monitoring plan** (24h, 72h, 1-week):
   - Crash rate threshold: > 0.5% triggers hotfix evaluation
   - Store rating monitoring
   - Community feedback triage

6. **Present go/no-go decision** to user with all department statuses.
