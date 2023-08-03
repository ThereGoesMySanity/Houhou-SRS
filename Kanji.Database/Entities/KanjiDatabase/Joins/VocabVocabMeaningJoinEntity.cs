using System.Collections.Generic;
using System.Data;
using Kanji.Database.Dao;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities.Joins;

[Table(SqlHelper.Table_Vocab_VocabMeaning)]
public class VocabVocabMeaningJoinEntity : Entity
{
    [Column(SqlHelper.Field_Vocab_VocabMeaning_VocabId)]
    public long VocabId { get; set; }
    [Column(SqlHelper.Field_Vocab_VocabMeaning_VocabMeaningId)]
    public long MeaningId { get; set; }
}
