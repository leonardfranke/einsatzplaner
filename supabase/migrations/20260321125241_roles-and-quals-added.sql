
  create table "api"."MemberQualification" (
    "DepartmentId" text not null,
    "MemberId" text not null,
    "QualificationId" text not null,
    "RoleId" text not null
      );



  create table "api"."MemberRole" (
    "DepartmentId" text not null,
    "MemberId" text not null,
    "RoleId" text not null
      );



  create table "api"."Qualifications" (
    "Id" text not null,
    "DepartmentId" text not null,
    "Name" text not null,
    "RoleId" text not null
      );



  create table "api"."Roles" (
    "Id" text not null,
    "DepartmentId" text not null,
    "Name" text not null,
    "LockingPeriod" smallint not null,
    "IsFree" boolean not null
      );


CREATE UNIQUE INDEX "MemberQualification_pkey" ON api."MemberQualification" USING btree ("DepartmentId", "MemberId", "QualificationId");

CREATE UNIQUE INDEX "MemberRole_pkey" ON api."MemberRole" USING btree ("DepartmentId", "MemberId", "RoleId");

CREATE UNIQUE INDEX "Qualifications_pkey" ON api."Qualifications" USING btree ("Id");

CREATE UNIQUE INDEX "Roles_pkey" ON api."Roles" USING btree ("Id");

alter table "api"."MemberQualification" add constraint "MemberQualification_pkey" PRIMARY KEY using index "MemberQualification_pkey";

alter table "api"."MemberRole" add constraint "MemberRole_pkey" PRIMARY KEY using index "MemberRole_pkey";

alter table "api"."Qualifications" add constraint "Qualifications_pkey" PRIMARY KEY using index "Qualifications_pkey";

alter table "api"."Roles" add constraint "Roles_pkey" PRIMARY KEY using index "Roles_pkey";

alter table "api"."MemberQualification" add constraint "MemberQualification_DepartmentId_MemberId_RoleId_fkey" FOREIGN KEY ("DepartmentId", "MemberId", "RoleId") REFERENCES api."MemberRole"("DepartmentId", "MemberId", "RoleId") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."MemberQualification" validate constraint "MemberQualification_DepartmentId_MemberId_RoleId_fkey";

alter table "api"."MemberQualification" add constraint "MemberQualification_QualificationId_fkey" FOREIGN KEY ("QualificationId") REFERENCES api."Qualifications"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."MemberQualification" validate constraint "MemberQualification_QualificationId_fkey";

alter table "api"."MemberRole" add constraint "MemberRole_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."MemberRole" validate constraint "MemberRole_DepartmentId_fkey";

alter table "api"."MemberRole" add constraint "MemberRole_MemberId_fkey" FOREIGN KEY ("MemberId") REFERENCES api."Members"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."MemberRole" validate constraint "MemberRole_MemberId_fkey";

alter table "api"."MemberRole" add constraint "MemberRole_RoleId_fkey" FOREIGN KEY ("RoleId") REFERENCES api."Roles"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."MemberRole" validate constraint "MemberRole_RoleId_fkey";

alter table "api"."Qualifications" add constraint "Qualifications_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."Qualifications" validate constraint "Qualifications_DepartmentId_fkey";

alter table "api"."Qualifications" add constraint "Qualifications_RoleId_fkey" FOREIGN KEY ("RoleId") REFERENCES api."Roles"("Id") ON UPDATE RESTRICT ON DELETE CASCADE not valid;

alter table "api"."Qualifications" validate constraint "Qualifications_RoleId_fkey";

alter table "api"."Roles" add constraint "Roles_DepartmentId_fkey" FOREIGN KEY ("DepartmentId") REFERENCES api."Departments"("Id") ON UPDATE RESTRICT ON DELETE RESTRICT not valid;

alter table "api"."Roles" validate constraint "Roles_DepartmentId_fkey";

grant delete on table "api"."MemberQualification" to "anon";

grant insert on table "api"."MemberQualification" to "anon";

grant references on table "api"."MemberQualification" to "anon";

grant select on table "api"."MemberQualification" to "anon";

grant trigger on table "api"."MemberQualification" to "anon";

grant truncate on table "api"."MemberQualification" to "anon";

grant update on table "api"."MemberQualification" to "anon";

grant delete on table "api"."MemberQualification" to "authenticated";

