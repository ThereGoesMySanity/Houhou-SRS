using System.Collections.Generic;
using System.Data;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities;

[Table(SqlHelper.Table_VocabMeaning)]
public class VocabMeaning : Entity
{
    public VocabMeaning()
    {
        this.Categories = new HashSet<VocabCategory>();
        this.VocabEntity = new HashSet<VocabEntity>();
    }

    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }
    public string Meaning { get; set; }

    [Ignore]
    public ICollection<VocabCategory> Categories { get; set; }
    [Ignore]
    public ICollection<VocabEntity> VocabEntity { get; set; }
}
