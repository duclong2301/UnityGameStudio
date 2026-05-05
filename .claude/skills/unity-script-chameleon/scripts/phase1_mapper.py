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
            raw = re.sub(r"^