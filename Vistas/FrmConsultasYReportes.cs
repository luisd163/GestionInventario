using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Modelo;
using SistemaProveedores.Config;

namespace Vistas
{
    public sealed class FrmConsultasYReportes : Form
    {
        [Flags]
        private enum Filtro { Ninguno = 0, Ciudad = 1, Proveedor = 2, Producto = 4, Categoria = 8, Multicategoria = 16 }

        private sealed class Opcion
        {
            public string Titulo { get; }
            public Filtro Filtro { get; }
            public Func<DataTable> Ejecutar { get; }
            public Opcion(string titulo, Filtro filtro, Func<DataTable> ejecutar)
            {
                Titulo = titulo; Filtro = filtro; Ejecutar = ejecutar;
            }

            public override string ToString() { return Titulo; }
        }

        private sealed class ComboItem
        {
            public string Codigo { get; }
            public string Texto { get; }
            public ComboItem(string codigo, string texto) { Codigo = (codigo ?? "").Trim(); Texto = texto; }
            public override string ToString() { return Codigo + " - " + Texto; }
        }

        private readonly ComboBox cboConsulta = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboCiudad = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboProveedor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboProducto = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckedListBox clbCategorias = new CheckedListBox { CheckOnClick = true };
        private readonly DataGridView grid = new DataGridView();
        private readonly Label lblInfo = new Label { AutoSize = true, ForeColor = Color.DimGray };

        private DataTable tablaActual;
        private string tituloActual = "Reporte";

        // Impresión
        private DataTable datosImp;
        private float[] pesos;
        private int filaImp, pagina;

        public FrmConsultasYReportes()
        {
            Text = "Consultas y Reportes";
            ClientSize = new Size(1100, 620);
            Font = new Font("Segoe UI", 9.5f);
            ConstruirInterfaz();
            CargarFiltros();
            CargarOpciones();
        }

        // ---------- Helpers ----------
        private static Label Etiqueta(string texto, int x, int y)
        {
            return new Label { Text = texto, Location = new Point(x, y), AutoSize = true };
        }

        private static Button Boton(string texto, int x, int y, EventHandler accion, int ancho)
        {
            var b = new Button { Text = texto, Location = new Point(x, y), Size = new Size(ancho, 30) };
            b.Click += accion;
            return b;
        }

        private static void Aviso(string mensaje, MessageBoxIcon icono = MessageBoxIcon.Warning)
        {
            MessageBox.Show(mensaje, "Consultas y Reportes", MessageBoxButtons.OK, icono);
        }

        private static DataTable ATabla<T>(IEnumerable<T> datos)
        {
            var t = new DataTable();
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                t.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
            }

            foreach (T d in datos)
            {
                DataRow f = t.NewRow();
                foreach (PropertyInfo p in props) f[p.Name] = p.GetValue(d) ?? DBNull.Value;
                t.Rows.Add(f);
            }

            return t;
        }

        private static string Texto(object valor)
        {
            if (valor == null || valor == DBNull.Value) return "";
            if (valor is decimal) return ((decimal)valor).ToString("N2");
            if (valor is DateTime) return ((DateTime)valor).ToString("g");
            return Convert.ToString(valor);
        }

        private static string CodigoSel(ComboBox cbo, string que)
        {
            var item = cbo.SelectedItem as ComboItem;
            if (item == null) throw new ArgumentException("Seleccione " + que + ".");
            return item.Codigo;
        }

        private string CiudadSel()
        {
            string ciudad = Convert.ToString(cboCiudad.SelectedItem);
            if (string.IsNullOrWhiteSpace(ciudad)) throw new ArgumentException("Seleccione una ciudad.");
            return ciudad;
        }

        private IList<string> CodigosMarcados()
        {
            var lista = new List<string>();
            foreach (object o in clbCategorias.CheckedItems) lista.Add(((ComboItem)o).Codigo);
            if (lista.Count == 0) throw new ArgumentException("Marque al menos una categoría.");
            return lista;
        }

