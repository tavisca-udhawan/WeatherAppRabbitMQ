namespace Tavisca.Common.Plugins.ServiceCaller
{
    public abstract class AbstractValidator<T>
    {
        public abstract bool Validate(T input);
      
    }
}
