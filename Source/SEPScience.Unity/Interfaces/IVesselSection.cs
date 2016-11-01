#region license
/*The MIT License (MIT)
IVesselSection - Interface for SEP experiment vessels

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

using System;
using System.Collections.Generic;
using UnityEngine;
using SEPScience.Unity.Unity;

namespace SEPScience.Unity.Interfaces
{
	public interface IVesselSection
	{
		Guid ID { get; }

		float Signal { get; }

		Sprite SignalSprite { get; }

		bool IsConnected { get; }

		bool CanTransmit { get; set; }

		bool AutoTransmitAvailable { get; }

		string ECTotal { get; }

		string Name { get; }

		string Situation { get; }

		string ExpCount { get; }

		bool IsVisible { get; set; }

		IList<IExperimentSection> GetExperiments();

		void setParent(SEP_VesselSection section);

		void PauseAll();

		void StartAll();

		void Update();
	}
}
