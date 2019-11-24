/****** Object: Table "public"."IdentityRoleClaims" Script Date: 2019-10-28 11:26:42 AM ******/
CREATE TABLE "public"."IdentityRoleClaims" (
    "Id"         serial NOT NULL,
    "RoleId"     varchar (450) NOT NULL,
    "ClaimType"  varchar  NULL,
    "ClaimValue" varchar  NULL,
		CONSTRAint pk_IdentityRoleClaims_Id PRIMARY KEY ("Id")
);

CREATE INDEX "IX_IdentityRoleClaims_RoleId" ON "public"."IdentityRoleClaims" ("RoleId" ASC NULLS LAST);


/****** Object: Table "public"."IdentityRoles" Script Date: 2019-10-28 11:27:18 AM ******/
CREATE TABLE "public"."IdentityRoles" (
    "Id"               varchar (450) NOT NULL,
    "Name"             varchar (256) NULL,
    "NormalizedName"   varchar (256) NULL,
    "ConcurrencyStamp" varchar  NULL,
		CONSTRAint "PK_IdentityRoles_Id" PRIMARY KEY ("Id")
);


CREATE UNIQUE INDEX "RoleNameIndex"
    ON "public"."IdentityRoles"("NormalizedName" ASC) WHERE ("NormalizedName" IS NOT NULL);


/****** Object: Table "public"."IdentityUserClaims" Script Date: 2019-10-28 11:27:32 AM ******/
CREATE TABLE "public"."IdentityUserClaims" (
    "Id"         serial NOT NULL,
    "UserId"     varchar (450) NOT NULL,
    "ClaimType"  varchar  NULL,
    "ClaimValue" varchar  NULL,
		CONStRAint PK_IdentityUserClaims_Id PRIMARY KEY ("Id")
);

CREATE INDEX "IX_IdentityUserClaims_UserId"
    ON "public"."IdentityUserClaims"("UserId" ASC);


/****** Object: Table "public"."IdentityUserLogins" Script Date: 2019-10-28 11:28:07 AM ******/
CREATE TABLE "public"."IdentityUserLogins" (
    "LoginProvider"       varchar (128) NOT NULL,
    "ProviderKey"         varchar (128) NOT NULL,
    "ProviderDisplayName" varchar  NULL,
    "UserId"              varchar (450) NOT NULL,
		CONSTRAint "PK_IdentityUserLogins_LP_PK" PRIMARY KEY ("LoginProvider", "ProviderKey")
);

CREATE INDEX "IX_IdentityUserLogins_UserId"
    ON "public"."IdentityUserLogins"("UserId" ASC);


/****** Object: Table "public"."IdentityUserRoles" Script Date: 2019-10-28 11:28:17 AM ******/
CREATE TABLE "public"."IdentityUserRoles" (
    "UserId" varchar (450) NOT NULL,
    "RoleId" varchar (450) NOT NULL,
		CONSTRAint "PK_IdentityUserRoles_UID_RID" PRIMARY KEY ("UserId", "RoleId")
);

CREATE INDEX "IX_IdentityUserRoles_RoleId"
    ON "public"."IdentityUserRoles"("RoleId" ASC);


/****** Object: Table "public"."IdentityUsers" Script Date: 2019-10-28 11:28:32 AM ******/
-- Table: public."IdentityUsers"

-- DROP TABLE public."IdentityUsers";

CREATE TABLE "public"."IdentityUsers"
(
    "Id" character varying(450)  NOT NULL,
    "UserName" character varying(256),
	"FirstName"  varchar (450) NOT NULL,
	"LastName"  varchar (450) NULL,
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" bit(1) NOT NULL,
    "PasswordHash" character varying,
    "SecurityStamp" character varying,
    "ConcurrencyStamp" character varying,
    "PhoneNumber" character varying,
    "PhoneNumberConfirmed" bit(1) NOT NULL,
    "TwoFactorEnabled" bit(1) NOT NULL,
    "LockoutEnd" timestamp without time zone,
    "LockoutEnabled" bit(1) NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    CONSTRAINT "PK_IdentityUsers_Id" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."IdentityUsers"
    OWNER to postgres;

-- Index: EmailIndex

-- DROP INDEX public."EmailIndex";

CREATE INDEX "EmailIndex"
    ON public."IdentityUsers" USING btree
    ("NormalizedEmail" COLLATE pg_catalog."default")
    TABLESPACE pg_default;

-- Index: UserNameIndex

-- DROP INDEX public."UserNameIndex";

CREATE UNIQUE INDEX "UserNameIndex"
    ON public."IdentityUsers" USING btree
    ("NormalizedUserName" COLLATE pg_catalog."default")
    TABLESPACE pg_default
    WHERE "NormalizedUserName" IS NOT NULL;


/****** Object: Table "public"."IdentityUserTokens" Script Date: 2019-10-28 11:28:42 AM ******/
CREATE TABLE "public"."IdentityUserTokens" (
    "UserId"        varchar (450) NOT NULL,
    "LoginProvider" varchar (128) NOT NULL,
    "Name"          varchar (128) NOT NULL,
    "Value"         varchar  NULL
);


ALTER TABLE "public"."IdentityRoleClaims"
    ADD CONSTRAint "FK_IdentityRoleClaims_IdentityRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "public"."IdentityRoles" ("Id") ON DELETE CASCADE;

ALTER TABLE "public"."IdentityUserClaims"
    ADD CONSTRAint "FK_IdentityUserClaims_IdentityUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "public"."IdentityUsers" ("Id") ON DELETE CASCADE;
		

ALTER TABLE "public"."IdentityUserLogins"
    ADD CONSTRAint "FK_IdentityUserLogins_IdentityUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "public"."IdentityUsers" ("Id") ON DELETE CASCADE;

ALTER TABLE "public"."IdentityUserRoles"
    ADD CONSTRAint "FK_IdentityUserRoles_IdentityRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "public"."IdentityRoles" ("Id") ON DELETE CASCADE;

ALTER TABLE "public"."IdentityUserRoles"
    ADD CONSTRAint "FK_IdentityUserRoles_IdentityUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "public"."IdentityUsers" ("Id") ON DELETE CASCADE;