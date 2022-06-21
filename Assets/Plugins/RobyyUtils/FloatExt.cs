namespace Plugins.RobyyUtils
{
    public static class FloatExt {
        public static float Remap (this float value, float minFrom, float maxFrom, float minTo, float maxTo) {
            return (value - minFrom) / (maxFrom - minFrom) * (maxTo - minTo) + minTo;
        }

        public static float Remap01(this float value, float min, float max) => value.Remap(min, max, 0f,1f);
    }
}