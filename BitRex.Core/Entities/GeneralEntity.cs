using BitRex.Core.Enums;

namespace BitRex.Core.Entities
{
    public class GeneralEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Status Status { get; set; }
        public string StatusDesc { get { return Status.ToString(); } }
    }
}
