# Phase 2 — Class Rename

> ⚠️ File này chưa được cung cấp. Cần upload `Phase2ClassRename.md` để hoàn thiện skill.

## Nội dung dự kiến

Phase 2 bao gồm:
1. **Backup** — Git tag hoặc manual folder copy trước khi apply rename
2. **Apply Rename** — Đọc `class_rename_mapping.md`, thực hiện đúng thứ tự trong `AGENT_EXECUTION_CHECKLIST`
3. **Verify** — Compare GUIDs, check compile errors, detect missing scripts mới

## Thứ tự apply (từ AGENT_EXECUTION_CHECKLIST trong Phase 1 output)

```
[1] Load EXCLUDED_SYMBOLS → blacklist
[2] Apply NAMESPACE_MAP   → replace trong namespace + using, tất cả .cs
[3] Apply CLASS_MAP       → Pass 1: replace trong code
                         → Pass 2: rename .cs + .meta (Rename File = YES)
[4] Apply METHOD_MAP      → replace \bMethod\b
[5] Apply FIELD_MAP       → serialized=YES: thêm FormerlySerializedAs trước
                         → sau đó rename field
[6] Apply PARAM_MAP       → replace params trong signatures + call sites
[7] Log MANUAL_REVIEW_FLAGS → ghi vào phase3_report.json, không tự sửa
```

## TODO

Upload `Phase2ClassRename.md` và chạy lại `/skill-creator unity-script-chameleon` để cập nhật.
