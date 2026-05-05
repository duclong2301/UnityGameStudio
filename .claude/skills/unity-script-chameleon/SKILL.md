---
name: unity-script-chameleon
description: >
  Full Unity project rename pipeline — renames C# classes, namespaces, methods, fields, params,
  and asset files across an entire Unity project for reskinning purposes. Use this skill whenever
  the user mentions: renaming Unity scripts, reskinning a Unity game, changing class names in Unity,
  refactoring Unity namespaces, auto-generating rename mappings, running phase0/phase1/phase2/phase3
  of a reskin pipeline, or any workflow involving scan_report.json / reskin_config.json /
  class_rename_mapping.md. Also trigger when the user asks about GUID-safe rename, FormerlySerializedAs
  automation, or bulk C# symbol rename in Unity. This skill orchestrates a multi-phase agentic pipeline:
  Phase 0 (deep scan) → Phase 1 (config + AI mapping) → Phase 2 (backup) → Phase 3 (apply rename).
---

# Unity Script Chameleon

Agentic pipeline để rename toàn bộ C# symbols trong một Unity project — namespaces, classes, methods,
fields, params, file names — không phá vỡ GUIDs hay serialized data.

---

## Tổng quan Pipeline

```
Phase 0: Deep Scan          → scan_report.json
Phase 1a: Claude Interview  → reskin_config.json
Phase 1b: AI Mapping        → class_rename_mapping.md
Phase 2: Backup + Git Tag   → safety checkpoint
Phase 3: Apply Rename       → project renamed
Phase 4+: Sprite / Scene... → (future phases)
```

**Nguyên tắc bất biến:**
- Phase 0 là read-only — không sửa gì, chỉ scan.
- GUID không bao giờ thay đổi — chỉ đổi file content + tên file.
- Serialized fields → phải thêm `[FormerlySerializedAs]` trước khi rename.
- `phase1_mapper.py` là generic script — không bao giờ sửa, chỉ sửa `reskin_config.json`.
- Unity Editor phải đóng trước Phase 3.

---

## Phase Detection

Khi user bắt đầu, xác định họ đang ở phase nào:

| Dấu hiệu | Phase cần làm |
|---|---|
| Chưa có `scan_report.json` | Bắt đầu Phase 0 |
| Có `scan_report.json`, chưa có `reskin_config.json` | Bắt đầu Phase 1a |
| Có `reskin_config.json`, chưa có `class_rename_mapping.md` | Phase 1b (chạy script) |
| Có mapping, chưa backup | Phase 2 |
| Có backup, chưa rename | Phase 3 |
| User hỏi về phase cụ thể | Jump thẳng vào phase đó |

---

## Phase 0 — Deep Scan

📖 **Chi tiết đầy đủ:** `references/phase0_deep_scan.md`

**Tóm tắt:**
- Chạy `phase0_scanner.py` để scan toàn bộ project via Python (file system)
- Chạy Unity MCP tools để verify từ Editor side
- Output: `scan_report.json` + `scan_human_review.txt`

**Dữ liệu thu thập:**
- C# symbols: namespaces, classes, methods, fields, serialized fields, usings
- GUID mapping (từ .meta files) — detect duplicates + orphans
- Sprite inventory (dimensions, sprite mode, frames)
- Dependency graph: scene/prefab → script GUID references
- Animation clips (.anim property paths, animator parameters)
- Assembly definitions (.asmdef names, rootNamespace, references)

**Khi nào xong:** `scan_report.json` tồn tại, không có critical warnings, tất cả GUIDs tracked.

---

## Phase 1a — Claude Gen Config

📖 **Chi tiết đầy đủ:** `references/phase1_mapping.md`

Claude hỏi user 4 câu tuần tự:

```
1. Tên game mới là gì?
2. Art style / theme visual?  (e.g., "pixel art, vibrant fantasy")
3. Strategy đổi tên?          (semantic | prefix_swap | obfuscate)
4. SDK nào cần exclude thêm?  (để trống nếu không có)
```

Sau đó:
- Đọc `scan_report.json` → auto-detect SDK từ `using` statements
- Merge SDK auto-detected + SDK user nhập + base DLL list
- Gen `reskin_config.json`

