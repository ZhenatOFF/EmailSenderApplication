using Common.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;


namespace WebClient.Controllers
{
    public class EmailController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Email email)
        {
            //подключение к очереди
            var factory = new ConnectionFactory() { HostName = "localhost" };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.QueueDeclare("email-queue", exclusive: false);

            var consumer = new EventingBasicConsumer(channel);


            var replyQueue = channel.QueueDeclare("", exclusive: true);  //создаем временную очередь для ответа от сервера
            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = replyQueue.QueueName;
            properties.CorrelationId = Guid.NewGuid().ToString();        //генерируем id корреляции для получения корректного ответа


            channel.BasicConsume(queue: replyQueue.QueueName, autoAck: true, consumer: consumer);


            var json = JsonSerializer.Serialize(email);
            var body = Encoding.UTF8.GetBytes(json);

            string result;

            channel.BasicPublish("", "email-queue", properties, body);

            String response = "Email sent";

            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId.Equals(properties.CorrelationId))   //сверяем id корреляции
                {
                    var body = ea.Body.ToArray();
                    response = Encoding.UTF8.GetString(body);
                }

            };

            TempData["Status"] = response;

            return View();
        }


    }
}
