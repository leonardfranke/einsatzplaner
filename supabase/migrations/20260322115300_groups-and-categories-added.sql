
  create table "api"."EventCategories" (
    "Id" text not null,
    "DepartmentId" text not null,
    "Name" text not null
      );



  create table "api"."Groups" (
    "Id" text not null,
    "DepartmentId" text not null,
    "Name" text not null
      );



  create table "api"."MemberGroup" (
    "DepartmentId" text not null,
    "MemberId" text not null,
    "GroupId" text not null
      );


CREATE UNIQUE INDEX "EventCategories_pkey" ON api."EventCategories" USING btree ("Id");

CREATE UNIQUE INDEX "Groups_pkey" ON api."Groups" USING btree ("Id");

CREATE UNIQUE INDEX "MemberGroup_pkey" ON api."MemberGroup" USING btree ("DepartmentId", "MemberId", "GroupId");

alter table "api"."EventCategories" add constraint "EventCategories_pkey" PRIMARY KEY using index "EventCategories_pkey";

alter table "api"."Groups" add constraint "Groups_pkey" PRIMARY KEY using index "Groups_pkey";

alter table "api"."MemberGroup" add constraint "MemberGroup_pkey" PRIMARY KEY using index "MemberGroup_pkey";

alter table "api"."EventCategories" add constraint "EventCategories_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."EventCategories" validate constraint "EventCategories_DepartmentId_fkey";

alter table "api"."Groups" add constraint "Groups_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."Groups" validate constraint "Groups_DepartmentId_fkey";

alter table "api"."MemberGroup" add constraint "MemberGroup_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."MemberGroup" validate constraint "MemberGroup_DepartmentId_fkey";

alter table "api"."MemberGroup" add constraint "MemberGroup_GroupId_fkey" FOREIGN KEY ("GroupId") REFERENCES api."Groups"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."MemberGroup" validate constraint "MemberGroup_GroupId_fkey";

alter table "api"."MemberGroup" add constraint "MemberGroup_MemberId_fkey" FOREIGN KEY ("MemberId") REFERENCES api."Members"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."MemberGroup" validate constraint "MemberGroup_MemberId_fkey";

grant delete on table "api"."EventCategories" to "anon";

grant insert on table "api"."EventCategories" to "anon";

grant references on table "api"."EventCategories" to "anon";

grant select on table "api"."EventCategories" to "anon";

grant trigger on table "api"."EventCategories" to "anon";

grant truncate on table "api"."EventCategories" to "anon";

grant update on table "api"."EventCategories" to "anon";

grant delete on table "api"."EventCategories" to "authenticated";

grant insert on table "api"."EventCategories" to "authenticated";

grant references on table "api"."EventCategories" to "authenticated";

grant select on table "api"."EventCategories" to "authenticated";

grant trigger on table "api"."EventCategories" to "authenticated";

grant truncate on table "api"."EventCategories" to "authenticated";

grant update on table "api"."EventCategories" to "authenticated";

grant delete on table "api"."EventCategories" to "service_role";

grant insert on table "api"."EventCategories" to "service_role";

grant references on table "api"."EventCategories" to "service_role";

grant select on table "api"."EventCategories" to "service_role";

grant trigger on table "api"."EventCategories" to "service_role";

grant truncate on table "api"."EventCategories" to "service_role";

grant update on table "api"."EventCategories" to "service_role";

grant delete on table "api"."Groups" to "anon";

grant insert on table "api"."Groups" to "anon";

grant references on table "api"."Groups" to "anon";

grant select on table "api"."Groups" to "anon";

grant trigger on table "api"."Groups" to "anon";

grant truncate on table "api"."Groups" to "anon";

grant update on table "api"."Groups" to "anon";

grant delete on table "api"."Groups" to "authenticated";

grant insert on table "api"."Groups" to "authenticated";

grant references on table "api"."Groups" to "authenticated";

grant select on table "api"."Groups" to "authenticated";

grant trigger on table "api"."Groups" to "authenticated";

grant truncate on table "api"."Groups" to "authenticated";

grant update on table "api"."Groups" to "authenticated";

grant delete on table "api"."Groups" to "service_role";

grant insert on table "api"."Groups" to "service_role";

grant references on table "api"."Groups" to "service_role";

grant select on table "api"."Groups" to "service_role";

grant trigger on table "api"."Groups" to "service_role";

grant truncate on table "api"."Groups" to "service_role";

grant update on table "api"."Groups" to "service_role";

grant delete on table "api"."MemberGroup" to "anon";

grant insert on table "api"."MemberGroup" to "anon";

grant references on table "api"."MemberGroup" to "anon";

grant select on table "api"."MemberGroup" to "anon";

grant trigger on table "api"."MemberGroup" to "anon";

grant truncate on table "api"."MemberGroup" to "anon";

grant update on table "api"."MemberGroup" to "anon";

grant delete on table "api"."MemberGroup" to "authenticated";

grant insert on table "api"."MemberGroup" to "authenticated";

grant references on table "api"."MemberGroup" to "authenticated";

grant select on table "api"."MemberGroup" to "authenticated";

grant trigger on table "api"."MemberGroup" to "authenticated";

grant truncate on table "api"."MemberGroup" to "authenticated";

grant update on table "api"."MemberGroup" to "authenticated";

grant delete on table "api"."MemberGroup" to "service_role";

grant insert on table "api"."MemberGroup" to "service_role";

grant references on table "api"."MemberGroup" to "service_role";

grant select on table "api"."MemberGroup" to "service_role";

grant trigger on table "api"."MemberGroup" to "service_role";

grant truncate on table "api"."MemberGroup" to "service_role";

grant update on table "api"."MemberGroup" to "service_role";


