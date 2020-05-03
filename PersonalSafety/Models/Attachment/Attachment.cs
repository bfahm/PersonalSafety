using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalSafety.Models
{
    public class Attachment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Event")]
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public string Url { get; set; }
    }
}
