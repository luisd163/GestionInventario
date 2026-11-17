using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Modelo;
using SistemaProveedores.Config;

namespace Vistas
{
    public sealed class FrmEntidadesCatalogo : Form
    {
        private sealed class ComboItem
        {
            public string Codigo { get; }
            public string Texto { get; }
            public ComboItem(string codigo, string texto) { Codigo = (codigo ?? "").Trim(); Texto = texto; }
            public override string ToString() { return Codigo + " - " + Texto; }
        }

        private sealed class Campo
        {
            public string Etiqueta { get; }
            public int Ancho { get; }
            public Control Control { get; }

            private Campo(string etiqueta, int ancho, Control control)
            {
                Etiqueta = etiqueta; Ancho = ancho; Control = control;
            }

            public static Campo Texto(string etiqueta, int ancho, int max)
            {
                return new Campo(etiqueta, ancho, new TextBox { MaxLength = max });
            }

            public static Campo Lista(string etiqueta, int ancho)
            {
                return new Campo(etiqueta, ancho, new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
            }

            public ComboBox Combo { get { return Control as ComboBox; } }

            public string Valor
            {
                get
                {
                    var c = Control as ComboBox;
                    if (c != null)
                    {
                        var item = c.SelectedItem as ComboItem;
                        return item == null ? "" : item.Codigo;
                    }

                    return Control.Text.Trim();
                }
            }

            public void Poner(string valor)
            {
                valor = (valor ?? "").Trim();
                var c = Control as ComboBox;
                if (c == null) { Control.Text = valor; return; }
                c.SelectedIndex = -1;
                foreach (ComboItem i in c.Items)
                {
                    if (i.Codigo == valor) { c.SelectedItem = i; break; }
                }
            }

            public void SoloLectura(bool valor)
            {
                var t = Control as TextBox;
                if (t != null) t.ReadOnly = valor; else Control.Enabled = !valor;
            }
        }

        private sealed class PestanaCrud
        {
            public TabPage Pagina { get; }
            private readonly Campo[] campos;
            private readonly Func<DataTable> listar;
            private readonly Func<string, DataTable> buscar;
            private readonly Action<string[]> crear;
            private readonly Action<string[]> actualizar;
            private readonly Action<string> eliminar;
            private readonly Action alCambiar;
            private readonly bool soloLectura;
            private readonly DataGridView grid = new DataGridView();
            private readonly TextBox txtBuscar = new TextBox { MaxLength = 3, Width = 60 };
            private bool editando;

            public PestanaCrud(string titulo, Campo[] campos, Func<DataTable> listar, Func<string, DataTable> buscar,
                Action<string[]> crear, Action<string[]> actualizar, Action<string> eliminar, bool escribe, Action alCambiar)
            {
                this.campos = campos; this.listar = listar; this.buscar = buscar;
                this.crear = crear; this.actualizar = actualizar; this.eliminar = eliminar;
                this.alCambiar = alCambiar; soloLectura = !escribe;

                Pagina = new TabPage(titulo);
                grid.Dock = DockStyle.Fill;
                grid.ReadOnly = true;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.MultiSelect = false;
                grid.RowHeadersVisible = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grid.BackgroundColor = SystemColors.Window;
                grid.CellClick += (s, e) => { if (e.RowIndex >= 0) CargarSeleccion(e.RowIndex); };

                var panel = new Panel { Dock = DockStyle.Top, Height = 125 };
                int x = 15;
                foreach (Campo c in campos)
                {
                    panel.Controls.Add(Etiqueta(c.Etiqueta, x, 12));
                    c.Control.Location = new Point(x, 32);
                    c.Control.Width = c.Ancho;
                    panel.Controls.Add(c.Control);
                    x += c.Ancho + 15;
                }

                Button btnNuevo = Boton("Nuevo", 15, 75, (s, e) => Limpiar());
                Button btnAgregar = Boton("Agregar", 135, 75, (s, e) => Agregar());
                Button btnEditar = Boton("Editar", 255, 75, (s, e) => Editar());
                Button btnBorrar = Boton("Borrar", 375, 75, (s, e) => Borrar());
                foreach (Button b in new[] { btnNuevo, btnAgregar, btnEditar, btnBorrar }) b.Enabled = !soloLectura;

                txtBuscar.Location = new Point(590, 78);
                panel.Controls.AddRange(new Control[]
                {
                    btnNuevo, btnAgregar, btnEditar, btnBorrar,
                    Etiqueta("Código:", 530, 82), txtBuscar,
                    Boton("Consultar", 660, 75, (s, e) => Consultar(), 100),
                    Boton("Ver todos", 770, 75, (s, e) => { txtBuscar.Clear(); Cargar(); }, 100)
                });

                if (soloLectura)
                {
                    var aviso = Etiqueta("Modo solo lectura: su rol no permite crear, editar ni eliminar.", 15, 105);
                    aviso.ForeColor = Color.DimGray;
                    panel.Controls.Add(aviso);
                    foreach (Campo c in campos) c.SoloLectura(true);
                }

                Pagina.Controls.Add(grid);
                Pagina.Controls.Add(panel);
            }

