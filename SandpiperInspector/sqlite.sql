-- Convenience for development
--- Linking tables ---
DROP TABLE IF EXISTS node_multi_link_entries;
DROP TABLE IF EXISTS node_multi_links;
DROP TABLE IF EXISTS node_unique_links;

DROP TABLE IF EXISTS pool_multi_link_entries;
DROP TABLE IF EXISTS pool_multi_links;
DROP TABLE IF EXISTS pool_unique_links;

DROP TABLE IF EXISTS slice_multi_link_entries;
DROP TABLE IF EXISTS slice_multi_links;
DROP TABLE IF EXISTS slice_unique_links;

-- Core tables
DROP TABLE IF EXISTS grain_payloads;
DROP TABLE IF EXISTS slice_grains;

DROP TABLE IF EXISTS grains;

DROP TABLE IF EXISTS subscriptions;
DROP TABLE IF EXISTS plan_slices;
DROP TABLE IF EXISTS plans;

DROP TABLE IF EXISTS slices;
DROP TABLE IF EXISTS pools;
DROP TABLE IF EXISTS nodes;
DROP TABLE IF EXISTS instance_responders;
DROP TABLE IF EXISTS instances;
DROP TABLE IF EXISTS controllers;

--- Basic key values
DROP TABLE IF EXISTS slice_types;
DROP TABLE IF EXISTS unique_key_fields;
DROP TABLE IF EXISTS multi_key_fields;

-- Begin main structure
--- Valid values
CREATE TABLE slice_types (
		  sliceType TEXT PRIMARY KEY NOT NULL
	);

CREATE TABLE unique_key_fields (
		  uniqueKeyField TEXT PRIMARY KEY NOT NULL
	);

CREATE TABLE multi_key_fields (
		  multiKeyField TEXT PRIMARY KEY NOT NULL
	);

--- Core Tables
CREATE TABLE controllers (
		  controllerUUID CHAR(36) PRIMARY KEY NOT NULL
		, controllerDescription TEXT 
		, adminContact TEXT NOT NULL
		, adminEmail TEXT NOT NULL
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
	);

CREATE TABLE instances (
		  instanceUUID CHAR(36) PRIMARY KEY NOT NULL
		, softwareDescription TEXT NOT NULL
		, softwareVersion TEXT NOT NULL
		, capabilityLevel TEXT NOT NULL
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
	);

CREATE TABLE instance_responders (
		  instanceResponderID INTEGER PRIMARY KEY
		, instanceUUID CHAR(36) NOT NULL REFERENCES instances (instanceUUID)
		, capabilityURI TEXT NOT NULL
		, capabilityRole TEXT NOT NULL
		, instanceResponderOrder INTEGER NOT NULL DEFAULT 0
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
		, UNIQUE (instanceUUID, capabilityRole)
	);

CREATE TABLE nodes (
		  nodeUUID CHAR(36) PRIMARY KEY NOT NULL
		, controllerUUID CHAR(36) NOT NULL REFERENCES controllers (controllerUUID)
		, instanceUUID CHAR(36) NOT NULL REFERENCES instances (instanceUUID)
		, nodeDescription TEXT NOT NULL
		-- Allow one node to be flagged as this node, and only one
		, selfNode TEXT NULL UNIQUE CHECK (selfNode IS NULL OR selfNode = 'yes')
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
		, UNIQUE (controllerUUID, instanceUUID)
	);

CREATE TABLE pools (
		  poolUUID CHAR(36) PRIMARY KEY NOT NULL
		, nodeUUID CHAR(36) NOT NULL REFERENCES nodes (nodeUUID)
		, poolDescription TEXT NOT NULL
		, poolOrder INTEGER NOT NULL DEFAULT 0
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
		, UNIQUE (nodeUUID, poolOrder)
	);

CREATE TABLE slices (
		  sliceUUID CHAR(36) PRIMARY KEY NOT NULL
		, poolUUID CHAR(36) NOT NULL REFERENCES pools (poolUUID)
		, sliceDescription TEXT NOT NULL
		, sliceType TEXT NOT NULL REFERENCES slice_types (sliceType)
		, fileName TEXT NULL
		, sliceOrder INTEGER NOT NULL DEFAULT 0
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
		, unique (poolUUID, sliceDescription)
	);

CREATE TABLE grains (
		  grainUUID CHAR(36) PRIMARY KEY NOT NULL
		, grainKey TEXT NOT NULL
		, grainReference TEXT NOT NULL
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
	);

-- Grain payloads are stored separately to allow their contents to be selectively removed but the
-- history of their existence to be retained
CREATE TABLE grain_payloads (
		  grainContentID INTEGER PRIMARY KEY
		, grainUUID CHAR(36) NOT NULL UNIQUE REFERENCES grains (grainUUID)
		, encoding TEXT NOT NULL
		, payload BLOB NULL
	);

CREATE TABLE slice_grains (
		  sliceGrainID INTEGER PRIMARY KEY
		, sliceUUID CHAR(36) NOT NULL REFERENCES slices (sliceUUID)
		, grainUUID CHAR(36) NOT NULL REFERENCES grains (grainUUID)
		, grainOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (sliceUUID, grainUUID)
		, UNIQUE (sliceUUID, grainOrder)
	);

--- Plans
CREATE TABLE plans (
		  planUUID CHAR(36) PRIMARY KEY
		, primaryNodeUUID CHAR(36) NOT NULL REFERENCES nodes (nodeUUID)
		, secondaryNodeUUID CHAR(36) NOT NULL REFERENCES nodes (nodeUUID)
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
		, UNIQUE (primaryNodeUUID, secondaryNodeUUID)
		, CHECK (primaryNodeUUID <> secondaryNodeUUID)
	);

