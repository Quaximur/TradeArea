using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TradeArea.Shared
{
    public class SubdirectoryHandler : DelegatingHandler
    {
        private readonly string _subdirectory;

        public SubdirectoryHandler(HttpMessageHandler innerHandler, string subdirectory)
            : base(innerHandler)
        {
            _subdirectory = subdirectory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var old = request.RequestUri;

            var url = $"{old.Scheme}://{old.Host}:{old.Port}{_subdirectory}{request.RequestUri.AbsolutePath}";
            request.RequestUri = new Uri(url, UriKind.Absolute);

            return base.SendAsync(request, cancellationToken);
        }
    }
}