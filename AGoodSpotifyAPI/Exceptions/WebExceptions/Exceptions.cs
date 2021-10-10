using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Exceptions.WebExceptions
{
    public class BadRequestException : SpotWebException
    {
        public override string Message => "The request could not be understood by the server due to malformed syntax. The message body will contain more information; see Response Schema.";
        public new int Code { get; } = 400;

        public BadRequestException(string webMessage) : base(webMessage) { }
    }

    public class UnauthorizedException : SpotWebException
    {
        public override string Message => "The request requires user authentication or, if the request included authorization credentials, authorization has been refused for those credentials.";
        public new int Code { get; } = 401;

        public UnauthorizedException(string webMessage) : base(webMessage) { }
    }

    public class ForbiddenException : SpotWebException
    {
        public override string Message => "The server understood the request, but is refusing to fulfill it.";
        public new int Code { get; } = 403;

        public ForbiddenException(string webMessage) : base(webMessage) { }
    }

    public class NotFoundException : SpotWebException
    {
        public override string Message => "The requested resource could not be found. This error can be due to a temporary or permanent condition.";
        public new int Code { get; } = 404;

        public NotFoundException(string webMessage) : base(webMessage) { }
    }

    public class TooManyRequestException : SpotWebException
    {
        public override string Message => "Rate limiting has been applied. See https://developer.spotify.com/documentation/web-api/#rate-limiting";
        public new int Code { get; } = 429;

        public TooManyRequestException(string webMessage) : base(webMessage) { }
    }

    public class InternalServerErrorException : SpotWebException
    {
        public override string Message => "You should never receive this error because our clever coders catch them all … but if you are unlucky enough to get one, please report it to us through a comment at the bottom of this page.";
        public new int Code { get; } = 500;

        public InternalServerErrorException(string webMessage) : base(webMessage) { }
    }

    public class BadGatewayException : SpotWebException
    {
        public override string Message => "The server was acting as a gateway or proxy and received an invalid response from the upstream server.";
        public new int Code { get; } = 502;

        public BadGatewayException(string webMessage) : base(webMessage) { }
    }

    public class ServiceUnavailableException : SpotWebException
    {
        public override string Message => "The server is currently unable to handle the request due to a temporary condition which will be alleviated after some delay. You can choose to resend the request again.";
        public new int Code { get; } = 503;

        public ServiceUnavailableException(string webMessage) : base(webMessage) { }
    }
}
