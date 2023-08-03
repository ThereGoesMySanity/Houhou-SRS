using Kanji.Database.Dao;
using Kanji.Database.Helpers;
using SQLite;
using System.Collections.Generic;
using System.Data;

namespace Kanji.Database.Entities;

[Table(SqlHelper.Table_KanjiStrokes)]
public class KanjiStrokes : Entity
{
    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }

    public byte[] FramesSvg { get; set; }
}
