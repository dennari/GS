using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using EventStore;
using Growthstories.Core;
using CommonDomain;
using Growthstories.Domain.Messaging;

namespace SimpleTesting
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public abstract class spec_syntax : IListSpecifications
    {
        readonly List<IEvent> _given = new List<IEvent>();

        ICommand _when;
        readonly List<IEvent> _then = new List<IEvent>();
        readonly List<IEvent> _givenEvents = new List<IEvent>();
        bool _thenWasCalled;

        protected virtual Guid Id
        {
            get
            {
                return Guid.Empty;
            }

        }

        protected static DateTime Date(int year, int month = 1, int day = 1, int hour = 0)
        {
            return new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Unspecified);
        }

        protected static DateTime Time(int hour, int minute = 0, int second = 0)
        {
            return new DateTime(2011, 1, 1, hour, minute, second, DateTimeKind.Unspecified);
        }

        protected abstract void SetupServices();
        protected abstract IStoreEvents GetEventStore();


        protected class ExceptionThrown : EventBase, IAmUsedByUnitTests
        {
            private string Name;


            public ExceptionThrown(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return string.Format("Domain error '{0}'", Name);
            }


        }

        protected IEvent ClockWasSet(int year, int month = 1, int day = 1)
        {
            var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            return new SpecSetupEvent(() => Current.DateIs(date), "Test clock set to {0:yyyy-MM-dd}", date);
        }

        protected IEvent GuidWasFixed(string guid)
        {
            return new SpecSetupEvent(() => Current.GuidIs(guid), "Guid provider fixed to " + guid);
        }

        public void Given(params IEvent[] g)
        {
            _given.AddRange(g);
            foreach (var @event in g)
            {
                var setup = @event as SpecSetupEvent;
                if (setup != null)
                {
                    setup.Apply();
                }
                else _givenEvents.Add(@event);
            }
        }

        public void When(ICommand command)
        {
            _when = command;
        }

        [SetUp]
        public void Clear()
        {
            _when = null;
            _given.Clear();
            _then.Clear();
            _thenWasCalled = false;
            _givenEvents.Clear();
            SetupServices();
        }
        [TearDown]
        public void Teardown()
        {
            if (!_thenWasCalled)
                Assert.Fail("THEN was not called from the unit test");
        }

        protected void PrintSpecification()
        {
            Console.WriteLine("Test:          {0}", GetType().Name.Replace("_", " "));
            Console.WriteLine("Specification: {0}", TestContext.CurrentContext.Test.Name.Replace("_", " "));

            Console.WriteLine();
            if (_given.Any())
            {
                Console.WriteLine("GIVEN:");

                for (var i = 0; i < _given.Count; i++)
                {
                    PrintAdjusted("  " + (i + 1) + ". ", Describe.Object(_given[i]).Trim());
                }
            }
            else
            {
                Console.WriteLine("GIVEN no events");
            }

            if (_when != null)
            {
                Console.WriteLine();
                Console.WriteLine("WHEN:");
                PrintAdjusted("  ", Describe.Object(_when).Trim());
            }

            Console.WriteLine();

            if (_then.Any())
            {
                Console.WriteLine("THEN:");
                for (int i = 0; i < _then.Count; i++)
                {
                    PrintAdjusted("  " + (i + 1) + ". ", Describe.Object(_then[i]).Trim());
                }
            }
            else
            {
                Console.WriteLine("THEN nothing.");
            }
        }

        protected void PrintResults(ICollection<ExpectResult> exs)
        {
            var results = exs.ToArray();
            var failures = results.Where(f => f.Failure != null).ToArray();
            if (!failures.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Results: [Passed]");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Results: [Failed]");

            for (int i = 0; i < results.Length; i++)
            {
                PrintAdjusted("  " + (i + 1) + ". ", results[i].Expectation);
                PrintAdjusted("     ", results[i].Failure ?? "PASS");
            }
        }

        protected abstract void ExecuteCommand(IStoreEvents store, ICommand cmd);

        public void Expect(string error)
        {
            Expect(new ExceptionThrown(error));
        }

        bool _dontExecuteOnExpect;

        public void Expect(params IEvent[] g)
        {
            _thenWasCalled = true;
            if (g.Count() == 0)
            {
                return;
            }
            _then.AddRange(g);

            IEnumerable<IEvent> actual;
            if (_dontExecuteOnExpect) return;

            var store = GetEventStore();
            //using (var store = GetEventStore())
            //{
            if (_givenEvents.Count > 0)
            {

                using (var stream = store.OpenStream(Id, 0, int.MaxValue))
                {
                    foreach (var @event in _givenEvents)
                    {
                        var msg = new EventMessage { Body = @event };
                        //msg.Headers
                        stream.Add(msg);
                    }

                    stream.CommitChanges(Guid.NewGuid());
                }
            }

            try
            {
                ExecuteCommand(store, _when);
                using (var stream = store.OpenStream(Id, 0, int.MaxValue))
                {
                    actual = stream.CommittedEvents.Skip(_givenEvents.Count).Select(x => (IEvent)x.Body).ToArray();
                }

            }
            catch (DomainError e)
            {
                actual = new IEvent[] { new ExceptionThrown(e.Name) };
            }
            //}

            var results = CompareAssert(_then.ToArray(), actual.ToArray()).ToArray();

            PrintSpecification();
            PrintResults(results);

            if (results.Any(r => r.Failure != null))
                Assert.Fail("Specification failed");
        }

        public static string GetAdjusted(string adj, string text)
        {
            var first = true;
            var builder = new StringBuilder();
            foreach (var s in text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                builder.Append(first ? adj : new string(' ', adj.Length));
                builder.AppendLine(s);
                first = false;
            }
            return builder.ToString();
        }

        static void PrintAdjusted(string adj, string text)
        {
            Console.Write(GetAdjusted(adj, text));
        }


        protected static CompareObjects Comparer = new CompareObjects()
                {
                    MaxDifferences = 10,
                    ElementsToIgnore = new List<string>()
                    {
                        "Created",
                        "AggregateType"
                        //"EventId"
                    }
                };




        protected static IEnumerable<ExpectResult> CompareAssert(
            IEvent[] expected,
            IEvent[] actual)
        {
            var max = Math.Max(expected.Length, actual.Length);

            for (int i = 0; i < max; i++)
            {
                var ex = expected.Skip(i).FirstOrDefault();
                var ac = actual.Skip(i).FirstOrDefault();

                var expectedString = ex == null ? "No event expected" : Describe.Object(ex);
                var actualString = ac == null ? "No event actually" : Describe.Object(ac);

                var result = new ExpectResult { Expectation = expectedString };

                if (!Comparer.Compare(ex, ac))
                {
                    var realDiff = Comparer.DifferencesString
                       .Trim('\r', '\n')
                       .Replace("object1", "expected")
                       .Replace("object2", "actual");
                    var stringRepresentationsDiffer = expectedString != actualString;

                    result.Failure = stringRepresentationsDiffer ?
                        GetAdjusted("Was:  ", actualString) :
                        GetAdjusted("Diff: ", realDiff);
                }

                yield return result;
            }
        }

        public class ExpectResult
        {
            public string Failure;
            public string Expectation;
        }


        public IEnumerable<Specification> GetAll()
        {
            var type = GetType();
            if (type.IsAbstract)
                yield break;
            _dontExecuteOnExpect = true;

            var myMethods = GetType().GetMethods().Where(m => m.IsDefined(typeof(TestAttribute), true)).ToArray();
            foreach (var method in myMethods)
            {
                Clear();
                method.Invoke(this, null);
                yield return new Specification()
                {
                    CaseName = method.Name.Replace("_", " "),
                    GroupName = type.Name.Replace("_", " "),
                    Given = _givenEvents.ToArray(),
                    When = _when,
                    Then = _then.ToArray()
                };
            }
        }
    }


    public sealed class SpecSetupEvent : EventBase, IAmUsedByUnitTests
    {

        readonly string _describe;
        public readonly Action Apply;

        public SpecSetupEvent(Action apply, string describe, params object[] args)
        {
            Apply = apply;
            _describe = string.Format(describe, args);
        }
        public override string ToString()
        {
            return _describe;
        }


    }

    public interface IListSpecifications
    {
        IEnumerable<Specification> GetAll();
    }

    public class Specification
    {
        public string GroupName;
        public string CaseName;
        public IEvent[] Given;
        public ICommand When;
        public IEvent[] Then;
    }
    public interface IAmUsedByUnitTests { }


    public static class Current
    {
        static Func<DateTime> _getTime = GetUtc;
        static Func<Guid> _getGuid = GetGuid;

        static DateTime GetUtc()
        {
            return new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified);
        }
        static Guid GetGuid()
        {
            return Guid.NewGuid();
        }

        public static void DateIs(DateTime time)
        {
            _getTime = () => new DateTime(time.Ticks, DateTimeKind.Unspecified);
        }

        public static readonly DateTime MaxValue = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);


        public static void DateIs(int year, int month = 1, int day = 1)
        {
            DateIs(new DateTime(year, month, day));
        }

        public static void GuidIs(Guid guid)
        {
            _getGuid = () => guid;
        }
        public static void GuidIs(string guid)
        {
            var g = Guid.Parse(guid);
            _getGuid = () => g;
        }

        public static void Reset()
        {
            _getTime = GetUtc;
            _getGuid = GetGuid;

        }

        public static DateTime UtcNow { get { return _getTime(); } }
        public static Guid NewGuid { get { return _getGuid(); } }
    }

    public static class Describe
    {

        public static readonly IDictionary<Type, MethodInfo> Dict = ToDictionary();

        static Dictionary<Type, MethodInfo> ToDictionary()
        {
            var classes = typeof(Describe).Assembly.GetTypes()
                .Where(t => t.Name.StartsWith("Describ"));


            return classes.SelectMany(c => c.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(m => m.Name == "When")
                .Where(m => m.GetParameters().Length == 1))
                .ToDictionary(m => m.GetParameters().First().ParameterType, m => m);
        }


        public static string Object(object e)
        {
            MethodInfo info;
            var type = e.GetType();
            if (!Dict.TryGetValue(type, out info))
            {
                return e.ToString();
            }
            try
            {
                return (string)info.Invoke(null, new[] { e });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }


        public static bool TryDescribe(object e, out string description)
        {
            MethodInfo info;
            description = null;

            var type = e.GetType();
            if (!Dict.TryGetValue(type, out info))
            {
                return false;
            }
            try
            {
                description = (string)info.Invoke(null, new[] { e });
                return true;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }

}