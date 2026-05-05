# Phase 1 (v2) — Config Generation + Auto-gen Mapping

> **Thay đổi so với v1:** Tách thành 2 bước rõ ràng — **(1a) Claude hỏi user → gen `reskin_config.json`**, **(1b) script generic đọc config → gen mapping**. Script `phase1_mapper.py` dùng chung mọi project, không bao giờ bị sửa. Chỉ `reskin_config.json` là project-specific.
>
> **Rủi ro:** Rất thấp — chỉ đọc file, không sửa gì.
> **Input:** `scan_report.json` từ Phase 0
> **Output:** `reskin_config.json` + `class_rename_mapping.md` cho Phase 3

---

## Tổng quan flow

```
[Phase 0 xong] → scan_report.json tồn tại
        ↓
[Phase 1a] Claude hỏi user 4 câu
        ↓
Claude đọc scan_report.json → detect SDK có trong project
        ↓
Claude gen reskin_config.json (project-specific, nhỏ, dễ đọc)
        ↓
[Phase 1b] User chạy: python phase1_mapper.py
        ↓ (đọc reskin_config.json + scan_report.json)
        ↓ (gọi Anthropic API theo batch)
        ↓
class_rename_mapping.md  +  class_rename_mapping_raw.json
```

---

## Phase 1a — Claude gen `reskin_config.json`

### 1a.1 — Claude hỏi user (4 câu)

Khi user bắt đầu Phase 1, Claude hỏi tuần tự:

```
1. "Tên game mới là gì?"
   → Dùng làm project_name, prefix trong mapping

2. "Art style / theme visual?"
   → Ví dụ: "pixel art, vibrant fantasy"
   →        "minimalist, pastel, cute"
   →        "neon, cyberpunk, dark"

3. "Strategy đổi tên?"
   → semantic   : tên mới có nghĩa tương đương, phù hợp art style
   → prefix_swap: chỉ đổi prefix (ví dụ Wire→Tile, Route→Path)
   → obfuscate  : tên random hợp lệ C#, không liên quan tên cũ

4. "Có SDK nào đặc biệt cần exclude không?"
   → Ví dụ: Spine, LeanTween, Odin, PlayFab
   → Để trống nếu không có
```

### 1a.2 — Claude detect SDK từ scan_report.json

Trước khi gen config, Claude đọc `scan_report.json` và tìm các `using` statements không thuộc source code:

```python
# Claude thực hiện internally — không cần user làm gì
KNOWN_SDK_PREFIXES = {
    "Spine":        "Spine.*",
    "LeanTween":    "LeanTween.*",
    "Odin":         "Sirenix.*",
    "PlayFab":      "PlayFab.*",
    "GameAnalytics":"GameAnalytics.*",
    "Facebook":     "Facebook.*",
    "Adjust":       "com.adjust.*",
    "IronSource":   "IronSource.*",
    "AppLovin":     "MaxSdk.*",
}
# Scan usings trong scan_report → auto-detect SDK có trong project
# Merge với extra_sdk_prefixes từ user
```

### 1a.3 — Output: `reskin_config.json`

Claude tạo file này sau khi có đủ thông tin:

```json
{
  "_comment": "reskin_config.json — Project-specific config cho phase1_mapper.py",
  "_generated_by": "Claude Phase 1a",
  "_scan_report": "scan_report.json",

  "project_name": "HeroQuest",
  "art_style": "pixel art, vibrant fantasy, warm colors",
  "strategy": "semantic",

  "scan_scope": [
    "Assets/Scripts",
    "Assets/Base"
  ],

  "exclude_paths": [
    "Assets/Plugins",
    "Assets/NotchSolution",
    "Library",
    "Packages",
    "Temp"
  ],

  "dll_namespace_prefixes": [
    "UnityEngine", "UnityEditor", "Unity",
    "System", "Microsoft", "Newtonsoft",
    "TMPro", "DG", "Cysharp", "VContainer",
    "Zenject", "Photon", "Mirror", "Sirenix",
    "Mono", "AOT", "NUnit",
    "AppsFlyerSDK",
    "GoogleMobileAds"
  ],

  "api": {
    "model": "claude-opus-4-20250514",
    "batch_size": 60,
    "max_tokens": 2000,
    "retry_attempts": 3
  }
}
```

