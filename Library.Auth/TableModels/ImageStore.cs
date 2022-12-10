using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Auth.TableModels
{
    public class ImageStore
    {
        [Key]
        public string ImageId { get; set; }
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public DateTime UploadedOn { get; set; }

        // requires a public ICollection Reference to this class on SecureUser in order for ef to create the foreign key
        [ForeignKey("UploadedBy")]
        public SecureUser SecureUser { get; set; }
    }
}
