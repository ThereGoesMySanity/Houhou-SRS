using System;
using System.Collections.Generic;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities;

[Table(SqlHelper.Table_Vocab)]
public class VocabEntity : Entity
{
    public VocabEntity()
    {
        this.Meanings = new HashSet<VocabMeaning>();
        this.Kanji = new HashSet<KanjiEntity>();
        this.Categories = new HashSet<VocabCategory>();
        this.SrsEntries = new HashSet<SrsEntry>();
        this.Variants = new HashSet<VocabEntity>();
    }

    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }
    public long Seq { get; set; }
    public string KanjiWriting { get; set; }
    public string KanaWriting { get; set; }
    public bool IsCommon { get; set; }
    public Nullable<int> FrequencyRank { get; set; }
    public Nullable<int> JlptLevel { get; set; }
    public string Furigana { get; set; }
    [Column(SqlHelper.Field_Vocab_WaniKaniLevel)]
    public Nullable<int> WaniKaniLevel { get; set; }
    [Column(SqlHelper.Field_Vocab_WikipediaRank)]
    public Nullable<int> WikipediaRank { get; set; }
    public bool IsMain { get; set; }

    [Ignore]
    public ICollection<VocabMeaning> Meanings { get; set; }
    [Ignore]
    public ICollection<KanjiEntity> Kanji { get; set; }
    [Ignore]
    public ICollection<VocabCategory> Categories { get; set; }
    [Ignore]
    public ICollection<SrsEntry> SrsEntries { get; set; }
    [Ignore]
    public ICollection<VocabEntity> Variants { get; set; }
}
