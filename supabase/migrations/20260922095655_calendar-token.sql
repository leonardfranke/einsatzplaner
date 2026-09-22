
  create table "api"."CalendarToken" (
    "DepartmentId" text not null,
    "MemberId" text not null,
    "Token" text not null
      );


CREATE UNIQUE INDEX "CalendarToken_pkey" ON api."CalendarToken" USING btree ("DepartmentId", "MemberId");

alter table "api"."CalendarToken" add constraint "CalendarToken_pkey" PRIMARY KEY using index "CalendarToken_pkey";

alter table "api"."CalendarToken" add constraint "CalendarToken_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") not valid;

alter table "api"."CalendarToken" validate constraint "CalendarToken_DepartmentId_fkey";

alter table "api"."CalendarToken" add constraint "CalendarToken_MemberId_fkey" FOREIGN KEY ("MemberId") REFERENCES api."Members"("Id") ON DELETE CASCADE not valid;

alter table "api"."CalendarToken" validate constraint "CalendarToken_MemberId_fkey";

grant delete on table "api"."CalendarToken" to "anon";

grant insert on table "api"."CalendarToken" to "anon";

grant references on table "api"."CalendarToken" to "anon";

grant select on table "api"."CalendarToken" to "anon";

grant trigger on table "api"."CalendarToken" to "anon";

grant truncate on table "api"."CalendarToken" to "anon";

grant update on table "api"."CalendarToken" to "anon";

grant delete on table "api"."CalendarToken" to "authenticated";

grant insert on table "api"."CalendarToken" to "authenticated";

grant references on table "api"."CalendarToken" to "authenticated";

grant select on table "api"."CalendarToken" to "authenticated";

grant trigger on table "api"."CalendarToken" to "authenticated";

grant truncate on table "api"."CalendarToken" to "authenticated";

grant update on table "api"."CalendarToken" to "authenticated";

grant delete on table "api"."CalendarToken" to "service_role";

grant insert on table "api"."CalendarToken" to "service_role";

grant references on table "api"."CalendarToken" to "service_role";

grant select on table "api"."CalendarToken" to "service_role";

grant trigger on table "api"."CalendarToken" to "service_role";

grant truncate on table "api"."CalendarToken" to "service_role";

grant update on table "api"."CalendarToken" to "service_role";


