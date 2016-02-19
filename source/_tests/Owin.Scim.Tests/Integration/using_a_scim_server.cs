﻿namespace Owin.Scim.Tests.Integration
{
    using System;

    using Configuration;

    using Machine.Specifications;

    using Microsoft.Owin.Testing;

    using Model;
    using Model.Users;

    using Scim.Extensions;

    public class using_a_scim_server : IAssemblyContext
    {
        protected static TestServer Server
        {
            get { return _Server; }
        }

        public void OnAssemblyStart()
        {
            //ncrunch: no coverage start
            if (_Server != null) return;

            _Server = TestServer.Create(app =>
            {
                app.UseScimServer(
                    new ScimServerConfiguration
                    {
                        RequireSsl = false,
                        PublicOrigin = "https://helloworld.org/scim/v2"
                    }
                    .AddCompositionConditions(
                        fileInfo => fileInfo.Name.StartsWith("Sample.Host.Console", StringComparison.OrdinalIgnoreCase))
                    .AddAuthenticationScheme(
                        new AuthenticationScheme(
                            "oauthbearertoken",
                            "OAuth Bearer Token",
                            "Authentication scheme using the OAuth Bearer Token standard.", 
                            specUri: new Uri("https://tools.ietf.org/html/rfc6750"),
                            isPrimary: true))
                    .ConfigureETag(supported: true, isWeak: true)
                    .ModifyResourceType<User>(ModifyUserResourceType)
                );
            });
            // ncrunch: no coverage end
        }

        private void ModifyUserResourceType(ScimResourceTypeDefinitionBuilder<User> builder)
        {
            
        }

        public void OnAssemblyComplete()
        {
            if (_Server == null) return;
            
            _Server.Dispose();
        }

        private static TestServer _Server;
    }
}