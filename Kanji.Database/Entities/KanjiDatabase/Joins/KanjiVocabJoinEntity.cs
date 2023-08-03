using System.Collections.Generic;
using System.Data;
using Kanji.Database.Dao;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities.Joins;
[Table(SqlHelper.Table_Kanji_Vocab)]
public class KanjiVocabJoinEntity : Entity
{
    [Column(SqlHelper.Field_Kanji_Vocab_KanjiId)]
    public long KanjiId { get; set; }
    [Column(SqlHelper.Field_Kanji_Vocab_VocabId)]
    public long VocabId { get; set; }
}
