using System.Collections.Generic;
using System.Data;
using Modelo;

namespace Logica
{
    public interface IRolUsuarioDatos
    {
        IList<RolUsuario> Listar();
    }

    public interface IClasificacionDatos
    {
        IList<ClasificacionProducto> ListarPorProducto(string prCodigo);
        void Asignar(string prCodigo, string cCodigo);
        void Quitar(string prCodigo, string cCodigo);
    }

    public interface IConsultasDatos
    {
        IList<OfertaProductoDTO> CatalogoUnificadoOferta();
        IList<AuditoriaAccesoDTO> AuditoriaAccesosPorUsuario();
        IList<DistribucionProveedorDTO> DistribucionProveedoresFamilia();
        IList<ProveedorPrecioSobrePromDTO> ProveedoresPrecioSobrePromedio();
        IList<ProductoSinOfertaDTO> ProductosSinOferta();
    }

    public interface IReportesFiltradosDatos
    {
        IList<string> CiudadesDisponibles();
        DataTable ProveedoresPorCiudad(string ciudad);
        DataTable CoberturaPorProveedor(string pCodigo);
        DataTable ComparativaPrecios(string prCodigo);
        DataTable CatalogoPorCategoria(string cCodigo);
        IList<ResumenCategoriaDTO> MultiCategoria(IList<string> codigosCategoria);
    }
}