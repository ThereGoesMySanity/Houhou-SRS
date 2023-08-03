using System.Collections.Generic;
using System.Data;
using Kanji.Database.Dao;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities.Joins;

[Table(SqlHelper.Table_Kanji_Radical)]
public class KanjiRadicalJoinEntity : Entity
{
    [Column(SqlHelper.Field_Kanji_Radical_KanjiId)]
    public long KanjiId { get; set; }
    [Column(SqlHelper.Field_Kanji_Radical_RadicalId)]
    public long RadicalId { get; set; }
}
