using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace UnitTestProject1
{
	public class ProbableXunitException : Exception
	{
		public ProbableXunitException(XunitException innerException) :
			base("A failure was reported but this may be due to asynchrony. Run the test again or with increased delays.", innerException) { }
	}
}
