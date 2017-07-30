namespace Helper
{
    public class ServiceLocator
    {
        private static IServiceLocator _i;

        public static IServiceLocator I
        {
            get { return _i ?? (_i = new ServiceLocatorInstance()); }
        }
    }
}

