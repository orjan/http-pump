using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using HttpPump.Messages;
using Serilog;

namespace HttpPump.Actors
{
    internal class RequestProducer : ReceiveActor
    {
        private readonly ILogger log = Log.Logger.ForContext<RequestProducer>();
        private readonly int numberOfWorkers;
        private readonly string[] uris;
        private readonly HashSet<IActorRef> workers = new HashSet<IActorRef>();        
        private readonly IActorRef logger;
        private IActorRef workerPool;
        private int id;

        public RequestProducer(int numberOfWorkers, string[] uris)
        {
            this.numberOfWorkers = numberOfWorkers;
            this.uris = uris;
            logger = Context.ActorOf<RequestLogger>("consoleLogger");

            Receive<NewWorker>(worker =>
            {
                log.Debug("Adding http request worker: {Worker}", Sender);
                //Console.WriteLine("Adding worker: " + Sender.Path);
                workers.Add(Sender);
                RequestHttpWork(Sender);
            });

            Receive<Result>(worker =>
            {
                logger.Tell(worker);
                RequestHttpWork(Sender);
            });

            Receive<StopGenerationOfRequests>(stop =>
            { 
                Task.WaitAll(workerPool.GracefulStop(TimeSpan.FromSeconds(60)), logger.GracefulStop(TimeSpan.FromSeconds(60)));

                Sender.Tell("Done", Self);
            });
        }

        protected override void PreStart()
        {
            base.PreStart();

            var props = Props.Create(() => new MakeHttpRequest(Self))
                .WithRouter(new RoundRobinPool(this.numberOfWorkers));
            workerPool = Context.ActorOf(props, "httpWorker");
        }

        protected override void PostStop()
        {
            Task.WaitAll(workerPool.GracefulStop(TimeSpan.FromSeconds(60)), logger.GracefulStop(TimeSpan.FromSeconds(60)));            

            base.PostStop();
        }


        private void RequestHttpWork(IActorRef sender)
        {
            var requestId = id++;

            sender.Tell(new GetRequest {Id = requestId, Uri = new Uri(uris[requestId % uris.Length]) }, Self);
        }
    }
}