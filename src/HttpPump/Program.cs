using System;
using Akka.Actor;
using HttpPump.Actors;
using Serilog;

namespace HttpPump
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();
            Log.Logger = logger;

            var system = ActorSystem.Create("loadtest");

            // TODO: get worker and url:s from args
            var producer = Props.Create<RequestProducer>(()=> new RequestProducer(20, new []
            {
                "http://localhost/",
            }));

            logger.Information("Starting up AkkaPump, press enter to stop pumping request");
            var actorRef = system.ActorOf(producer, "requestProducer");
            
            Console.ReadLine();
            // TODO: find a better way to shutdown properly
            var result = actorRef.GracefulStop(TimeSpan.FromSeconds(60)).Result;
            Console.ReadLine();
        }
    }
}