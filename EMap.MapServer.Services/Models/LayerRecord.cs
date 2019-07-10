﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMap.MapServer.Services.Models
{
    public class LayerRecord:NameRecord
    {
        public string Path { get; set; }
        public int ServiceId { get; set; }
        [Display(Name ="所属服务")]
        [ForeignKey("ServiceId")]
        public virtual ServiceRecord Service { get; set; }
    }
}