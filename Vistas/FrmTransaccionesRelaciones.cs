using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Modelo;
using SistemaProveedores.Config;

namespace Vistas
{
    public sealed class FrmTransaccionesRelaciones : Form
    {
        private sealed class ComboItem
        {
            public string Codigo { get; }
            public string Texto { get; }
            public ComboItem(string codigo, string texto) { Codigo = (codigo ?? "").Trim(); Texto = texto; }
            public override string ToString() { return Codigo + " - " + Texto; }
        }

        private readonly bool escribe;
        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly TabPage tpClasif = new TabPage("Clasificación de productos");
        private readonly TabPage tpCatalogo = new TabPage("Catálogo de proveedor");

        private readonly Dictionary<string, string> nomProducto = new Dictionary<string, string>();
        private readonly Dictionary<string, string> nomCategoria = new Dictionary<string, string>();
        private readonly Dictionary<string, string> catPrincipal = new Dictionary<string, string>();

        // Clasificación
        private readonly ComboBox cboClProducto = NuevoCombo();
        private readonly ComboBox cboClCategoria = NuevoCombo();
        private readonly Label lblPrincipal = new Label { AutoSize = true, ForeColor = Color.DimGray };
        private readonly DataGridView gridCl = new DataGridView();

        // Catálogo
        private readonly ComboBox cboCaProveedor = NuevoCombo();
        private readonly ComboBox cboCaProducto = NuevoCombo();
        private readonly NumericUpDown nudCantidad = new NumericUpDown { Minimum = 0, Maximum = 1000000, Width = 110 };
        private readonly NumericUpDown nudPrecio = new NumericUpDown
        {
            Minimum = 0, Maximum = 999999999, DecimalPlaces = 2, ThousandsSeparator = true, Width = 140
        };
        private readonly DataGridView gridCa = new DataGridView();
        private bool editandoCatalogo;

        public FrmTransaccionesRelaciones(int pestanaInicial = 0)
        {
            Text = "Transacciones: clasificación y catálogo de proveedor";
            ClientSize = new Size(940, 560);
            Font = new Font("Segoe UI", 9.5f);
            escribe = ContenedorDependencias.Auth.PuedeEscribirCatalogos();

            CargarMaestros();
            ConstruirClasificacion();
            ConstruirCatalogo();
            tabs.TabPages.Add(tpClasif);
            tabs.TabPages.Add(tpCatalogo);
            Controls.Add(tabs);
            MostrarPestana(pestanaInicial);
        }

        public void MostrarPestana(int indice)
        {
            if (indice >= 0 && indice < tabs.TabPages.Count) tabs.SelectedIndex = indice;
        }

        // ---------- Helpers ----------
        private static ComboBox NuevoCombo()
        {
            return new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        }

        private static Label Etiqueta(string texto, int x, int y)
        {
            return new Label { Text = texto, Location = new Point(x, y), AutoSize = true };
        }

        private static Button Boton(string texto, int x, int y, EventHandler accion, int ancho = 120)
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
            MessageBox.Show(mensaje, "Transacciones", MessageBoxButtons.OK, icono);
        }

        private static string Codigo(ComboBox cbo)
        {
            var item = cbo.SelectedItem as ComboItem;
            return item == null ? null : item.Codigo;
        }

        private static string Nombre(Dictionary<string, string> dic, string clave)
        {
            string v;
            return dic.TryGetValue((clave ?? "").Trim(), out v) ? v : "";
        }

        private static void Seleccionar(ComboBox cbo, string codigo)
        {
            codigo = (codigo ?? "").Trim();
            cbo.SelectedIndex = -1;
            foreach (ComboItem i in cbo.Items)
            {
                if (i.Codigo == codigo) { cbo.SelectedItem = i; break; }
            }
        }

