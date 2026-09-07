alter table "api"."DeletionNotification" drop constraint "DeletionNotification_Member_fkey";

alter table "api"."EventNotification" drop constraint "EventNotification_Members_fkey";

alter table "api"."DeletionNotification" drop column "Member";

alter table "api"."DeletionNotification" add column "Members" text[] not null;

alter table "api"."DeletionNotification" disable row level security;

alter table "api"."EventNotification" alter column "Members" set not null;

alter table "api"."EventNotification" alter column "Members" set data type text[] using "Members"::text[];

alter table "api"."EventNotification" disable row level security;

alter table "api"."HelperNotification" disable row level security;


