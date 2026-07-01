#!/bin/bash
# Usage: bash tests/smoke/run-all.sh [dev|staging|prod]
# Requires: hurl (https://hurl.dev)

set -e

ENV=${1:-dev}
ENV_FILE="tests/smoke/${ENV}.env"
REPORT_DIR="tests/smoke/reports/${ENV}"

if [ ! -f "$ENV_FILE" ]; then
  echo "Environment file not found: $ENV_FILE"
  exit 1
fi

mkdir -p "$REPORT_DIR"

echo "Running smoke tests against: $ENV"
echo "Variables file: $ENV_FILE"
echo ""

hurl \
  --variables-file "$ENV_FILE" \
  --test \
  --report-html "$REPORT_DIR" \
  --glob "tests/smoke/**/*.hurl"

echo ""
echo "Smoke tests complete. Report: $REPORT_DIR/index.html"
