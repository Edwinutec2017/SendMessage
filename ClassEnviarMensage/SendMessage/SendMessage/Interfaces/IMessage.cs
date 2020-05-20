using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SendMessage.Interfaces
{
   public interface IMessage
    {
        Task<bool> AdjuntoArchivo(List<string> ubicacion);
        void CuentaEmail(string cuenta, string de);
        Task<bool> ParametrosDinamicos(object parametros);
        Task<bool> Correo(string asunto, List<string> para, List<string> cc);
    }
}
