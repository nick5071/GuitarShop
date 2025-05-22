using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TesteApp.Models
{
    public class Produtos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatório.")]
        [StringLength(500, ErrorMessage = "O descrição pode ter no máximo 500 caracteres")]
        public string Descricao { get; set; }

        [Precision(18, 2)]
        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, 999999.99, ErrorMessage = "O valor deve estar entre 0,01 e 999999,99")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A Primeira imagem é obrigatória.")]
        [StringLength(500, ErrorMessage = "O nome da imagem pode ter no máximo 500 caracteres")]
        public string Imagem { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser um número inteiro positivo.")]
        public int? Quantidade { get; set; }

        [StringLength(500, ErrorMessage = "O nome da imagem pode ter no máximo 500 caracteres")]
        public string? Imagem2 { get; set; }

        [StringLength(500, ErrorMessage = "O nome da imagem pode ter no máximo 500 caracteres")]
        public string? Imagem3 { get; set; }

        public void Validar()
        {
            if (string.IsNullOrEmpty(Nome) || Nome.Length > 100)
                throw new ApplicationException("O campo Nome é obrigatório");
            if (string.IsNullOrEmpty(Descricao) || Descricao.Length > 500)
                throw new ApplicationException("O campo Descricao é obrigatório");
            if (Preco <= 0 || Preco > 999999999)
                throw new ApplicationException("O campo Preço deve ser maior que zero.");
            if (!Quantidade.HasValue || Quantidade <= 0)
                throw new ApplicationException("O campo Quantidade é obrigatório e deve ser maior que zero.");
        }
    }
}
