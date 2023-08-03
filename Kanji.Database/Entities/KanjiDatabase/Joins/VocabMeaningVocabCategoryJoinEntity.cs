using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Kanji.Database.Dao;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities.Joins;

[Table(SqlHelper.Table_VocabMeaning_VocabCategory)]
public class VocabMeaningVocabCategoryJoinEntity : Entity
{
    [Column(SqlHelper.Field_VocabMeaning_VocabCategory_VocabCategoryId)]
    public long CategoryId { get; set; }
    [Column(SqlHelper.Field_VocabMeaning_VocabCategory_VocabMeaningId)]
    public long MeaningId { get; set; }
}