        // ---------- Interfaz ----------
        private void ConstruirInterfaz()
        {
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = SystemColors.Window;

            var izq = new Panel { Dock = DockStyle.Left, Width = 240 };
            cboCiudad.SetBounds(10, 30, 215, 28);
            cboProveedor.SetBounds(10, 85, 215, 28);
            cboProducto.SetBounds(10, 140, 215, 28);
            cboCategoria.SetBounds(10, 195, 215, 28);
            clbCategorias.SetBounds(10, 250, 215, 200);
            izq.Controls.AddRange(new Control[]
            {
                Etiqueta("Ciudad", 10, 10), cboCiudad,
                Etiqueta("Proveedor", 10, 65), cboProveedor,
                Etiqueta("Producto", 10, 120), cboProducto,
                Etiqueta("Categoría", 10, 175), cboCategoria,
                Etiqueta("Categorías (Reporte 5)", 10, 230), clbCategorias
            });

            var arriba = new Panel { Dock = DockStyle.Top, Height = 85 };
            cboConsulta.SetBounds(10, 28, 520, 28);
            cboConsulta.SelectedIndexChanged += (s, e) => ActualizarFiltros();
            lblInfo.Location = new Point(10, 60);
            arriba.Controls.AddRange(new Control[]
            {
                Etiqueta("Consulta o reporte", 10, 8), cboConsulta,
                Boton("Ejecutar", 545, 26, (s, e) => EjecutarSeleccion(), 100),
                Boton("Exportar PDF", 655, 26, (s, e) => ExportarPdf(), 120),
                Boton("Imprimir...", 785, 26, (s, e) => Imprimir(), 110),
                lblInfo
            });

            Controls.Add(grid);   // Fill primero, luego Left y Top.
            Controls.Add(izq);
            Controls.Add(arriba);
        }

        private void CargarFiltros()
        {
            try
            {
                foreach (string ciudad in ContenedorDependencias.Reportes.CiudadesDisponibles()) cboCiudad.Items.Add(ciudad);
                foreach (Proveedor p in ContenedorDependencias.Proveedores.Listar())
                    cboProveedor.Items.Add(new ComboItem(p.PCodigo, p.PNombre));
                foreach (Producto p in ContenedorDependencias.Productos.Listar())
                    cboProducto.Items.Add(new ComboItem(p.PrCodigo, p.PrNombre));
                foreach (Categoria c in ContenedorDependencias.Categorias.Listar())
                {
                    cboCategoria.Items.Add(new ComboItem(c.CCodigo, c.CNombre));
                    clbCategorias.Items.Add(new ComboItem(c.CCodigo, c.CNombre));
                }
            }
            catch (Exception ex)
            {
                Aviso("No se pudieron cargar los filtros: " + ex.Message, MessageBoxIcon.Error);
            }
        }

        private void CargarOpciones()
        {
            var c = ContenedorDependencias.Consultas;
            var r = ContenedorDependencias.Reportes;

            var lista = new List<Opcion>
            {
                new Opcion("Consulta 1 - Catálogo unificado de oferta por producto", Filtro.Ninguno,
                    () => ATabla(c.CatalogoUnificadoOferta())),
                new Opcion("Consulta 3 - Distribución de proveedores por familia de producto", Filtro.Ninguno,
                    () => ATabla(c.DistribucionProveedoresFamilia())),
                new Opcion("Subconsulta 1 - Proveedores con precio sobre el promedio", Filtro.Ninguno,
                    () => ATabla(c.ProveedoresPrecioSobrePromedio())),
                new Opcion("Subconsulta 2 - Productos sin oferta de proveedores", Filtro.Ninguno,
                    () => ATabla(c.ProductosSinOferta())),
                new Opcion("Reporte 1 - Proveedores por ciudad", Filtro.Ciudad,
                    () => r.ProveedoresPorCiudad(CiudadSel())),
                new Opcion("Reporte 2 - Cobertura de productos por proveedor", Filtro.Proveedor,
                    () => r.CoberturaPorProveedor(CodigoSel(cboProveedor, "un proveedor"))),
                new Opcion("Reporte 3 - Comparativa de precios por producto", Filtro.Producto,
                    () => r.ComparativaPreciosProducto(CodigoSel(cboProducto, "un producto"))),
                new Opcion("Reporte 4 - Catálogo general por categoría", Filtro.Categoria,
                    () => r.CatalogoPorCategoria(CodigoSel(cboCategoria, "una categoría"))),
                new Opcion("Reporte 5 - Multicategoría y disponibilidad", Filtro.Multicategoria,
                    () => ATabla(r.MultiCategoriaDisponibilidad(CodigosMarcados())))
            };

            // Consulta 2 (auditoría) es exclusiva del Administrador en ConsultasLN.
            if (ContenedorDependencias.Auth.EsAdministrador())
            {
                lista.Insert(1, new Opcion("Consulta 2 - Auditoría de accesos y permisos por usuario", Filtro.Ninguno,
                    () => ATabla(c.AuditoriaAccesosPorUsuario())));
            }

            foreach (Opcion o in lista) cboConsulta.Items.Add(o);
            cboConsulta.SelectedIndex = 0;
        }

        private void ActualizarFiltros()
        {
            var op = cboConsulta.SelectedItem as Opcion;
            Filtro f = op == null ? Filtro.Ninguno : op.Filtro;
            cboCiudad.Enabled = (f & Filtro.Ciudad) != 0;
            cboProveedor.Enabled = (f & Filtro.Proveedor) != 0;
            cboProducto.Enabled = (f & Filtro.Producto) != 0;
            cboCategoria.Enabled = (f & Filtro.Categoria) != 0;
            clbCategorias.Enabled = (f & Filtro.Multicategoria) != 0;
        }

