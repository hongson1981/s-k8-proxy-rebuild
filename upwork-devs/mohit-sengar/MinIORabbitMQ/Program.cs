﻿using Microsoft.Extensions.Configuration;
using Minio;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MinIO_RabbitMQ
{
    public class Program
    {
        public static string MinIOEndpoint { get; set; }
        public static string MinIOAccessKey { get; set; }
        public static string MinIOSecretKey { get; set; }
        public static string RabbitMQEndpoint { get; set; }

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            ConfigureAppSettings(args);
            try
            {
                Console.WriteLine("Initialize Minio Client...");
                var minioClient = new MinioClient(MinIOEndpoint,
                                           MinIOAccessKey,
                                           MinIOSecretKey
                                     ).WithSSL();
                // Create bucket if it doesn't exist.
                bool found = await minioClient.BucketExistsAsync("mybucket");
                if (found)
                {
                    Console.WriteLine("mybucket already exists");
                }
                else
                {
                    await minioClient.MakeBucketAsync("mybucket");
                    Console.WriteLine("mybucket is created successfully");
                }
                var OutgoingfilePath = "Outgoing//example.pdf";
                var IncomingfilePath = "Incoming//example.pdf";
                //Extract path from arguments
                ExtractArguments(args, ref OutgoingfilePath, ref IncomingfilePath);
                var fileName = "example.pdf";
                // Upload file
                await minioClient.PutObjectAsync("mybucket", fileName, OutgoingfilePath, contentType: "application/pdf");
                Console.WriteLine("example.pdf is uploaded successfully");
                //Get URL from MinIO
                String url = await minioClient.PresignedGetObjectAsync("mybucket", fileName, 60 * 60 * 24);
                Console.WriteLine("Uploaded file url: " + url);
                // Send Message
                ConnectionFactory factory = SendMessage(IncomingfilePath, url);
                //Receive message
                ReceiveMessage(factory);
                Console.WriteLine("Enter to stop program");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ReceiveMessage(ConnectionFactory factory)
        {
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
                    var minioClient = new MinioClient(MinIOEndpoint,
                                           MinIOAccessKey,
                                           MinIOSecretKey
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
                Thread.Sleep(10);
            }
        }

        private static ConnectionFactory SendMessage(string IncomingfilePath, string url)
        {
            var factory = new ConnectionFactory() { HostName = RabbitMQEndpoint };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "URL",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = url;
                var body = Encoding.UTF8.GetBytes(message + " " + IncomingfilePath);

                channel.BasicPublish(exchange: "",
                                     routingKey: "URL",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine("Message sent!");
            }
            Console.WriteLine("RabbitMQ sent successfully...");
            return factory;
        }

        private static void ExtractArguments(string[] args, ref string OutgoingfilePath, ref string IncomingfilePath)
        {
            if (args.Length > 0)
            {
                for (int index = 0; index < args.Length; index++)
                {
                    string item = args[index];
                    if (item.Equals("-f") && args.Length >= (index + 1))
                    {
                        IncomingfilePath = args[index + 1];
                    }
                    if (item.Equals("-o") && args.Length >= (index + 1))
                    {
                        OutgoingfilePath = args[index + 1];
                    }
                }
            }
        }

        private static void ConfigureAppSettings(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
            var settings = Configuration.GetSection("SettingsConfig");
            MinIOEndpoint = settings.GetSection("MinIOEndpoint").Value;
            MinIOAccessKey = settings.GetSection("MinIOAccessKey").Value;
            MinIOSecretKey = settings.GetSection("MinIOSecretKey").Value;
            RabbitMQEndpoint = settings.GetSection("RabbitMQEndpoint").Value;
        }
    }
}
