using System;
using System.Collections.Generic;
using System.Text;

namespace AGoodSpotifyAPI.Exceptions.WebExceptions
{
    public class SpotWebException : Exception
    {
        public int Code { get; }
        public string WebMessage { get; }

        public SpotWebException(string webMessage) => WebMessage = webMessage;
        public SpotWebException() { }

        public static SpotWebException GetWebException(int? code, string message) => code switch
        {
            400 => new BadRequestException(message),
            401 => new UnauthorizedException(message),
            403 => new ForbiddenException(message),
            404 => new NotFoundException(message),
            429 => new TooManyRequestException(message),
            500 => new InternalServerErrorException(message),
            502 => new BadGatewayException(message),
            503 => new ServiceUnavailableException(message),
            _ => new SpotWebException(message)
        };
    }
}
