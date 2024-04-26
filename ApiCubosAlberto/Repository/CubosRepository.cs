using ApiCubosAlberto.Data;
using ApiCubosAlberto.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ApiCoreCubos.Repositories
{
    public class CubosRepository
    {
        private CubosContext context;

        public CubosRepository(CubosContext context)
        {
            this.context = context;
        }

        public async Task<List<Cubo>> GetCubosAsync()
        {
            List<Cubo> cubos = await this.context.Cubos.ToListAsync();

            return cubos;
        }

        public async Task<List<Cubo>> GetCubosMarcaAsync(string marca)
        {
            List<Cubo> cubos = await this.context.Cubos.Where(x => x.Marca == marca).ToListAsync();

            return cubos;
        }

        public async Task RegisterUsuarioAsync(string nombre, string email, string password)
        {
            Usuario user = new Usuario();
            user.IdUsuario = await this.GetMaxIdUsuarioAsync();
            user.Nombre = nombre;
            user.Email = email;
            user.Password = password;
            user.Imagen = "";

            this.context.Usuarios.Add(user);
            await this.context.SaveChangesAsync();

        }

        private async Task<int> GetMaxIdUsuarioAsync()
        {
            if (this.context.Usuarios.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.Usuarios.MaxAsync(z => z.IdUsuario) + 1;
            }
        }

        public async Task<Usuario> LoginUsuarioAsync(string email, string password)
        {
            return await this.context.Usuarios.Where(x => x.Email == email && x.Password == password).FirstOrDefaultAsync();
        }

        public async Task<List<CompraCubos>> GetComprasUsuarioAsync(int idUsuario)
        {
            return await this.context.CompraCubos.Where(z => z.IdUsuario == idUsuario).ToListAsync();
        }
    }
}