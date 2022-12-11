using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities.Secutity;

namespace Utilities.Response
{

    /// <summary>
    /// Respuesta de petición segun el modelo
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    public class ModelResponse<T>
    {
        /// <summary>
        /// Gets or Sets del código de respuesta
        /// </summary>
        public HttpStatusCode CodigoRespuesta { get; set; }

        /// <summary>
        /// Gets or Sets Mensaje.
        /// </summary>
        public string Mensaje { get; set; }

        /// <summary>
        /// Gets or Sets CodigoTransaccion.
        /// </summary>
        public int CodigoTransaccion { get; set; }

        /// <summary>
        /// Gets or Sets Data.
        /// </summary>
        public Collection<T> Data { get; set; } = new Collection<T>();

        /// <summary>
        /// Get Date Response
        /// </summary>
        public DateTime Date { get => DateTime.Now; }

        /// <summary>
        /// Gets or Sets Service Version.
        /// </summary>
        //public string ServiceVersion { get; set; }

        /// <summary>
        /// Gets or Sets Token.
        /// </summary>
        public AuthorizationToken Token { get; set; }
    }
}
