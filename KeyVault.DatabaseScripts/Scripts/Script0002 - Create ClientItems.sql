CREATE TABLE "ClientItems" (
	"itemId"	TEXT UNIQUE,
	"created_utc"	TEXT NOT NULL,
	"itemData"	BLOB NOT NULL,
	PRIMARY KEY("itemId")
)