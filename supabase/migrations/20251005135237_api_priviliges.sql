grant usage on schema api to anon, authenticated, service_role;
grant all on all tables in schema api to anon, authenticated, service_role;
alter default privileges in schema api grant all on tables to anon, authenticated, service_role;