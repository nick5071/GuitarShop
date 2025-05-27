using System.ComponentModel.DataAnnotations;

namespace GuitarShop.Models
{
    public class Login
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Lembrar login?")]
        public bool RememberMe { get; set; }
    }
}
