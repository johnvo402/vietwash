#!/bin/bash
set -eo pipefail

ROOT="$(dirname "${BASH_SOURCE[0]}")/.."
cd $ROOT

if [ -f "$ROOT/.env" ]; then
  export $(grep -v '^#' "$ROOT/.env" | xargs)
fi
# Kiểm tra tham số
if [ -z "$1" ]; then
  echo "Vui lòng truyền vào tên service. Ví dụ: ./deploy.sh user-service"
  exit 1
fi

SERVICE=$1

ssh -p ${REMOTE_SERVER_PORT} ${REMOTE_SERVER_USER}@${REMOTE_SERVER_IP} << EOF
  echo "Vào thư mục micro..."
  cd ~/micro || exit 1

  echo "Git pull..."
  git pull || exit 1

  echo "Make publish NAME=${SERVICE}..."
  make publish NAME="${SERVICE}" || exit 1

  echo "Make staging SERVICE=${SERVICE}..."
  make staging SERVICE="${SERVICE}" || exit 1

  echo "Triển khai hoàn tất cho service: ${SERVICE}"
EOF
