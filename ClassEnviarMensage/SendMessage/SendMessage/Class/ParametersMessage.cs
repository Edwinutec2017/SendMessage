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


}
