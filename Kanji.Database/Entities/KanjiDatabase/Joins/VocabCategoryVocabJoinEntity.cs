using System.Collections.Generic;
using System.Data;
using Kanji.Database.Dao;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities.Joins;

[Table(SqlHelper.Table_VocabCategory_Vocab)]
public class VocabCategoryVocabJoinEntity : Entity
{
    [Column(SqlHelper.Field_VocabCategory_Vocab_VocabCategoryId)]
    public long CategoryId { get; set; }
    [Column(SqlHelper.Field_VocabCategory_Vocab_VocabId)]
    public long VocabId { get; set; }
}
