using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Orchestrator.Core.Models
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string InputData { get; set; }

        public string OutputData { get; set; }

        [Required]
        public int TaskStatusId { get; set; }

        public Guid RobotId { get; set; }
    }
}
