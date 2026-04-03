---
name: launch-checklist
description: "Generates a comprehensive launch-day checklist covering all departments and coordinating simultaneous launch activities."
argument-hint: "[launch date or 'today']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write
---

When this skill is invoked:

1. **Read** the release checklist and milestone docs.
2. **Generate the launch-day playbook**:

```markdown
# Launch Day Playbook — [Game Name] v[Version]
**Launch Date**: [Date]

## T-7 Days
- [ ] Store pages finalized (screenshots, description, trailer)
- [ ] Press builds sent to press contacts
- [ ] Community: launch announcement scheduled
- [ ] Final build: gold master candidate created

## T-24 Hours
- [ ] Final build: certified/approved by all platforms
- [ ] Devops: server capacity verified (if online)
- [ ] Analytics: launch event tracking verified
- [ ] Support: FAQ and known issues doc ready
- [ ] Community: launch post drafted, social content scheduled

## T-0 (Launch)
- [ ] Build: submitted/released to all stores
- [ ] Community: launch post published
- [ ] Monitoring: crash reporting active (threshold: > 0.5% = hotfix consideration)
- [ ] Analytics: launch dashboard open
- [ ] Team: on standby for first 4 hours

## T+24 Hours
- [ ] Review first day metrics vs. projections
- [ ] Community: respond to player feedback
- [ ] QA: triage any new crash reports

## Emergency Contacts
- Crash > 1%: alert [technical-director]
- Store issue: alert [release-manager]
- Community crisis: alert [community-manager]
```

3. **Ask for approval** before writing to `production/milestones/launch-playbook.md`.
