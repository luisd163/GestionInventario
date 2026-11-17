using System;
using System.Drawing;
using System.Windows.Forms;
using Config;
using SistemaProveedores.Config;

namespace Vistas
{
    public sealed class FrmLogin : Form
    {
        private readonly TextBox txtCodigo = new TextBox();
        private readonly TextBox txtContrasena = new TextBox();
        private readonly Button btnIngresar = new Button();
        private readonly Button btnSalir = new Button();

        public FrmLogin()
        {
            Text = "Iniciar sesión";
            ClientSize = new Size(360, 250);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 9.5f);
            AcceptButton = btnIngresar;
            CancelButton = btnSalir;
            Construir();
        }

        private void Construir()
        {
            var titulo = new Label
            {
                Text = "Sistema de Consulta de Productos y Proveedores",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 15),
                Size = new Size(320, 50)
            };

            var lblCodigo = new Label { Text = "Código de usuario (3 caracteres)", Location = new Point(20, 75), AutoSize = true };
            txtCodigo.Location = new Point(20, 97);
            txtCodigo.Width = 320;
            txtCodigo.MaxLength = 3;

            var lblContrasena = new Label { Text = "Contraseña", Location = new Point(20, 130), AutoSize = true };
            txtContrasena.Location = new Point(20, 152);
            txtContrasena.Width = 320;
            txtContrasena.MaxLength = 15;
            txtContrasena.UseSystemPasswordChar = true;

            btnIngresar.Text = "Ingresar";
            btnIngresar.Location = new Point(20, 200);
            btnIngresar.Size = new Size(150, 32);
            btnIngresar.Click += (s, e) => Ingresar();

            btnSalir.Text = "Salir";
            btnSalir.Location = new Point(190, 200);
            btnSalir.Size = new Size(150, 32);
            btnSalir.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { titulo, lblCodigo, txtCodigo, lblContrasena, txtContrasena, btnIngresar, btnSalir });
        }

        private void Ingresar()
        {
            try
            {
                // AuthLN valida credenciales y registra BTiempoIngreso en la Bitácora.
                bool ok = ContenedorDependencias.Auth.IniciarSesion(txtCodigo.Text, txtContrasena.Text);
                if (!ok)
                {
                    MessageBox.Show("Código o contraseña incorrectos.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtContrasena.Clear();
                    txtContrasena.Focus();
                    return;
                }

                AbrirMenu();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible iniciar sesión: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirMenu()
        {
            var menu = new FrmMenuPrincipal(SesionUsuario.NombreUsuario, SesionUsuario.CodigoRol);
            menu.FormClosed += (s, e) =>
            {
                if (menu.CerroSesion)
                {
                    txtCodigo.Clear();
                    txtContrasena.Clear();
                    Show();
                    txtCodigo.Focus();
                }
                else
                {
                    Close();
                }
            };

            Hide();
            menu.Show();
        }
    }
}
