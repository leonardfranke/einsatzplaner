create schema if not exists "api";

create table "api"."locations" (
    "id" uuid not null default gen_random_uuid(),
    "departmentId" text not null,
    "name" text not null,
    "latitude" double precision not null,
    "longitude" double precision not null,
    "originalName" text not null
);


CREATE UNIQUE INDEX locations_pkey ON api.locations USING btree (id);

alter table "api"."locations" add constraint "locations_pkey" PRIMARY KEY using index "locations_pkey";

grant delete on table "api"."locations" to "anon";

grant insert on table "api"."locations" to "anon";

grant references on table "api"."locations" to "anon";

grant select on table "api"."locations" to "anon";

grant trigger on table "api"."locations" to "anon";

grant truncate on table "api"."locations" to "anon";

grant update on table "api"."locations" to "anon";

grant delete on table "api"."locations" to "authenticated";

grant insert on table "api"."locations" to "authenticated";

grant references on table "api"."locations" to "authenticated";

grant select on table "api"."locations" to "authenticated";

grant trigger on table "api"."locations" to "authenticated";

grant truncate on table "api"."locations" to "authenticated";

grant update on table "api"."locations" to "authenticated";

grant delete on table "api"."locations" to "service_role";

grant insert on table "api"."locations" to "service_role";

grant references on table "api"."locations" to "service_role";

grant select on table "api"."locations" to "service_role";

grant trigger on table "api"."locations" to "service_role";

grant truncate on table "api"."locations" to "service_role";

grant update on table "api"."locations" to "service_role";


revoke delete on table "public"."locations" from "anon";

revoke insert on table "public"."locations" from "anon";

revoke references on table "public"."locations" from "anon";

revoke select on table "public"."locations" from "anon";

revoke trigger on table "public"."locations" from "anon";

revoke truncate on table "public"."locations" from "anon";

revoke update on table "public"."locations" from "anon";

revoke delete on table "public"."locations" from "authenticated";

revoke insert on table "public"."locations" from "authenticated";

revoke references on table "public"."locations" from "authenticated";

revoke select on table "public"."locations" from "authenticated";

revoke trigger on table "public"."locations" from "authenticated";

revoke truncate on table "public"."locations" from "authenticated";

revoke update on table "public"."locations" from "authenticated";

revoke delete on table "public"."locations" from "service_role";

revoke insert on table "public"."locations" from "service_role";

revoke references on table "public"."locations" from "service_role";

revoke select on table "public"."locations" from "service_role";

revoke trigger on table "public"."locations" from "service_role";

revoke truncate on table "public"."locations" from "service_role";

revoke update on table "public"."locations" from "service_role";

alter table "public"."locations" drop constraint "locations_pkey";

drop index if exists "public"."locations_pkey";

drop table "public"."locations";