**Lưu ý:** `dll_namespace_prefixes` = base list + SDK auto-detected + SDK user nhập thêm. Claude merge tất cả lại trước khi ghi file.

---

## Phase 1b — `phase1_mapper.py` (generic, dùng chung mọi project)

Script này **không bao giờ bị sửa**. Mọi thay đổi project-specific đều nằm trong `reskin_config.json`.

### 1b.1 — Entrypoint

```bash
# Chạy với config mặc định (reskin_config.json cùng thư mục)
python phase1_mapper.py

# Hoặc chỉ định rõ
python phase1_mapper.py \
  --config   reskin_config.json \
  --scan     scan_report.json \
  --output   class_rename_mapping.md
```

### 1b.2 — Script đầy đủ

```python
#!/usr/bin/env python3
"""
phase1_mapper.py  —  GENERIC, không sửa file này
Đọc reskin_config.json + scan_report.json → gen class_rename_mapping.md
"""

import json, os, re, sys, time, argparse
from datetime import datetime
from pathlib import Path
import anthropic

# ----------------------------------------------------------------
# UNITY LIFECYCLE WHITELIST — cố định, không thay đổi theo project
# ----------------------------------------------------------------
UNITY_LIFECYCLE_WHITELIST = {
    "Awake","Start","Update","FixedUpdate","LateUpdate",
    "OnEnable","OnDisable","OnDestroy","OnValidate","Reset",
    "OnCollisionEnter","OnCollisionStay","OnCollisionExit",
    "OnCollisionEnter2D","OnCollisionStay2D","OnCollisionExit2D",
    "OnTriggerEnter","OnTriggerStay","OnTriggerExit",
    "OnTriggerEnter2D","OnTriggerStay2D","OnTriggerExit2D",
    "OnControllerColliderHit","OnJointBreak","OnJointBreak2D",
    "OnMouseDown","OnMouseUp","OnMouseEnter","OnMouseExit",
    "OnMouseOver","OnMouseDrag","OnMouseUpAsButton",
    "OnBecameVisible","OnBecameInvisible",
    "OnPreRender","OnPostRender","OnRenderObject",
    "OnWillRenderObject","OnRenderImage","OnGUI",
    "OnDrawGizmos","OnDrawGizmosSelected",
    "OnAnimatorMove","OnAnimatorIK","OnAudioFilterRead",
    "OnTransformParentChanged","OnTransformChildrenChanged",
    "OnBeforeTransformParentChanged",
    "OnRectTransformDimensionsChange","OnCanvasGroupChanged",
    "OnApplicationPause","OnApplicationQuit","OnApplicationFocus",
    "OnNetworkSpawn","OnNetworkDespawn",
    "OnBeforeSerialize","OnAfterDeserialize",
    "GetHashCode","Equals","ToString","Finalize",
    "MemberwiseClone","GetType",
}


# ----------------------------------------------------------------
# LOAD CONFIG
# ----------------------------------------------------------------
def load_config(config_path: str) -> dict:
    with open(config_path, encoding="utf-8") as f:
        return json.load(f)


def load_scan_report(scan_path: str) -> dict:
    with open(scan_path, encoding="utf-8") as f:
        return json.load(f)


# ----------------------------------------------------------------
# COLLECT SYMBOLS
# ----------------------------------------------------------------
def collect_symbols(scan_report: dict, config: dict) -> dict:
    dll_prefixes = tuple(config.get("dll_namespace_prefixes", []))
    exclude_paths = config.get("exclude_paths", ["Library", "Packages", "Temp"])

    source_classes:    list[dict] = []
    source_namespaces: list[str]  = []
    source_methods:    list[dict] = []
    source_fields:     list[dict] = []
    source_params:     list[dict] = []
    excluded_dll:      set[str]   = set()
    excluded_lifecycle:set[str]   = set()

    for script in scan_report.get("scripts", []):
        filepath = script.get("filepath", "")

        # Skip excluded paths
        if any(p in filepath for p in exclude_paths):
            continue

        # Namespaces
        for ns in script.get("namespaces", []):
            if not ns.startswith(dll_prefixes):
                if ns not in source_namespaces:
                    source_namespaces.append(ns)
            else:
                excluded_dll.add(ns)

        # Classes
        for cls in script.get("classes", []):
            source_classes.append({"name": cls, "file": filepath})

        # Methods
        for m in script.get("methods", []):
            name = m["name"] if isinstance(m, dict) else m
            if name in UNITY_LIFECYCLE_WHITELIST:
                excluded_lifecycle.add(name)
                continue
            source_methods.append({"name": name, "file": filepath})

        # Fields
        for f in script.get("fields", []):
            fname       = f["name"]        if isinstance(f, dict) else f
            is_ser      = f.get("is_serialized", False) if isinstance(f, dict) else False
            source_fields.append({"name": fname, "file": filepath, "serialized": is_ser})

        # Params
        for p in script.get("params", []):
            pname  = p["param"]             if isinstance(p, dict) else p
            method = p.get("method", "?")   if isinstance(p, dict) else "?"
            source_params.append({"name": pname, "method": method, "file": filepath})

    # Sort dài → ngắn (bắt buộc cho Phase 3)
    source_namespaces.sort(key=len, reverse=True)
    source_classes.sort(key=lambda x: len(x["name"]), reverse=True)

    return {
        "namespaces":          source_namespaces,
        "classes":             source_classes,
        "methods":             source_methods,
        "fields":              source_fields,
        "params":              source_params,
        "excluded_dll":        sorted(excluded_dll),
        "excluded_lifecycle":  sorted(excluded_lifecycle),
        "total_cs_files":      len(scan_report.get("scripts", [])),
    }


# ----------------------------------------------------------------
# ANTHROPIC API — BATCH CALLS
# ----------------------------------------------------------------
SYSTEM_PROMPT = """
Bạn là rename mapping generator cho Unity game reskin tool.
Nhiệm vụ: Nhận danh sách C# symbol names, trả về JSON mapping old→new.

Quy tắc BẮTBUỘC:
1. Trả về JSON ONLY — không text, không markdown backticks, không giải thích
2. Mỗi symbol trong input PHẢI có entry trong output, không bỏ sót
3. Nếu không cần đổi → map sang chính nó: "PlayerHP": "PlayerHP"
4. Class/struct/enum/interface/method: PascalCase
5. Field/param/variable: camelCase (private), PascalCase (public property)
6. Tên mới KHÔNG được trùng nhau
7. Tên mới phải là valid C# identifier
8. KHÔNG ĐỔI TÊN: Start, Update, Awake, FixedUpdate, LateUpdate, OnEnable,
   OnDisable, OnDestroy, OnCollision*, OnTrigger*, OnMouse*, OnGUI,
   OnApplicationPause, OnApplicationQuit, Equals, GetHashCode, ToString
""".strip()


def call_api_batch(client, batch_type: str, items: list[str], config: dict) -> dict:
    project_name = config["project_name"]
    strategy     = config["strategy"]
    art_style    = config["art_style"]
    api_cfg      = config.get("api", {})
    model        = api_cfg.get("model", "claude-opus-4-20250514")
    max_tokens   = api_cfg.get("max_tokens", 2000)
    retries      = api_cfg.get("retry_attempts", 3)

    strategy_desc = {
        "prefix_swap": f"Đổi prefix cũ thành prefix mới phù hợp project '{project_name}'. Giữ phần còn lại.",
        "semantic":    f"Tên mới có nghĩa tương đương, phù hợp art style: {art_style}. Project: {project_name}.",
        "obfuscate":   "Tên hợp lệ C# không liên quan tên cũ, đảm bảo unique.",
    }.get(strategy, "Giữ nghĩa gốc, đổi context sang game mới.")

    user_msg = f"""Category: {batch_type}
Strategy: {strategy_desc}
Count: {len(items)}

Symbols:
{json.dumps(items, ensure_ascii=False)}

Trả về JSON mapping cho tất cả {len(items)} symbols."""

    for attempt in range(retries):
        try:
            resp = client.messages.create(
                model=model, max_tokens=max_tokens,
                system=SYSTEM_PROMPT,
                messages=[{"role": "user", "content": user_msg}],
            )
            raw = resp.content[0].text.strip()
            raw = re.sub(r"^```(?:json)?\s*", "", raw)
            raw = re.sub(r"\s*```$", "", raw)
            return json.loads(raw)
        except json.JSONDecodeError:
            m = re.search(r"\{[\s\S]*\}", raw)
            if m:
                try:
                    return json.loads(m.group(0))
                except Exception:
                    pass
            print(f"  [WARN] Attempt {attempt+1}: JSON parse failed, retrying...")
            time.sleep(2)
        except Exception as e:
            print(f"  [WARN] Attempt {attempt+1}: {e}, retrying...")
            time.sleep(3)

    print(f"  [ERROR] {batch_type} failed — identity mapping fallback")
    return {item: item for item in items}


