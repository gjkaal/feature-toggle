using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FeatureServices.Storage.DbModel
{
    public class DbRecord
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

    public class TenantConfiguration : DbRecord
    {
        public virtual List<FeatureValue> FeatureValue { get; set; }
    }

    public class FeatureValue : DbRecord
    {
        public virtual int TenantConfigurationId { get; set; }
        public virtual TenantConfiguration TenantConfiguration { get; set; }
    }
}