        private bool Ejecutar(Action accion, string exito)
        {
            try
            {
                accion();
                if (exito != null) Aviso(exito, MessageBoxIcon.Information);
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                Aviso("Esa relación ya existe.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                Aviso("Operación rechazada por integridad referencial: verifique que el producto, proveedor y categoría existan.");
            }
            catch (Exception ex)
            {
                Aviso(ex.Message);
            }

            return false;
        }

        private void CargarMaestros()
        {
            try
            {
                foreach (Categoria c in ContenedorDependencias.Categorias.Listar())
                {
                    string cod = c.CCodigo.Trim();
                    nomCategoria[cod] = c.CNombre;
                    cboClCategoria.Items.Add(new ComboItem(cod, c.CNombre));
                }

                foreach (Producto p in ContenedorDependencias.Productos.Listar())
                {
                    string cod = p.PrCodigo.Trim();
                    nomProducto[cod] = p.PrNombre;
                    catPrincipal[cod] = (p.CategoriaProducto ?? "").Trim();
                    cboClProducto.Items.Add(new ComboItem(cod, p.PrNombre));
                    cboCaProducto.Items.Add(new ComboItem(cod, p.PrNombre));
                }

                foreach (Proveedor pr in ContenedorDependencias.Proveedores.Listar())
                {
                    cboCaProveedor.Items.Add(new ComboItem(pr.PCodigo, pr.PNombre));
                }
            }
            catch (Exception ex)
            {
                Aviso("No se pudieron cargar los datos maestros: " + ex.Message, MessageBoxIcon.Error);
            }
        }

        // ---------- Clasificación (M:N Producto - Categoría) ----------
        private void ConstruirClasificacion()
        {
            ConfigurarGrid(gridCl);
            var panel = new Panel { Dock = DockStyle.Top, Height = 130 };
            cboClProducto.SetBounds(15, 35, 320, 28);
            cboClCategoria.SetBounds(350, 35, 280, 28);
            cboClProducto.SelectedIndexChanged += (s, e) => CargarClasificacion();
            lblPrincipal.Location = new Point(15, 105);

            Button asignar = Boton("Asignar", 15, 70, (s, e) => Asignar());
            Button quitar = Boton("Quitar", 145, 70, (s, e) => Quitar());
            asignar.Enabled = quitar.Enabled = escribe;

            panel.Controls.AddRange(new Control[]
            {
                Etiqueta("Producto", 15, 12), cboClProducto,
                Etiqueta("Categoría a asignar", 350, 12), cboClCategoria,
                asignar, quitar, lblPrincipal
            });

            tpClasif.Controls.Add(gridCl);
            tpClasif.Controls.Add(panel);
            CargarClasificacion();
        }

        private void CargarClasificacion()
        {
            var t = new DataTable();
            t.Columns.Add("Producto");
            t.Columns.Add("Cód. categoría");
            t.Columns.Add("Categoría");
            lblPrincipal.Text = "";

            string pr = Codigo(cboClProducto);
            if (pr != null)
            {
                try
                {
                    foreach (ClasificacionProducto c in ContenedorDependencias.Clasificacion.ListarPorProducto(pr))
                    {
                        t.Rows.Add(c.Producto.Trim(), c.CategoriaProducto.Trim(), Nombre(nomCategoria, c.CategoriaProducto));
                    }

                    string principal;
                    if (catPrincipal.TryGetValue(pr, out principal) && principal.Length > 0)
                    {
                        lblPrincipal.Text = "Categoría principal del producto: " + principal + " - " + Nombre(nomCategoria, principal);
                    }
                }
                catch (Exception ex)
                {
                    Aviso(ex.Message);
                }
            }

            gridCl.DataSource = t;
            gridCl.ClearSelection();
        }

        private void Asignar()
        {
            string pr = Codigo(cboClProducto), cat = Codigo(cboClCategoria);
            if (pr == null || cat == null) { Aviso("Seleccione un producto y una categoría."); return; }
            if (Ejecutar(() => ContenedorDependencias.Clasificacion.Asignar(pr, cat), "Categoría asignada correctamente."))
            {
                CargarClasificacion();
            }
        }

        private void Quitar()
        {
            string pr = Codigo(cboClProducto);
            if (pr == null || gridCl.SelectedRows.Count == 0) { Aviso("Seleccione una asignación de la lista."); return; }
            string cat = Convert.ToString(gridCl.SelectedRows[0].Cells[1].Value);
            if (MessageBox.Show("¿Quitar la categoría " + cat + " del producto " + pr + "?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            if (Ejecutar(() => ContenedorDependencias.Clasificacion.Quitar(pr, cat), "Asignación eliminada correctamente."))
            {
                CargarClasificacion();
            }
        }

        // ---------- Catálogo de proveedor (M:N Proveedor - Producto) ----------
        private void ConstruirCatalogo()
        {
            ConfigurarGrid(gridCa);
            gridCa.CellClick += (s, e) => { if (e.RowIndex >= 0) CargarSeleccionCatalogo(e.RowIndex); };

            var panel = new Panel { Dock = DockStyle.Top, Height = 135 };
            cboCaProveedor.SetBounds(15, 35, 280, 28);
            cboCaProducto.SetBounds(310, 35, 280, 28);
            nudCantidad.Location = new Point(605, 35);
            nudPrecio.Location = new Point(730, 35);
            cboCaProveedor.SelectedIndexChanged += (s, e) => { LimpiarCatalogo(); CargarCatalogo(); };

            Button nuevo = Boton("Nuevo", 15, 80, (s, e) => LimpiarCatalogo());
            Button vincular = Boton("Vincular", 145, 80, (s, e) => Vincular());
            Button actualizar = Boton("Actualizar", 275, 80, (s, e) => ActualizarCatalogo());
            Button desvincular = Boton("Desvincular", 405, 80, (s, e) => Desvincular());
            foreach (Button b in new[] { nuevo, vincular, actualizar, desvincular }) b.Enabled = escribe;
            nudCantidad.Enabled = nudPrecio.Enabled = escribe;
            cboCaProducto.Enabled = escribe;

            panel.Controls.AddRange(new Control[]
            {
                Etiqueta("Proveedor", 15, 12), cboCaProveedor,
                Etiqueta("Producto", 310, 12), cboCaProducto,
                Etiqueta("Cantidad", 605, 12), nudCantidad,
                Etiqueta("Precio unitario", 730, 12), nudPrecio,
                nuevo, vincular, actualizar, desvincular
            });

            tpCatalogo.Controls.Add(gridCa);
            tpCatalogo.Controls.Add(panel);
            CargarCatalogo();
        }

        private void CargarCatalogo()
        {
            var t = new DataTable();
            t.Columns.Add("Cód. producto");
            t.Columns.Add("Producto");
            t.Columns.Add("Cantidad", typeof(int));
            t.Columns.Add("Precio", typeof(decimal));

            string prov = Codigo(cboCaProveedor);
            if (prov != null)
            {
                try
                {
                    foreach (CatalogoProveedor c in ContenedorDependencias.Proveedores.ObtenerCatalogo(prov))
                    {
                        t.Rows.Add(c.Producto.Trim(), Nombre(nomProducto, c.Producto), c.CaCantidad, c.CaPrecio);
                    }
                }
                catch (Exception ex)
                {
                    Aviso(ex.Message);
                }
            }

            gridCa.DataSource = t;
            gridCa.Columns["Precio"].DefaultCellStyle.Format = "N2";
            gridCa.ClearSelection();
        }

        private void CargarSeleccionCatalogo(int fila)
        {
            DataGridViewRow r = gridCa.Rows[fila];
            Seleccionar(cboCaProducto, Convert.ToString(r.Cells[0].Value));
            nudCantidad.Value = Math.Min(nudCantidad.Maximum, Math.Max(nudCantidad.Minimum, Convert.ToDecimal(r.Cells[2].Value)));
            nudPrecio.Value = Math.Min(nudPrecio.Maximum, Math.Max(nudPrecio.Minimum, Convert.ToDecimal(r.Cells[3].Value)));
            editandoCatalogo = true;
            cboCaProducto.Enabled = false; // La llave compuesta no se modifica.
        }

        private void LimpiarCatalogo()
        {
            editandoCatalogo = false;
            cboCaProducto.Enabled = escribe;
            cboCaProducto.SelectedIndex = -1;
            nudCantidad.Value = 0;
            nudPrecio.Value = 0;
            gridCa.ClearSelection();
        }

        private bool ValidarSeleccionCatalogo(out string prov, out string prod)
        {
            prov = Codigo(cboCaProveedor);
            prod = Codigo(cboCaProducto);
            if (prov == null || prod == null)
            {
                Aviso("Seleccione un proveedor y un producto.");
                return false;
            }

            return true;
        }

        private void Vincular()
        {
            string prov, prod;
            if (editandoCatalogo) { Aviso("Pulse \"Nuevo\" para vincular un producto."); return; }
            if (!ValidarSeleccionCatalogo(out prov, out prod)) return;
            int cant = (int)nudCantidad.Value;
            decimal precio = nudPrecio.Value;
            if (Ejecutar(() => ContenedorDependencias.Proveedores.AgregarProductoCatalogo(prov, prod, cant, precio),
                    "Producto vinculado al proveedor."))
            {
                LimpiarCatalogo();
                CargarCatalogo();
            }
        }

        private void ActualizarCatalogo()
        {
            string prov, prod;
            if (!editandoCatalogo) { Aviso("Seleccione una fila del catálogo."); return; }
            if (!ValidarSeleccionCatalogo(out prov, out prod)) return;
            int cant = (int)nudCantidad.Value;
            decimal precio = nudPrecio.Value;
            if (Ejecutar(() => ContenedorDependencias.Proveedores.ActualizarProductoCatalogo(prov, prod, cant, precio),
                    "Catálogo actualizado correctamente."))
            {
                LimpiarCatalogo();
                CargarCatalogo();
            }
        }

        private void Desvincular()
        {
            string prov, prod;
            if (!editandoCatalogo) { Aviso("Seleccione una fila del catálogo."); return; }
            if (!ValidarSeleccionCatalogo(out prov, out prod)) return;
            if (MessageBox.Show("¿Desvincular el producto " + prod + " del proveedor " + prov + "?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            if (Ejecutar(() => ContenedorDependencias.Proveedores.EliminarProductoCatalogo(prov, prod),
                    "Producto desvinculado."))
            {
                LimpiarCatalogo();
                CargarCatalogo();
            }
        }
    }
}