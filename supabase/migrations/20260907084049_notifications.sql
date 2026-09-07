create type "api"."HelperChangeStatus" as enum ('Locked', 'Preselected', 'Available', 'NotAvailable', 'RequirementDeleted');


  create table "api"."DeletionNotification" (
    "DepartmentId" text not null,
    "EventId" text not null,
    "Group" text,
    "EventCategory" text,
    "Date" timestamp with time zone,
    "Member" text
      );


alter table "api"."DeletionNotification" enable row level security;


  create table "api"."EventNotification" (
    "DepartmentId" text not null,
    "PreviousDate" timestamp with time zone,
    "NewDate" timestamp with time zone,
    "PreviousLocationText" text,
    "NewLocationText" text,
    "Members" text,
    "EventId" text not null
      );


alter table "api"."EventNotification" enable row level security;


  create table "api"."HelperNotification" (
    "EventId" text not null,
    "RoleId" text not null default ''::text,
    "PreviousStatus" jsonb not null,
    "NewStatus" jsonb not null,
    "DepartmentId" text not null
      );


alter table "api"."HelperNotification" enable row level security;

CREATE UNIQUE INDEX "DeletionNotification_pkey" ON api."DeletionNotification" USING btree ("DepartmentId", "EventId");

CREATE UNIQUE INDEX "EventNotification_pkey" ON api."EventNotification" USING btree ("DepartmentId", "EventId");

CREATE UNIQUE INDEX "HelperNotification_pkey" ON api."HelperNotification" USING btree ("EventId", "RoleId");

alter table "api"."DeletionNotification" add constraint "DeletionNotification_pkey" PRIMARY KEY using index "DeletionNotification_pkey";

alter table "api"."EventNotification" add constraint "EventNotification_pkey" PRIMARY KEY using index "EventNotification_pkey";

alter table "api"."HelperNotification" add constraint "HelperNotification_pkey" PRIMARY KEY using index "HelperNotification_pkey";

alter table "api"."DeletionNotification" add constraint "DeletionNotification_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") not valid;

alter table "api"."DeletionNotification" validate constraint "DeletionNotification_DepartmentId_fkey";

alter table "api"."DeletionNotification" add constraint "DeletionNotification_EventCategory_fkey" FOREIGN KEY ("EventCategory") REFERENCES api."EventCategories"("Id") not valid;

alter table "api"."DeletionNotification" validate constraint "DeletionNotification_EventCategory_fkey";

alter table "api"."DeletionNotification" add constraint "DeletionNotification_Group_fkey" FOREIGN KEY ("Group") REFERENCES api."Groups"("Id") not valid;

alter table "api"."DeletionNotification" validate constraint "DeletionNotification_Group_fkey";

alter table "api"."DeletionNotification" add constraint "DeletionNotification_Member_fkey" FOREIGN KEY ("Member") REFERENCES api."Members"("Id") ON DELETE CASCADE not valid;

alter table "api"."DeletionNotification" validate constraint "DeletionNotification_Member_fkey";

alter table "api"."EventNotification" add constraint "EventNotification_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") not valid;

alter table "api"."EventNotification" validate constraint "EventNotification_DepartmentId_fkey";

alter table "api"."EventNotification" add constraint "EventNotification_Members_fkey" FOREIGN KEY ("Members") REFERENCES api."Members"("Id") ON DELETE CASCADE not valid;

alter table "api"."EventNotification" validate constraint "EventNotification_Members_fkey";

alter table "api"."HelperNotification" add constraint "HelperNotification_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") not valid;

alter table "api"."HelperNotification" validate constraint "HelperNotification_DepartmentId_fkey";

grant references on table "api"."DeletionNotification" to "anon";

grant trigger on table "api"."DeletionNotification" to "anon";

grant truncate on table "api"."DeletionNotification" to "anon";

grant references on table "api"."DeletionNotification" to "authenticated";

grant trigger on table "api"."DeletionNotification" to "authenticated";

grant truncate on table "api"."DeletionNotification" to "authenticated";

grant references on table "api"."DeletionNotification" to "service_role";

grant trigger on table "api"."DeletionNotification" to "service_role";

grant truncate on table "api"."DeletionNotification" to "service_role";

grant references on table "api"."EventNotification" to "anon";

grant trigger on table "api"."EventNotification" to "anon";

grant truncate on table "api"."EventNotification" to "anon";

grant references on table "api"."EventNotification" to "authenticated";

grant trigger on table "api"."EventNotification" to "authenticated";

grant truncate on table "api"."EventNotification" to "authenticated";

grant references on table "api"."EventNotification" to "service_role";

grant trigger on table "api"."EventNotification" to "service_role";

grant truncate on table "api"."EventNotification" to "service_role";

grant references on table "api"."HelperNotification" to "anon";

grant trigger on table "api"."HelperNotification" to "anon";

grant truncate on table "api"."HelperNotification" to "anon";

grant references on table "api"."HelperNotification" to "authenticated";

grant trigger on table "api"."HelperNotification" to "authenticated";

grant truncate on table "api"."HelperNotification" to "authenticated";

grant references on table "api"."HelperNotification" to "service_role";

grant trigger on table "api"."HelperNotification" to "service_role";

grant truncate on table "api"."HelperNotification" to "service_role";


