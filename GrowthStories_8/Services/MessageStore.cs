using Growthstories.Domain.Interfaces;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Services
{
    /// <summary>
    /// Helper class that knows how to store arbitrary messages in append-only store
    /// (including envelopes, audit batches etc)
    /// </summary>
    public class MessageStore
    {
        readonly IAppendOnlyStore _appendOnlyStore;


        readonly IDictionary<string, Type> _contractToType = new Dictionary<string, Type>();
        readonly IDictionary<Type, string> _typeToContract = new Dictionary<Type, string>();



        public void LoadDataContractsFromAssemblyOf(Type type)
        {

            var types = type.Assembly.GetExportedTypes();

            var contracts = types.Where(t => !t.IsAbstract)
                                 .Where(t => t.IsDefined(typeof(DataContractAttribute), false));
            RuntimeTypeModel.Default.AutoAddMissingTypes = true;
            foreach (var contract in contracts)
            {
                var name = ContractEvil.GetContractReference(contract);
                _contractToType.Add(name, contract);
                _typeToContract.Add(contract, name);
                RuntimeTypeModel.Default.Add(contract, true);
            }
            RuntimeTypeModel.Default.CompileInPlace();
        }

        public void Dispose()
        {
            _appendOnlyStore.Close();
            _appendOnlyStore.Dispose();
        }

        public MessageStore(IAppendOnlyStore appendOnlyStore)
        {
            _appendOnlyStore = appendOnlyStore;

        }

        public IEnumerable<StreamRecord> EnumerateMessages(string key, long version, int count)
        {
            var records = _appendOnlyStore.ReadRecords(key, 0, int.MaxValue);
            foreach (var record in records)
            {
                using (var mem = new MemoryStream(record.Data))
                using (var bin = new BinaryReader(mem))
                {
                    var msgCount = bin.ReadInt32();
                    var objects = new object[msgCount];
                    for (int i = 0; i < msgCount; i++)
                    {
                        var name = bin.ReadString();
                        var type = _contractToType[name];
                        var len = bin.ReadInt32();
                        objects[i] = RuntimeTypeModel.Default.Deserialize(bin.BaseStream, null, type, len);
                    }
                    yield return new StreamRecord(record.StreamVersion, key, objects);
                }
            }
        }

        public long GetVersion()
        {
            return _appendOnlyStore.GetCurrentVersion();
        }


        public IEnumerable<StoreRecord> EnumerateAllItems(long startingFrom, int take)
        {
            // we don't use any index = just skip all audit things
            foreach (var record in _appendOnlyStore.ReadRecords(startingFrom, take))
            {
                using (var mem = new MemoryStream(record.Data))
                using (var bin = new BinaryReader(mem))
                {
                    var msgCount = bin.ReadInt32();
                    var objects = new object[msgCount];
                    for (int i = 0; i < msgCount; i++)
                    {
                        try
                        {
                            var name = bin.ReadString();
                            var type = _contractToType[name];
                            var len = bin.ReadInt32();
                            objects[i] = RuntimeTypeModel.Default.Deserialize(bin.BaseStream, null, type, len);
                        }
                        catch (ProtoException ex)
                        {
                            throw new InvalidOperationException(
                                "Failed to deserialize class, probably event store format changed. Please delete all *.dat files.",
                                ex);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            throw new InvalidOperationException("Failed to deserialize class, probably event store format changed. Please delete all *.dat files.", ex);
                        }
                    }
                    yield return new StoreRecord(record.StoreVersion, objects);
                }
            }
        }

        public void AppendToStore(string name, long streamVersion, ICollection<object> messages)
        {
            using (var mem = new MemoryStream())
            using (var bin = new BinaryWriter(mem))
            {
                bin.Write(messages.Count);
                foreach (var message in messages)
                {
                    var contract = _typeToContract[message.GetType()];
                    bin.Write(contract);
                    using (var inner = new MemoryStream())
                    {
                        RuntimeTypeModel.Default.Serialize(inner, message);
                        bin.Write((int)inner.Position);
                        bin.Write(inner.ToArray());
                    }
                }
                _appendOnlyStore.Append(name, mem.ToArray(), streamVersion);
            }
        }
    }

    public struct StreamRecord
    {
        public readonly long StreamVersion;
        public readonly string Key;
        public readonly object[] Items;

        public StreamRecord(long streamVersion, string key, object[] items)
        {
            StreamVersion = streamVersion;
            Key = key;
            Items = items;
        }
    }

    public struct StoreRecord
    {
        public readonly object[] Items;

        public readonly long StreamVersion;



        public StoreRecord(long streamVersion, object[] items)
        {
            Items = items;
            StreamVersion = streamVersion;

        }
    }



    public static class ContractEvil
    {
        public sealed class Helper
        {
            readonly Tuple<string, object>[] _attributes;

            public Helper(IEnumerable<object> attributes)
            {
                _attributes = attributes.Select(a => Tuple.Create(a.GetType().Name, a)).ToArray();
            }

            public string GetString(string name, string property)
            {
                if (_attributes.Length == 0)
                    return null;

                var match = _attributes.FirstOrDefault(t => t.Item1 == name);
                if (null == match)
                    return null;

                var type = match.Item2.GetType();
                var propertyInfo = type.GetProperty(property);
                if (null == propertyInfo)
                    throw new InvalidOperationException(string.Format("{0}.{1} not found", name, property));
                var result = propertyInfo.GetValue(match.Item2, null) as string;
                if (String.IsNullOrEmpty(result))
                    return null;

                return result;
            }
        }

        public static string Combine(params Func<string>[] retriever)
        {
            foreach (var func in retriever)
            {
                string result = func();
                if (null != result)
                    return result;
            }
            return null;
        }



        public static string GetContractReference(Type type)
        {
            var attribs = type.GetCustomAttributes(false);
            var helper = new Helper(attribs);



            var name = Combine(
                () => helper.GetString("ProtoContractAttribute", "Name"),
                () => helper.GetString("DataContractAttribute", "Name"),
                () => helper.GetString("XmlTypeAttribute", "TypeName"),
                () => type.Name);

            var ns = Combine(
                () => helper.GetString("DataContractAttribute", "Namespace"),
                () => helper.GetString("XmlTypeAttribute", "Namespace"));

            if (null != ns)
            {
                ns = ns.Trim() + '/';
            }

            ns = AppendNesting(ns, type);

            return ns + name;
        }

        static string AppendNesting(string ns, Type type)
        {
            var list = new List<string>();
            while (type.IsNested)
            {
                list.Insert(0, type.DeclaringType.Name);
                type = type.DeclaringType;
            }
            if (list.Count == 0)
            {
                return ns;
            }
            var suffix = string.Join("/", Enumerable.ToArray(list)) + "/";
            return ns + suffix;
        }
    }
}
