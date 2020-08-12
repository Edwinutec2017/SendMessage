using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SendMessage.Class;
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
   public class Message: IMessage
    {
        #region ATRIBUTOS
        private string parametros = null;
        private string name = null;
        private string file = null;
        private  ParametersMessage parametersMessage;
        private  CuentaEmail cuentaEmail;
        private List<Base64FileRequest> base64 = null;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region CONSTRUCTOR

        public Message(ParametersMessage _parametersMessage, CuentaEmail _cuentaEmail)
        {
            parametersMessage = _parametersMessage;
            cuentaEmail = _cuentaEmail;
        }
        #endregion

        #region ARCHIVO ADJUNTO
        public async Task<bool> AdjuntoArchivo(List<string> ubicacion)
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
               
                resp = false;
            }
            Dispose();
            return await Task.FromResult(resp);
        }
        #endregion

        #region LIBERACION MEMORIA 
        private  void ClearVariables() {
            parametros = null;
            name = null;
            file = null;
        }

        public void Dispose()
        {
            ClearVariables();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced);
        }
        #endregion

        #region CORREO
        public async Task<bool> Correo(ComplementEmail _complementEmail)
        {
            bool resp = false;
            try
            {
                _log.Info("Iniciando Proceso de envio correo a RabbitMQ");
                var parametro = new ConnectionFactory
                {
                    HostName = parametersMessage.Host,
                    Port = AmqpTcpEndpoint.UseDefaultPort,
                    UserName = parametersMessage.UserRabbitMQ,
                    Password = parametersMessage.Password
                };
                using (var connection = parametro.CreateConnection())
                {
                    _log.Info("Conexion Exitosa a RabbitMQ !");
                    using (var canales = connection.CreateModel())
                    {
                        
                        var _emailRequest = new List<EmailRequest>()
                        {
                            new EmailRequest()
                            {
                                CuentaMail=cuentaEmail.CuentaId,
                                De= cuentaEmail.EnviaMail,
                                Para=_complementEmail.Para,
                                CC=_complementEmail.CopiaMail,
                                Asunto=_complementEmail.Asunto,
                                ParametrosDinamicos=parametros,
                                Base64Files= base64

                            }
                        };
                        var properties = canales.CreateBasicProperties();
                        properties.DeliveryMode = 2;
                        canales.ConfirmSelect();
                        canales.BasicPublish(
                        exchange: parametersMessage.Channel,
                        routingKey: parametersMessage.Key,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_emailRequest)));
                        canales.WaitForConfirmsOrDie();
                        _emailRequest.Clear();
                        canales.Close();
                    }
                }
               
                resp = true;
                _log.Info("Correo Enviados a RabbitMQ");
            }
            catch (Exception ex)
            {
                _log.Warn($"Error de Conexion Revise las Credenciales!! {ex.StackTrace}");
                resp = false;
              
            }
            Dispose();
            return await Task.FromResult(resp);
        }
        #endregion

        #region PARAMETROS DINAMICOS
        public async Task<bool> ParametrosDinamicos(object parametros)
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
            Dispose();
            return await Task.FromResult(resp);
            
        }
        #endregion
    }
}
