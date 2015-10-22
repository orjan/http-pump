using System;
using System.Net;

namespace HttpPump.Messages
{
    internal class Result
    {
        public int Status { get; set; }
        public int Id { get; set; }
        public TimeSpan Time { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}