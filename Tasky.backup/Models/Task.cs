﻿using System.ComponentModel.DataAnnotations;

namespace Tasky.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
    }
}
