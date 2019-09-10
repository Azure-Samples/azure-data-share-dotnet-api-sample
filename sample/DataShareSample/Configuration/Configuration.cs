// -----------------------------------------------------------------------
//  <copyright file="Configuration.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace DataShareSample
{
    using System;

    public class Configuration
    {
        public static readonly Uri ArmEndpoint = new Uri("https://management.azure.com");
        public static readonly Uri AuthorizationEndpoint = new Uri("https://login.windows.net");
        public const string Location = "EastUS2";

        public Principal Provider { get; set; }

        public Principal Consumer { get; set; }
    }
}