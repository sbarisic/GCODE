using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using static GCODE.GCODE;

namespace GCODE {
	class Program {
		static void Main(string[] args) {
			METRIC();

			Feed(400);
			SelectTool(1);

			Rapid(Z: 1);
			Rapid(X: 1, Y: 1);
			Spindle(2000);

			for (int i = 0; i < 2; i++) {
				float Offset = 1.3f;
				Comment("Start rectangle " + i);

				Linear(X: 1 + i * Offset, Y: 1 + i * Offset);
				Linear(Z: -1);
				Linear(X: 9 - i * Offset);
				Linear(Y: 9 - i * Offset);
				Linear(X: 1 + i * Offset);
				Linear(Y: 1 + i * Offset);
				Linear(Z: 1);

				Comment("Stop rectangle " + i);
			}

			Spindle(0);
			Rapid(X: -1, Y: -1);

			File.WriteAllText("test.gcode", GenerateGCode());
		}
	}
}
