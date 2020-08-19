
using SendMessage.Class;
using System;
using System.Threading.Tasks;

namespace SendMessage.Interfaces
{
   public interface IMessage:IDisposable
    {
        Task<bool> Publish(EmailParams _emailRequest);
    }
}