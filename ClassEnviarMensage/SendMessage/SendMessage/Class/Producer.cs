using System;
using System.Collections.Generic;
using System.Text;

namespace SendMessage.Class
{
 public class EmailReques
    {
        #region PROPERTIES
        public string CuentaMail { get; set; }
        public string De { get; set; }
        public List<string> Para { get; set; }
        public string Asunto { get; set; }
        public List<string> CC { get; set; }
        public List<string> CCO { get; set; }
        public string RutaArchivo { get; set; }
        public List<Base64FileReques> Base64Files { get; set; }
        public dynamic ParametrosDinamicos { get; set; }
        public bool Enviado { get; set; }
        #endregion
    }
    public class Base64FileReques
    {
        #region PROPERTIES
        public string Base64Data { get; set; }
        public string FileName { get; set; }
        #endregion
    }
}
