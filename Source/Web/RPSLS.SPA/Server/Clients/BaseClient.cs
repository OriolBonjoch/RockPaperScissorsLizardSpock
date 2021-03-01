﻿using Microsoft.AspNetCore.Http;

namespace RPSLS.SPA.Server.Clients
{
    public class BaseClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BaseClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected Grpc.Core.Metadata GetRequestMetadata()
        {
            var metadata = new Grpc.Core.Metadata();
            var routeAs = _httpContextAccessor.HttpContext.Request.Headers["azds-route-as"].ToString();
            if (!string.IsNullOrEmpty(routeAs))
            {
                metadata.Add("azds-route-as", routeAs);
            }
            return metadata;
        }
    }
}
