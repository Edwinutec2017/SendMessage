using System;
using System.Collections.Generic;
using System.Text;

namespace SendMessage.Class
{
    #region Parametros de Conexion
    public class ParametersMessage
    {
        public string Host { get; set; }
        public string UserRabbitMQ { get; set; }
        public string Password { get; set; }
        public string Channel { get; set; }
        public string Key { get; set; }
    }
    #endregion
    #region PARAMETROS DE CUENTA
    public class CuentaEmail {
        public string CuentaId { get; set; }
        public string EnviaMail { get; set; }

    }
    #endregion
    #region Complementos Email
    public class ComplementEmail {
        public string Asunto { get; set; }
        public List<string> Para { get; set; }
        public List<string> CopiaMail { get; set; }
    }
    #endregion

}
