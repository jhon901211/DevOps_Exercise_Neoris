namespace Utilities.Secutity
{
    using System;

    /// <summary>
    /// clase AuthorizationToken
    /// </summary>
    public class AuthorizationToken
    {
        /// <summary>
        /// Gets or sets TipoToken.
        /// </summary>
        public string TipoToken { get; set; }

        /// <summary>
        /// Gets or sets TokenAcceso.
        /// </summary>
        public string TokenAcceso { get; set; }

        /// <summary>
        /// Gets or sets TokenRenovacion.
        /// </summary>
        public string TokenRenovacion { get; set; }

        /// <summary>
        /// Gets or sets TiempoExpiracion
        /// </summary>
        public DateTime TiempoExpiracion { get; set; }
    }
}