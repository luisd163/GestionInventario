using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Config;
using Modelo;
using SistemaProveedores.Config;

namespace Vistas
{
    public sealed class FrmUsuariosYBitacora : Form
    {
        // RolUsuario no expone sus propiedades (son privadas), por eso los roles se definen aquí.
        private sealed class RolItem
        {
            public string Codigo { get; }
            public string Nombre { get; }
            public RolItem(string codigo, string nombre) { Codigo = codigo; Nombre = nombre; }
            public override string ToString() { return Nombre; }
        }

        private static readonly RolItem[] Roles =
        {
            new RolItem("001", "Administrador"),
            new RolItem("002", "Tradicional"),
            new RolItem("003", "Esparadico")
        };

        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly TabPage tpUsuarios = new TabPage("Usuarios");
        private readonly TabPage tpBitacora = new TabPage("Bitácora de accesos");

        private readonly TextBox txtCodigo = new TextBox { MaxLength = 3 };
        private readonly TextBox txtNombre = new TextBox { MaxLength = 50 };
        private readonly TextBox txtContrasena = new TextBox { MaxLength = 15, UseSystemPasswordChar = true };
        private readonly ComboBox cboRol = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DataGridView gridUsuarios = new DataGridView();

        private readonly DateTimePicker dtpDesde = new DateTimePicker { ShowCheckBox = true, Checked = false, Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker dtpHasta = new DateTimePicker { ShowCheckBox = true, Checked = false, Format = DateTimePickerFormat.Short };
        private readonly DataGridView gridBitacora = new DataGridView();

        private IList<Usuario> usuarios = new List<Usuario>();
        private DataTable tablaBitacora;

        public FrmUsuariosYBitacora(bool abrirBitacora = false)
        {
            if (!ContenedorDependencias.Auth.EsAdministrador())
            {
                throw new UnauthorizedAccessException("Esta ventana es exclusiva del rol Administrador.");
            }

            Text = "Usuarios y Bitácora";
            ClientSize = new Size(900, 560);
            Font = new Font("Segoe UI", 9.5f);

            ConstruirUsuarios();
            ConstruirBitacora();
            tabs.TabPages.Add(tpUsuarios);
            tabs.TabPages.Add(tpBitacora);
            Controls.Add(tabs);

            CargarUsuarios();
            CargarBitacora();
            MostrarPestana(abrirBitacora);
        }

        public void MostrarPestana(bool bitacora)
        {
            tabs.SelectedTab = bitacora ? tpBitacora : tpUsuarios;
        }

        // ---------- Helpers de construcción ----------
        private static Label Etiqueta(string texto, int x, int y)
        {
            return new Label { Text = texto, Location = new Point(x, y), AutoSize = true };
        }

        private static Button Boton(string texto, int x, int y, EventHandler accion, int ancho = 110)
        {
            var b = new Button { Text = texto, Location = new Point(x, y), Size = new Size(ancho, 30) };
            b.Click += accion;
            return b;
        }

        private static void ConfigurarGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.MultiSelect = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BackgroundColor = SystemColors.Window;
        }

        private static void Aviso(string mensaje, MessageBoxIcon icono = MessageBoxIcon.Warning)
        {
            MessageBox.Show(mensaje, "Usuarios y Bitácora", MessageBoxButtons.OK, icono);
        }

        // ---------- Pestaña Usuarios ----------
        private void ConstruirUsuarios()
        {
            ConfigurarGrid(gridUsuarios);
            gridUsuarios.CellClick += (s, e) => { if (e.RowIndex >= 0) CargarSeleccion(e.RowIndex); };

            var panel = new Panel { Dock = DockStyle.Top, Height = 150 };

            txtCodigo.Location = new Point(15, 35); txtCodigo.Width = 80;
            txtNombre.Location = new Point(110, 35); txtNombre.Width = 260;
            txtContrasena.Location = new Point(385, 35); txtContrasena.Width = 150;
            cboRol.Location = new Point(550, 35); cboRol.Width = 190;
            cboRol.Items.AddRange(Roles);

            var pista = Etiqueta("Para actualizar un usuario debe ingresar nuevamente su contraseña. Solo puede existir un Administrador.", 15, 118);
            pista.ForeColor = Color.DimGray;

            panel.Controls.AddRange(new Control[]
            {
                Etiqueta("Código (3)", 15, 15), txtCodigo,
                Etiqueta("Nombre", 110, 15), txtNombre,
                Etiqueta("Contraseña (máx. 15)", 385, 15), txtContrasena,
                Etiqueta("Rol", 550, 15), cboRol,
                Boton("Nuevo", 15, 75, (s, e) => LimpiarUsuario()),
                Boton("Crear", 135, 75, (s, e) => CrearUsuario()),
                Boton("Actualizar", 255, 75, (s, e) => ActualizarUsuario()),
                Boton("Eliminar", 375, 75, (s, e) => EliminarUsuario()),
                pista
            });

            tpUsuarios.Controls.Add(gridUsuarios); // Fill primero, luego el panel superior.
            tpUsuarios.Controls.Add(panel);
        }

