﻿using System;
using SQLite;

namespace Kanji.Database.Entities;

[Table("SrsEntrySet")]
public class SrsEntry : Entity
{
    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public DateTimeOffset? NextAnswerDate { get; set; }
    public string Meanings { get; set; } = "";
    public string Readings { get; set; } = "";
    public short CurrentGrade { get; set; }
    public int FailureCount { get; set; }
    public int SuccessCount { get; set; }
    public string AssociatedVocab { get; set; }
    public string AssociatedKanji { get; set; }
    public string MeaningNote { get; set; }
    public string ReadingNote { get; set; }
    public DateTimeOffset? SuspensionDate { get; set; }
    public string Tags { get; set; }
    public DateTimeOffset? LastUpdateDate { get; set; }
    public bool IsDeleted { get; set; }
    public long? ServerId { get; set; }
}
