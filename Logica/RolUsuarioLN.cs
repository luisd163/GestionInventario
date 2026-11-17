using System;
using System.Collections.Generic;
using Modelo;

namespace Logica
{
    public sealed class RolUsuarioLN
    {
        private readonly IRolUsuarioDatos rolDatos;

        public RolUsuarioLN(IRolUsuarioDatos rolDatos)
        {
            this.rolDatos = rolDatos ?? throw new ArgumentNullException(nameof(rolDatos));
        }

        public IList<RolUsuario> Listar()
        {
            ReglasLN.ExigirAdministrador();
            return rolDatos.Listar();
        }
    }
}