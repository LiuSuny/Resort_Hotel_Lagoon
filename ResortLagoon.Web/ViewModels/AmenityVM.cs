using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResortLagoon.Domain.Entities;

namespace ResortLagoon.Web.ViewModels
{
    /// <summary>
    /// This class is for view binding instead of using viewbag or viewdata
    /// </summary>
    public class AmenityVM
    {
        public Amenity? Amenity { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? VillaList { get; set; }
    }
}
