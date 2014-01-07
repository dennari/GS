using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMobileSample.Model
{
    public interface IThreadIdFactory
    {
        int CurrentId { get; }
    }



    /*
    public static class ThreadStatic
    {
        public static ICurrentThread Current { get }
    }
    */
}
