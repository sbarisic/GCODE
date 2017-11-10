using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;
using System.Drawing;

using num = System.Single;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace GCODE {
	public class CodeGenerator {
		StringBuilder Code;
		int LineNum;
		int LineInc;

		public CodeGenerator() {
			Code = new StringBuilder();
			LineNum = 10;
			LineInc = 10;
		}

		public void EmitLine(string Src) {
			string F = string.Format("N{0} ", LineNum);

			Console.Write(F);
			Code.Append(F);
			LineNum += LineInc;

			Console.WriteLine(Src);
			Code.AppendLine(Src);
		}

		public void EmitCode(string Code, params string[] Args) {
			EmitLine(string.Format("{0} {1}", Code, string.Join(" ", Args)).Trim());
		}

		public void EmitCode(string Code, int Num, params string[] Args) {
			EmitCode(string.Format("{0}{1}", Code, Num), Args);
		}

		public string Compile() {
			return Code.ToString();
		}
	}

	public enum Units {
		Imperial = 1,
		Metric = 2
	}

	public static class GCODE {
		static CodeGenerator Gen;

		static GCODE() {
			Gen = new CodeGenerator();
			Units = Units.Metric;
			Offset = new Vector3(0, 0, 0);
			ToolDrawDepth = 2;
		}

		static string ToStr<T>(T Val) {
			if (typeof(T) == typeof(num))
				return ((num)(object)Val).ToString(CultureInfo.InvariantCulture);
			return Val.ToString();
		}

		static string ToStr<T>(string Prefix, T Val) {
			return Prefix + ToStr(Val);
		}

		// PUBLIC

		static Units _Units = (Units)0;
		public static Units Units {
			get {
				return _Units;
			}
			set {
				if (_Units != value) {
					_Units = value;

					if (_Units == Units.Imperial)
						Gen.EmitCode("G", 20);
					else if (_Units == Units.Metric) {
						Gen.EmitCode("G", 21);
					} else
						throw new Exception("Invalid unit type " + _Units);
				}
			}
		}

		public static Vector3 Position {
			get;
			private set;
		}

		public static Vector3 Offset;
		public static float ToolDrawDepth;

		public static string Compile() {
			return Gen.Compile();
		}

		public static void Feed(num Val) {
			Gen.EmitCode("F" + ToStr(Val));
		}

		public static void Comment(string Txt) {
			// TODO
			//Emit(string.Format(";({0})", Txt));
		}

		// G codes

		public static void Rapid(Vector3 Pos) {
			Position = Pos += Offset;
			Gen.EmitCode("G", 0, ToStr("X", Pos.X), ToStr("Y", Pos.Y), ToStr("Z", Pos.Z));
		}

		public static void Rapid(float X, float Y, float Z) {
			Rapid(new Vector3(X, Y, Z));
		}

		public static void Linear(Vector3 Pos) {
			Position = Pos += Offset;
			Gen.EmitCode("G", 1, ToStr("X", Pos.X), ToStr("Y", Pos.Y), ToStr("Z", Pos.Z));
		}

		public static void Linear(float X, float Y, float Z) {
			Linear(new Vector3(X, Y, Z));
		}

		public static void Rectangle(float W, float H) {
			Vector3 S = Position;
			Vector3 OldOffset = Offset;
			Offset = Vector3.Zero;
			Linear(S + new Vector3(W, 0, 0));
			Linear(S + new Vector3(W, H, 0));
			Linear(S + new Vector3(0, H, 0));
			Linear(S);
			Offset = OldOffset;
		}

		public static RectangleF Path(GraphicsPath P) {
			Vector2 Scale = new Vector2(1, -1);

			PointF[] Points = P.PathData.Points;
			byte[] Types = P.PathData.Types;

			int LastFirst = 0;
			int i = 0;

			Vector3 Start = Position;
			Vector3 OldOffset = Offset;
			Offset = Start + Offset;

			for (i = 0; i < Points.Length; i++) {
				byte PointType = (byte)(Types[i] & 0x7);
				bool FirstPoint = Types[i] == 0;
				bool LastPoint = (Types[i] & 0x80) != 0;

				if (FirstPoint)
					LastFirst = i;

				float X = Points[i].X * Scale.X;
				float Y = Points[i].Y * Scale.Y;

				if (FirstPoint)
					Rapid(X, Y, 0);

				Linear(X, Y, -ToolDrawDepth);

				if (LastPoint) {
					X = Points[LastFirst].X * Scale.X;
					Y = Points[LastFirst].Y * Scale.Y;

					Linear(X, Y, -ToolDrawDepth);
					Linear(X, Y, 0);
				}
			}

			Offset = OldOffset;

			RectangleF R = P.GetBounds();
			R.Y = -R.Y;
			R.Height = -R.Height;
			return R;
		}

		public static RectangleF PathText(string Txt, string FontFile, float FontSize, StringAlignment H = StringAlignment.Near, StringAlignment V = StringAlignment.Far) {
			PrivateFontCollection Collection = new PrivateFontCollection();
			Collection.AddFontFile(FontFile);
			Font F = new Font(Collection.Families[0], FontSize);

			StringFormat SF = new StringFormat();
			SF.Alignment = H;
			SF.LineAlignment = V;

			GraphicsPath P = new GraphicsPath();
			P.AddString(Txt, F.FontFamily, (int)F.Style, F.Size, new PointF(0, 0), SF);
			P.Flatten();

			return Path(P);
		}

		// M codes

		public static void SelectTool(int Num) {
			Gen.EmitCode("M", 6, ToStr("T", Num));
		}

		public static void Spindle(int RPM, bool CCW = false) {
			if (RPM == 0) {
				Gen.EmitCode("M", 5);
				return;
			}

			Gen.EmitCode("G", 97);
			int Code = CCW ? 4 : 3;
			Gen.EmitCode("M", Code, ToStr("S", RPM));
		}
	}
}
