alter table "api"."Members" add column "CreatedAt" timestamp with time zone not null default (now() AT TIME ZONE 'utc'::text);


