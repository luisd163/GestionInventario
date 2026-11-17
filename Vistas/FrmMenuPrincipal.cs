using System;
using System.Drawing;
using System.Windows.Forms;
using Config;
using SistemaProveedores.Config;

namespace Vistas
{
    public sealed class FrmMenuPrincipal : Form
    {
        private readonly string nombreUsuario;
        private readonly string codigoRol;
        private bool confirmado;

        public bool CerroSesion { get; private set; }

        public FrmMenuPrincipal(string nombreUsuario, string codigoRol)
        {
            this.nombreUsuario = nombreUsuario;
            this.codigoRol = codigoRol;

            Text = "Sistema de Consulta de Productos y Proveedores";
            IsMdiContainer = true;
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 600);
            Font = new Font("Segoe UI", 9.5f);

            ConstruirMenu();
            ConstruirEstado();
        }

        private static string NombreRol(string codigo)
        {
            switch (codigo)
            {
                case "ADM": return "Administrador";
                case "TRD": return "Usuario Tradicional";
                case "ESP": return "Usuario Esporádico";
                default: return codigo;
            }
        }

        private static ToolStripMenuItem Item(string texto, EventHandler accion, bool habilitado = true)
        {
            var item = new ToolStripMenuItem(texto) { Enabled = habilitado };
            if (accion != null)
            {
                item.Click += accion;
            }

            return item;
        }

        private void ConstruirMenu()
        {
            // Matriz de permisos por rol.
            bool esAdmin = ContenedorDependencias.Auth.EsAdministrador();
            bool escribe = ContenedorDependencias.Auth.PuedeEscribirCatalogos();

            var entidades = new ToolStripMenuItem("&Entidades");
            entidades.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Categorías", (s, e) => Pendiente("Categorías")),
                Item("Productos", (s, e) => Pendiente("Productos")),
                Item("Proveedores", (s, e) => Pendiente("Proveedores")),
                new ToolStripSeparator(),
                Item("Usuarios", (s, e) => AbrirUsuariosBitacora(false), esAdmin)
            });

            var transacciones = new ToolStripMenuItem("&Transacciones") { Enabled = escribe };
            transacciones.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Catálogo de proveedor", (s, e) => Pendiente("Catálogo de proveedor"), escribe),
                Item("Clasificación de productos", (s, e) => Pendiente("Clasificación de productos"), escribe)
            });

            var reportes = new ToolStripMenuItem("&Reportes / Consultas");
            reportes.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Consultas del sistema", (s, e) => Pendiente("Consultas del sistema")),
                Item("Reportes filtrados", (s, e) => Pendiente("Reportes filtrados"))
            });

            var auditoria = new ToolStripMenuItem("&Auditoría") { Enabled = esAdmin };
            auditoria.DropDownItems.Add(Item("Bitácora de accesos", (s, e) => AbrirUsuariosBitacora(true), esAdmin));

            var salir = new ToolStripMenuItem("&Salir");
            salir.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Cerrar sesión", (s, e) => CerrarSesion()),
                Item("Salir de la aplicación", (s, e) => Close())
            });

            var menu = new MenuStrip();
            menu.Items.AddRange(new ToolStripItem[] { entidades, transacciones, reportes, auditoria, salir });
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private void ConstruirEstado()
        {
            var estado = new StatusStrip();
            estado.Items.Add(new ToolStripStatusLabel(
                "Usuario: " + nombreUsuario + "   |   Rol: " + NombreRol(codigoRol) + "   |   Ingreso: " + DateTime.Now.ToString("g")));
            Controls.Add(estado);
        }

        private void Pendiente(string modulo)
        {
            // Punto de extensión: reemplazar por la apertura del formulario hijo cuando exista.
            MessageBox.Show("El módulo \"" + modulo + "\" se entrega en la siguiente parte de la interfaz.",
                "En construcción", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AbrirUsuariosBitacora(bool bitacora)
        {
            try
            {
                foreach (Form hijo in MdiChildren)
                {
                    if (hijo is FrmUsuariosYBitacora existente)
                    {
                        existente.MostrarPestana(bitacora);
                        existente.Activate();
                        return;
                    }
                }

                var frm = new FrmUsuariosYBitacora(bitacora) { MdiParent = this };
                frm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo abrir la ventana", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CerrarSesion()
        {
            if (MessageBox.Show("¿Desea cerrar la sesión actual?", "Cerrar sesión",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            CerroSesion = true;
            confirmado = true;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel)
            {
                return;
            }

            if (!confirmado && e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("¿Desea salir de la aplicación?", "Salir",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (SesionUsuario.EstaAutenticado)
            {
                try
                {
                    // AuthLN registra BTiempoSalida y finaliza la sesión.
                    ContenedorDependencias.Auth.CerrarSesion();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo registrar la salida en la bitácora: " + ex.Message,
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
