using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ClassLibrary1
{
     /*
     * Metodas nuskaito failus, sukuria įrašus ir juos laiko atmintyje, kad būtų visada greitai pasiekiami. 
     * Faile “full” yra apie 1mln įrašų. Insert ir Delete po ~100 tūkst.
     * Kaip galima būtų jį optimizuoti?
     */
    public class Addresses
    {
        public ConcurrentDictionary<string, Entry> Entries { get; set; } = new ConcurrentDictionary<string, Entry>();
    }

    public class Entry
    {
        public string Id;
    }

    public class Program
    {
        private readonly string _incomingDirectory;
        public static Addresses Cache { get; set; } = new Addresses();

        private async Task ProcessIncomingFilesAsync()
        {
            var files = Directory.EnumerateFiles(_incomingDirectory).ToList();
            var cache = Cache;
            var serializer = new XmlSerializer(typeof(Addresses));

            await Task.Run(() =>
            {
                foreach (var file in files.Where(f => (Path.GetFileName(f) ?? string.Empty).Contains("full")))
                {
                    using (var reader = XmlReader.Create(file))
                    {
                        var obj = (Addresses)serializer.Deserialize(reader);
                        cache.Entries = new ConcurrentDictionary<string, Entry>(obj.Entries.ToDictionary(e => e.Id));
                    }
                }
            });

            await Task.Run(() =>
            {
                foreach (var file in files.Where(f => (Path.GetFileName(f) ?? string.Empty).Contains("insert")))
                {
                    using (var reader = XmlReader.Create(file))
                    {
                        var obj = (Addresses)serializer.Deserialize(reader);
                        Parallel.ForEach(obj.Entries.Values, new ParallelOptions { MaxDegreeOfParallelism = 3 }, entry =>
                        {
                            cache.Entries.TryAdd(entry.Id, entry);
                        });
                    }
                }
            });

            await Task.Run(() =>
            {
                foreach (var file in files.Where(f => (Path.GetFileName(f) ?? string.Empty).Contains("delete")))
                {
                    using (var reader = XmlReader.Create(file))
                    {
                        var obj = (Addresses)serializer.Deserialize(reader);
                        var idsToDelete = obj.Entries.Select(e => e.Id).ToHashSet();
                        foreach (var id in idsToDelete)
                        {
                            cache.Entries.TryRemove(id, out _);
                        }
                    }
                }
            });
        }
    }
}