def gen_all_mappings(symbols: dict, config: dict, api_key: str) -> dict:
    client     = anthropic.Anthropic(api_key=api_key)
    batch_size = config.get("api", {}).get("batch_size", 60)

    result = {
        "namespace_map": {}, "class_map": {},
        "method_map": {},    "field_map": {},
        "param_map": {},
    }

    def process(category: str, items: list[str], key: str):
        if not items:
            return
        total = len(items)
        batches = (total + batch_size - 1) // batch_size
        print(f"\n  [{category}] {total} symbols → {batches} batch(es)")
        for i in range(0, total, batch_size):
            batch = items[i : i + batch_size]
            print(f"    Batch {i//batch_size+1}: {len(batch)} items...")
            result[key].update(call_api_batch(client, category, batch, config))
            time.sleep(0.5)

    process("NAMESPACE", symbols["namespaces"],
            "namespace_map")
    process("CLASS",     [c["name"] for c in symbols["classes"]],
            "class_map")
    process("METHOD",    list(dict.fromkeys(m["name"] for m in symbols["methods"])),
            "method_map")
    process("FIELD",     list(dict.fromkeys(f["name"] for f in symbols["fields"])),
            "field_map")
    process("PARAM",     list(dict.fromkeys(p["name"] for p in symbols["params"])),
            "param_map")

    return result


