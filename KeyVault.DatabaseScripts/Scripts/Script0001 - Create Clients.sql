CREATE TABLE "Clients" (
	"clientId"	TEXT UNIQUE,
	"certificateThumbprint"	TEXT NOT NULL,
	"created_utc"	TEXT NOT NULL,
	"disabled"	INTEGER NOT NULL DEFAULT 0,
	"disabledDate"	TEXT,
	PRIMARY KEY("clientId")
)