using System;
using System.Collections.Generic;
using System.Text;

namespace SendMessage.Interfaces
{
   public interface IMessage
    {
        bool AdjuntoArchivo(List<string> ubicacion);
        void CuentaEmail(string cuenta, string de);
        bool ParametrosDinamicos(object parametros);
        bool Correo(string asunto, List<string> para, List<string> cc);
    }
}