CREATE TABLE plan_slices (
		  planSliceID INTEGER PRIMARY KEY
		, planUUID CHAR(36) NOT NULL REFERENCES plans (planUUID)
		, sliceUUID CHAR(36) NOT NULL REFERENCES slices (sliceUUID)
		, planSliceOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (planUUID, sliceUUID)
		, UNIQUE (planUUID, planSliceOrder)
	);

CREATE TABLE subscriptions (
		  subscriptionUUID CHAR(36) PRIMARY KEY
		, planSliceID INTEGER NOT NULL REFERENCES plan_slices (planSliceID)
		, subscriptionOrder INTEGER NOT NULL DEFAULT 0
		, createdOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
		, UNIQUE (planSliceID, subscriptionOrder)
	);

--- Linking tables
CREATE TABLE node_unique_links (
		  nodeUniqueLinkUUID CHAR(36) PRIMARY KEY
		, nodeUUID CHAR(36) NOT NULL REFERENCES nodes (nodeUUID)
		, keyField TEXT NOT NULL REFERENCES unique_key_fields (uniqueKeyField)
		, keyValue TEXT NOT NULL
		, keyDescription TEXT NULL
		, linkOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (nodeUUID, keyField)
	);

CREATE TABLE node_multi_links (
		  nodeMultiLinkUUID CHAR(36) PRIMARY KEY
		, nodeUUID CHAR(36) NOT NULL REFERENCES nodes (nodeUUID)
		, keyField TEXT NOT NULL REFERENCES multi_key_fields (multiKeyField)
		, linkOrder INTEGER NOT NULL DEFAULT 0
	);
		
CREATE TABLE node_multi_link_entries (
		  nodeMultiLinkEntryUUID CHAR(36) PRIMARY KEY
		, nodeMultiLinkUUID CHAR(36) NOT NULL REFERENCES node_multi_links (nodeMultiLinkUUID)
		, keyValue TEXT NOT NULL
		, keyDescription TEXT NULL
		, linkEntryOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (nodeMultiLinkUUID, keyValue)
	);

CREATE TABLE pool_unique_links (
		  poolUniqueLinkUUID CHAR(36) PRIMARY KEY
		, poolUUID CHAR(36) NOT NULL REFERENCES pools (poolUUID)
		, keyField TEXT NOT NULL REFERENCES unique_key_fields (keyField)
		, keyValue TEXT NOT NULL
		, keyDescription TEXT NULL
		, linkOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (poolUUID, keyField)
	);

CREATE TABLE pool_multi_links (
		  poolMultiLinkUUID CHAR(36) PRIMARY KEY
		, poolUUID CHAR(36) NOT NULL REFERENCES pools (poolUUID)
		, keyField TEXT NOT NULL
		, linkOrder INTEGER NOT NULL DEFAULT 0
	);
		
CREATE TABLE pool_multi_link_entries (
		  poolMultiLinkEntryUUID CHAR(36) PRIMARY KEY
		, poolMultiLinkUUID CHAR(36) NOT NULL REFERENCES pool_multi_links (poolMultiLinkUUID)
		, keyValue TEXT NOT NULL
		, keyDescription TEXT NULL
		, linkEntryOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (poolMultiLinkUUID, keyValue)
	);

CREATE TABLE slice_unique_links (
		  sliceUniqueLinkUUID CHAR(36) PRIMARY KEY
		, sliceUUID CHAR(36) NOT NULL REFERENCES slices (sliceUUID)
		, keyField TEXT NOT NULL REFERENCES unique_key_fields (keyField)
		, keyValue TEXT NOT NULL
		, keyDescription TEXT NULL
		, linkOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (sliceUUID, keyField)
	);

CREATE TABLE slice_multi_links (
		  sliceMultiLinkUUID CHAR(36) PRIMARY KEY
		, sliceUUID CHAR(36) NOT NULL REFERENCES slices (sliceUUID)
		, keyField TEXT NOT NULL
		, linkOrder INTEGER NOT NULL DEFAULT 0
	);
		
CREATE TABLE slice_multi_link_entries (
		  sliceMultiLinkEntryUUID CHAR(36) PRIMARY KEY
		, sliceMultiLinkUUID CHAR(36) NOT NULL REFERENCES slice_multi_links (sliceMultiLinkUUID)
		, keyValue TEXT NOT NULL
		, keyDescription TEXT NULL
		, linkEntryOrder INTEGER NOT NULL DEFAULT 0
		, UNIQUE (sliceMultiLinkUUID, keyValue)
	);

-- Key value table data
INSERT INTO slice_types (sliceType) VALUES
  ('aces-file'), ('aces-apps')
, ('partspro-file'), ('napa-interchange-file')
, ('pies-file'), ('pies-items'), ('pies-pricesheets')
, ('asset-file'), ('asset-archive'), ('asset-files')
, ('binary-blob'), ('xml-file'), ('text-file');

INSERT INTO unique_key_fields (uniqueKeyField) VALUES
  ('autocare-vcdb-version'), ('autocare-pcdb-version'), ('autocare-qdb-version'), ('autocare-padb-version')
, ('napa-validvehicles-version'), ('napa-translation-version')
, ('primary-reference'), ('secondary-reference')
, ('master-slice');

INSERT INTO multi_key_fields (multiKeyField) VALUES
  ('autocare-branding-brand'), ('autocare-branding-brandowner'), ('autocare-branding-parent'), ('autocare-branding-subbrand')
, ('autocare-pcdb-parttype'), ('autocare-vcdb-make')
, ('dunbradstreet-duns')
, ('model-year')
, ('napa-branding-mfr'), ('napa-line-code'), ('napa-translation-pcc')
, ('swift-bic');

-- TODO: Logging tables