            public void Cargar()
            {
                Intentar(() => Mostrar(listar()));
            }

            private void Mostrar(DataTable tabla)
            {
                grid.DataSource = tabla;
                grid.ClearSelection();
            }

            private void Intentar(Action accion)
            {
                try { accion(); }
                catch (Exception ex) { Aviso(ex.Message); }
            }

            private void CargarSeleccion(int fila)
            {
                DataGridViewRow r = grid.Rows[fila];
                for (int i = 0; i < campos.Length; i++)
                {
                    campos[i].Poner(Convert.ToString(r.Cells[i].Value));
                }

                editando = true;
                campos[0].SoloLectura(true); // La llave no se modifica.
            }

            private void Limpiar()
            {
                editando = false;
                foreach (Campo c in campos) c.Poner("");
                campos[0].SoloLectura(soloLectura);
                txtBuscar.Clear();
                grid.ClearSelection();
                campos[0].Control.Focus();
            }

            private string[] Valores()
            {
                var v = new string[campos.Length];
                for (int i = 0; i < campos.Length; i++) v[i] = campos[i].Valor;
                return v;
            }

            private void Consultar()
            {
                string codigo = txtBuscar.Text.Trim();
                if (codigo.Length == 0) { Cargar(); return; }
                Intentar(() =>
                {
                    DataTable t = buscar(codigo);
                    Mostrar(t);
                    if (t.Rows.Count == 0) Aviso("No se encontró un registro con código " + codigo + ".", MessageBoxIcon.Information);
                    else CargarSeleccion(0);
                });
            }

            private void Agregar()
            {
                if (editando) { Aviso("Pulse \"Nuevo\" para agregar un registro."); return; }
                Ejecutar(() => crear(Valores()), "Registro agregado correctamente.");
            }

            private void Editar()
            {
                if (!editando) { Aviso("Seleccione un registro de la lista."); return; }
                Ejecutar(() => actualizar(Valores()), "Registro actualizado correctamente.");
            }

