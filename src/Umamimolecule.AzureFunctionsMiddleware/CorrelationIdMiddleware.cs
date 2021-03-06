﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to extract correlation identifier from a request header.
    /// </summary>
    public class CorrelationIdMiddleware : HttpMiddleware
    {
        private readonly IEnumerable<string> correlationIdHeaders;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
        /// </summary>
        /// <param name="correlationIdHeaders">The collection of headers which will be inspected, in order.  The first matching header found will be used for the correlation ID.</param>
        public CorrelationIdMiddleware(IEnumerable<string> correlationIdHeaders)
        {
            this.correlationIdHeaders = correlationIdHeaders;
        }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task InvokeAsync(HttpContext context)
        {
            var correlationId = this.GetCorrelationId(context.Request);
            context.TraceIdentifier = correlationId;

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }

        private string GetCorrelationId(HttpRequest request)
        {
            foreach (var correlationIdHeader in this.correlationIdHeaders)
            {
                if (request.Headers.TryGetValue(correlationIdHeader, out StringValues value) &&
                    !StringValues.IsNullOrEmpty(value))
                {
                    return value.ToString();
                }
            }

            return Guid.NewGuid().ToString();
        }
    }
}
