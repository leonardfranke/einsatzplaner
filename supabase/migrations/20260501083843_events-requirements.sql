create type "api"."EnteringType" as enum ('Locked', 'Preselected', 'Available', 'Recommended');


  create table "api"."Enterings" (
    "DepartmentId" text not null,
    "EventId" text not null,
    "RoleId" text not null,
    "MemberId" text not null,
    "EnteringType" smallint not null
      );



  create table "api"."Events" (
    "Id" text not null,
    "DepartmentId" text not null,
    "GroupId" text,
    "EventCategoryId" text,
    "Date" timestamp with time zone not null,
    "LocationId" uuid,
    "LocationLatitude" double precision,
    "LocationLongitude" double precision,
    "LocationText" text
      );



  create table "api"."QualificationRequirements" (
    "DepartmentId" text not null,
    "EventId" text not null,
    "RoleId" text not null,
    "QualificationId" text not null,
    "RequiredAmount" smallint not null
      );



  create table "api"."Requirements" (
    "DepartmentId" text not null,
    "EventId" text not null,
    "RoleId" text not null,
    "RequiredAmount" smallint not null,
    "LockingTime" timestamp with time zone not null,
    "RecommendedGroups" text[] not null
      );


CREATE UNIQUE INDEX "Enterings_pkey" ON api."Enterings" USING btree ("DepartmentId", "EventId", "RoleId", "MemberId");

CREATE UNIQUE INDEX "Events_pkey" ON api."Events" USING btree ("Id", "DepartmentId");

CREATE UNIQUE INDEX "QualificationRequirements_pkey" ON api."QualificationRequirements" USING btree ("DepartmentId", "EventId", "RoleId", "QualificationId");

CREATE UNIQUE INDEX "Requirements_pkey" ON api."Requirements" USING btree ("DepartmentId", "EventId", "RoleId");

alter table "api"."Enterings" add constraint "Enterings_pkey" PRIMARY KEY using index "Enterings_pkey";

alter table "api"."Events" add constraint "Events_pkey" PRIMARY KEY using index "Events_pkey";

alter table "api"."QualificationRequirements" add constraint "QualificationRequirements_pkey" PRIMARY KEY using index "QualificationRequirements_pkey";

alter table "api"."Requirements" add constraint "Requirements_pkey" PRIMARY KEY using index "Requirements_pkey";

alter table "api"."Enterings" add constraint "Enterings_DepartmentId_EventId_RoleId_fkey" FOREIGN KEY ("DepartmentId", "EventId", "RoleId") REFERENCES api."Requirements"("DepartmentId", "EventId", "RoleId") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."Enterings" validate constraint "Enterings_DepartmentId_EventId_RoleId_fkey";

alter table "api"."Enterings" add constraint "Enterings_MemberId_fkey" FOREIGN KEY ("MemberId") REFERENCES api."Members"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."Enterings" validate constraint "Enterings_MemberId_fkey";

alter table "api"."Events" add constraint "Events_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."Events" validate constraint "Events_DepartmentId_fkey";

alter table "api"."Events" add constraint "Events_EventCategoryId_fkey" FOREIGN KEY ("EventCategoryId") REFERENCES api."EventCategories"("Id") ON UPDATE RESTRICT ON DELETE SET NULL not valid;

alter table "api"."Events" validate constraint "Events_EventCategoryId_fkey";

alter table "api"."Events" add constraint "Events_GroupId_fkey" FOREIGN KEY ("GroupId") REFERENCES api."Groups"("Id") ON UPDATE RESTRICT ON DELETE SET NULL not valid;

alter table "api"."Events" validate constraint "Events_GroupId_fkey";

alter table "api"."Events" add constraint "Events_LocationId_fkey" FOREIGN KEY ("LocationId") REFERENCES api.locations(id) ON UPDATE RESTRICT ON DELETE SET NULL not valid;

alter table "api"."Events" validate constraint "Events_LocationId_fkey";

alter table "api"."QualificationRequirements" add constraint "QualificationRequirements_DepartmentId_EventId_RoleId_fkey" FOREIGN KEY ("DepartmentId", "EventId", "RoleId") REFERENCES api."Requirements"("DepartmentId", "EventId", "RoleId") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."QualificationRequirements" validate constraint "QualificationRequirements_DepartmentId_EventId_RoleId_fkey";

alter table "api"."QualificationRequirements" add constraint "QualificationRequirements_QualificationId_fkey" FOREIGN KEY ("QualificationId") REFERENCES api."Qualifications"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."QualificationRequirements" validate constraint "QualificationRequirements_QualificationId_fkey";

