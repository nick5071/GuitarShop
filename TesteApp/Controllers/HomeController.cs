using System.Diagnostics;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using GuitarShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesteApp.Models;
using System.Text.RegularExpressions;

namespace TesteApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly Conexao _conexao;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public HomeController(Conexao conexao, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _conexao = conexao;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(Registro model)
        {
            if(model.Email.Length > 100)
            {
                TempData["Erro"] = "O email deve ter no máximo 100 caracteres";
                return RedirectToAction("Registro");
            }
            var usuarioExistente = await _userManager.FindByEmailAsync(model.Email);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError(string.Empty, "Este e-mail já está cadastrado.");
                return View("Registro", model);
            }
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    TempData["Mensagem"] = "Registro realizado com sucesso!";
                    return RedirectToAction("Registro");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View("Registro",model);
        }

        [Authorize]
        public IActionResult PaginaUsuario()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logar(Login model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "E-mail não encontrado.");
                    return View("Login", model);
                }

                var senhaCorreta = await _userManager.CheckPasswordAsync(user, model.Password);

                if (!senhaCorreta)
                {
                    ModelState.AddModelError(string.Empty, "Senha incorreta.");
                    return View("Login", model);
                }


                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, 
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Produtos");
                }

                ModelState.AddModelError(string.Empty, "Erro ao realizar login.");
            }

            return View("Login", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(string senhaAtual, string novaSenha, string confirmarSenha)
        {
            if (novaSenha != confirmarSenha)
            {
                TempData["Erro"] = "A nova senha e a confirmação não coincidem.";
                return RedirectToAction("PaginaUsuario");
            }

            var user = await _userManager.GetUserAsync(User);

            var resultado = await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);

            if (resultado.Succeeded)
            {
                TempData["Sucesso"] = "Senha atualizada com sucesso.";
                await _signInManager.RefreshSignInAsync(user); 
            }
            else
            {
                TempData["Erro"] = string.Join(", ", resultado.Errors.Select(e => e.Description));
            }

            return RedirectToAction("PaginaUsuario");
        }



        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult AcessoNegado()
        {
            return View();
        }

        [Authorize]
        public IActionResult Detalhes(int Id)
        {
            var Produtos = _conexao.Produtos.FirstOrDefault(p => p.Id == Id);

            if (Produtos == null)
            {
                return NotFound();
            }

            return View(Produtos);
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize]
        public IActionResult Produtos()
        {
            var produtos = _conexao.Produtos.ToList();
            return View(produtos);
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        [Authorize]
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

        private bool EnviarEmail(string destino, string assunto, string corpoHtml)
        {
            try
            {
                var smtpHost = _config["Smtp:Host"];
                var smtpPort = int.Parse(_config["Smtp:Port"]);
                var smtpUser = _config["Smtp:Username"];
                var smtpPass = _config["Smtp:Password"];

                var smtp = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                };

                var mensagem = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = assunto,
                    Body = corpoHtml,
                    IsBodyHtml = true
                };
                mensagem.To.Add(destino);

                smtp.Send(mensagem);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar email: " + ex.Message);
                return false;
            }
        }

        [HttpGet]
        public IActionResult EsqueciSenha()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RedefinirSenha(string email, string token)
        {
            if (email == null || token == null)
            {
                return BadRequest("Parâmetros inválidos.");
            }

            var model = new RedefinirSenhaViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NovaSenha);

            if (result.Succeeded)
            {
                TempData["Mensagem"] = "Senha alterada com sucesso!";
                return RedirectToAction("Login");
            }

            foreach (var erro in result.Errors)
            {
                ModelState.AddModelError(string.Empty, erro.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EsqueciSenha(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Mensagem"] = "Se o e-mail existir, um link foi enviado.";
                return RedirectToAction("Login");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("RedefinirSenha", "Home", new { email, token }, Request.Scheme);

            var resultado = EnviarEmail(email, "Redefinição de Senha", $"Clique no link para redefinir sua senha: <a href='{link}'>Redefinir Senha</a>");

            if (resultado)
                TempData["Mensagem"] = "E-mail enviado com sucesso.";
            else
                TempData["Erro"] = "Falha ao enviar o e-mail.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult PaginaAdmin()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarUsuario(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Erro"] = "Informe um e-mail válido.";
                return View("PaginaAdmin");
            }

            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return View("PaginaAdmin");
            }

            var roles = await _userManager.GetRolesAsync(usuario);
            ViewBag.Roles = roles;

            return View("PaginaAdmin", usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletarUsuario(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("PaginaAdmin");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                TempData["Erro"] = "Não é permitido excluir administradores.";
                return RedirectToAction("PaginaAdmin");
            }

            await _userManager.DeleteAsync(user);
            TempData["Sucesso"] = "Usuário deletado com sucesso.";
            return RedirectToAction("PaginaAdmin");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AtualizarDadosUsuario(string nomeCompleto, string telefone)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(telefone))
            {
                if (telefone.Length < 14 || !Regex.IsMatch(telefone, @"^\(\d{2}\) \d{5}-\d{4}$"))
                {
                    TempData["Erro"] = "Número de telefone inválido. Use o formato (99) 99999-9999.";
                    return RedirectToAction("PaginaUsuario");
                }
            }

            if (!string.IsNullOrWhiteSpace(nomeCompleto))
            {
                if (nomeCompleto.Length > 100)
                {
                    TempData["Erro"] = "O nome completo deve ter no máximo 100 caracteres.";
                    return RedirectToAction("PaginaUsuario");
                }
            }

            bool alterado = false;

            if (!string.IsNullOrWhiteSpace(telefone) && telefone != user.PhoneNumber)
            {
                user.PhoneNumber = telefone;
                alterado = true;
            }

            if (!string.IsNullOrWhiteSpace(nomeCompleto) && nomeCompleto != user.UserName)
            {
                user.UserName = nomeCompleto;
                alterado = true;
            }

            if (alterado)
            {
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Sucesso"] = "Dados atualizados!";
                    return RedirectToAction("PaginaUsuario");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                TempData["Sucesso"] = "Nenhuma alteração detectada.";
            }

            return View("PaginaUsuario");
        }

        [Authorize]
        public async Task<IActionResult> DeletarUsuarioLogadoAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                TempData["Sucesso"] = "Conta excluída com sucesso.";
                return RedirectToAction("Login"); 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            TempData["Erro"] = "Erro ao excluir conta.";
            return RedirectToAction("PaginaUsuario");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
