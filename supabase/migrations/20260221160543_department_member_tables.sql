create table "api"."departments" (
    "id" uuid not null default gen_random_uuid(),
    "name" text not null,
    "url" text
);


create table "api"."members" (
    "id" uuid not null default gen_random_uuid(),
    "departmentId" uuid not null,
    "name" text not null,
    "isAdmin" boolean not null default false,
    "isDummy" boolean not null default false,
    "emailNotificationActive" boolean not null default true
);


CREATE UNIQUE INDEX departments_pkey ON api.departments USING btree (id);

CREATE UNIQUE INDEX members_pkey ON api.members USING btree (id);

alter table "api"."departments" add constraint "departments_pkey" PRIMARY KEY using index "departments_pkey";

alter table "api"."members" add constraint "members_pkey" PRIMARY KEY using index "members_pkey";

alter table "api"."members" add constraint "members_departmentId_fkey" FOREIGN KEY ("departmentId") REFERENCES api.departments(id) ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."members" validate constraint "members_departmentId_fkey";

grant delete on table "api"."departments" to "anon";

grant insert on table "api"."departments" to "anon";

grant references on table "api"."departments" to "anon";

grant select on table "api"."departments" to "anon";

grant trigger on table "api"."departments" to "anon";

grant truncate on table "api"."departments" to "anon";

grant update on table "api"."departments" to "anon";

grant delete on table "api"."departments" to "authenticated";

grant insert on table "api"."departments" to "authenticated";

grant references on table "api"."departments" to "authenticated";

grant select on table "api"."departments" to "authenticated";

grant trigger on table "api"."departments" to "authenticated";

grant truncate on table "api"."departments" to "authenticated";

grant update on table "api"."departments" to "authenticated";

grant delete on table "api"."departments" to "service_role";

grant insert on table "api"."departments" to "service_role";

grant references on table "api"."departments" to "service_role";

grant select on table "api"."departments" to "service_role";

grant trigger on table "api"."departments" to "service_role";

grant truncate on table "api"."departments" to "service_role";

grant update on table "api"."departments" to "service_role";

grant delete on table "api"."members" to "anon";

grant insert on table "api"."members" to "anon";

grant references on table "api"."members" to "anon";

grant select on table "api"."members" to "anon";

grant trigger on table "api"."members" to "anon";

grant truncate on table "api"."members" to "anon";

grant update on table "api"."members" to "anon";

grant delete on table "api"."members" to "authenticated";

grant insert on table "api"."members" to "authenticated";

grant references on table "api"."members" to "authenticated";

grant select on table "api"."members" to "authenticated";

grant trigger on table "api"."members" to "authenticated";

grant truncate on table "api"."members" to "authenticated";

grant update on table "api"."members" to "authenticated";

grant delete on table "api"."members" to "service_role";

grant insert on table "api"."members" to "service_role";

grant references on table "api"."members" to "service_role";

grant select on table "api"."members" to "service_role";

grant trigger on table "api"."members" to "service_role";

grant truncate on table "api"."members" to "service_role";

grant update on table "api"."members" to "service_role";


