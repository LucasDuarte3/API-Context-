using Exo.WebApi.Contexts;
using Exo.WebApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace Exo.WebApi.Repositories
{
    public class UsuarioRepository
    {
        private readonly ExoContext _context;

        public UsuarioRepository(ExoContext context)
        {
            _context = context;
        }

        public List<Usuario> Listar()
        {
            return _context.Usuarios.ToList();
        }

        public Usuario BuscarPorId(int id)
        {
            return _context.Usuarios.Find(id);
        }

        public void Cadastrar(Usuario novoUsuario)
        {
            _context.Usuarios.Add(novoUsuario);
            _context.SaveChanges();
        }

        public void Atualizar(int id, Usuario usuarioAtualizado)
        {
            Usuario usuarioExistente = _context.Usuarios.Find(id);
            if (usuarioExistente != null)
            {
                usuarioExistente.Email = usuarioAtualizado.Email;
                usuarioExistente.Senha = usuarioAtualizado.Senha;

                _context.Usuarios.Update(usuarioExistente);
                _context.SaveChanges();
            }
        }

        public void Deletar(int id)
        {
            Usuario usuarioBuscado = _context.Usuarios.Find(id);
            if (usuarioBuscado != null)
            {
                _context.Usuarios.Remove(usuarioBuscado);
                _context.SaveChanges();
            }
        }
    }
}