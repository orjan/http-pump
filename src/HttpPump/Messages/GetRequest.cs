using System;

namespace HttpPump.Messages
{
    internal class GetRequest
    {
        public Uri Uri { get; set; }
        public int Id { get; set; }
    }
}