using Akka.Actor;
using HttpPump.Messages;
using Serilog;

namespace HttpPump.Actors
{
    internal class RequestLogger : ReceiveActor
    {
        private readonly ILogger log = Log.Logger.ForContext<RequestProducer>();

        public RequestLogger()
        {
            Receive<Result>(worker =>
            {
                // var message = string.Format("{0} GET {1} {2} {3}", worker.Id, worker.Time.TotalMilliseconds, worker.Status, worker.HttpStatusCode.ToString());
                log.Information("{RequestId} GET {Time} {StatusCode} {StatusMessage}", worker.Id, worker.Time.TotalMilliseconds, worker.Status, worker.HttpStatusCode.ToString());
            });
        }
    }
}