using System.ComponentModel.DataAnnotations;

namespace GuitarShop.Models
{
    public class Registro
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não coincidem.")]
        public string ConfirmPassword { get; set; }

        [StringLength(20, ErrorMessage = "O Telefone pode ter no máximo 20 caracteres")]
        public string? Telefone { get; set; }

        [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres")]
        public string? NomeCompleto { get; set; }
    }
}
