namespace FeatureServices
{
    public class FeatureToggle
    {
        public string Name { get; set; }
        public string ApiKey { get; internal set; }
        public string Key { get; internal set; }
    }

    public class FeatureToggle<T> : FeatureToggle
    {
        public T Value { get; set; }
    }
}
