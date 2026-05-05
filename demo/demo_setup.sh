#!/usr/bin/env bash
set -euo pipefail

DB_NAME="sumstories"
DB_USER="postgres"
DB_HOST="localhost"
DB_PORT="5433"
DB_PASSWORD="sumpassword"

export PGPASSWORD="${DB_PASSWORD}"

echo "AAAAAAAAAAAAAAAAAAAAA"

if ! psql -U "${DB_USER}" -h "${DB_HOST}" -p "${DB_PORT}" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='${DB_NAME}'" | grep -q 1; then
  echo "Database ${DB_NAME} was not found"
  psql -U "${DB_USER}" -h "${DB_HOST}" -p "${DB_PORT}" -d postgres -c "CREATE DATABASE ${DB_NAME} OWNER ${DB_USER} ENCODING 'UTF8' TEMPLATE template0;"
fi

psql -U "${DB_USER}" -h "${DB_HOST}" -p "${DB_PORT}" -d "${DB_NAME}" -f demo/init_demo_db.sql

echo "{
  \"Database\": {
    \"Host\": \"localhost\",
    \"Port\": 5433,
    \"Database\": \"sumstories\",
    \"Username\": \"postgres\",
    \"Password\": \"sumpassword\",
    \"SslMode\": \"Disable\"
  }
}" > appsettings.json
echo "DB_SCHEMA='test'" > .env
