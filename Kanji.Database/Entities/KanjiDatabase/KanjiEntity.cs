using System;
using System.Collections.Generic;
using Kanji.Database.Helpers;
using SQLite;

namespace Kanji.Database.Entities;

[Table(SqlHelper.Table_Kanji)]
public class KanjiEntity : Entity
{
    public KanjiEntity()
    {
        this.Radicals = new HashSet<RadicalEntity>();
        this.Vocabs = new HashSet<VocabEntity>();
        this.Meanings = new HashSet<KanjiMeaning>();
        this.SrsEntries = new HashSet<SrsEntry>();
    }

    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }
    public string Character { get; set; }
    public int? StrokeCount { get; set; }
    public short? Grade { get; set; }
    public int? MostUsedRank { get; set; }
    public short? JlptLevel { get; set; }
    public int? UnicodeValue { get; set; }
    public int? NewspaperRank { get; set; }
    [Column(SqlHelper.Field_Kanji_WaniKaniLevel)]
    public int? WaniKaniLevel { get; set; }
    public string OnYomi { get; set; }
    public string KunYomi { get; set; }
    public string Nanori { get; set; }

    [Ignore]
    public ICollection<RadicalEntity> Radicals { get; set; }
    [Ignore]
    public ICollection<VocabEntity> Vocabs { get; set; }
    [Ignore]
    public ICollection<KanjiMeaning> Meanings { get; set; }
    [Ignore]
    public ICollection<SrsEntry> SrsEntries { get; set; }
}