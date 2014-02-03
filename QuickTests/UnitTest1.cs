using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace QuickTests
{


    public class MyObject : JObject
    {
        public const string PARENT_ID = "parentId";
        public const string ANCESTOR_ID = "ancestorId";
        public const string TYPE = "type";
        public const string KIND = "kind";

        public readonly IDictionary<string, string> Required = new Dictionary<string, string>()
        {
            {PARENT_ID,null},
            {ANCESTOR_ID,null},
            {TYPE,null},
            {KIND,"plant"}
        };


        public MyObject()
        {
            this[PARENT_ID] = Guid.NewGuid();
        }

        public MyObject(JObject other)
            : base(other)
        {
            if (!this.IsValid(other))
                throw new ArgumentException("not valid");
        }

        private Guid _ParentId = Guid.NewGuid();

        [JsonProperty(PropertyName = PARENT_ID)]
        public Guid Property2
        {
            get { return _ParentId; }
            set { _ParentId = value; }
        }

        private string _Type = "addPlant";

        [JsonProperty(PropertyName = TYPE)]
        public string Property1
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public bool IsValid(JObject other)
        {
            JToken Out = null;
            return Required.All(x => other.TryGetValue(x.Key, out Out) && (x.Value == null || x.Value == (string)((JValue)Out).Value));
        }

    }


    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MyOtherObject
    {
        public const string PARENT_ID = "parentId";
        public const string ANCESTOR_ID = "ancestorId";
        public const string TYPE = "type";
        public const string KIND = "kind";



        public MyOtherObject()
        {
        }


        private Guid _ParentId = Guid.NewGuid();

        [JsonProperty(PropertyName = PARENT_ID, Required = Required.Always)]
        public Guid Property2
        {
            get { return _ParentId; }
            set { _ParentId = value; }
        }

        private string _Type = "addPlant";

        [JsonProperty(PropertyName = TYPE, DefaultValueHandling = DefaultValueHandling.Include)]
        public string Property1
        {
            get { return _Type; }
            set { _Type = value; }
        }
    }



    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {
            var o = new MyObject();
            var p = (JValue)o[MyObject.PARENT_ID];
            //p.

            Assert.IsInstanceOfType(p.Value, typeof(Guid));

            Console.WriteLine(o.ToString());
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));

            var other = new JObject()
            {
                {MyObject.ANCESTOR_ID,new JValue(Guid.NewGuid())},
                {MyObject.PARENT_ID,new JValue(Guid.NewGuid())},
                {MyObject.TYPE,new JValue("SetProperty")},
                {MyObject.KIND,new JValue("plant")},
            };
            Assert.AreEqual(true, o.IsValid(other));
            var other2 = new JObject(other);
            other2[MyObject.KIND] = new JValue("garden");
            Assert.AreEqual(false, o.IsValid(other2));
            var other3 = new JObject(other);
            other3.Remove(MyObject.TYPE);
            Assert.AreEqual(false, o.IsValid(other3));

            var oo = new MyObject(other);
            Console.WriteLine(oo.ToString());


            try
            {
                var ooo = new MyObject(other2);
                throw new UnauthorizedAccessException();
            }
            catch (ArgumentException e)
            { }
        }

        [TestMethod]
        public void TestMethod2()
        {
            var o = new MyOtherObject() { Property1 = "SetProperty" };

            var json = JsonConvert.SerializeObject(o, Formatting.Indented);
            Console.WriteLine(json);


            var oo = JsonConvert.DeserializeObject<MyOtherObject>(json);

            Assert.AreEqual(o.Property1, oo.Property1);
            Assert.AreEqual(o.Property2, oo.Property2);


            try
            {
                var o2 = JsonConvert.DeserializeObject<MyOtherObject>(@"{""type"":""SomeType"",""parentIdd"":""bc98135b-3bfc-4cd4-8a0d-03a424af42c7""}");
                throw new AssertFailedException();
            }
            catch (JsonSerializationException) { }

            var o3 = JsonConvert.DeserializeObject<MyOtherObject>(@"{""parentId"":""bc98135b-3bfc-4cd4-8a0d-03a424af42c7""}");
            var fresh = new MyOtherObject();
            Assert.AreEqual(fresh.Property1, o3.Property1);

        }



        [TestMethod]
        public void TestLogIO()
        {
            try
            {

                Int32 port = 28777;
                string host = "dennari-macbook.lan";
                var message = "+log|my_stream|my_node|info|this is log message\r\n";

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                using (var client = new TcpClient(host, port))
                {
                    client.SendTimeout = 2000;
                    using (var stream = client.GetStream())
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Write(data, 0, data.Length);

                    }
                }



            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }


        }

        [TestMethod]
        public void TestDefaultExceptionString()
        {
            TcpClient client = null;
            try
            {
                var a = client.SendTimeout;
            }
            catch (Exception e)
            {

                Console.WriteLine("Exception: {0}", e);

            }
        }

        [TestMethod]
        public void TestBlobKeyToFileName()
        {

            var t = "AMIfv944Cl22xmFIGM4K-0BdbPBAiZLkxum4k3dn1xeU8dQLnY0HH02ngrSK_iXNZbbsDFtLbldpxGYqxVuvpr3nrQMKZnAeeUL26N0W-IsNorCjCh8xlLpiJ4Az914xhxOoxmi4pPIp3M1bDnMM4FphNYc3T54h3A";
            Regex regEx = new Regex(@"[^a-zA-Z]+");
            t = regEx.Replace(t, "");

            Console.WriteLine(t);
        }


    }
}
