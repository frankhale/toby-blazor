using System.ComponentModel.DataAnnotations;

namespace TobyBlazor.Models
{
  public class Preferences
  {
    [Key]
    public int Id { get; set; }
    public int CurrentVideoPage { get; set; }
    public int CurrentVideoPageLinkPage { get; set; } // This is a terrible name (change later)        
    public int CurrentRecentlyPlayedVideoPage { get; set; } // Whoops, doubled down... LOL!
  }
}
