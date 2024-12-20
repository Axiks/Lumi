using MassTransit;
using MassTransit.Clients;
using MassTransit.Transports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Vanilla.Common.Message_Broker;
using static MassTransit.Logging.OperationName;

namespace Vanilla_App.Message_Broker
{
    public class BrokerRunner
    {
/*        IBusControl busControl;

        public BrokerRunner()
        {
            *//*busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("rabbitmq://localhost", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });

            busControl.Start();*//*

            busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.PrefetchCount = 50;
                cfg.UseConcurrencyLimit(50);

                cfg.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("my_queue", e =>
                {
                    e.Handler<TgUserResponse>(context =>
                    {
                        return Console.Out.WriteLineAsync($"[CREATEACCOUNT {DateTime.Now.ToString("s")}] Name: {context.Message.Name}, Email: {context.Message.Email}");
                    });
                    //e.Consumer<MyConsumer>();
                });


                *//*var host = cfg.Host(new Uri("urlrlrlrlr"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });*/
                /*cfg.ReceiveEndpoint(host, "endooooineeettt",
                    ep =>
                    {
                        // Endpoint configuration stuff
                    });*//*
            });


            var request = new TgUserRequest { UserId = Guid.NewGuid() };

            busControl.Publish(request).Wait();
            busControl.CreateRequestClient



        }



        public async Task<TgUserResponse> GetTgUserData(Guid userId)
        {
            TgUserResponse response;
            var request = new TgUserRequest { UserId = Guid.NewGuid() };

            IRequestClient<TgUserRequest, TgUserResponse> client;

            var client =
                busControl.CreateRequestClient<TgUserRequest>("", TimeSpan.FromSeconds(10));



            Task.Run(async () =>
            {
                request.StartTime = DateTime.Now;
                //The request is going to be sent to the Queue
                //Then the Consumer App will consume the request.
                //When the Consumer App is finished doing it's work, it will respond
                //At this point, the original request will be acknowledged, 
                //removed from the queue and then the response will be 
                //sent back here.
                //If there's a communication issue, a timeout exception 
                //will be thrown.  I don't have a try/catch block here because
                //I wanted to simplify the code.
                var response = await client.GetResponse<MyResponse>(request);
                //do something with response  
            }).Wait();





            busControl.Add


            var client = busControl.CreateRequestClient<TgUserRequest>(new Uri($"queue:"));


            var requestClient = busControl.CreateRequestClient<TgUserRequest>(
                new Uri("queue:get-data-queue"), TimeSpan.FromSeconds(30));

            IRequestClient<TgUserRequest> _client = busControl.CreateRequestClient<TgUserRequest>(new Uri("exchange:order-status"));




            var response = await _client.GetResponse<TgUserResponse>(request);

            var x = busControl.Send(request);

            var y = busControl.Publish(request);

            var response = await _requestClient.GetResponse<GetDataResponse>(new GetDataRequest { Id = id });
            return response.Message.Data;
        }*/

        /*public async Task StartBroker() { 
            await busControl.StartAsync();
            await RunGetUserTgDataBroker();
        }

        async Task RunGetUserTgDataBroker()
        {

            try
            {




                var requestClient = busControl.CreateRequestClient<TgUserRequest>(new Uri("queue:your_queue_name"), TimeSpan.FromSeconds(30));

                // Створення RequestClient
                var y = busControl.CreateRequestClient<TgUserRequest>();

                var requestClient = busControl.CreateRequestClient<TgUserRequest>(
                    new Uri("queue:get-data-queue"), TimeSpan.FromSeconds(30));

                var x = new TgUserRequester(requestClient);


                // Надсилання запиту
                var response = await requestClient.GetResponse<TgUserResponse>(new TgUserRequest
                {
                    Id = Guid.NewGuid()
                });

                Console.WriteLine($"Received data: {response.Message.Data}");
            }
            finally
            {
                // Зупинка шини
                await busControl.StopAsync();
                Console.WriteLine("Bus stopped...");
            }
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMassTransit(x =>
            {
                // elided...

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h => {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddHostedService<Worker>();
        });*/
    }
}
