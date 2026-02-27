revoke delete on table "api"."departments" from "anon";

revoke insert on table "api"."departments" from "anon";

revoke references on table "api"."departments" from "anon";

revoke select on table "api"."departments" from "anon";

revoke trigger on table "api"."departments" from "anon";

revoke truncate on table "api"."departments" from "anon";

revoke update on table "api"."departments" from "anon";

revoke delete on table "api"."departments" from "authenticated";

revoke insert on table "api"."departments" from "authenticated";

revoke references on table "api"."departments" from "authenticated";

revoke select on table "api"."departments" from "authenticated";

revoke trigger on table "api"."departments" from "authenticated";

revoke truncate on table "api"."departments" from "authenticated";

revoke update on table "api"."departments" from "authenticated";

revoke delete on table "api"."departments" from "service_role";

revoke insert on table "api"."departments" from "service_role";

revoke references on table "api"."departments" from "service_role";

revoke select on table "api"."departments" from "service_role";

revoke trigger on table "api"."departments" from "service_role";

revoke truncate on table "api"."departments" from "service_role";

revoke update on table "api"."departments" from "service_role";

revoke delete on table "api"."members" from "anon";

revoke insert on table "api"."members" from "anon";

revoke references on table "api"."members" from "anon";

revoke select on table "api"."members" from "anon";

revoke trigger on table "api"."members" from "anon";

revoke truncate on table "api"."members" from "anon";

revoke update on table "api"."members" from "anon";

revoke delete on table "api"."members" from "authenticated";

revoke insert on table "api"."members" from "authenticated";

revoke references on table "api"."members" from "authenticated";

revoke select on table "api"."members" from "authenticated";

revoke trigger on table "api"."members" from "authenticated";

revoke truncate on table "api"."members" from "authenticated";

revoke update on table "api"."members" from "authenticated";

revoke delete on table "api"."members" from "service_role";

revoke insert on table "api"."members" from "service_role";

revoke references on table "api"."members" from "service_role";

revoke select on table "api"."members" from "service_role";

revoke trigger on table "api"."members" from "service_role";

revoke truncate on table "api"."members" from "service_role";

revoke update on table "api"."members" from "service_role";

alter table "api"."members" drop constraint "members_departmentId_fkey";

alter table "api"."departments" drop constraint "departments_pkey";

alter table "api"."members" drop constraint "members_pkey";

drop index if exists "api"."departments_pkey";

drop index if exists "api"."members_pkey";

drop table "api"."departments";

drop table "api"."members";


  create table "api"."Departments" (
    "Id" uuid not null default gen_random_uuid(),
    "Name" text not null,
    "URL" text
      );



  create table "api"."Members" (
    "Id" uuid not null default gen_random_uuid(),
    "DepartmentId" uuid not null,
    "Name" text not null,
    "IsAdmin" boolean not null default false,
    "IsDummy" boolean not null default false,
    "EmailNotificationActive" boolean not null default true
      );


CREATE UNIQUE INDEX "Departments_pkey" ON api."Departments" USING btree ("Id");

CREATE UNIQUE INDEX "Members_pkey" ON api."Members" USING btree ("Id");

alter table "api"."Departments" add constraint "Departments_pkey" PRIMARY KEY using index "Departments_pkey";

alter table "api"."Members" add constraint "Members_pkey" PRIMARY KEY using index "Members_pkey";

alter table "api"."Members" add constraint "Members_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."Members" validate constraint "Members_DepartmentId_fkey";

grant delete on table "api"."Departments" to "anon";

grant insert on table "api"."Departments" to "anon";

grant references on table "api"."Departments" to "anon";

grant select on table "api"."Departments" to "anon";

grant trigger on table "api"."Departments" to "anon";

grant truncate on table "api"."Departments" to "anon";

grant update on table "api"."Departments" to "anon";

grant delete on table "api"."Departments" to "authenticated";

grant insert on table "api"."Departments" to "authenticated";

grant references on table "api"."Departments" to "authenticated";

grant select on table "api"."Departments" to "authenticated";

grant trigger on table "api"."Departments" to "authenticated";

grant truncate on table "api"."Departments" to "authenticated";

grant update on table "api"."Departments" to "authenticated";

grant delete on table "api"."Departments" to "service_role";

grant insert on table "api"."Departments" to "service_role";

grant references on table "api"."Departments" to "service_role";

grant select on table "api"."Departments" to "service_role";

grant trigger on table "api"."Departments" to "service_role";

grant truncate on table "api"."Departments" to "service_role";

grant update on table "api"."Departments" to "service_role";

grant delete on table "api"."Members" to "anon";

grant insert on table "api"."Members" to "anon";

grant references on table "api"."Members" to "anon";

grant select on table "api"."Members" to "anon";

grant trigger on table "api"."Members" to "anon";

grant truncate on table "api"."Members" to "anon";

grant update on table "api"."Members" to "anon";

grant delete on table "api"."Members" to "authenticated";

grant insert on table "api"."Members" to "authenticated";

grant references on table "api"."Members" to "authenticated";

grant select on table "api"."Members" to "authenticated";

grant trigger on table "api"."Members" to "authenticated";

grant truncate on table "api"."Members" to "authenticated";

grant update on table "api"."Members" to "authenticated";

grant delete on table "api"."Members" to "service_role";

grant insert on table "api"."Members" to "service_role";

grant references on table "api"."Members" to "service_role";

grant select on table "api"."Members" to "service_role";

grant trigger on table "api"."Members" to "service_role";

grant truncate on table "api"."Members" to "service_role";

grant update on table "api"."Members" to "service_role";


