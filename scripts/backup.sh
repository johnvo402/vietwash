#!/bin/bash
ENV_FILE="$(dirname "$0")/../.env"

if [ -f "$ENV_FILE" ]; then
    export $(grep -v '^#' "$ENV_FILE" | xargs)
else
    echo ".env file not found"
    exit 1
fi

set -e
set -o pipefail

DB_LIST=("AuthDb")
backup() {
  for db in ${DB_LIST[@]}; do
   FILE_NAME=$db.bak

    echo "Backing up database '$db'"

    docker exec -it sqlserver opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $DB_PASSWORD -C -Q "BACKUP DATABASE [$db] TO DISK = N'/var/opt/mssql/backups/$FILE_NAME' WITH NOFORMAT, NOINIT, NAME = '$db-full', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

    docker cp sqlserver:/var/opt/mssql/backups/$FILE_NAME ./backup/$FILE_NAME

  done
  echo "Done!"
}

sql() {
  scp -r -P $REMOTE_SERVER_PORT $REMOTE_SERVER_USER@$REMOTE_SERVER_IP:~/micro/.backup .
}

restore() {
  for db in ${DB_LIST[@]}; do

   FILE_NAME=$db.bak
    echo "Backing up database '$db'"

    docker exec -it sqlserver opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $DB_PASSWORD -C -Q "RESTORE DATABASE [$db] FROM DISK = N'/backup/$FILE_NAME' WITH FILE = 1, NOUNLOAD, REPLACE, RECOVERY, STATS = 5"

  done
  echo "Done!"
}

case "$1" in
backup)
  backup
  ;;
sql)
  sql
  ;;
restore)
  restore
  ;;
*) ;;
esac