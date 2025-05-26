using System.ComponentModel.DataAnnotations;

namespace T2_VoTrongHung_2280601119.Models
{
    // Category.cs
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }
    }

}