# ----------------------------------------------------------------
# VALIDATE
# ----------------------------------------------------------------
def validate_mapping(mapping: dict) -> list[str]:
    issues = []
    for category, mp in mapping.items():
        seen = {}
        for old, new in mp.items():
            if new != old:
                if new in seen:
                    issues.append(f"[{category}] Collision: '{old}' và '{seen[new]}' → '{new}'")
                seen[new] = old
        if category == "method_map":
            for old, new in mp.items():
                if old in UNITY_LIFECYCLE_WHITELIST and new != old:
                    issues.append(f"[CRITICAL] Lifecycle renamed: {old} → {new}")
        for old, new in mp.items():
            if not re.match(r'^[a-zA-Z_][a-zA-Z0-9_]*$', new):
                issues.append(f"[{category}] Invalid identifier: '{old}' → '{new}'")
    return issues


# ----------------------------------------------------------------
# RENDER MARKDOWN
# ----------------------------------------------------------------
def render_markdown(mapping: dict, symbols: dict, scan_report: dict, config: dict) -> str:
    now          = datetime.now().strftime("%Y-%m-%dT%H:%M:%S")
    project_name = config["project_name"]
    strategy     = config["strategy"]

    total_changed = sum(
        sum(1 for o, n in mp.items() if o != n)
        for mp in mapping.values()
    )

    class_file  = {c["name"]: c["file"] for c in symbols["classes"]}
    method_file = {m["name"]: m["file"] for m in symbols["methods"]}
    field_info  = {f["name"]: f          for f in symbols["fields"]}
    param_meth  = {p["name"]: p["method"] for p in symbols["params"]}

    L = []
    w = L.append

    w(f"# Class Rename Mapping — {project_name}")
    w(f"<!-- Generated: {now} | Strategy: {strategy} | Changed: {total_changed} symbols -->")
    w(f"<!-- INPUT: scan_report.json | FOR: Phase 3 agent -->")
    w(f"<!-- AGENT: Đọc AGENT_EXECUTION_CHECKLIST ở cuối file -->")
    w("")
    w("---")
    w("")

    # METADATA
    w("## METADATA")
    w("")
    w("| Key | Value |")
    w("|-----|-------|")
    w(f"| generated_at | {now} |")
    w(f"| project | {project_name} |")
    w(f"| strategy | {strategy} |")
    w(f"| art_style | {config.get('art_style', '')} |")
    w(f"| total_cs_files_scanned | {symbols['total_cs_files']} |")
    w(f"| total_symbols_changed | {total_changed} |")
    w(f"| excluded_dll_refs | {len(symbols['excluded_dll'])} |")
    w(f"| excluded_lifecycle_methods | {len(symbols['excluded_lifecycle'])} |")
    w("")
    w("---")
    w("")

    # EXCLUDED
    w("## EXCLUDED_SYMBOLS")
    w("<!-- Agent Phase 3: KHÔNG rename bất cứ thứ gì trong section này -->")
    w("")
    w("### Unity Lifecycle (auto-excluded)")
    w("")
    w(" ".join(f"`{m}`" for m in sorted(symbols["excluded_lifecycle"])) or "_none_")
    w("")
    w("### DLL / SDK References (auto-excluded)")
    w("")
    w(" ".join(f"`{d}`" for d in symbols["excluded_dll"][:40]) or "_none_")
    w("")
    w("---")
    w("")

    # NAMESPACE_MAP
    w("## NAMESPACE_MAP")
    w("<!-- Agent: replace trong namespace declarations + using statements -->")
    w("<!-- Thứ tự: dài → ngắn (đã sort sẵn) -->")
    w("")
    w("| Old Namespace | New Namespace |")
    w("|---|---|")
    for old, new in mapping["namespace_map"].items():
        w(f"| `{old}` | `{new}` |")
    if not mapping["namespace_map"]:
        w("| _none_ | _none_ |")
    w("")
    w("---")
    w("")

    # CLASS_MAP
    w("## CLASS_MAP")
    w("<!-- Agent: word-boundary replace \\bOldName\\b → NewName trong tất cả .cs -->")
    w("<!-- Rename File = YES → rename cả .cs + .meta -->")
    w("<!-- Thứ tự: dài → ngắn (đã sort sẵn) -->")
    w("")
    w("| Old Class | New Class | Source File | Rename File |")
    w("|---|---|---|---|")
    for old, new in mapping["class_map"].items():
        filepath    = class_file.get(old, "_unknown_")
        need_rename = "**YES**" if old != new else "no"
        w(f"| `{old}` | `{new}` | `{filepath}` | {need_rename} |")
    if not mapping["class_map"]:
        w("| _none_ | _none_ | _none_ | no |")
    w("")
    w("---")
    w("")

    # METHOD_MAP
    w("## METHOD_MAP")
    w("<!-- Agent: replace \\bOldMethod\\b(?=\\s*[\\(<]) -->")
    w("")
    w("| Old Method | New Method | Defined In |")
    w("|---|---|---|")
    for old, new in mapping["method_map"].items():
        if old == new:
            continue
        w(f"| `{old}` | `{new}` | `{method_file.get(old, '_multiple_')}` |")
    if all(o == n for o, n in mapping["method_map"].items()):
        w("| _no changes_ | - | - |")
    w("")
    w("---")
    w("")

    # FIELD_MAP
    w("## FIELD_MAP")
    w("<!-- Serialized = YES → thêm [FormerlySerializedAs(\"old\")] trước khi rename -->")
    w("")
    w("| Old Field | New Field | Serialized | Defined In |")
    w("|---|---|---|---|")
    for old, new in mapping["field_map"].items():
        if old == new:
            continue
        info  = field_info.get(old, {})
        is_s  = "**YES**" if info.get("serialized") else "no"
        fpath = info.get("file", "_unknown_")
        w(f"| `{old}` | `{new}` | {is_s} | `{fpath}` |")
    if all(o == n for o, n in mapping["field_map"].items()):
        w("| _no changes_ | - | - | - |")
    w("")
    w("---")
    w("")

    # PARAM_MAP
    w("## PARAM_MAP")
    w("<!-- Agent: replace \\bOldParam\\b trong signatures + call sites -->")
    w("")
    w("| Old Param | New Param | Used In Method |")
    w("|---|---|---|")
    for old, new in mapping["param_map"].items():
        if old == new:
            continue
        w(f"| `{old}` | `{new}` | `{param_meth.get(old, '_unknown_')}` |")
    if all(o == n for o, n in mapping["param_map"].items()):
        w("| _no changes_ | - | - |")
    w("")
    w("---")
    w("")

    # MANUAL_REVIEW_FLAGS
    w("## MANUAL_REVIEW_FLAGS")
    w("<!-- Agent Phase 3: SKIP những items này, chỉ ghi vào phase3_report.json -->")
    w("")
    w("| File | Line | Content | Reason |")
    w("|---|---|---|---|")
    flags = scan_report.get("flags", {}).get("string_literal_class_refs", [])
    for flag in flags:
        w(f"| `{flag.get('file','')}` | {flag.get('line','-')} | `{flag.get('content','')}` | Reflection/string literal |")
    if not flags:
        w("| _none detected_ | - | - | - |")
    w("")
    w("---")
    w("")

    # AGENT EXECUTION CHECKLIST
    w("## AGENT_EXECUTION_CHECKLIST")
    w("<!-- Agent Phase 3 đọc section này để biết thứ tự thực hiện chính xác -->")
    w("")
    w("```")
    w("PRE-CONDITIONS:")
    w("  [ ] scan_report.json tồn tại (Phase 0 đã chạy)")
    w("  [ ] Backup đã tạo (Phase 2 đã chạy)")
    w("  [ ] Unity Editor đã ĐÓNG")
    w("")
    w("EXECUTION ORDER:")
    w("  [1] Load EXCLUDED_SYMBOLS → blacklist")
    w("  [2] Apply NAMESPACE_MAP   → replace trong namespace + using, tất cả .cs")
    w("  [3] Apply CLASS_MAP       → Pass 1: replace trong code")
    w("                           → Pass 2: rename .cs + .meta (Rename File = YES)")
    w("  [4] Apply METHOD_MAP      → replace \\bMethod\\b")
    w("  [5] Apply FIELD_MAP       → serialized=YES: thêm FormerlySerializedAs trước")
    w("                           → sau đó rename field")
    w("  [6] Apply PARAM_MAP       → replace params trong signatures + call sites")
    w("  [7] Log MANUAL_REVIEW_FLAGS → ghi vào phase3_report.json, không tự sửa")
    w("")
    w("POST-CONDITIONS:")
    w("  [ ] Mở Unity Editor")
    w("  [ ] Verify: 0 compile errors")
    w("  [ ] Verify: 0 missing scripts mới")
    w("  [ ] Verify: GUID unchanged (compare với scan_report.guid_map)")
    w("```")
    w("")

    return "\n".join(L)


