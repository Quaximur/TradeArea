namespace SlopSquad.Infrastructure.Contexts
{
    public abstract class SceneContext
    {
        public string ToSceneName { get; set; }

        public SceneContext(string toSceneName)
        {
            ToSceneName = toSceneName;
        }
    }
}