        private static string NombreRol(string codigo)
        {
            string c = (codigo ?? "").Trim();
            foreach (RolItem r in Roles)
            {
                if (r.Codigo == c) return r.Nombre;
            }

            return c;
        }

        private void CargarUsuarios()
        {
            try
            {
                usuarios = ContenedorDependencias.Usuarios.Listar();
                var tabla = new DataTable();
                tabla.Columns.Add("Código");
                tabla.Columns.Add("Nombre");
                tabla.Columns.Add("Rol");
                tabla.Columns.Add("RolCodigo");
                foreach (Usuario u in usuarios)
                {
                    tabla.Rows.Add((u.UCodigo ?? "").Trim(), u.UNombre, NombreRol(u.RolUsuario), (u.RolUsuario ?? "").Trim());
                }

                gridUsuarios.DataSource = tabla;
                gridUsuarios.Columns["RolCodigo"].Visible = false;
                gridUsuarios.ClearSelection();
            }
            catch (Exception ex)
            {
                Aviso("No se pudieron cargar los usuarios: " + ex.Message, MessageBoxIcon.Error);
            }
        }

        private void CargarSeleccion(int fila)
        {
            DataGridViewRow r = gridUsuarios.Rows[fila];
            txtCodigo.Text = Convert.ToString(r.Cells["Código"].Value);
            txtNombre.Text = Convert.ToString(r.Cells["Nombre"].Value);
            txtContrasena.Clear();
            string rol = Convert.ToString(r.Cells["RolCodigo"].Value);
            cboRol.SelectedIndex = -1;
            foreach (RolItem item in cboRol.Items)
            {
                if (item.Codigo == rol) { cboRol.SelectedItem = item; break; }
            }

            txtCodigo.ReadOnly = true; // La llave no se modifica al actualizar.
        }

        private void LimpiarUsuario()
        {
            txtCodigo.ReadOnly = false;
            txtCodigo.Clear();
            txtNombre.Clear();
            txtContrasena.Clear();
            cboRol.SelectedIndex = -1;
            gridUsuarios.ClearSelection();
            txtCodigo.Focus();
        }

        private string RolSeleccionado()
        {
            var item = cboRol.SelectedItem as RolItem;
            if (item == null) throw new ArgumentException("Seleccione un rol.");
            return item.Codigo;
        }

        private void ValidarAdministradorUnico(string codigo, string rol)
        {
            if (rol != "001") return;
            foreach (Usuario u in usuarios)
            {
                if ((u.RolUsuario ?? "").Trim() == "001" && (u.UCodigo ?? "").Trim() != codigo.Trim())
                {
                    throw new InvalidOperationException("Ya existe un usuario Administrador; solo puede haber uno.");
                }
            }
        }

