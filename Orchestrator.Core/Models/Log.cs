using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Orchestrator.Core.Models
{
    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Message { get; set; }
        public int LogTypeId { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
