using Acnys.Core.Request;

namespace Sample.Api.Facade
{
    public class TestQuery : Query<string>
    {
        public string Data { get; }

        public TestQuery(string data)
        {
            Data = data;
        }
    }
}