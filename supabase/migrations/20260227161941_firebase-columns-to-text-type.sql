ALTER TABLE api."Members"
DROP CONSTRAINT IF EXISTS "Members_DepartmentId_fkey";


ALTER TABLE api."Members"
DROP CONSTRAINT IF EXISTS "Members_pkey";

ALTER TABLE api."Departments"
DROP CONSTRAINT IF EXISTS "Departments_pkey";


ALTER TABLE api."Departments"
ALTER COLUMN "Id" SET DATA TYPE text USING "Id"::text;

ALTER TABLE api."Members"
ALTER COLUMN "Id" SET DATA TYPE text USING "Id"::text;

ALTER TABLE api."Members"
ALTER COLUMN "DepartmentId" SET DATA TYPE text USING "DepartmentId"::text;


ALTER TABLE api."Departments"
ADD CONSTRAINT "Departments_pkey" PRIMARY KEY ("Id");

ALTER TABLE api."Members"
ADD CONSTRAINT "Members_pkey" PRIMARY KEY ("Id");


ALTER TABLE api."Members"
ADD CONSTRAINT "Members_DepartmentId_fkey"
FOREIGN KEY ("DepartmentId")
REFERENCES api."Departments"("Id")
ON DELETE CASCADE;