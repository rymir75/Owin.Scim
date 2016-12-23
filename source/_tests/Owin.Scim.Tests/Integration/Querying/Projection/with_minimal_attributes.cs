namespace Owin.Scim.Tests.Integration.Querying.Projection
{
    using System.Collections.Generic;

    using Machine.Specifications;

    public class with_minimal_attributes : when_requesting_specific_attributes
    {
        Establish context = () => Attributes = new List<string> { "userName" };

        It should_return_requested_attributes_and_returned_always = 
            () => 
                JsonResponse.Keys.ShouldContainOnly(
                    new List<string>
                    {
                        "schemas",
                        "id",
                        "userName"
                    });
    }

    public class with_some_attributes : when_post_quering
    {
        Establish context = () => Attributes = new List<string> { "userName,externalId" };

        It should_return_requested_attributes_and_returned_always =
            () =>
                JsonResponse.Keys.ShouldContainOnly(
                    new List<string>
                    {
                        "id",
                        "userName",
                        "externalId"
                    });
    }
}