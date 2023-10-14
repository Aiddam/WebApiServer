namespace Server.Interfaces
{
    public interface IDependencyService
    {
        public void AddTransient<TInterface, TType>() where TInterface : class where TType : class;

        public void AddTransient(Type typeInterface, Type type);
    }
}
