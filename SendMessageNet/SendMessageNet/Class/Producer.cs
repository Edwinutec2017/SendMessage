using System;
using System.Collections.Generic;
using System.Text;

namespace SendMessageNet.Class
{
    #region REQUEST
    public class EmailRequest
    {
        #region PROPERTIES
        public string CuentaMail { get; set; }
        public string De { get; set; }
        public List<string> Para { get; set; }
        public string Asunto { get; set; }
        public List<string> CC { get; set; }
        public List<string> CCO { get; set; }
        public string RutaArchivo { get; set; }
        public List<Base64FileRequest> Base64Files { get; set; }
        public dynamic ParametrosDinamicos { get; set; }


        #endregion
    }
    #endregion
    #region BASE 64 FILE 
    public class Base64FileRequest
    {
        #region PROPERTIES
        public string Base64Data { get; set; }
        public string FileName { get; set; }
        #endregion
    }
    #endregion
}
