using SendMessage.Class;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SendMessage.Interfaces
{
   public interface IMessage
    {
        Task<bool> AdjuntoArchivo(List<string> ubicacion);
        Task<bool> ParametrosDinamicos(object parametros);
        Task<bool> Correo(ComplementEmail _complementEmail);
    }
}