            private void Borrar()
            {
                if (!editando) { Aviso("Seleccione un registro de la lista."); return; }
                string codigo = campos[0].Valor;
                if (MessageBox.Show("¿Eliminar el registro " + codigo + "?", "Confirmar",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                Ejecutar(() => eliminar(codigo), "Registro eliminado correctamente.");
            }

            private void Ejecutar(Action accion, string exito)
            {
                try
                {
                    accion();
                    if (alCambiar != null) alCambiar();
                    Mostrar(listar());
                    Limpiar();
                    Aviso(exito, MessageBoxIcon.Information);
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    Aviso("Ya existe un registro con ese código.");
                }
                catch (SqlException ex) when (ex.Number == 547)
                {
                    Aviso("Operación rechazada por integridad referencial: el registro está relacionado con otros datos o la referencia indicada no existe.");
                }
                catch (Exception ex)
                {
                    Aviso(ex.Message);
                }
            }
        }

        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly Campo campoCategoria = Campo.Lista("Categoría", 190);

        public FrmEntidadesCatalogo(int pestanaInicial = 0)
        {
            Text = "Categorías, Productos y Proveedores";
            ClientSize = new Size(940, 560);
            Font = new Font("Segoe UI", 9.5f);

            bool escribe = ContenedorDependencias.Auth.PuedeEscribirCatalogos();
            RecargarCategorias();

            var categorias = new PestanaCrud("Categorías",
                new[] { Campo.Texto("Código (3)", 80, 3), Campo.Texto("Nombre (máx. 30)", 300, 30) },
                ListarCategorias, BuscarCategoria,
                v => ContenedorDependencias.Categorias.Crear(v[0], v[1]),
                v => ContenedorDependencias.Categorias.Actualizar(v[0], v[1]),
                c => ContenedorDependencias.Categorias.Eliminar(c),
                escribe, RecargarCategorias);

            var productos = new PestanaCrud("Productos",
                new[]
                {
                    Campo.Texto("Código (3)", 80, 3), Campo.Texto("Nombre (máx. 50)", 200, 50),
                    Campo.Texto("Descripción (máx. 500)", 300, 500), campoCategoria
                },
                ListarProductos, BuscarProducto,
                v => ContenedorDependencias.Productos.Crear(v[0], v[1], v[2], v[3]),
                v => ContenedorDependencias.Productos.Actualizar(v[0], v[1], v[2], v[3]),
                c => ContenedorDependencias.Productos.Eliminar(c),
                escribe, null);

            var proveedores = new PestanaCrud("Proveedores",
                new[]
                {
                    Campo.Texto("Código (3)", 80, 3), Campo.Texto("Nombre (máx. 50)", 220, 50),
                    Campo.Texto("Ciudad (máx. 30)", 150, 30), Campo.Texto("Dirección (máx. 100)", 280, 100)
                },
                ListarProveedores, BuscarProveedor,
                v => ContenedorDependencias.Proveedores.Crear(v[0], v[1], v[2], v[3]),
                v => ContenedorDependencias.Proveedores.Actualizar(v[0], v[1], v[2], v[3]),
                c => ContenedorDependencias.Proveedores.Eliminar(c),
                escribe, null);

            tabs.TabPages.Add(categorias.Pagina);
            tabs.TabPages.Add(productos.Pagina);
            tabs.TabPages.Add(proveedores.Pagina);
            Controls.Add(tabs);

            categorias.Cargar();
            productos.Cargar();
            proveedores.Cargar();
            MostrarPestana(pestanaInicial);
        }

        public void MostrarPestana(int indice)
        {
            if (indice >= 0 && indice < tabs.TabPages.Count) tabs.SelectedIndex = indice;
        }

        // ---------- Helpers ----------
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

        private static void Aviso(string mensaje, MessageBoxIcon icono = MessageBoxIcon.Warning)
        {
            MessageBox.Show(mensaje, "Entidades del catálogo", MessageBoxButtons.OK, icono);
        }

        private static string Limpio(string valor)
        {
            return (valor ?? "").Trim();
        }

        private static DataTable Tabla(params string[] columnas)
        {
            var t = new DataTable();
            foreach (string c in columnas) t.Columns.Add(c);
            return t;
        }

        private void RecargarCategorias()
        {
            try
            {
                ComboBox cbo = campoCategoria.Combo;
                string actual = campoCategoria.Valor;
                cbo.Items.Clear();
                foreach (Categoria c in ContenedorDependencias.Categorias.Listar())
                {
                    cbo.Items.Add(new ComboItem(c.CCodigo, c.CNombre));
                }

                campoCategoria.Poner(actual);
            }
            catch (Exception ex)
            {
                Aviso("No se pudieron cargar las categorías: " + ex.Message);
            }
        }

        // ---------- Categorías ----------
        private static DataTable ListarCategorias()
        {
            DataTable t = Tabla("Código", "Nombre");
            foreach (Categoria c in ContenedorDependencias.Categorias.Listar()) t.Rows.Add(Limpio(c.CCodigo), c.CNombre);
            return t;
        }

        private static DataTable BuscarCategoria(string codigo)
        {
            DataTable t = Tabla("Código", "Nombre");
            Categoria c = ContenedorDependencias.Categorias.ObtenerPorCodigo(codigo);
            if (c != null) t.Rows.Add(Limpio(c.CCodigo), c.CNombre);
            return t;
        }

        // ---------- Productos ----------
        private static DataTable ListarProductos()
        {
            DataTable t = Tabla("Código", "Nombre", "Descripción", "Categoría");
            foreach (Producto p in ContenedorDependencias.Productos.Listar())
                t.Rows.Add(Limpio(p.PrCodigo), p.PrNombre, p.PrDescripcion, Limpio(p.CategoriaProducto));
            return t;
        }

        private static DataTable BuscarProducto(string codigo)
        {
            DataTable t = Tabla("Código", "Nombre", "Descripción", "Categoría");
            Producto p = ContenedorDependencias.Productos.ObtenerPorCodigo(codigo);
            if (p != null) t.Rows.Add(Limpio(p.PrCodigo), p.PrNombre, p.PrDescripcion, Limpio(p.CategoriaProducto));
            return t;
        }

        // ---------- Proveedores ----------
        private static DataTable ListarProveedores()
        {
            DataTable t = Tabla("Código", "Nombre", "Ciudad", "Dirección");
            foreach (Proveedor p in ContenedorDependencias.Proveedores.Listar())
                t.Rows.Add(Limpio(p.PCodigo), p.PNombre, p.PCiudad, p.PDireccion);
            return t;
        }

        private static DataTable BuscarProveedor(string codigo)
        {
            DataTable t = Tabla("Código", "Nombre", "Ciudad", "Dirección");
            Proveedor p = ContenedorDependencias.Proveedores.ObtenerPorCodigo(codigo);
            if (p != null) t.Rows.Add(Limpio(p.PCodigo), p.PNombre, p.PCiudad, p.PDireccion);
            return t;
        }
    }
}