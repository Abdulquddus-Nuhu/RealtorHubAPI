using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealtorHubAPI.Entities
{
    public class BaseEntity : IBaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeletedBy { get;  set; } = string.Empty;
        public virtual DateTime? Deleted { get;  set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Modified { get; set; }
        public virtual string? LastModifiedBy { get; set; }
        protected BaseEntity()
        {
            IsDeleted = false;
            Created = DateTime.UtcNow;
        }
    }
}
