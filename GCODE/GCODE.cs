using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using num = System.Single;

namespace GCODE {
	public static class GCODE {
		static StringBuilder Out;

		static GCODE() {
			Out = new StringBuilder();
		}

		static string ToArg<T>(string VariableName, T Val) {
			if (Val == null)
				return "";

			return VariableName + Val.ToString() + " ";
		}

		public static void Emit(string Src) {
			Out.AppendLine(Src.Trim());
		}

		public static string GenerateGCode() {
			Emit("M02");

			return Out.ToString().Replace(',', '.');
		}

		// Other

		public static void Feed(num Val) {
			Emit("F" + Val);
		}

		public static void Comment(string Txt) {
			Emit(string.Format(";({0})", Txt));
		}

		// G codes

		public static void IMPERIAL() => Emit("G20");
		public static void METRIC() => Emit("G21");

		public static void Rapid(num? X = null, num? Y = null, num? Z = null) {
			Emit("G0 " + ToArg("X", X) + ToArg("Y", Y) + ToArg("Z", Z));
		}

		public static void Linear(num? X = null, num? Y = null, num? Z = null) {
			Emit("G1 " + ToArg("X", X) + ToArg("Y", Y) + ToArg("Z", Z));
		}

		// M codes

		public static void SelectTool(int Num) {
			Emit("M06 T" + Num);
		}

		public static void Spindle(int RPM, bool CCW = false) {
			if (RPM == 0) {
				Emit("M05");
				return;
			}

			Emit("G97");
			string Code = CCW ? "M04" : "M03";
			Emit(Code + " S" + RPM);
		}
	}
}
