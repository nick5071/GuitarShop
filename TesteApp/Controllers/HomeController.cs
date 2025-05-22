using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesteApp.Models;

namespace TesteApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly Conexao _conexao;

        public HomeController(Conexao conexao)
        {
            _conexao = conexao;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detalhes(int Id)
        {
            var Produtos = _conexao.Produtos.FirstOrDefault(p => p.Id == Id);

            if (Produtos == null)
            {
                return NotFound();
            }

            return View(Produtos);
        }

        public IActionResult Deletar(int id)
        {
            var produtoDeletar = _conexao.Produtos.FirstOrDefault(a => a.Id == id);

            if (produtoDeletar == null)
            {
                return NotFound();
            }


            if (!string.IsNullOrEmpty(produtoDeletar.Imagem))
            {
                var caminhoImagem = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", produtoDeletar.Imagem.TrimStart('/'));
                if (System.IO.File.Exists(caminhoImagem))
                {
                    System.IO.File.Delete(caminhoImagem);
                }
            }

            _conexao.Produtos.Remove(produtoDeletar);
            _conexao.SaveChanges();

            return RedirectToAction("Produtos");
        }

        [HttpPost]
        public IActionResult CadastrarProduto(Produtos produtos_, IFormFile imagem, IFormFile imagem2, IFormFile imagem3)
        {
            if (string.IsNullOrEmpty(produtos_.Nome) || produtos_.Nome.Length > 100)
                ModelState.AddModelError(nameof(produtos_.Nome), "O campo Nome é obrigatório");

            if (string.IsNullOrEmpty(produtos_.Descricao) || produtos_.Descricao.Length > 500)
                ModelState.AddModelError(nameof(produtos_.Descricao), "O campo Descrição é obrigatório");

            if (produtos_.Preco <= 0 || produtos_.Preco > 999999999)
                ModelState.AddModelError(nameof(produtos_.Preco), "O campo Preço deve ser maior que zero.");

            if (!produtos_.Quantidade.HasValue || produtos_.Quantidade <= 0)
                ModelState.AddModelError(nameof(produtos_.Quantidade), "O campo Quantidade deve ser maior que zero.");
            if (!decimal.TryParse(produtos_.Preco.ToString(), out var precoValido) || precoValido <= 0)
            {
                ModelState.AddModelError("Preco", "Informe um valor numérico válido maior que zero.");
                return View("Index", produtos_);
            }
            if (ModelState["Quantidade"]?.Errors.Count > 0)
            {
                ModelState["Quantidade"].Errors.Clear();
                ModelState.AddModelError("Quantidade", "Informe uma quantidade válida em números inteiros.");
            }

            if (produtos_.Quantidade == 0 || produtos_.Quantidade == null)
            {
                ModelState.AddModelError("Quantidade", "Informe uma quantidade válida em números inteiros.");
                return View("Index", produtos_);
            }
            if (imagem != null && imagem.Length > 0)
            {
                var caminhoImagem = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imagem.FileName);
                using (var stream = new FileStream(caminhoImagem, FileMode.Create))
                {
                    imagem.CopyTo(stream);
                }
                produtos_.Imagem = "/images/" + imagem.FileName;
            }

            if (imagem2 != null && imagem2.Length > 0)
            {
                var caminhoImagem1 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imagem2.FileName);
                using (var stream = new FileStream(caminhoImagem1, FileMode.Create))
                {
                    imagem2.CopyTo(stream);
                }
                produtos_.Imagem2 = "/images/" + imagem2.FileName;
            }

            if (imagem3 != null && imagem3.Length > 0)
            {
                var caminhoImagem2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imagem3.FileName);
                using (var stream = new FileStream(caminhoImagem2, FileMode.Create))
                {
                    imagem3.CopyTo(stream);
                }
                produtos_.Imagem3 = "/images/" + imagem3.FileName;
            }


            var Produtos = _conexao.Produtos.Add(produtos_);
            _conexao.SaveChanges();
            TempData["Mensagem"] = "Produto cadastrado com sucesso!";
            return RedirectToAction("Index");
        }

        public IActionResult Produtos()
        {
            var produtos = _conexao.Produtos.ToList();
            return View(produtos);
        }

        public IActionResult Editar(int id)
        {
            var Produto = _conexao.Produtos.FirstOrDefault(Produto => Produto.Id == id);
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (Produto == null)
            {
                return NotFound();
            }

            return View(Produto);
        }

        [HttpPost]
        public IActionResult EditarProduto(Produtos produtoAtualizado, IFormFile imagem, IFormFile imagem2, IFormFile imagem3, int id)
        {
            produtoAtualizado.Validar();
            var produto = _conexao.Produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null)
            {
                return NotFound();
            }

            produto.Nome = produtoAtualizado.Nome;
            produto.Descricao = produtoAtualizado.Descricao;
            produto.Preco = produtoAtualizado.Preco;
            produto.Quantidade = produtoAtualizado.Quantidade;

            if (imagem != null && imagem.Length > 0)
            {
                var nomeArquivo = Path.GetFileName(imagem.FileName);
                var caminhoImagem = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", nomeArquivo);

                using (var stream = new FileStream(caminhoImagem, FileMode.Create))
                {
                    imagem.CopyTo(stream);
                }

                produto.Imagem = "/images/" + nomeArquivo;
            }
            if (imagem2 != null && imagem2.Length > 0)
            {
                var nomeArquivo2 = Path.GetFileName(imagem2.FileName);
                var caminhoImagem2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", nomeArquivo2);

                using (var stream = new FileStream(caminhoImagem2, FileMode.Create))
                {
                    imagem2.CopyTo(stream);
                }

                produto.Imagem2 = "/images/" + nomeArquivo2;
            }
            if (imagem3 != null && imagem3.Length > 0)
            {
                var nomeArquivo3 = Path.GetFileName(imagem3.FileName);
                var caminhoImagem3 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", nomeArquivo3);

                using (var stream = new FileStream(caminhoImagem3, FileMode.Create))
                {
                    imagem3.CopyTo(stream);
                }

                produto.Imagem3 = "/images/" + nomeArquivo3;
            }

            _conexao.SaveChanges();
            return RedirectToAction("Produtos");
        }


        public IActionResult BuscarProduto2(string filtroNome, string precoMin, string precoMax)
        {
            var produtosFiltrados = _conexao.Produtos.AsQueryable();

            if (!string.IsNullOrEmpty(filtroNome))
            {
                var nomeLower = filtroNome.ToLower();
                produtosFiltrados = produtosFiltrados.Where(p => EF.Functions.Like(p.Nome.ToLower(), $"%{nomeLower}%"));
            }

            if (decimal.TryParse(precoMin, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precoMinDecimal))
            {
                produtosFiltrados = produtosFiltrados
                    .Where(p => p.Preco >= precoMinDecimal * 100); 
            }

            if (decimal.TryParse(precoMax, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precoMaxDecimal))
            {
                produtosFiltrados = produtosFiltrados
                    .Where(p => p.Preco <= precoMaxDecimal * 100);
            }

            ViewBag.FiltroNome = filtroNome;
            ViewBag.PrecoMin = precoMin;
            ViewBag.PrecoMax = precoMax;

            return View("Produtos", produtosFiltrados.ToList());

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
