using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PortScanner
{
	public class Analysis
	{
		public string BLD { get; set; }

		public string UBG { get; set; }

		public string BIL { get; set; }

		public string PRO { get; set; }

		public string NIT { get; set; }

		public string KET { get; set; }

		public string GLU { get; set; }

		public string pH { get; set; }

		public string SG { get; set; }

		public string LEU { get; set; }
	}

	class User
	{
		[JsonProperty("id")]
		public int ID { get; set; }
		[JsonProperty("first_name")]
		public string FirstName { get; set; }
		[JsonProperty("last_name")]
		public string LastName { get; set; }
		[JsonProperty("patronymic")]
		public string Patronymic { get; set; }
		[JsonProperty("snils")]
		public string Snils { get; set; }
	}

	class FulfillUserAnalyse
	{
		[JsonProperty("user")]
		public int User { get; set; }
		[JsonProperty("analyse")]
		public Analysis Analyse { get; set; }
	}
}
