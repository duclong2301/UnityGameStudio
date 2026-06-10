---
name: unity-prefab-architect
description: >
  Guidance for working with GameObjects, Prefabs, Nested Prefabs, and Prefab Variants in Unity.
  Use when building or modifying reusable objects — turning a GameObject into a prefab, nesting
  prefabs, creating a variant of a base, or deciding how to structure repeated content. Enforces:
  prefab everything, compose with nested prefabs, use variants instead of duplicating.
---

# Unity Prefab Architect

Decision-first rules for reusable content. Apply before creating or duplicating any GameObject.

## Core Rules
- **Prefab everything** — repeated GameObjects should be prefab assets (folders excepted), so
  components stay synced and updates propagate from one source.
- **Nested Prefabs** — put small parts (weapons, VFX, widgets) inside larger prefabs; each part
  edits independently and updates flow to every parent.
- **Variants, not duplicates** — same base, few differences (enemy vs. elite) → make a Variant.
  Base edits propagate down; the variant only stores its overrides.

> Reusable + differs from a base → **Variant** · built from parts → **Nested** · otherwise → **Prefab**.
> Never duplicate a prefab to tweak a value.

## Workflows
- **GameObject → Prefab**: build the object + components → save as a prefab asset → replace scene
  copies with instances → verify a fresh instance.
- **Nested**: ensure each sub-part is its own prefab → instantiate under the parent (keep links) →
  save the parent; edit the child prefab later to update all parents.
- **Variant**: confirm the base exists → instantiate it (keep the prefab link, don't unpack) →
  override only what differs → save as a variant of the base → verify base + variant.

## Pitfalls
- Unpacking the instance before saving breaks the variant link → keep it connected.
- Duplicating instead of varying causes drift and blocks bulk updates.
- Monolithic prefabs hide reusable parts → split into nested prefabs.
- Loose scene objects can't be updated in bulk → prefab them.

## Conventions
- Store under `Assets/Prefabs/<Category>/`; name variants to reveal their base (`Enemy_Goblin_Elite`).
