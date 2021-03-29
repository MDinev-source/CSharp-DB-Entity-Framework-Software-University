namespace VaporStore.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    public class GameTag
    {
        [Key]
        public int GameId { get; set; }
        public Game Game { get; set; }

        [Key]
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
