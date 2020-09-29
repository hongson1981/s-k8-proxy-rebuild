using Minio;
using RabbitMQ.Client;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MinIORabbitMQSender
{
    public class Sender
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            try
            {
                #region Minio Client
                Console.WriteLine("Initialize Minio Client...");

                var minioClient = new MinioClient("play.min.io",
                                           "Q3AM3UQ867SPQQA43P2F",
                                           "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG"
                                     ).WithSSL();

                // Create bucket if it doesn't exist.
                bool found = await minioClient.BucketExistsAsync("mybucket");
                if (found)
                {
                    Console.WriteLine("mybucket already exists");
                }
                else
                {
                    // Create bucket 'my-bucketname'.
                    await minioClient.MakeBucketAsync("mybucket");
                    Console.WriteLine("mybucket is created successfully");
                }
                var OutgoingfilePath = "Outgoing//example.pdf";
                var IncomingfilePath = "Incoming//example.pdf";
                //Extract path from arguments
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
                var fileName = "example.pdf";
                // Upload file
                await minioClient.PutObjectAsync("mybucket", fileName, OutgoingfilePath, contentType: "application/pdf");
                Console.WriteLine("example.pdf is uploaded successfully");

                //Get URL
                String url = await minioClient.PresignedGetObjectAsync("mybucket", fileName, 60 * 60 * 24);
                Console.WriteLine("Uploaded file url: " + url);

                #endregion Minio Client

                #region RabbitMQ
                // Send Message
                var factory = new ConnectionFactory() { HostName = "localhost" };
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
                #endregion RabbitMQ

                Console.WriteLine("Enter to stop program");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