# ----------------------------------------------------------------
# MAIN
# ----------------------------------------------------------------
def main():
    parser = argparse.ArgumentParser(description="Phase 1 Mapper — generic, reads reskin_config.json")
    parser.add_argument("--config", default="reskin_config.json")
    parser.add_argument("--scan",   default="scan_report.json")
    parser.add_argument("--output", default="class_rename_mapping.md")
    parser.add_argument("--api-key", default=os.getenv("ANTHROPIC_API_KEY"))
    args = parser.parse_args()

    if not args.api_key:
        sys.exit("[ERROR] ANTHROPIC_API_KEY không được set. Dùng --api-key hoặc set env var.")

    print(f"\n{'='*60}")
    print("PHASE 1 — AUTO-GEN MAPPING")
    print(f"  Config:      {args.config}")
    print(f"  Scan report: {args.scan}")
    print(f"  Output:      {args.output}")
    print(f"{'='*60}")

    config      = load_config(args.config)
    scan_report = load_scan_report(args.scan)

    print(f"\n  Project:   {config['project_name']}")
    print(f"  Strategy:  {config['strategy']}")
    print(f"  Art style: {config['art_style']}")

    print("\n[1/4] Collecting symbols...")
    symbols = collect_symbols(scan_report, config)
    print(f"  Namespaces:  {len(symbols['namespaces'])}")
    print(f"  Classes:     {len(symbols['classes'])}")
    print(f"  Methods:     {len(symbols['methods'])}")
    print(f"  Fields:      {len(symbols['fields'])}")
    print(f"  Params:      {len(symbols['params'])}")
    print(f"  DLL excluded:       {len(symbols['excluded_dll'])}")
    print(f"  Lifecycle excluded: {len(symbols['excluded_lifecycle'])}")

    print("\n[2/4] Calling Anthropic API...")
    mapping = gen_all_mappings(symbols, config, args.api_key)

    print("\n[3/4] Validating mapping...")
    issues = validate_mapping(mapping)
    if issues:
        print(f"  [WARNING] {len(issues)} issues:")
        for issue in issues:
            print(f"    - {issue}")
    else:
        print("  No issues ✓")

    print("\n[4/4] Rendering Markdown...")
    md = render_markdown(mapping, symbols, scan_report, config)

    with open(args.output, "w", encoding="utf-8") as f:
        f.write(md)

    json_path = args.output.replace(".md", "_raw.json")
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(mapping, f, indent=2, ensure_ascii=False)

    print(f"\n✅ Done!")
    print(f"   Mapping MD:  {args.output}")
    print(f"   Raw JSON:    {json_path}")


