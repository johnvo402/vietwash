#!/bin/bash
set -eo pipefail

ROOT="$(dirname "${BASH_SOURCE[0]}")/.."
cd $ROOT

if [ -f "$ROOT/.env" ]; then
  export $(grep -v '^#' "$ROOT/.env" | xargs)
fi

# Kiểm tra tham số
if [ $# -eq 0 ]; then
  echo "Vui lòng truyền vào ít nhất 1 service. Ví dụ: ./deploy.sh user-service"
  exit 1
fi

# Convert list of services to a string
SERVICES="$@"

ssh -p ${REMOTE_SERVER_PORT} ${REMOTE_SERVER_USER}@${REMOTE_SERVER_IP} bash << EOF
  echo "Vào thư mục micro..."
  cd ~/micro || exit 1

  echo "Git pull..."
  git pull || exit 1

    echo "Make publish NAME=${SERVICES}..."
    make publish NAME="${SERVICES}" || exit 1
  
    echo "Make staging SERVICE=${SERVICES}..."
    make staging SERVICE="${SERVICES}" || exit 1

    make dev SERVICE="database" || exit 1
    
    echo "Triển khai hoàn tất cho services: ${SERVICES}"
EOF
