BEGIN;
-- Database: sumstories

-- DROP DATABASE IF EXISTS sumstories;

CREATE DATABASE IF NOT EXISTS sumstories
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- Clear Tables
-- DROP TABLE IF EXISTS public.accounts;
-- DROP TABLE IF EXISTS public.sumthings;
-- DROP TABLE IF EXISTS public.categories;
-- DROP TABLE IF EXISTS public.attributes;
-- DROP TABLE IF EXISTS public.folders;
-- DROP TABLE IF EXISTS public.calendars;
-- DROP TABLE IF EXISTS public.months;
-- DROP TABLE IF EXISTS public.periods;
-- DROP TABLE IF EXISTS public.seasons;
-- DROP TABLE IF EXISTS public.timeline_events;

-- Table: public.accounts

CREATE TABLE IF NOT EXISTS public.accounts (
    id bigint NOT NULL DEFAULT nextval('accounts_id_seq'::regclass),
    username text COLLATE pg_catalog."default" NOT NULL,
    email text COLLATE pg_catalog."default" NOT NULL,
    password text COLLATE pg_catalog."default" NOT NULL,
    salt character varying COLLATE pg_catalog."default",
    session_key character varying COLLATE pg_catalog."default",
    CONSTRAINT accounts_pkey PRIMARY KEY (id)
);

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.accounts
    OWNER to pg_database_owner;

-- Table: public.sumthings

CREATE TABLE IF NOT EXISTS public.sumthings
(
    id bigint NOT NULL DEFAULT nextval('sumthings_id_seq'::regclass),
    account bigint NOT NULL,
    name text COLLATE pg_catalog."default",
    category integer,
    attributes bigint[],
    last_updated timestamp without time zone,
    CONSTRAINT sumthings_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.sumthings
    OWNER to pg_database_owner;

-- Table: public.categories

CREATE TABLE IF NOT EXISTS public.categories
(
    id integer NOT NULL DEFAULT nextval('categories_id_seq'::regclass),
    name text COLLATE pg_catalog."default",
    default_attributes bigint[],
    CONSTRAINT categories_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.categories
    OWNER to pg_database_owner;
	
-- Table: public.attributes

CREATE TABLE IF NOT EXISTS public.attributes
(
    id bigint NOT NULL DEFAULT nextval('attributes_id_seq'::regclass),
    account bigint,
    name text COLLATE pg_catalog."default",
    text_value text COLLATE pg_catalog."default",
    num_value integer,
    maximum_value bigint,
    accuracy integer,
    subattributes bigint[],
    type integer,
    CONSTRAINT attributes_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.attributes
    OWNER to pg_database_owner;

-- Table: public.folders

CREATE TABLE IF NOT EXISTS public.folders
(
    id bigint NOT NULL DEFAULT nextval('folders_id_seq'::regclass),
    account bigint,
    name text COLLATE pg_catalog."default",
    category integer,
    items bigint[],
    subfolders bigint[],
    CONSTRAINT folders_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.folders
    OWNER to pg_database_owner;
	
-- Table: public.calendars

CREATE TABLE IF NOT EXISTS public.calendars
(
    id bigint NOT NULL DEFAULT nextval('calendars_id_seq'::regclass),
    account bigint,
    name text COLLATE pg_catalog."default",
    periods bigint[],
    months bigint[],
    seasons bigint[],
    CONSTRAINT calendars_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.calendars
    OWNER to pg_database_owner;
	
-- Table: public.months

CREATE TABLE IF NOT EXISTS public.months
(
    id bigint NOT NULL DEFAULT nextval('months_id_seq'::regclass),
    name text COLLATE pg_catalog."default",
    days integer,
    CONSTRAINT months_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.months
    OWNER to pg_database_owner;
	
-- Table: public.periods

CREATE TABLE IF NOT EXISTS public.periods
(
    id bigint NOT NULL DEFAULT nextval('periods_id_seq'::regclass),
    name text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    calendar bigint,
    start_year integer,
    end_year integer,
    CONSTRAINT periods_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.periods
    OWNER to pg_database_owner;
	
-- Table: public.seasons

CREATE TABLE IF NOT EXISTS public.seasons
(
    id bigint NOT NULL DEFAULT nextval('seasons_id_seq'::regclass),
    name text COLLATE pg_catalog."default",
    start_date integer[],
    CONSTRAINT seasons_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.seasons
    OWNER to pg_database_owner;
	
-- Table: public.timeline_events

CREATE TABLE IF NOT EXISTS public.timeline_events
(
    id bigint NOT NULL DEFAULT nextval('timeline_events_id_seq'::regclass),
    account bigint,
    name text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    category bigint,
    calendar integer,
    start_date integer[],
    end_date integer[],
    CONSTRAINT timeline_events_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.timeline_events
    OWNER to pg_database_owner;

COMMIT;