grant insert on table "api"."MemberQualification" to "authenticated";

grant references on table "api"."MemberQualification" to "authenticated";

grant select on table "api"."MemberQualification" to "authenticated";

grant trigger on table "api"."MemberQualification" to "authenticated";

grant truncate on table "api"."MemberQualification" to "authenticated";

grant update on table "api"."MemberQualification" to "authenticated";

grant delete on table "api"."MemberQualification" to "service_role";

grant insert on table "api"."MemberQualification" to "service_role";

grant references on table "api"."MemberQualification" to "service_role";

grant select on table "api"."MemberQualification" to "service_role";

grant trigger on table "api"."MemberQualification" to "service_role";

grant truncate on table "api"."MemberQualification" to "service_role";

grant update on table "api"."MemberQualification" to "service_role";

grant delete on table "api"."MemberRole" to "anon";

grant insert on table "api"."MemberRole" to "anon";

grant references on table "api"."MemberRole" to "anon";

grant select on table "api"."MemberRole" to "anon";

grant trigger on table "api"."MemberRole" to "anon";

grant truncate on table "api"."MemberRole" to "anon";

grant update on table "api"."MemberRole" to "anon";

grant delete on table "api"."MemberRole" to "authenticated";

grant insert on table "api"."MemberRole" to "authenticated";

grant references on table "api"."MemberRole" to "authenticated";

grant select on table "api"."MemberRole" to "authenticated";

grant trigger on table "api"."MemberRole" to "authenticated";

grant truncate on table "api"."MemberRole" to "authenticated";

grant update on table "api"."MemberRole" to "authenticated";

grant delete on table "api"."MemberRole" to "service_role";

grant insert on table "api"."MemberRole" to "service_role";

grant references on table "api"."MemberRole" to "service_role";

grant select on table "api"."MemberRole" to "service_role";

grant trigger on table "api"."MemberRole" to "service_role";

grant truncate on table "api"."MemberRole" to "service_role";

grant update on table "api"."MemberRole" to "service_role";

grant delete on table "api"."Qualifications" to "anon";

grant insert on table "api"."Qualifications" to "anon";

grant references on table "api"."Qualifications" to "anon";

grant select on table "api"."Qualifications" to "anon";

grant trigger on table "api"."Qualifications" to "anon";

grant truncate on table "api"."Qualifications" to "anon";

grant update on table "api"."Qualifications" to "anon";

grant delete on table "api"."Qualifications" to "authenticated";

grant insert on table "api"."Qualifications" to "authenticated";

grant references on table "api"."Qualifications" to "authenticated";

grant select on table "api"."Qualifications" to "authenticated";

grant trigger on table "api"."Qualifications" to "authenticated";

grant truncate on table "api"."Qualifications" to "authenticated";

grant update on table "api"."Qualifications" to "authenticated";

grant delete on table "api"."Qualifications" to "service_role";

grant insert on table "api"."Qualifications" to "service_role";

grant references on table "api"."Qualifications" to "service_role";

grant select on table "api"."Qualifications" to "service_role";

grant trigger on table "api"."Qualifications" to "service_role";

grant truncate on table "api"."Qualifications" to "service_role";

grant update on table "api"."Qualifications" to "service_role";

grant delete on table "api"."Roles" to "anon";

grant insert on table "api"."Roles" to "anon";

grant references on table "api"."Roles" to "anon";

grant select on table "api"."Roles" to "anon";

grant trigger on table "api"."Roles" to "anon";

grant truncate on table "api"."Roles" to "anon";

grant update on table "api"."Roles" to "anon";

grant delete on table "api"."Roles" to "authenticated";

grant insert on table "api"."Roles" to "authenticated";

grant references on table "api"."Roles" to "authenticated";

grant select on table "api"."Roles" to "authenticated";

grant trigger on table "api"."Roles" to "authenticated";

grant truncate on table "api"."Roles" to "authenticated";

grant update on table "api"."Roles" to "authenticated";

grant delete on table "api"."Roles" to "service_role";

grant insert on table "api"."Roles" to "service_role";

grant references on table "api"."Roles" to "service_role";

grant select on table "api"."Roles" to "service_role";

grant trigger on table "api"."Roles" to "service_role";

grant truncate on table "api"."Roles" to "service_role";

grant update on table "api"."Roles" to "service_role";


