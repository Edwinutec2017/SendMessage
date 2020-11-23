using log4net;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
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
    public class Message : IMessage
    {
        #region ATRIBUTOS
        private string parametros = null;
        private string name = null;
        private string file = null;
        private ParametersMessage parametersMessage;
        private List<Base64FileReques> base64 = null;
        private static int _retryCount;
        private IConnection _connection;
        private Fecha _fecha;
        private static int _contador;
        private static bool _resp;
        private static  ILog _log ;

        #endregion

        #region CONSTRUCTOR
        public Message(ParametersMessage _parametersMessage)
        {
            parametersMessage = _parametersMessage;
            _retryCount = 5;
            _fecha = new Fecha();
            _contador = 0;
            _resp = false;
            _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        #endregion

        #region ARCHIVO ADJUNTO
        private async Task<List<Base64FileReques>> AdjuntoArchivo(List<string> ubicacion)
        {

            _resp = false;
            _contador = 0;

            try {

                var policyBase64 = RetryPolicy.Handle<Exception>().Or<NullReferenceException>().
                    WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _contador++;
                        _log.Warn($"Intento {_contador} para  poner el archivo adjunto {_contador} {_fecha.FechaNow().Result}");
                    });

                policyBase64.Execute(()=> {
                    base64 = new List<Base64FileReques>();
                    if (ubicacion != null)
                    {
                        if (ubicacion.Count > 0)
                        {
                            foreach (string ruta in ubicacion)
                            {
                                if (ruta != null && ruta != "")
                                {
                                    name = Path.GetFileName(ruta);
                                    file = Convert.ToBase64String(File.ReadAllBytes(ruta));

                                    base64.Add(new Base64FileReques()
                                    {
                                        FileName = name,
                                        Base64Data = file
                                    });
                                }

                            }
                            _resp = true;
                        }
                    }
                    else
                        _log.Info($"No ay archivo adjunto en esta notificacion {_fecha.FechaNow().Result}");
                });

            }
            catch (Exception ex) {
                _log.Fatal($"Intentos para agregar el archivo adjunto {_contador} {_fecha.FechaNow().Result}");
                _log.ErrorFormat($"Excepcion {ex.StackTrace} {_fecha.FechaNow().Result}");

                _resp = false;
            }
            Dispose();
            return await Task.FromResult(base64);
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

        #region VALIDAR conexion A RABBITMQ
        private Task<bool> Connection() {
            bool resp = false;
            int contadorConexion = 0;
            try
            {
                var policyRabbit = RetryPolicy.Handle<Exception>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    contadorConexion++;
                    _log.Warn($"Intento {contadorConexion} para establecer la coneccion a RabbitMq..  !!!! {_fecha.FechaNow().Result}");
                });

                policyRabbit.Execute(() =>
                {
                    var parametro = new ConnectionFactory
                    {
                        HostName = parametersMessage.Host,
                        Port = parametersMessage.Port,
                        UserName = parametersMessage.UserRabbitMQ,
                        Password = parametersMessage.Password
                    };
                    _connection = parametro.CreateConnection();
                    _log.Info($"Coneccion a RabbitMq  !!!!! {_fecha.FechaNow().Result}");
                    resp = true;
                });
            }
            catch (Exception ex) {

                _log.Fatal($"No se logro establecer conexion a  RabbitMq total de Intentos {contadorConexion}....{_fecha.FechaNow().Result} !!!");
                _log.Warn($"Exception {ex.StackTrace} {_fecha.FechaNow().Result}");
            }
            return Task.FromResult(resp);

        }
        #endregion

        public async Task<bool> Publish(EmailReques _emailRequest)
        {
            _resp = false;
            try
            {
                var policy = RetryPolicy.Handle<Exception>()
                           .Or<BrokerUnreachableException>()
                           .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)), (ex, time) =>
                           {
                        
                             _log.Warn($"Intento para poner mensage en cola en RabbitMq !!!  -- {_fecha.FechaNow().Result}  ");
                          });

                policy.Execute(() => {
                    _log.Info($"Iniciando Proceso de poner en cola en RabbitMQ {_fecha.FechaNow().Result}");

                    if (Connection().Result)
                    {
                        using (var canales = _connection.CreateModel())
                        {
                            var _email = new List<EmailReques>()
                        {
                           _emailRequest
                        };
                            var properties = canales.CreateBasicProperties();
                            properties.DeliveryMode = 2;
                            canales.ConfirmSelect();
                            canales.BasicPublish(
                            exchange: parametersMessage.Channel,
                            routingKey: parametersMessage.Key,
                            basicProperties: properties,
                            body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_email)));
                            canales.WaitForConfirmsOrDie();
                            _email.Clear();
                            canales.Close();
                            _connection.Close();
                            _log.Info($"Mensage puesto en la cola en RabbitMq {_fecha.FechaNow().Result}");
                            _resp = true;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _log.Fatal($"Total de intentos para poner en cola el mensage {_contador} {_fecha.FechaNow().Result}");
                _log.Warn($"Excepcion {ex.StackTrace} {_fecha.FechaNow().Result}");
            }
            Dispose();
            return await Task.FromResult(_resp);
        }
        #endregion

        #region PARAMETROS DINAMICOS
        private async Task<bool> ParametrosDinamicos(object parametros)
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
