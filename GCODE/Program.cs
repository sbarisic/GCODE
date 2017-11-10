using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Drawing.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using static GCODE.GCODE;

namespace GCODE {
	class Program {
		static void Main(string[] args) {
			Feed(400);
			Rapid(0, 0, 1);
			SelectTool(1);
			Spindle(2000);

			ToolDrawDepth = 1.2f;
			RectangleF R = PathText("Hello", "fonts\\1CamBam_Stick_1.ttf", 10.0f);

			SelectTool(2);
			R.Inflate(1, -1);

			for (int i = 0; i < 4; i++) {
				R.Inflate(0.9f, -0.8f);
				Rapid(R.X, R.Y, 1);
				Linear(R.X, R.Y, -1f);
				Rectangle(R.Width, R.Height);
				Linear(R.X, R.Y, 1);
			}


			Spindle(0);
			Offset = Vector3.Zero;
			Rapid(new Vector3(0, 0, 1));

			File.WriteAllText("test.gcode", Compile());
		}
	}
}
