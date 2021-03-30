BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "RadicalSet" (
	"ID"	integer NOT NULL,
	"Character"	nvarchar(300) NOT NULL,
	PRIMARY KEY("ID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "KanjiMeaningSet" (
	"ID"	integer NOT NULL,
	"Language"	nvarchar(10),
	"Meaning"	nvarchar(300) NOT NULL,
	"Kanji_ID"	integer NOT NULL,
	PRIMARY KEY("ID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "VocabCategorySet" (
	"ID"	integer NOT NULL,
	"ShortName"	nvarchar(300) NOT NULL,
	"Label"	nvarchar(300) NOT NULL,
	PRIMARY KEY("ID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "KanjiRadical" (
	"Kanji_ID"	integer NOT NULL,
	"Radicals_ID"	integer NOT NULL,
	PRIMARY KEY("Kanji_ID","Radicals_ID")
);
CREATE TABLE IF NOT EXISTS "VocabMeaningVocabCategory" (
	"VocabMeaningVocabCategory_VocabCategory_ID"	integer NOT NULL,
	"Categories_ID"	integer NOT NULL,
	PRIMARY KEY("VocabMeaningVocabCategory_VocabCategory_ID","Categories_ID")
);
CREATE TABLE IF NOT EXISTS "VocabEntityVocabMeaning" (
	"VocabEntity_ID"	integer NOT NULL,
	"Meanings_ID"	integer NOT NULL,
	PRIMARY KEY("VocabEntity_ID","Meanings_ID")
);
CREATE TABLE IF NOT EXISTS "VocabCategoryVocabEntity" (
	"Categories_ID"	integer NOT NULL,
	"VocabCategoryVocabEntity_VocabCategory_ID"	integer NOT NULL,
	PRIMARY KEY("Categories_ID","VocabCategoryVocabEntity_VocabCategory_ID")
);
CREATE TABLE IF NOT EXISTS "KanjiEntityVocabEntity" (
	"Kanji_ID"	integer NOT NULL,
	"Vocabs_ID"	integer NOT NULL,
	PRIMARY KEY("Kanji_ID","Vocabs_ID")
);
CREATE TABLE IF NOT EXISTS "VocabMeaningSet" (
	"ID"	integer NOT NULL,
	"Meaning"	nvarchar(600),
	PRIMARY KEY("ID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "KanjiStrokes" (
	"ID"	INTEGER NOT NULL,
	"FramesSvg"	BLOB,
	PRIMARY KEY("ID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "KanjiSet" (
	"ID"	integer NOT NULL,
	"Character"	nvarchar(300),
	"StrokeCount"	integer,
	"Grade"	smallint,
	"MostUsedRank"	integer,
	"JlptLevel"	smallint,
	"OnYomi"	nvarchar(300),
	"KunYomi"	nvarchar(300),
	"Nanori"	nvarchar(300),
	"UnicodeValue"	integer,
	"NewspaperRank"	integer,
	"WkLevel"	integer,
	PRIMARY KEY("ID" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "VocabSet" (
	"ID"	integer NOT NULL,
	"Seq"	integer NOT NULL,
	"KanjiWriting"	nvarchar(300),
	"KanaWriting"	nvarchar(300) NOT NULL,
	"IsCommon"	boolean NOT NULL,
	"FrequencyRank"	INT64,
	"Furigana"	nvarchar(300),
	"JlptLevel"	integer,
	"WkLevel"	integer,
	"WikiRank"	integer,
	"IsMain"	boolean NOT NULL,
	PRIMARY KEY("ID" AUTOINCREMENT)
);
CREATE INDEX IF NOT EXISTS "Index_Vocab_KanaWriting" ON "VocabSet" (
	"KanaWriting"
);
CREATE INDEX IF NOT EXISTS "Index_Vocab_Seq" ON "VocabSet" (
	"Seq"
);
COMMIT;
