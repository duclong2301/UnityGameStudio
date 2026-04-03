#!/bin/bash
# SubagentStart hook: Log which agent is being invoked

INPUT=$(cat)

AGENT=$(echo "$INPUT" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('agentName','unknown'))" 2>/dev/null)
TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

LOG_DIR="production/session-state"
mkdir -p "$LOG_DIR"

echo "[$TIMESTAMP] Agent invoked: $AGENT" >> "$LOG_DIR/agent-log.txt"

exit 0