        private void Ejecutar(Action accion, string exito)
        {
            try
            {
                accion();
                CargarUsuarios();
                LimpiarUsuario();
                Aviso(exito, MessageBoxIcon.Information);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                Aviso("Ya existe un usuario con ese código.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                Aviso("El usuario tiene registros en la bitácora y no puede eliminarse, o el rol no es válido.");
            }
            catch (Exception ex)
            {
                Aviso(ex.Message);
            }
        }

        private void CrearUsuario()
        {
            if (txtCodigo.ReadOnly) { Aviso("Pulse \"Nuevo\" para crear un usuario."); return; }
            Ejecutar(() =>
            {
                string rol = RolSeleccionado();
                ValidarAdministradorUnico(txtCodigo.Text, rol);
                ContenedorDependencias.Usuarios.Crear(txtNombre.Text, txtCodigo.Text, txtContrasena.Text, rol);
            }, "Usuario creado correctamente.");
        }

        private void ActualizarUsuario()
        {
            if (!txtCodigo.ReadOnly) { Aviso("Seleccione un usuario de la lista."); return; }
            Ejecutar(() =>
            {
                string rol = RolSeleccionado();
                string codigo = txtCodigo.Text.Trim();
                if (codigo == SesionUsuario.CodigoUsuario && rol != SesionUsuario.CodigoRol)
                {
                    throw new InvalidOperationException("No puede cambiar el rol de su propia cuenta.");
                }

                ValidarAdministradorUnico(codigo, rol);
                ContenedorDependencias.Usuarios.Actualizar(txtNombre.Text, codigo, txtContrasena.Text, rol);
            }, "Usuario actualizado correctamente.");
        }

        private void EliminarUsuario()
        {
            if (!txtCodigo.ReadOnly) { Aviso("Seleccione un usuario de la lista."); return; }
            string codigo = txtCodigo.Text.Trim();
            if (codigo == SesionUsuario.CodigoUsuario) { Aviso("No puede eliminar su propia cuenta."); return; }
            if (MessageBox.Show("¿Eliminar el usuario " + codigo + "?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Ejecutar(() => ContenedorDependencias.Usuarios.Eliminar(codigo), "Usuario eliminado correctamente.");
        }

        // ---------- Pestaña Bitácora ----------
        private void ConstruirBitacora()
        {
            ConfigurarGrid(gridBitacora);

            var panel = new Panel { Dock = DockStyle.Top, Height = 70 };
            dtpDesde.Location = new Point(15, 30); dtpDesde.Width = 130;
            dtpHasta.Location = new Point(160, 30); dtpHasta.Width = 130;
            panel.Controls.AddRange(new Control[]
            {
                Etiqueta("Desde (opcional)", 15, 8), dtpDesde,
                Etiqueta("Hasta (opcional)", 160, 8), dtpHasta,
                Boton("Consultar", 310, 27, (s, e) => CargarBitacora()),
                Boton("Exportar PDF", 430, 27, (s, e) => ExportarBitacora(), 120)
            });

            tpBitacora.Controls.Add(gridBitacora);
            tpBitacora.Controls.Add(panel);
        }

        private void CargarBitacora()
        {
            try
            {
                DateTime? desde = dtpDesde.Checked ? dtpDesde.Value.Date : (DateTime?)null;
                DateTime? hasta = dtpHasta.Checked ? dtpHasta.Value.Date : (DateTime?)null;
                tablaBitacora = ContenedorDependencias.Bitacora.Listar(desde, hasta);
                gridBitacora.DataSource = tablaBitacora;

                RenombrarColumna("BCodigo", "Código");
                RenombrarColumna("BTiempoIngreso", "Ingreso");
                RenombrarColumna("BTiempoSalida", "Salida");
                RenombrarColumna("Usuario", "Cód. usuario");
                RenombrarColumna("UNombre", "Nombre de usuario");
            }
            catch (Exception ex)
            {
                Aviso("No se pudo consultar la bitácora: " + ex.Message, MessageBoxIcon.Error);
            }
        }

        private void RenombrarColumna(string nombre, string titulo)
        {
            if (gridBitacora.Columns.Contains(nombre))
            {
                gridBitacora.Columns[nombre].HeaderText = titulo;
            }
        }

        private void ExportarBitacora()
        {
            if (tablaBitacora == null || tablaBitacora.Rows.Count == 0)
            {
                Aviso("No hay registros para exportar.");
                return;
            }

            using (var dlg = new SaveFileDialog { Filter = "Archivo PDF (*.pdf)|*.pdf", FileName = "Bitacora.pdf" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    ContenedorDependencias.Reportes.ExportarPdf(tablaBitacora, "Bitácora de accesos", dlg.FileName);
                    Aviso("Reporte exportado correctamente.", MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Aviso("No se pudo exportar: " + ex.Message, MessageBoxIcon.Error);
                }
            }
        }
    }
}
