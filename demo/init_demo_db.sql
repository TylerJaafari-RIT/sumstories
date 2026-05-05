-- BEGIN;
-- Database: sumstories

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
    id bigserial PRIMARY KEY,
    username text COLLATE pg_catalog."default" NOT NULL,
    email text COLLATE pg_catalog."default" NOT NULL,
    password text COLLATE pg_catalog."default" NOT NULL,
    salt character varying COLLATE pg_catalog."default",
    session_key character varying COLLATE pg_catalog."default"
);

-- Table: public.sumthings

CREATE TABLE IF NOT EXISTS public.sumthings (
    id bigserial PRIMARY KEY,
    account bigint NOT NULL,
    name text COLLATE pg_catalog."default",
    category integer,
    attributes bigint[],
    last_updated timestamp without time zone
);

-- Table: public.categories

CREATE TABLE IF NOT EXISTS public.categories (
    id serial PRIMARY KEY,
    name text COLLATE pg_catalog."default",
    default_attributes bigint[]
);

INSERT INTO public.categories (name, default_attributes)
VALUES ('character', '{1,2}');

-- Table: public.attributes

CREATE TABLE IF NOT EXISTS public.attributes
(
    id bigserial PRIMARY KEY,
    account bigint,
    name text COLLATE pg_catalog."default",
    text_value text COLLATE pg_catalog."default",
    num_value integer,
    maximum_value bigint,
    accuracy integer,
    subattributes bigint[],
    type integer
);

INSERT INTO public.attributes (account, name, type)
VALUES (null, 'Full Name', 0);
INSERT INTO public.attributes (account, name, text_value, num_value, accuracy, type)
VALUES (null, 'Age', 'years', 0, 0, 1);

-- Table: public.folders

CREATE TABLE IF NOT EXISTS public.folders
(
    id bigserial PRIMARY KEY,
    account bigint,
    name text COLLATE pg_catalog."default",
    category integer,
    items bigint[],
    subfolders bigint[]
);
	
-- Table: public.calendars

CREATE TABLE IF NOT EXISTS public.calendars
(
    id bigserial PRIMARY KEY,
    account bigint,
    name text COLLATE pg_catalog."default",
    periods bigint[],
    months bigint[],
    seasons bigint[]
);

-- Table: public.months

CREATE TABLE IF NOT EXISTS public.months
(
    id bigserial PRIMARY KEY,
    name text COLLATE pg_catalog."default",
    days integer
);

-- Table: public.periods

CREATE TABLE IF NOT EXISTS public.periods
(
    id bigserial PRIMARY KEY,
    name text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    calendar bigint,
    start_year integer,
    end_year integer
);

-- Table: public.seasons

CREATE TABLE IF NOT EXISTS public.seasons
(
    id bigserial PRIMARY KEY,
    name text COLLATE pg_catalog."default",
    start_date integer[]
);

-- Table: public.timeline_events

CREATE TABLE IF NOT EXISTS public.timeline_events
(
    id bigserial PRIMARY KEY,
    account bigint,
    name text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    category bigint,
    calendar integer,
    start_date integer[],
    end_date integer[]
);

-- COMMIT;