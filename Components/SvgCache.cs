using System;
using System.Reflection;

namespace CollectionViewIssues.Components;

public class SvgCache
{
    private static Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();
    public static void ClearCache()
    {
        _cache.Clear();
    }
    public static byte[] GetSvgFromEmbeddedResource(string name)
    {
        //-- Name für Zugriff auf Embedded-Ressourcen
        if (!Path.HasExtension(name))
            name += ".svg";

        name = "Images." + name;

        //-- im Cache nachsehen
        string cacheName = name;
        if (_cache.ContainsKey(cacheName))
        {
            return _cache[cacheName];
        }

        //-- nicht im Cache - dann jetzt in den Embedded-Ressoursen nachsehen
        var svg = TryLoadSvg(name);

        if (svg != null)
        {
            lock (_cache)
            {
                try
                {
                    if (!_cache.ContainsKey(cacheName))
                        _cache.Add(cacheName, svg);
                }
                catch (Exception ex)
                {
                    //ex.Trace("SvgCache (add to cache)");
                }
            }
        }
        return svg;
    }
    
    private static byte[] TryLoadSvg(string name)
    {
        Assembly AssemblyCache = Assembly.GetCallingAssembly();
        if (AssemblyCache == null)
            return null;

        string svgName = System.IO.Path.HasExtension(name) ? System.IO.Path.ChangeExtension(name, ".svg") : name + ".svg";

        var manifestResourceNames = AssemblyCache.GetManifestResourceNames();
        string realRessource =
            manifestResourceNames
            .FirstOrDefault(x => x.EndsWith("." + svgName, StringComparison.CurrentCultureIgnoreCase));

        if (string.IsNullOrEmpty(realRessource))
        {
            manifestResourceNames = AssemblyCache.GetManifestResourceNames();
            realRessource =
                manifestResourceNames
                .FirstOrDefault(x => x.EndsWith("." + svgName, StringComparison.CurrentCultureIgnoreCase));
        }

        if (string.IsNullOrEmpty(realRessource))
            return null;

        var stream = AssemblyCache.GetManifestResourceStream(realRessource);
        if (stream == null)
            return null;

        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        stream.CopyTo(ms);
        byte[] result = ms.ToArray();

        stream.Close();
        stream.Dispose();
        ms.Close();
        ms.Dispose();

        return result;
    }
}
