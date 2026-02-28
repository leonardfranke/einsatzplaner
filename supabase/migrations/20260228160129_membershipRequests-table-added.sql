
  create table "api"."MembershipRequests" (
    "DepartmentId" text not null,
    "UserId" text not null,
    "UserName" text not null
      );


CREATE UNIQUE INDEX "MembershipRequests_pkey" ON api."MembershipRequests" USING btree ("DepartmentId", "UserId");

alter table "api"."MembershipRequests" add constraint "MembershipRequests_pkey" PRIMARY KEY using index "MembershipRequests_pkey";

alter table "api"."MembershipRequests" add constraint "MembershipRequests_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."MembershipRequests" validate constraint "MembershipRequests_DepartmentId_fkey";

grant delete on table "api"."MembershipRequests" to "anon";

grant insert on table "api"."MembershipRequests" to "anon";

grant references on table "api"."MembershipRequests" to "anon";

grant select on table "api"."MembershipRequests" to "anon";

grant trigger on table "api"."MembershipRequests" to "anon";

grant truncate on table "api"."MembershipRequests" to "anon";

grant update on table "api"."MembershipRequests" to "anon";

grant delete on table "api"."MembershipRequests" to "authenticated";

grant insert on table "api"."MembershipRequests" to "authenticated";

grant references on table "api"."MembershipRequests" to "authenticated";

grant select on table "api"."MembershipRequests" to "authenticated";

grant trigger on table "api"."MembershipRequests" to "authenticated";

grant truncate on table "api"."MembershipRequests" to "authenticated";

grant update on table "api"."MembershipRequests" to "authenticated";

grant delete on table "api"."MembershipRequests" to "service_role";

grant insert on table "api"."MembershipRequests" to "service_role";

grant references on table "api"."MembershipRequests" to "service_role";

grant select on table "api"."MembershipRequests" to "service_role";

grant trigger on table "api"."MembershipRequests" to "service_role";

grant truncate on table "api"."MembershipRequests" to "service_role";

grant update on table "api"."MembershipRequests" to "service_role";


