﻿using System.Net.Http;
using System.Text;
using Owin.Scim.Querying;

namespace Owin.Scim.Tests.Integration.Querying.Projection
{
    using System;
    using System.Collections.Generic;

    using Extensions;

    using Machine.Specifications;

    using Model.Users;

    using Ploeh.AutoFixture;

    using Users;

    using v2.Model;

    public class when_requesting_specific_attributes : using_a_scim_server
    {
        Because of = async () =>
        {
            var autoFixture = new Fixture();

            var existingUser = autoFixture.Build<ScimUser2>()
                .With(x => x.UserName, UserNameUtility.GenerateUserName())
                .With(x => x.Password, "somePass!2")
                .With(x => x.PreferredLanguage, "en-US,en,es")
                .With(x => x.Locale, "en-US")
                .With(x => x.Timezone, @"US/Eastern")
                .With(x => x.Emails, null)
                .With(x => x.PhoneNumbers, null)
                .With(x => x.Ims, null)
                .With(x => x.Photos, null)
                .With(x => x.Addresses, null)
                .With(x => x.X509Certificates, null)
                .Create(seed: new ScimUser2());

            // Insert the first user so there's one already in-memory.
            await (await Server
                .HttpClient
                .PostAsync(
                    new UriBuilder(new Uri("http://localhost/v2/users"))
                    {
                        Query = "attributes=" + string.Join(",", Attributes ?? new List<string>())
                    }.ToString(), new ScimObjectContent<ScimUser>(existingUser))
                .AwaitResponse()
                .AsTask).DeserializeTo(() => JsonResponse);
        };

        protected static IList<string> Attributes;

        protected static IDictionary<string, object> JsonResponse;
    }

    public class when_post_quering : using_a_scim_server
    {
        Because of = async () =>
        {
            var autoFixture = new Fixture();

            var existingUser = autoFixture.Build<ScimUser2>()
                .With(x => x.UserName, UserNameUtility.GenerateUserName())
                .With(x => x.Password, "somePass!2")
                .With(x => x.PreferredLanguage, "en-US,en,es")
                .With(x => x.Locale, "en-US")
                .With(x => x.Timezone, @"US/Eastern")
                .With(x => x.Emails, null)
                .With(x => x.PhoneNumbers, null)
                .With(x => x.Ims, null)
                .With(x => x.Photos, null)
                .With(x => x.Addresses, null)
                .With(x => x.X509Certificates, null)
                .Create(seed: new ScimUser2());

            // Insert the first user so there's one already in-memory.
            await Server.HttpClient.PostAsync(
                    new UriBuilder(new Uri("http://localhost/v2/users")).ToString(), 
                    new ScimObjectContent<ScimUser>(existingUser)).AwaitResponse().AsTask;

            await (await Server
                .HttpClient
                .PostAsync(
                    new UriBuilder(new Uri("http://localhost/v2/users/.search")).ToString(), 
                    new StringContent(string.Format("{{\"attributes\": \"{0}\"}}", Attributes), Encoding.UTF8, "application/json"))
                .AwaitResponse()
                .AsTask).DeserializeTo(() => JsonResponse).AwaitResponse().AsTask;
        };

        protected static IList<string> Attributes;

        protected static IDictionary<string, object> JsonResponse;
    }
}