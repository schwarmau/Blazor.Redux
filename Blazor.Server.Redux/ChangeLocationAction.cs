namespace Blazor.Server.Redux
{
    public class ChangeLocationAction : IAction
    {
        public string Location { get; set; }

        public ChangeLocationAction(string location)
        {
            Location = location;
        }
    }
}
