using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReadingListPlus;

namespace ReadingListPlus.Tests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void StaticLow()
        {
            foreach (var i in Enumerable.Range(0, 100))
            {
                try
                {
                    Console.WriteLine("{0}:", i);
                    Scheduler.GetStaticPosition(Priority.Low, i);
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                }
            }
        }
    }
}