**Output:** `reskin_config.json` (project-specific, nhỏ, dễ đọc)

---

## Phase 1b — AI Mapping Script

📖 **Chi tiết đầy đủ:** `references/phase1_mapping.md` (section "Phase 1b")
📜 **Script:** `scripts/phase1_mapper.py`

```bash
python phase1_mapper.py \
  --config reskin_config.json \
  --scan   scan_report.json \
  --output class_rename_mapping.md
```

Script gọi Anthropic API theo batch để gen mapping cho:
- `namespace_map` — replace trong namespace declarations + using statements
- `class_map` — word-boundary replace `\bOldName\b`, rename .cs + .meta nếu cần
- `method_map` — replace `\bOldMethod\b(?=\s*[\(<])`
- `field_map` — serialized=YES → thêm FormerlySerializedAs trước khi rename
- `param_map` — replace params trong signatures + call sites

**Output:** `class_rename_mapping.md` + `class_rename_mapping_raw.json`

**Human review required:** User phải approve mapping trước khi tiếp tục Phase 2.

---

## Phase 2 — Backup + Git Tag

📖 **Chi tiết:** `references/phase2_class_rename.md` *(pending — file chưa được cung cấp)*

**Tóm tắt workflow:**
```bash
# Git approach (recommended)
git add -A && git commit -m "pre-reskin: backup before Phase 3"
git tag pre-reskin-backup

# Manual approach
cp -r UnityProject UnityProject_backup_$(date +%Y%m%d)
```

**Checklist:**
- [ ] Git tag hoặc folder backup đã tạo
- [ ] `scan_report.json` đã backup riêng (dùng verify ở Phase 7)
- [ ] Unity Editor đã ĐÓNG
- [ ] Xác nhận với user trước khi tiến hành Phase 3

---

## Phase 3 — Apply Rename

📖 **Chi tiết:** `references/phase2_class_rename.md` *(pending)*

**Execution order (từ `AGENT_EXECUTION_CHECKLIST` trong mapping file):**

```
PRE-CONDITIONS:
  [ ] scan_report.json tồn tại
  [ ] Backup đã tạo
  [ ] Unity Editor đã ĐÓNG

EXECUTION ORDER:
  [1] Load EXCLUDED_SYMBOLS → blacklist
  [2] Apply NAMESPACE_MAP   → replace trong namespace + using
  [3] Apply CLASS_MAP       → Pass 1: replace trong code
                           → Pass 2: rename .cs + .meta
  [4] Apply METHOD_MAP      → \bMethod\b replace
  [5] Apply FIELD_MAP       → serialized=YES: FormerlySerializedAs trước
  [6] Apply PARAM_MAP       → replace trong signatures + call sites
  [7] Log MANUAL_REVIEW_FLAGS → ghi vào phase3_report.json, không tự sửa

POST-CONDITIONS:
  [ ] Mở Unity Editor
  [ ] 0 compile errors
  [ ] 0 missing scripts mới
  [ ] GUID unchanged (compare với scan_report.guid_map)
```

---

## Error Handling

| Problem | Solution |
|---|---|
| GUID không match | Ctrl+R (Reimport) trong Unity, scan lại |
| Missing scripts detected | Check danh sách, xem script nào bị delete |
| API call fail trong Phase 1b | Script fallback về identity mapping cho batch đó |
| Compile errors sau Phase 3 | Restore từ backup, check mapping collision |
| `[CRITICAL]` trong validation | Fix collision trước khi chạy Phase 3 |

---

## Quick Reference

```bash
# Phase 0
python phase0_scanner.py --project-path /path/to/project --output scan_report.json

# Phase 1b  
python phase1_mapper.py  # đọc reskin_config.json + scan_report.json tự động

# Verify sau Phase 3
python phase7_verify.py --original scan_report.json --current project/
```

**Files mà Claude tạo:** `reskin_config.json`
**Files do script tạo:** `scan_report.json`, `class_rename_mapping.md`, `class_rename_mapping_raw.json`
**Files do user tạo:** backup/git tag
