using System.ComponentModel.DataAnnotations;

namespace Shopbannoithat.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên vai trò")]
        public string Name { get; set; }
    }
}
