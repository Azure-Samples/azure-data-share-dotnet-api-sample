// -----------------------------------------------------------------------
//  <copyright file="RetryHandler.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace DataShareSample
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest.TransientFaultHandling;

    public class RetryHandler : DelegatingHandler
    {
        private const int MaxRetries = 3;

        private readonly HttpStatusCodeErrorDetectionStrategy errorDetectionStrategy;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.errorDetectionStrategy = new HttpStatusCodeErrorDetectionStrategy();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < RetryHandler.MaxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                if (!this.errorDetectionStrategy.IsTransient(
                        new HttpRequestWithStatusException { StatusCode = response.StatusCode }) &&
                    !(await response.Content.ReadAsStringAsync().ConfigureAwait(false)).Contains("PrincipalNotFound"))
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }

            return response;
        }
    }
}
