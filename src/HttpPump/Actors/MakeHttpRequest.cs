using System;
using System.Net;
using Akka.Actor;
using HttpPump.Messages;

namespace HttpPump.Actors
{
    internal class MakeHttpRequest : ReceiveActor
    {
        private readonly IActorRef actorRef;

        public MakeHttpRequest(IActorRef actorRef)
        {
            this.actorRef = actorRef;
            Receive<GetRequest>(request =>
            {
                var before = HighResolutionDateTime.UtcNow;
                var httpStatusCode = HttpGet(request.Uri);
                var time = HighResolutionDateTime.UtcNow - before;

                Sender.Tell(new Result { Status = (int)httpStatusCode, Id = request.Id, Time = time, HttpStatusCode = httpStatusCode }, Self);
            });
        }

        protected override void PreStart()
        {
            base.PreStart();
            actorRef.Tell(new NewWorker(), Self);
            //Context.ActorSelection("/user/requestProducer").Tell(new NewWorker(), Self);
        }


        public static HttpStatusCode HttpGet(Uri uri)
        {
            var request = WebRequest.CreateHttp(uri);

            HttpWebResponse response = null;
            try
            {
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                response = request.GetResponse() as HttpWebResponse;
                return response.StatusCode;
            }
            catch (WebException e)
            {
                response = (HttpWebResponse) e.Response;
                return response.StatusCode;
            }
            finally
            {
                response?.Dispose();
            }
        }
    }
}