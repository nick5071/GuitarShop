# GuitarShop com Registro e login feito com C# MVC ASP.NET usando ASP.NET Core Identity e Envio de Email

• Aplicativo de loja Virtual Web Crud feito com C# ASP.NET CORE

• Banco de dados SQL SERVER

• ASP.NET Core Identity
 
• .NET 8
 
• SMTP Gmail

• Entity Framework

• Razor Pages

• Bootstrap

# É obrigatorio ter o Banco de dados SQL SERVER
Comandos pacote NuGet (Entity framework)


  Importante: Mude o Server do DefaultConnection no appsettings.json, para o nome do servidor de sua máquina

  -Realizar os seguintes comandos ou criar o banco manualmente:
  
  • Add-migration Migrations (Criar banco de dados)
  
  • Update-Database (atualizar banco de dados)

# Importante:
Para alteração de senha por Envio de email na aba de Login você deve mudar o:
        "Username": "SeuEmail",
        "Password": "SenhaDoSeuEmail"

no appsettings.Development.json, coloque o email que deseja que envie o email para a redefinição de Senha para o usuario, para isso o email desejado deve ativar a verificação em duas etapas e ir em https://myaccount.google.com/apppasswords, Na seção "Selecionar app", escolha "Mail". Vai aparecer uma senha como: abcd efgh ijkl mnop (sem espaços — copie inteira), e cole no "Password", isso porque o Gmail não aceita mais sua senha normal em conexões SMTP com apps externos. É obrigatório usar uma "senha de app" se você tem autenticação em 2 etapas (2FA) ativada
  
 
