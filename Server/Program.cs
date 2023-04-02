using Common.Models;
using Common.Senders;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var logger = new LoggerConfiguration()
                                // add console as logging target
                                .WriteTo.Console()
                                // set minimum level to log
                                .MinimumLevel.Debug()
                                .CreateLogger();

            EmailSender emailSender = new EmailSender
            {
                SenderEmail = "erzhan.97531@gmail.com",
                Password = "bsfhzmficzmatuxd"         //пароль устройства
            };

            //подключение к очереди
            var factory = new ConnectionFactory() { HostName = "localhost" };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.QueueDeclare("email-queue", exclusive: false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                var recipientEmail = JsonSerializer.Deserialize<Email>(message);            //десериализуем данные о сообщении

                logger.Information($" Received email\n" +
                    $"\tRecipient address: {recipientEmail.Address}\n" +
                    $"\tSubject: {recipientEmail.Subject}\n" +
                    $"\tMessage: {recipientEmail.Message}");


                string response;    

                //отправка email 
                try
                {
                    await emailSender.SendEmailAsync(recipientEmail.Address, recipientEmail.Subject, recipientEmail.Message);

                    logger.Information($"Email was sent to {recipientEmail.Address}");
                    response = "Email sent";
                }
                catch (Exception ex)
                {
                    logger.Error($"{ex.Message}\n{ex.StackTrace}");

                    response = "Error sending email";
                }


                //ответ клиенту
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);

                channel.BasicPublish("", ea.BasicProperties.ReplyTo, ea.BasicProperties, responseBytes);
            };

            channel.BasicConsume(queue: "email-queue", autoAck: true, consumer: consumer);


            logger.Information($"Subscribed to the queue 'email-queue'");


            Console.ReadKey(); 
            
        }
    }
}