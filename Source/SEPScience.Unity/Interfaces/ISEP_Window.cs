#region license
/*The MIT License (MIT)
ISEP_Window - Interface for the SEP window

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System.Collections.Generic;
using UnityEngine;

namespace SEPScience.Unity.Interfaces
{
	public interface ISEP_Window
	{
		bool IsVisible { get; set; }

		bool IsMinimized { get; set; }

		bool ShowAllVessels { get; }

		float Scale { get; }

		Vector3 WindowPos { get; set; }

		IList<IVesselSection> GetVessels { get; }

		IList<IVesselSection> GetBodyVessels(string body);

		IVesselSection CurrentVessel { get; }

		IList<string> GetBodies { get; }

		string CurrentBody { get; set; }

		void SetAppState(bool on);

		void UpdateWindow();

		void ChangeVessel(bool forward);
	}
}