        // ---------- Ejecución ----------
        private void EjecutarSeleccion()
        {
            var op = cboConsulta.SelectedItem as Opcion;
            if (op == null) { Aviso("Seleccione una consulta o reporte."); return; }

            try
            {
                DataTable t = op.Ejecutar();
                tablaActual = t;
                tituloActual = op.Titulo;
                grid.DataSource = t;
                lblInfo.Text = op.Titulo + "  |  " + t.Rows.Count + " registro(s)";
            }
            catch (Exception ex)
            {
                Aviso(ex.Message);
            }
        }

        private bool HayDatos()
        {
            if (tablaActual == null || tablaActual.Rows.Count == 0)
            {
                Aviso("No hay datos para exportar o imprimir. Ejecute una consulta primero.");
                return false;
            }

            return true;
        }

        // Respeta el orden que el usuario aplicó al hacer clic en las columnas.
        private DataTable DatosActuales()
        {
            return tablaActual.DefaultView.ToTable();
        }

        // ---------- Exportar PDF ----------
        private void ExportarPdf()
        {
            if (!HayDatos()) return;

            string nombre = tituloActual;
            foreach (char ch in Path.GetInvalidFileNameChars()) nombre = nombre.Replace(ch, '_');

            using (var dlg = new SaveFileDialog { Filter = "Archivo PDF (*.pdf)|*.pdf", FileName = nombre + ".pdf" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    ContenedorDependencias.Reportes.ExportarPdf(DatosActuales(), tituloActual, dlg.FileName);
                    Aviso("Reporte exportado correctamente.", MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Aviso("No se pudo exportar: " + ex.Message, MessageBoxIcon.Error);
                }
            }
        }

        // ---------- Imprimir (PrintDocument nativo) ----------
        private void Imprimir()
        {
            if (!HayDatos()) return;

            try
            {
                datosImp = DatosActuales();
                pesos = new float[datosImp.Columns.Count];
                for (int c = 0; c < pesos.Length; c++)
                {
                    int max = datosImp.Columns[c].ColumnName.Length;
                    for (int r = 0; r < datosImp.Rows.Count && r < 60; r++)
                    {
                        max = Math.Max(max, Texto(datosImp.Rows[r][c]).Length);
                    }

                    pesos[c] = Math.Min(Math.Max(max, 4), 40);
                }

                using (var doc = new PrintDocument { DocumentName = tituloActual })
                {
                    doc.DefaultPageSettings.Landscape = true;
                    doc.BeginPrint += (s, e) => { filaImp = 0; pagina = 0; };
                    doc.PrintPage += ImprimirPagina;
                    using (var vista = new PrintPreviewDialog { Document = doc, WindowState = FormWindowState.Maximized })
                    {
                        vista.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Aviso("No se pudo imprimir: " + ex.Message, MessageBoxIcon.Error);
            }
        }

        private void ImprimirPagina(object sender, PrintPageEventArgs e)
        {
            pagina++;
            Graphics g = e.Graphics;
            Rectangle m = e.MarginBounds;
            int n = datosImp.Columns.Count;

            float suma = 0;
            foreach (float p in pesos) suma += p;
            var xs = new float[n + 1];
            xs[0] = m.Left;
            for (int c = 0; c < n; c++) xs[c + 1] = xs[c] + m.Width * pesos[c] / suma;

            using (var fTitulo = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (var fCab = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var fTxt = new Font("Segoe UI", 8.5f))
            using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                float y = m.Top;
                g.DrawString(tituloActual, fTitulo, Brushes.Black, m.Left, y);
                y += fTitulo.GetHeight(g) + 8;
                float alto = fTxt.GetHeight(g) + 5;

                for (int c = 0; c < n; c++)
                {
                    g.DrawString(datosImp.Columns[c].ColumnName, fCab, Brushes.Black,
                        new RectangleF(xs[c] + 2, y, xs[c + 1] - xs[c] - 4, alto), sf);
                }

                y += alto;
                g.DrawLine(Pens.Black, m.Left, y, m.Right, y);
                y += 2;

                while (filaImp < datosImp.Rows.Count && y + alto <= m.Bottom - 20)
                {
                    for (int c = 0; c < n; c++)
                    {
                        g.DrawString(Texto(datosImp.Rows[filaImp][c]), fTxt, Brushes.Black,
                            new RectangleF(xs[c] + 2, y, xs[c + 1] - xs[c] - 4, alto), sf);
                    }

                    y += alto;
                    filaImp++;
                }

                g.DrawString("Página " + pagina, fTxt, Brushes.Gray, m.Right - 70, m.Bottom - 14);
            }

            e.HasMorePages = filaImp < datosImp.Rows.Count;
        }
    }
}