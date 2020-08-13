using Polly;
using SendMessage;
using SendMessage.Class;
using SendMessage.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PruebaClassSendMessage
{
    class Program
    {
        static void Main(string[] args)
        {

            #region parametros Requeridos
            ParametersMessage parametersMessage = new ParametersMessage()
            {
                Host = "http://localhost/",
                UserRabbitMQ = "USRENVCORREO",
                Password = "Crecer$2020",
                Channel = "notificacion",
                Key = "email.#"
            };
            CuentaEmail cuentaEmail = new CuentaEmail()
            {
                CuentaId = "GENERACION_DEUDA_SEPP",
                EnviaMail = "servicioaempresas@crecer.com.sv",

            };
            ComplementEmail complementEmail = new ComplementEmail()
            {
                Asunto = "Prueba de plantilla 2020",
                Para = new List<string>() {
                        "bscenolasc@crecer.com.sv",
                    },
                CopiaMail = new List<string>() {
                          "edwinnolas2020@gmail.com"

                    }

            };
            #endregion


            IMessage imessage = new Message(parametersMessage, cuentaEmail);
           
            // imessage.AdjuntoArchivo(doc);
         // imessage.ParametrosDinamicos(ningunRegistro);

            var resp = imessage.Correo(complementEmail).Result;

            Console.WriteLine(resp);

            Console.Read();
        }


    }
}
