using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using static DTPortal.Common.CommonResponse;
using Confluent.Kafka;


namespace DTPortal.Core.Utilities
{
    public class LogClient : ILogClient
    {
        private readonly ILogger<LogClient> _logger;
        public static bool initLibrary = false;
        private readonly KafkaConfig _kafkaConfig;
        private readonly IProducer<Null, string> _producer;

        public LogClient(ILogger<LogClient> logger,
                         IKafkaConfigProvider kafkaConfigProvider)
        {
            _logger = logger;
            _logger.LogDebug("-->LogClient");
            // Load Kafka configuration
            _kafkaConfig = kafkaConfigProvider.GetKafkaConfiguration();

            if (_kafkaConfig == null)
                throw new Exception("Kafka configuration not found.");

            // Create Kafka Producer
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaConfig?.BootstrapServers,
                Acks = Acks.All,
                MessageTimeoutMs = 5000
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
            _logger.LogInformation("Kafka producer initialized.");
        }

        private async Task<int> SendKafkaMessageAsync(string topic, string message)
        {
            try
            {
                _logger.LogError("Sending message to Kafka topic: {Topic}", topic);
                _logger.LogError("Message content: {Message}", message);
                var msg = new Message<Null, string> { Value = message };
                var deliveryResult = await _producer.ProduceAsync(topic, msg);
                _logger.LogInformation($"Kafka: Message delivered to {deliveryResult.TopicPartitionOffset}");
                return 0;
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError($"Kafka ProduceException: {ex.Message}");
                //_logger.LogInformation("Topic name:",topic);
                //_logger.LogInformation("request body:", message);
                //_logger.LogInformation("Topic name1:", topic.ToString());
                //_logger.LogInformation("request body1:", message.ToString());
                //_logger.LogInformation("Error Message:",ex.Message);
                //_logger.LogError($"Kafka produce error: {ex.Error.Reason}");
                //_logger.LogError("Sending message to Kafka topic: {Topic}", topic);
                //_logger.LogError("Message content: {Message}", message);
                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kafka send failed: {ex.Message}");
                _logger.LogError("Sending message to Kafka topic: {Topic}", topic);
                _logger.LogError("Message content: {Message}", message);
                return -1;
            }
        }

        public async Task<int> SendCentralLogMessage(LogMessage logMessage)
        {
            var json = JsonConvert.SerializeObject(logMessage);
            return await SendKafkaMessageAsync(_kafkaConfig.CentralLogTopic, json);
        }

        public async Task<int> SendServiceLogMessage(LogMessage logMessage)
        {
            var json = JsonConvert.SerializeObject(logMessage);
            return await SendKafkaMessageAsync(_kafkaConfig.ServiceLogTopic, json);
        }

        public async Task<int> SendAdminLogMessage(string adminLogMessage)
        {
            return await SendKafkaMessageAsync(_kafkaConfig.AdminLogTopic, adminLogMessage);
        }

        public async Task<Response> SendAuthenticationLogMessage(
            TemporarySession tempSession,
            string suid,
            string serviceName,
            string logMessage,
            string logMessageType,
            string transactionType,
            bool centralLog,
            string callStack = null)
        {
            Response response = new Response();
            try
            {
                var log = new LogMessage
                {
                    identifier = suid,
                    transactionID = Guid.NewGuid().ToString(),
                    serviceName = serviceName,
                    startTime = tempSession.AuthNStartTime,
                    endTime = DateTime.Now.ToString("s"),
                    logMessage = logMessage,
                    transactionType = transactionType,
                    correlationID = tempSession.CoRelationId,
                    serviceProviderName = tempSession.Clientdetails.ClientId,
                    serviceProviderAppName = tempSession.Clientdetails.ClientId,
                    logMessageType = logMessageType,
                    callStack = callStack,
                    signatureType = "NONE"
                };

                if (serviceName ==LogClientServices.walletAuthenticationLog ||
                    serviceName == LogClientServices.AuthenticationSuccess)
                {
                    if (logMessageType == LogClientServices.Success) log.authenticationType = LogClientServices.Approved;
                    else if (logMessageType == LogClientServices.Failure) log.authenticationType = LogClientServices.Failed;
                    else
                    {
                        log.authenticationType = LogClientServices.Declined;
                        log.logMessageType = LogClientServices.Failure;
                    }
                }

                log.checksum = PKIMethods.Instance.AddChecksum(JsonConvert.SerializeObject(log));

                if (centralLog)
                    await SendCentralLogMessage(log);

                await SendServiceLogMessage(log);

                response.Success = true;
                response.Result = log.checksum;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kafka logging failed: {ex.Message}");
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<Response> SendAuthenticationLogMessage(
            DateTime StartTime,
            string suid,
            string serviceName,
            string logMessage,
            string logMessageType,
            string transactionType,
            bool centralLog)
        {
            Response response = new Response();
            try
            {
                var log = new LogMessage
                {
                    identifier = suid,
                    transactionID = Guid.NewGuid().ToString(),
                    serviceName = serviceName,
                    startTime = StartTime.ToString("s"),
                    endTime = DateTime.Now.ToString("s"),
                    logMessage = logMessage,
                    logMessageType = logMessageType,
                    transactionType = transactionType,
                    correlationID = Guid.NewGuid().ToString()
                };

                log.checksum = PKIMethods.Instance.AddChecksum(JsonConvert.SerializeObject(log));

                if (centralLog)
                   await SendCentralLogMessage(log);

                await SendServiceLogMessage(log);

                response.Success = true;
                response.Result = log.checksum;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kafka logging failed: {ex.Message}");
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<Response> SendWalletAuthenticationLog(
            string suid,
            string serviceProviderAppName,
            string serviceName,
            string logMessage,
            string logMessageType,
            string transactionType,
            string callStack,
            bool centralLog)
        {
            Response response = new Response();
            try
            {
                var log = new LogMessage
                {
                    identifier = suid,
                    transactionID = Guid.NewGuid().ToString(),
                    serviceName = serviceName,
                    startTime = DateTime.Now.ToString("s"),
                    endTime = DateTime.Now.ToString("s"),
                    serviceProviderAppName= serviceProviderAppName,
                    logMessage = logMessage,
                    logMessageType = logMessageType,
                    transactionType = transactionType,
                    callStack = callStack,
                    correlationID = Guid.NewGuid().ToString(),
                    signatureType = "NONE"
                };

                if (logMessageType == LogClientServices.Success) log.authenticationType = LogClientServices.Approved;
                else if (logMessageType == LogClientServices.Failure) log.authenticationType = LogClientServices.Failed;
                else
                {
                    log.authenticationType = LogClientServices.Declined;
                    log.logMessageType = LogClientServices.Failure;
                }

                log.checksum = PKIMethods.Instance.AddChecksum(JsonConvert.SerializeObject(log));

                if (centralLog)
                    await SendCentralLogMessage(log);

                await SendServiceLogMessage(log);

                response.Success = true;
                response.Result = log.checksum;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kafka logging failed: {ex.Message}");
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}
