
namespace Utilities.Response
{
    using System.Collections.ObjectModel;
    using System.Net;
    using Utilities.Secutity;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ManagerResponse<T>
    {
        /// <summary>
        ///  Logica para respuesta de la petición.
        /// </summary>
        /// <typeparam name="T">Objeto Generico</typeparam>
        /// <param name="mensaje">Mensaje de error</param>
        /// <returns></returns>
        public static ModelResponse<T> ResponseError(string mensaje)
        {
            return new ModelResponse<T>
            {
                CodigoRespuesta = HttpStatusCode.BadRequest,
                CodigoTransaccion = 0,
                Mensaje = mensaje,
                Data = null,
                Token = null
            };
        }


        /// <summary>
        ///  Logica para respuesta de la petición.
        /// </summary>
        /// <typeparam name="T">Objeto Generico</typeparam>
        /// <param name="mensaje">Mensaje de error</param>
        /// <returns></returns>
        public static ModelResponse<T> ResponseUnauthorized(string mensaje)
        {
            return new ModelResponse<T>
            {
                CodigoRespuesta = HttpStatusCode.Unauthorized,
                CodigoTransaccion = 0,
                Mensaje = mensaje,
                Data = null,
                Token = null
            };
        }

        /// <summary>
        ///  Logica para respuesta de la petición.
        /// </summary>
        /// <typeparam name="T">Objeto Generico</typeparam>
        /// <param name="mensaje">Mensaje de operación exitosa</param>
        /// <returns></returns>
        public static ModelResponse<T> ResponseOk(string mensaje)
        {
            return new ModelResponse<T>
            {
                CodigoRespuesta = HttpStatusCode.OK,
                CodigoTransaccion = 125,
                Mensaje = mensaje,
                Data = null,
                Token = null
            };
        }

        /// <summary>
        ///  Logica para respuesta de la petición.
        /// </summary>
        /// <typeparam name="T">Objeto Generico</typeparam>
        /// <param name="mensaje">Mensaje de operación exitosa</param>
        /// <returns></returns>
        public static ModelResponse<T> ResponseOk(string mensaje, AuthorizationToken token)
        {
            return new ModelResponse<T>
            {
                CodigoRespuesta = HttpStatusCode.OK,
                CodigoTransaccion = 125,
                Mensaje = mensaje,
                Data = null,
                Token = token
            };
        }

        /// <summary>
        ///  Logica para respuesta de la petición.
        /// </summary>
        /// <typeparam name="T">Objeto Generico</typeparam>
        /// <param name="mensaje">Mensaje de error interno en el servicio</param>
        public static ModelResponse<T> ResponseInternalServerError(string mensaje)
        {
            return new ModelResponse<T>
            {
                CodigoRespuesta = HttpStatusCode.InternalServerError,
                CodigoTransaccion = 0,
                Mensaje = mensaje,
                Data = null,
                Token = null
            };
        }

        /// <summary>
        ///  Indica que se ha generado un conflicto en validaciones de negocio.
        /// </summary>
        /// <typeparam name="T">Objeto Generico</typeparam>
        /// <param name="mensaje">Mensaje de error interno en el servicio</param>
        public static ModelResponse<T> PreconditionRequired(string mensaje, Collection<T> error)
        {
            return new ModelResponse<T>
            {
                CodigoRespuesta = HttpStatusCode.PreconditionRequired,
                CodigoTransaccion = 0,
                Mensaje = mensaje,
                Data = error,
                Token = null
            };
        }
    }
}
