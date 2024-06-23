using System;
using System.ComponentModel.DataAnnotations;

namespace TobyBlazor.Models;

public class Video : IDataItem
{
    [Key] public int Id { get; init; }

    public string Title { get; init; }
    public string YTId { get; init; }
    public string Group { get; set; }
    public DateTime CreateDate { get; init; }
}