﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResortLagoon.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Price per night")]
        [Range(10, 10000)]
        public decimal Price { get; set; }
        public double Sqft { get; set; }
        [Range(1, 10)]
        public int Occupancy { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }  //use to upload villa images
        [Display(Name = "Image Url")]
        public string? ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        [ValidateNever]
        public IEnumerable<Amenity> VillaAmenity { get; set; }

    }
}
