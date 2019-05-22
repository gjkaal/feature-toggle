using System;
using System.ComponentModel.DataAnnotations;

namespace FeatureServices.Storage.DbModel
{
    public abstract class DbRecord
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        
        public string Name { get; set; }
        public Guid Reference { get; set; }        
        public int Tenant { get; set; }
        [Timestamp]
        public byte[] TimeStamp { get; set; }
        public DateTime Created { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
