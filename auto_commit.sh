#!/bin/bash
REPO_DIR="$HOME/automation"
LOG_FILE="$REPO_DIR/data/auto_commit.log"
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')

cd "$REPO_DIR" || { echo "[$TIMESTAMP] ERROR: repo 디렉터리 없음" >> "$LOG_FILE"; exit 1; }

COMMIT_MSG="${1:-auto: 작업 완료 $(date '+%Y-%m-%d %H:%M')}"
TARGET="${2:-.}"

git add "$TARGET"

if git diff --cached --quiet; then
  echo "[$TIMESTAMP] SKIP: 변경사항 없음" >> "$LOG_FILE"
  exit 0
fi

git commit -m "$COMMIT_MSG"
if [ $? -ne 0 ]; then
  echo "[$TIMESTAMP] ERROR: 커밋 실패" >> "$LOG_FILE"
  exit 1
fi

git push origin main
if [ $? -ne 0 ]; then
  echo "[$TIMESTAMP] ERROR: 푸시 실패" >> "$LOG_FILE"
  exit 1
fi

echo "[$TIMESTAMP] OK: $COMMIT_MSG" >> "$LOG_FILE"
echo "[$TIMESTAMP] OK: $COMMIT_MSG"
