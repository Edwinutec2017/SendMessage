using Newtonsoft.Json;
using RabbitMQ.Client;
using SendMessage.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;

namespace SendMessage
{
   public class Message: IMessage,IDisposable
    {
        #region ATRIBUTOS
        private string parametros = null;
        private string name = null;
        private string file = null;
        private string hostName, user, pass, cuenta, de, canal, llave;
        private List<Base64FileRequest> base64 = null;
        #endregion
        #region CONSTRUCTOR
        public Message(string host, string user, string pass, string canal, string llave)
        {
            this.hostName = host;
            this.user = user;
            this.pass = pass;
            this.canal = canal;
            this.llave = llave;
        }
        #endregion
        #region ARCHIVO ADJUNTO
        public bool AdjuntoArchivo(List<string> ubicacion)
        {
            bool resp = false;
            try {
                base64 = new List<Base64FileRequest>();
                if (ubicacion.Count > 0)
                {
                    foreach (string ruta in ubicacion)
                    {
                        name = Path.GetFileName(ruta);
                        file = Convert.ToBase64String(File.ReadAllBytes(ruta));

                        base64.Add(new Base64FileRequest()
                        {
                            FileName = name,
                            Base64Data = file
                        });
                    }
                    resp = true;
                }
            }
            catch (Exception ex) {
                resp = false;
            }

            GC.Collect(2,GCCollectionMode.Forced);
            return resp;
        }
        #endregion
        #region CUENAT MAIL
        public void CuentaEmail(string cuenta, string de)
        {
            this.cuenta = cuenta;
            this.de = de;
             GC.Collect(2, GCCollectionMode.Forced);
        }
        #endregion
        public void Dispose()
        {
            file = null;
            GC.Collect(2, GCCollectionMode.Forced);
        }
        #region CORREO
        public bool Correo(string asunto, List<string> para, List<string> cc)
        {
            try
            {
                var parametro = new ConnectionFactory
                {
                    HostName = hostName,
                    Port = AmqpTcpEndpoint.UseDefaultPort,
                    UserName = user,
                    Password = pass
                };
                using (var connection = parametro.CreateConnection())
                {
                    using (var canales = connection.CreateModel())
                    {
                        var _emailRequest = new List<EmailRequest>()
                        {
                            new EmailRequest()
                            {
                                CuentaMail=cuenta,
                                De= de,
                                Para=para,
                                CC=cc,
                                Asunto=asunto,
                                ParametrosDinamicos=parametros,
                                Base64Files= base64

                            }
                        };
                        var properties = canales.CreateBasicProperties();
                        properties.DeliveryMode = 2;
                        canales.ConfirmSelect();
                        canales.BasicPublish(
                        exchange: canal,
                        routingKey: llave,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_emailRequest)));
                        canales.WaitForConfirmsOrDie();
                        _emailRequest.Clear();
                    }
                }
                GC.Collect(2, GCCollectionMode.Forced);
                return true;
            }
            catch (Exception)
            {
                GC.Collect(2, GCCollectionMode.Forced);
                return false;
            }
        }
        #endregion
        public bool ParametrosDinamicos(object parametros)
        {
            bool resp=false;
            try
            {
            if (parametros !=null) {
                this.parametros = JsonConvert.SerializeObject(parametros);
                    resp = true;
            }
            } catch (Exception ex) {
                resp = false;
            }
            GC.Collect(2, GCCollectionMode.Forced);
            return resp;
            
        }

    }
}
