using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Interfaces
{


    public interface IDomainError
    {
        string Name { get; }
    }

    public class DomainError : Exception, IDomainError
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public DomainError() { }
        public DomainError(string message) : base(message) { }
        public DomainError(string format, params object[] args) : base(string.Format(format, args)) { }

        /// <summary>
        /// Creates domain error exception with a string name, that is easily identifiable in the tests
        /// </summary>
        /// <param name="name">The name to be used to identify this exception in tests.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static DomainError Named(string name, string format, params object[] args)
        {
            var message = "[" + name + "] " + string.Format(format, args);
            return new DomainError(message)
            {
                Name = name
            };
        }

        public string Name { get; private set; }

        public DomainError(string message, Exception inner) : base(message, inner) { }


    }

}
