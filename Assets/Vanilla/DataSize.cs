namespace Vanilla.StringFormatting
{

	public static class DataSize
	{

		public const long Kb = 1  * 1024;
		public const long Mb = Kb * 1024;
		public const long Gb = Mb * 1024;
		public const long Tb = Gb * 1024;
		public const long Pb = Tb * 1024;
		public const long Eb = Pb * 1024;


		public static string AsDataSize(this long bytes)
		{
			if (bytes < Kb) return $"{bytes:0.##}b";

			if (bytes >= Kb &&
			    bytes < Mb) return $"{(double)bytes / Kb:0.##}Kb";

			if (bytes >= Mb &&
			    bytes < Gb) return $"{(double)bytes / Mb:0.##}Mb";

			if (bytes >= Gb &&
			    bytes < Tb) return $"{(double)bytes / Gb:0.##}Gb";

			if (bytes >= Tb &&
			    bytes < Pb) return $"{(double)bytes / Tb:0.##}Tb";

			if (bytes >= Pb &&
			    bytes < Eb) return $"{(double)bytes / Pb:0.##}Pb";

			if (bytes >= Eb) return $"{(double)bytes / Eb:0.##}Eb";

			return "a whole bunch";
		}


		public static string AsDataSize(this ulong bytes)
		{
			if (bytes < Kb) return $"{bytes:0.##}b";

			if (bytes >= Kb &&
			    bytes < Mb) return $"{(double)bytes / Kb:0.##}Kb";

			if (bytes >= Mb &&
			    bytes < Gb) return $"{(double)bytes / Mb:0.##}Mb";

			if (bytes >= Gb &&
			    bytes < Tb) return $"{(double)bytes / Gb:0.##}Gb";

			if (bytes >= Tb &&
			    bytes < Pb) return $"{(double)bytes / Tb:0.##}Tb";

			if (bytes >= Pb &&
			    bytes < Eb) return $"{(double)bytes / Pb:0.##}Pb";

			if (bytes >= Eb) return $"{(double)bytes / Eb:0.##}Eb";

			return "a whole bunch";
		}
		
	}

}