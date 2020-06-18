using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SendMessage.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

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
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
        public Task<bool> AdjuntoArchivo(List<string> ubicacion)
        {
            bool resp = false;
            try {
                base64 = new List<Base64FileRequest>();

                if (ubicacion!=null) {
                    if (ubicacion.Count > 0)
                    {
                        foreach (string ruta in ubicacion)
                        {
                            if (ruta != null && ruta !="") {
                                name = Path.GetFileName(ruta);
                                file = Convert.ToBase64String(File.ReadAllBytes(ruta));

                                base64.Add(new Base64FileRequest()
                                {
                                    FileName = name,
                                    Base64Data = file
                                });
                            }

                        }
                        resp = true;
                    }
                    else
                        _log.Info("No existe ninguna direccion");

                }else
                    _log.Info("No posee archivo adjunto ");

            }
            catch (Exception ex) {
                _log.ErrorFormat($"Error en el formato del archivo adjunto {ex.StackTrace}");
                _log.Warn("O error de conexion a Rabbit " +
                    "");
                resp = false;
            }
            GC.Collect(2,GCCollectionMode.Forced);
            return Task.FromResult(resp);
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
        #region LIBERACION MEMORIA 
        public void Dispose()
        {
            file = null;
            GC.Collect(2, GCCollectionMode.Forced);
        }
        #endregion
        #region CORREO
        public Task<bool> Correo(string asunto, List<string> para, List<string> cc)
        {
            bool resp = false;
            try
            {

                _log.Info("Iniciando Porceso de envio correo a Rabbti");
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
               
                resp = true;
                GC.Collect(2, GCCollectionMode.Forced);
                _log.Info("Correo Enviados a Rabbit");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat($"Formato del correo no es el correcto {ex.StackTrace}");
                resp = false;
                GC.Collect(2, GCCollectionMode.Forced);
            }
            return Task.FromResult(resp);
        }
        #endregion
        #region PARAMETROS DINAMICOS
        public Task<bool> ParametrosDinamicos(object parametros)
        {
            bool resp=false;
            try
            {
            if (parametros !=null) {
                this.parametros = JsonConvert.SerializeObject(parametros);
                    resp = true;
                    _log.Info("Parametros dinamicos correctamente!!  ");
            }
            } catch (Exception ex) {
                _log.Error($"Error en los parametros dinamico {ex}");
                resp = false;
            }
            GC.Collect(2, GCCollectionMode.Forced);
            return Task.FromResult(resp);
            
        }
        #endregion
    }
}
