namespace FeatureServices
{
    public class FeatureToggle<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }
        public string ApiKey { get; internal set; }
        public string Key { get; internal set; }
    }
}