if __name__ == "__main__":
    main()
```

---

## Tóm tắt phân tách trách nhiệm

| File | Ai tạo | Thay đổi theo project? | Mục đích |
|---|---|---|---|
| `scan_report.json` | Phase 0 script | ✅ Có (data project) | Input cho Phase 1 |
| `reskin_config.json` | **Claude** (Phase 1a) | ✅ Có (config project) | Chứa tên game, art style, SDK list |
| `phase1_mapper.py` | Developer (1 lần) | ❌ Không bao giờ | Script generic, dùng lại mãi mãi |
| `class_rename_mapping.md` | Phase 1b script | ✅ Có (output) | Input cho Phase 3 |

---

## Điều kiện pass Phase 1

| Điều kiện | Pass |
|---|---|
| `reskin_config.json` tồn tại và valid JSON | ✅ |
| `class_rename_mapping.md` được tạo thành công | ✅ |
| Không có `[CRITICAL]` issue trong validation | ✅ |
| `AGENT_EXECUTION_CHECKLIST` section tồn tại | ✅ |
| Human đã review + approve mapping | ✅ |

---

*Phase trước: [Phase 0 — Deep Scan](./Phase0DeepScan.md)*
*Phase tiếp theo: Phase 2 — Backup + Git tag → Phase 3 — Class Rename*
