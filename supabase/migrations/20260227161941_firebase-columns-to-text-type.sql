drop index if exists "api"."Departments_pkey";

drop index if exists "api"."Members_pkey";

alter table "api"."Departments" alter column "Id" set data type text using "Id"::text;

alter table "api"."Members" alter column "DepartmentId" set data type text using "DepartmentId"::text;

alter table "api"."Members" alter column "Id" set default ''::text;

alter table "api"."Members" alter column "Id" set data type text using "Id"::text;

CREATE UNIQUE INDEX "Departments_pkey" ON api."Departments" USING btree ("Id");

CREATE UNIQUE INDEX "Members_pkey" ON api."Members" USING btree ("Id");


