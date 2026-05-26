using VContainer;

namespace SlopSquad.Infrastructure.Bootstraps
{
    public class RootBoot : Bootstrap
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneLoader>(Lifetime.Singleton);

            //// REGISTRATIONS
            
            ////

            builder.RegisterBuildCallback(OnBuild);
        }

        private void OnBuild(IObjectResolver container)
        {
        }
    }
}