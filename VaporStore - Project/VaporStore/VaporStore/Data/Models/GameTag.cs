namespace VaporStore.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class GameTag
    {
        [Required]
        [ForeignKey(nameof(Game))]
        public int GameId { get; set; }

        public Game Game { get; set; }

        [Required]
        [ForeignKey(nameof(Tag))]
        public int Tagid { get; set; }

        public Tag Tag { get; set; }

    }
}
