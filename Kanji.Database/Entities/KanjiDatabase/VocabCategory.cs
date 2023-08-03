using System.Collections.Generic;
using System.Data;
using Kanji.Database.Helpers;
using SQLite;
namespace Kanji.Database.Entities;

[Table(SqlHelper.Table_VocabCategory)]
public class VocabCategory : Entity
{
    [PrimaryKey]
    [AutoIncrement]
    public long ID { get; set; }
    public string ShortName { get; set; }
    public string Label { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        // Used for selecting items by typing the first letter.
        // TODO: This should use the same value as VocabCategoriesToStringConverter.
        return Label;
    }
    public override bool Equals(object obj)
    {
        return (obj as VocabCategory)?.ID == ID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}
