using UnityEngine;
using UnityEngine.UIElements;

public static class UIToolkitConverts
{
    public struct IntConverters {
        public static string ResolveInt(int value)
        {
            return value == 0 ? "--" : value.ToString();
        }

        public static string ResolveFloat(float value)
        {
            return value == 0 ? "--" : value.ToString();
        }
    }

    static bool registered;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif // UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void Register()
    {
        if (registered)
        {
            return;
        }

        registered = true;

        RegisterIntConverters();
    }

    private static void RegisterIntConverters()
    {
        var group = new ConverterGroup(nameof(IntConverters));
        group.AddConverter((ref float v) => IntConverters.ResolveFloat(v));
        group.AddConverter((ref int v) => IntConverters.ResolveInt(v));
        ConverterGroups.RegisterConverterGroup(group);
    }

}
