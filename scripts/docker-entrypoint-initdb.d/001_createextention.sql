\connect postgres
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

\connect auth_service
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

\connect project_service
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

\connect ecommerce_service
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
