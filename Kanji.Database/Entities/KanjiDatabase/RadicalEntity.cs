using System.Collections.Generic;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities;

[Table(SqlHelper.Table_Radical)]
public class RadicalEntity : Entity
{
    public RadicalEntity()
    {
        this.Kanji = new HashSet<KanjiEntity>();
    }

    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }
    public string Character { get; set; }

    [Ignore]
    public ICollection<KanjiEntity> Kanji { get; set; }
}
