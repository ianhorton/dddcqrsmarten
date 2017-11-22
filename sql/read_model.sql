create table export_definition
(
	id varchar not null
		constraint export_definition_pkey
			primary key,
	title varchar not null
)
;

create unique index export_definition_id_uindex
	on export_definition (id)
;

create table export_row
(
	id varchar not null
		constraint export_row_pkey
			primary key,
	name varchar,
	export_definition_id varchar
		constraint export_definition_id
			references export_definition
)
;

create unique index export_row_id_uindex
	on export_row (id)
;

