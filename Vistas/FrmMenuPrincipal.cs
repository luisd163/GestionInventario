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
                case "001": return "Administrador";
                case "002": return "Tradicional";
                case "003": return "Esparadico";
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
                Item("Categorías", (s, e) => AbrirEntidades(0)),
                Item("Productos", (s, e) => AbrirEntidades(1)),
                Item("Proveedores", (s, e) => AbrirEntidades(2)),
                new ToolStripSeparator(),
                Item("Usuarios", (s, e) => AbrirUsuariosBitacora(false), esAdmin)
            });

            var transacciones = new ToolStripMenuItem("&Transacciones") { Enabled = escribe };
            transacciones.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Catálogo de proveedor", (s, e) => AbrirTransacciones(1), escribe),
                Item("Clasificación de productos", (s, e) => AbrirTransacciones(0), escribe)
            });

            var reportes = new ToolStripMenuItem("&Reportes / Consultas");
            reportes.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Consultas y reportes", (s, e) => AbrirConsultas())
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

        private void AbrirEntidades(int pestana)
        {
            try
            {
                foreach (Form hijo in MdiChildren)
                {
                    if (hijo is FrmEntidadesCatalogo existente)
                    {
                        existente.MostrarPestana(pestana);
                        existente.Activate();
                        return;
                    }
                }

                var frm = new FrmEntidadesCatalogo(pestana) { MdiParent = this };
                frm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo abrir la ventana", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AbrirTransacciones(int pestana)
        {
            try
            {
                foreach (Form hijo in MdiChildren)
                {
                    if (hijo is FrmTransaccionesRelaciones existente)
                    {
                        existente.MostrarPestana(pestana);
                        existente.Activate();
                        return;
                    }
                }

                var frm = new FrmTransaccionesRelaciones(pestana) { MdiParent = this };
                frm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo abrir la ventana", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AbrirConsultas()
        {
            try
            {
                foreach (Form hijo in MdiChildren)
                {
                    if (hijo is FrmConsultasYReportes existente)
                    {
                        existente.Activate();
                        return;
                    }
                }

                var frm = new FrmConsultasYReportes { MdiParent = this };
                frm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo abrir la ventana", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