alter table "api"."Requirements" add constraint "Requirements_DepartmentId_EventId_fkey" FOREIGN KEY ("DepartmentId", "EventId") REFERENCES api."Events"("DepartmentId", "Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."Requirements" validate constraint "Requirements_DepartmentId_EventId_fkey";

alter table "api"."Requirements" add constraint "Requirements_RoleId_fkey" FOREIGN KEY ("RoleId") REFERENCES api."Roles"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."Requirements" validate constraint "Requirements_RoleId_fkey";

grant delete on table "api"."Enterings" to "anon";

grant insert on table "api"."Enterings" to "anon";

grant references on table "api"."Enterings" to "anon";

grant select on table "api"."Enterings" to "anon";

grant trigger on table "api"."Enterings" to "anon";

grant truncate on table "api"."Enterings" to "anon";

grant update on table "api"."Enterings" to "anon";

grant delete on table "api"."Enterings" to "authenticated";

grant insert on table "api"."Enterings" to "authenticated";

grant references on table "api"."Enterings" to "authenticated";

grant select on table "api"."Enterings" to "authenticated";

grant trigger on table "api"."Enterings" to "authenticated";

grant truncate on table "api"."Enterings" to "authenticated";

grant update on table "api"."Enterings" to "authenticated";

grant delete on table "api"."Enterings" to "service_role";

grant insert on table "api"."Enterings" to "service_role";

grant references on table "api"."Enterings" to "service_role";

grant select on table "api"."Enterings" to "service_role";

grant trigger on table "api"."Enterings" to "service_role";

grant truncate on table "api"."Enterings" to "service_role";

grant update on table "api"."Enterings" to "service_role";

grant delete on table "api"."Events" to "anon";

grant insert on table "api"."Events" to "anon";

grant references on table "api"."Events" to "anon";

grant select on table "api"."Events" to "anon";

grant trigger on table "api"."Events" to "anon";

grant truncate on table "api"."Events" to "anon";

grant update on table "api"."Events" to "anon";

grant delete on table "api"."Events" to "authenticated";

grant insert on table "api"."Events" to "authenticated";

grant references on table "api"."Events" to "authenticated";

grant select on table "api"."Events" to "authenticated";

grant trigger on table "api"."Events" to "authenticated";

grant truncate on table "api"."Events" to "authenticated";

grant update on table "api"."Events" to "authenticated";

grant delete on table "api"."Events" to "service_role";

grant insert on table "api"."Events" to "service_role";

grant references on table "api"."Events" to "service_role";

grant select on table "api"."Events" to "service_role";

grant trigger on table "api"."Events" to "service_role";

grant truncate on table "api"."Events" to "service_role";

grant update on table "api"."Events" to "service_role";

grant delete on table "api"."QualificationRequirements" to "anon";

grant insert on table "api"."QualificationRequirements" to "anon";

grant references on table "api"."QualificationRequirements" to "anon";

grant select on table "api"."QualificationRequirements" to "anon";

grant trigger on table "api"."QualificationRequirements" to "anon";

grant truncate on table "api"."QualificationRequirements" to "anon";

grant update on table "api"."QualificationRequirements" to "anon";

grant delete on table "api"."QualificationRequirements" to "authenticated";

grant insert on table "api"."QualificationRequirements" to "authenticated";

grant references on table "api"."QualificationRequirements" to "authenticated";

grant select on table "api"."QualificationRequirements" to "authenticated";

grant trigger on table "api"."QualificationRequirements" to "authenticated";

grant truncate on table "api"."QualificationRequirements" to "authenticated";

grant update on table "api"."QualificationRequirements" to "authenticated";

grant delete on table "api"."QualificationRequirements" to "service_role";

grant insert on table "api"."QualificationRequirements" to "service_role";

grant references on table "api"."QualificationRequirements" to "service_role";

grant select on table "api"."QualificationRequirements" to "service_role";

grant trigger on table "api"."QualificationRequirements" to "service_role";

grant truncate on table "api"."QualificationRequirements" to "service_role";

grant update on table "api"."QualificationRequirements" to "service_role";

grant delete on table "api"."Requirements" to "anon";

grant insert on table "api"."Requirements" to "anon";

grant references on table "api"."Requirements" to "anon";

grant select on table "api"."Requirements" to "anon";

grant trigger on table "api"."Requirements" to "anon";

grant truncate on table "api"."Requirements" to "anon";

grant update on table "api"."Requirements" to "anon";

grant delete on table "api"."Requirements" to "authenticated";

grant insert on table "api"."Requirements" to "authenticated";

grant references on table "api"."Requirements" to "authenticated";

grant select on table "api"."Requirements" to "authenticated";

grant trigger on table "api"."Requirements" to "authenticated";

grant truncate on table "api"."Requirements" to "authenticated";

grant update on table "api"."Requirements" to "authenticated";

grant delete on table "api"."Requirements" to "service_role";

grant insert on table "api"."Requirements" to "service_role";

grant references on table "api"."Requirements" to "service_role";

grant select on table "api"."Requirements" to "service_role";

grant trigger on table "api"."Requirements" to "service_role";

grant truncate on table "api"."Requirements" to "service_role";

grant update on table "api"."Requirements" to "service_role";


