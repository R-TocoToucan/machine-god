#!/usr/bin/env python3
"""
task_router.py
Classifies incoming task complexity and routes to the appropriate agent tier.

Tier 1 - Opus   : new system design, complex debugging, architecture
Tier 2 - Sonnet : mid-level coding, refactoring, UI logic, VN scripts
Tier 3 - Haiku  : status checks, notifications, log parsing, short responses
"""

import os, sys, json
import anthropic

MODEL_CLASSIFIER = "claude-haiku-4-5-20251001"
MODELS = {
    "opus":   "claude-opus-4-6",
    "sonnet": "claude-sonnet-4-6",
    "haiku":  "claude-haiku-4-5-20251001",
}

CLASSIFIER_PROMPT = """You are a task complexity classifier for a game development automation pipeline.

Classify the given task into exactly one tier:

OPUS - new systems from scratch (200+ lines), complex multi-file debugging,
       Win32 API, Unity engine integration, architecture design,
       new combat / upgrade / character systems

SONNET - modifying existing code (50-200 lines), UI component updates,
         resource balance calculations, refactoring, documentation,
         Naninovel VN script generation, mid-level bug fixes

HAIKU - status checks, Telegram notification replies, log parsing,
        quest completion processing, tasks under 50 lines or pure text output

Respond ONLY with valid JSON, no other text:
{"tier": "opus"|"sonnet"|"haiku", "reason": "<one sentence>", "estimated_tokens": <int>}"""

SYSTEM_PROMPT = """You are an expert game developer working on Stellar Command,
a Unity URP idle self-improvement game.
Produce clean, well-commented code in English.
Korean is reserved for in-game dialogue, quest text, and character scripts only."""


def classify(task: str) -> dict:
    """Classify task complexity using Haiku."""
    client = anthropic.Anthropic(api_key=os.environ.get("ANTHROPIC_API_KEY"))
    resp = client.messages.create(
        model=MODEL_CLASSIFIER,
        max_tokens=128,
        messages=[{"role": "user", "content": CLASSIFIER_PROMPT + "\n\nTask: " + task}],
    )
    raw = resp.content[0].text.strip()
    if raw.startswith("```"):
        raw = raw.split("```")[1]
        if raw.startswith("json"):
            raw = raw[4:]
    return json.loads(raw.strip())

def run_task(task: str, tier: str) -> str:
    """Execute task with the assigned model tier."""
    client = anthropic.Anthropic(api_key=os.environ.get("ANTHROPIC_API_KEY"))
    resp = client.messages.create(
        model=MODELS.get(tier, MODELS["sonnet"]),
        max_tokens=4096,
        system=SYSTEM_PROMPT,
        messages=[{"role": "user", "content": task}],
    )
    return resp.content[0].text


def route(task: str, dry_run: bool = False) -> dict:
    """Full pipeline: classify -> execute -> return result."""
    print("[router] Classifying task...")
    result = classify(task)
    tier = result["tier"]
    print(f"[router] Tier: {tier.upper()} | {result['reason']}")
    print(f"[router] Model: {MODELS[tier]}")

    if dry_run:
        return {"classification": result, "output": None}

    print(f"[router] Executing...")
    output = run_task(task, tier)
    return {"classification": result, "output": output}


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print('Usage: python task_router.py "<task>" [--dry-run]')
        sys.exit(1)
    res = route(sys.argv[1], dry_run="--dry-run" in sys.argv)
    if res["output"]:
        print("\n--- OUTPUT ---")
        print(res["output"])
