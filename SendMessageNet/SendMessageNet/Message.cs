using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SendMessageNet.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SendMessageNet
{
    public class Message : IMessage, IDisposable
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
            try
            {
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
                else
                    _log.Info("No posee archivo adjunto ");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat($"Error en el formato del archivo adjunto {ex.StackTrace}");
                resp = false;
            }
            GC.Collect(2, GCCollectionMode.Forced);
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
                _log.Info("Correo Enviado Correctamente desde el Nugget ");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat($"Formato del coreo no es el correto Nugget {ex.StackTrace}");
                resp = false;
                GC.Collect(2, GCCollectionMode.Forced);
            }
            return Task.FromResult(resp);
        }
        #endregion
        #region PARAMETROS DINAMICOS
        public Task<bool> ParametrosDinamicos(object parametros)
        {
            bool resp = false;
            try
            {
                if (parametros != null)
                {
                    this.parametros = JsonConvert.SerializeObject(parametros);
                    resp = true;
                    _log.Info("Parametros dinamicos correctamente!!  ");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error en los parametros dinamico {ex}");
                resp = false;
            }
            GC.Collect(2, GCCollectionMode.Forced);
            return Task.FromResult(resp);

        }
        #endregion
    }
}
