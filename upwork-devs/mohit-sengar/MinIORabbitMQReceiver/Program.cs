using Minio;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MinIORabbitMQReceiver
{
    class Receiver
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "URL",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var split = message.Split(' ');
                    var urlParseResult = split[0].Split('/');
                    string bucketName = urlParseResult[3];
                    string fileName = urlParseResult[4].Split('?')[0];
                    var filePath = "Incoming/" + fileName;
                    //Extract incoming file path if exists
                    if (split.Length > 1) 
                    {
                        filePath = split[1];
                    }
                    Console.WriteLine("Message Received!");
                    var minioClient = new MinioClient("play.min.io",
                                           "Q3AM3UQ867SPQQA43P2F",
                                           "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG"
                                     ).WithSSL();
                    // Check whether the object exists using statObject().
                    await minioClient.StatObjectAsync(bucketName, fileName);
                    var exePath = Path.GetDirectoryName(System.Reflection
                                  .Assembly.GetExecutingAssembly().CodeBase);
                    using (FileStream outputFileStream = new FileStream(filePath, FileMode.Create))
                    {
                        // Get input stream to have content of 'my-objectname' from 'my-bucketname'
                        await minioClient.GetObjectAsync(bucketName, fileName,
                                                         (stream) =>
                                                         {
                                                             stream.CopyTo(outputFileStream);
                                                         });
                    }
                };
                channel.BasicConsume(queue: "URL",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Listening to the Messages!");
                Console.ReadLine();
            }
        }
    }
}
