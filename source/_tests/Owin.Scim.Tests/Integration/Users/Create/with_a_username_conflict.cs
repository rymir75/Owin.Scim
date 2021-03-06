namespace Owin.Scim.Tests.Integration.Users.Create
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;

    using Machine.Specifications;

    using Model.Users;

    using v2.Model;

    public class with_a_username_conflict : when_creating_a_user
    {
        Establish context = async () =>
        {
            UserDto = new ScimUser2
            {
                UserName = UserNameUtility.GenerateUserName()
            };

            // Insert the first user so there's one already in-memory.
            Response = await Server
                .HttpClient
                .PostAsync("v2/users", new ObjectContent<ScimUser>(UserDto, new JsonMediaTypeFormatter()))
                .AwaitResponse()
                .AsTask;
        };

        It should_return_conflict = () => Response.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
    }
}