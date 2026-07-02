using RelPro.Infrastructure.Context;

namespace RelPro.Infrastructure.Http;

internal sealed class TokenForwardingHandler : DelegatingHandler
{
    private readonly IRequestContext _ctx;

    public TokenForwardingHandler(IRequestContext ctx) => _ctx = ctx;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_ctx.SessionToken))
            request.Headers.TryAddWithoutValidation("userToken", _ctx.SessionToken);

        return base.SendAsync(request, cancellationToken);
    }
}
