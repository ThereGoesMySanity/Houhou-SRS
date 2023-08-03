using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities;

[Table(SqlHelper.Table_KanjiMeaning)]
public class KanjiMeaning : Entity
{
    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }
    public string Language { get; set; }
    public string Meaning { get; set; }
    [Column("Kanji_ID")]
    public long KanjiID
    {
        get { return Kanji.ID; }
        set { /* noop */ }
    }


    [Ignore]
    public KanjiEntity Kanji { get; set; }
}