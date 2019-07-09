﻿using System.ComponentModel.DataAnnotations;

namespace EMap.MapServer.Services.Models
{
    public class NameRecord:BaseRecord
    {
        [Required]
        [Display(Name ="名称")]
        public string Name { get; set; }
    }
}