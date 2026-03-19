using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace BASE.Service.Core.QueueService
{
    /// <summary>
    /// Service dùng để làm việc với Kafka (producer).
    /// </summary>
    public class KafkaService : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private bool _disposed;

        public KafkaService(string bootstrapServers, string clientId = "easymart-backend")
        {
            if (string.IsNullOrWhiteSpace(bootstrapServers))
            {
                throw new ArgumentException("BootstrapServers không được để trống.", nameof(bootstrapServers));
            }

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = clientId
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        /// <summary>
        /// Gửi message dạng string lên Kafka.
        /// </summary>
        public async Task Publish(string topic, string key, string value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("Topic không được để trống.", nameof(topic));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var message = new Message<string, string>
            {
                Key = key,
                Value = value
            };

            // Confluent.Kafka không hỗ trợ CancellationToken trực tiếp trên ProduceAsync,
            // nhưng ta vẫn cho phép truyền token để compatibility với async pipeline.
            cancellationToken.ThrowIfCancellationRequested();
            await _producer.ProduceAsync(topic, message).ConfigureAwait(false);
        }

        /// <summary>
        /// Gửi message object (serialize JSON) lên Kafka.
        /// </summary>
        public Task Publish<T>(string topic, string key, T payload, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(payload);
            return Publish(topic, key, json, cancellationToken);
        }

        public void Flush(TimeSpan? timeout = null)
        {
            if (_disposed) return;

            if (timeout.HasValue)
            {
                _producer.Flush(timeout.Value);
            }
            else
            {
                _producer.Flush();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _producer.Flush(TimeSpan.FromSeconds(5));
                _producer.Dispose();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}

