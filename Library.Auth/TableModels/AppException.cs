using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.TableModels
{
    public class AppException
    {
        //Message
        //OccurredOn
        //MessageType
        [Key]
        public string ExceptionId { get; set; }
        public string Message { get; set; }
        public DateTime OccurredOn { get; set; }
        public Int16 MessageType { get; set; }
    }
}
