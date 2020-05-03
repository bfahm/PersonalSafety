namespace PersonalSafety.Models
{
    public class AttachmentRepository : BaseRepository<Attachment>, IAttachmentRepository
    {
        private readonly AppDbContext context;

        public AttachmentRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
