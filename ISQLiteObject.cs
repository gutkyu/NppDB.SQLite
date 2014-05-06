using System;
namespace NppDB.SQLite
{
    internal interface ISQLiteObject
    {
        string FullName { get; }
        string Title { get; set; }
    }
}
