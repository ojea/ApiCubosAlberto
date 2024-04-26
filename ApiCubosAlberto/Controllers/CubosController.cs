using ApiCoreCubos.Repositories;
using ApiCoreOAuthEmpleados.Helpers;
using ApiCubosAlberto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;

namespace ApiCoreCubos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CubosController : ControllerBase
    {
        private CubosRepository repo;
        private HelperActionServicesOAuth helper;

        public CubosController(CubosRepository repo, HelperActionServicesOAuth helper)
        {
            this.repo = repo;
            this.helper = helper;
        }
        [HttpGet]
        public async Task<ActionResult<List<Cubo>>> GetCubos()
        {
            return await this.repo.GetCubosAsync();
        }
        [HttpGet]
        [Route("[action]/{marca}")]

        public async Task<ActionResult<List<Cubo>>> GetCubosMarca(string marca)
        {
            List<Cubo> cubos = await this.repo.GetCubosMarcaAsync(marca);

            return cubos;
        }

        //[HttpPost]
        //[Route("[action]")]

        //public async Task<ActionResult> InsertUsuario(RegistroModel model)
        //{
        //    await this.repo.RegisterUsuarioAsync(model.Nombre, model.Email, model.Password);

        //    return Ok();

        //}

        [Authorize]
        [HttpGet]
        [Route("[action]")]

        public async Task<ActionResult<List<CompraCubos>>> ComprasUsuario()
        {
            string jsonUsuario = HttpContext.User.FindFirst(x => x.Type == "UserData").Value;

            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(jsonUsuario);

            List<CompraCubos> compras = await this.repo.GetComprasUsuarioAsync(usuario.IdUsuario);

            return compras;
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<Usuario>> PerfilUsuario()
        {
            //internamente, cuando recibimos el token, el usuairo es validado y almacena datos como HttpContext.User.Identity.IsAuthenticated. Como hemos incluido la Key de los claims, automaticamente tambien tenemos dichos claims como en las aplicaciones MCV
            Claim claim = HttpContext.User.FindFirst(x => x.Type == "UserData");
            //recuperamos el json del empleado 
            string jsonUsuario = claim.Value;
            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(jsonUsuario);

            return usuario;

        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Login(LoginModel model)
        {

            Usuario usuario = await this.repo.LoginUsuarioAsync(model.Email, model.Password);
            if (usuario == null)
            {
                return Unauthorized();
            }
            else
            {
                SigningCredentials credentials = new SigningCredentials(this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);
                string jsonUsuario = JsonConvert.SerializeObject(usuario);
                Claim[] informacion = new[]
                {
                    new Claim("UserData", jsonUsuario)
                };

                JwtSecurityToken token = new JwtSecurityToken(
                        claims: informacion,
                        issuer: this.helper.Issuer,
                        audience: this.helper.Audience,
                        signingCredentials: credentials,
                        expires: DateTime.UtcNow.AddMinutes(30),
                        notBefore: DateTime.UtcNow
                        );

                return Ok(
                    new
                    {
                        response = new JwtSecurityTokenHandler().WriteToken(token)
                    });
            }
        }
    }
}