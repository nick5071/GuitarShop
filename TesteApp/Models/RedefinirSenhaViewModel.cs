using System.ComponentModel.DataAnnotations;

namespace GuitarShop.Models
{
    public class RedefinirSenhaViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmarSenha { get; set; }
    }
}
