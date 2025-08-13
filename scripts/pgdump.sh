#!/bin/bash

set -eo pipefail

ROOT="$(dirname "${BASH_SOURCE[0]}")/.."
cd $ROOT

if [ -f "$ROOT/.env" ]; then
  export $(grep -v '^#' "$ROOT/.env" | xargs)
fi

DB_LIST=("auth_service" "ecommerce_service" "project_service" "finance_service" "notification_service")

backup() {
  for db in ${DB_LIST[@]}; do
    local filepath="$ROOT/.pgdump/$db.sql"
    rm -f $filepath
    case "$db" in
    "default")
      DB_NAME="postgres"
      ;;
    *)
      DB_NAME=$db
      ;;
    esac

    echo "START backup $db"
    docker exec -e PGPASSWORD="$DB_PASSWORD" database pg_dump -U "$POSTGRES_USER" -d "$DB_NAME" > "$filepath"
    echo "END backup $db"
    printf "\n"
  done
}

sql() {
  echo "Running: scp -r -P $REMOTE_SERVER_PORT $REMOTE_SERVER_USER@$REMOTE_SERVER_IP:~/micro/.pgdump ."
  scp -r -P $REMOTE_SERVER_PORT $REMOTE_SERVER_USER@$REMOTE_SERVER_IP:~/micro/.pgdump .
}
sql_to_server() {
  echo "Running: scp -r -P $REMOTE_SERVER_PORT .pgdump \
    $REMOTE_SERVER_USER@$REMOTE_SERVER_IP:~/micro/"
  scp -r -P $REMOTE_SERVER_PORT .pgdump \
    $REMOTE_SERVER_USER@$REMOTE_SERVER_IP:~/micro/
}
restore() {
  for db in ${DB_LIST[@]}; do
    case "$db" in
    "default")
      DB_NAME="postgres"
      ;;
    *)
      DB_NAME=$db
      ;;
    esac
    echo "START restore $db"
    docker exec -e PGPASSWORD="$DB_PASSWORD" database sh -c "psql -U $POSTGRES_USER -d $DB_NAME < /backup/$db.sql"
    echo "END restore $db"
    printf "\n"
  done
}

case "$1" in
backup)
  backup
  ;;
sql)
  sql
  ;;
sql_to_server)
  sql_to_server
  ;;
restore)
  restore
  ;;
*) ;;
esac
