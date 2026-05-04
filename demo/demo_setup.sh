psql -d sumstories -U postgres -f demo/init_demo_db.sql
echo "{
  \"Database\": {
    \"Host\": \"localhost\",
    \"Port\": 5432,
    \"Database\": \"sumstories\",
    \"Username\": \"postgres\",
    \"Password\": \"sumpassword\",
    \"SslMode\": \"Disable\"
  }
}" >> appsettings.json
echo "DB_SCHEMA='test'" > .env
