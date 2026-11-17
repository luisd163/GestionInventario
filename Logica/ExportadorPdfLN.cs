using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Logica
{
    // Generador PDF mínimo (sin librerías externas): A4 horizontal, Helvetica, tabla de texto.
    internal static class ExportadorPdfLN
    {
        private const int Ancho = 842, Alto = 595, Margen = 30, FilasPorPagina = 38;

        internal static void Exportar(DataTable datos, string titulo, string ruta)
        {
            int columnas = Math.Max(1, datos.Columns.Count);
            double anchoCol = (double)(Ancho - 2 * Margen) / columnas;
            int maxChars = Math.Max(4, (int)(anchoCol / 4.3));
            int paginas = Math.Max(1, (int)Math.Ceiling(datos.Rows.Count / (double)FilasPorPagina));

            var contenidos = new List<string>();
            for (int pag = 0; pag < paginas; pag++)
            {
                var sb = new StringBuilder();
                sb.Append("BT /F2 13 Tf ").Append(Margen).Append(" 560 Td (")
                  .Append(Escapar(titulo + (paginas > 1 ? "  -  pág. " + (pag + 1) + "/" + paginas : "")))
                  .Append(") Tj ET\n");
                int y = 535;
                for (int c = 0; c < datos.Columns.Count; c++)
                {
                    sb.Append("BT /F2 8 Tf ").Append(Margen + (int)(c * anchoCol)).Append(' ').Append(y)
                      .Append(" Td (").Append(Escapar(Recortar(datos.Columns[c].ColumnName, maxChars))).Append(") Tj ET\n");
                }

                y -= 14;
                int inicio = pag * FilasPorPagina;
                int fin = Math.Min(datos.Rows.Count, inicio + FilasPorPagina);
                for (int r = inicio; r < fin; r++)
                {
                    for (int c = 0; c < datos.Columns.Count; c++)
                    {
                        string texto = datos.Rows[r][c] == DBNull.Value ? "" : Convert.ToString(datos.Rows[r][c]);
                        sb.Append("BT /F1 8 Tf ").Append(Margen + (int)(c * anchoCol)).Append(' ').Append(y)
                          .Append(" Td (").Append(Escapar(Recortar(texto, maxChars))).Append(") Tj ET\n");
                    }

                    y -= 12;
                }

                contenidos.Add(sb.ToString());
            }

            var offsets = new List<long>();
            using (var ms = new MemoryStream())
            {
                Action<string> escribir = s => { byte[] b = Encoding.Latin1.GetBytes(s); ms.Write(b, 0, b.Length); };
                Action<int, string> objeto = (n, cuerpo) =>
                {
                    offsets.Add(ms.Position);
                    escribir(n + " 0 obj\n" + cuerpo + "\nendobj\n");
                };

                escribir("%PDF-1.4\n");
                var kids = new StringBuilder();
                for (int i = 0; i < paginas; i++)
                {
                    kids.Append(5 + 2 * i).Append(" 0 R ");
                }

                objeto(1, "<< /Type /Catalog /Pages 2 0 R >>");
                objeto(2, "<< /Type /Pages /Kids [" + kids + "] /Count " + paginas + " >>");
                objeto(3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
                objeto(4, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");
                for (int i = 0; i < paginas; i++)
                {
                    objeto(5 + 2 * i, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + Ancho + " " + Alto + "] " +
                        "/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents " + (6 + 2 * i) + " 0 R >>");
                    objeto(6 + 2 * i, "<< /Length " + Encoding.Latin1.GetByteCount(contenidos[i]) + " >>\nstream\n" +
                        contenidos[i] + "endstream");
                }

                long posXref = ms.Position;
                escribir("xref\n0 " + (offsets.Count + 1) + "\n0000000000 65535 f \n");
                foreach (long o in offsets)
                {
                    escribir(o.ToString("D10") + " 00000 n \n");
                }

                escribir("trailer\n<< /Size " + (offsets.Count + 1) + " /Root 1 0 R >>\nstartxref\n" + posXref + "\n%%EOF");
                File.WriteAllBytes(ruta, ms.ToArray());
            }
        }

        private static string Recortar(string texto, int max)
        {
            texto = texto ?? "";
            return texto.Length <= max ? texto : texto.Substring(0, max - 1) + ".";
        }

        private static string Escapar(string texto)
        {
            return (texto ?? "").Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")
                                .Replace("\r", " ").Replace("\n", " ");
        }
    }
}