#region license
/*The MIT License (MIT)
SEP_TextMeshProHolder - Monobehaviour for loading and process UI prefabs

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

using SEPScience.Unity;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace SEPScience.SEP_UI
{
	/// <summary>
	/// Extension of the TextMeshProUGUI class that allows for text field updating through Unity Events
	/// </summary>
	public class SEP_TextMeshProHolder : TextMeshProUGUI
	{
		/// <summary>
		/// Reference to the text script attached to the same GameObject; attach the script either in the Unity Editor
		/// or through code at startup
		/// </summary>
		private TextHandler _handler;

		/// <summary>
		/// Get the attached TextHandler and add an event listener
		/// </summary>
		new private void Awake()
		{
			base.Awake();

			_handler = GetComponent<TextHandler>();

			if (_handler == null)
				return;

			_handler.OnTextUpdate.AddListener(new UnityAction<string>(UpdateText));
			_handler.OnColorUpdate.AddListener(new UnityAction<Color>(UpdateColor));
		}

		/// <summary>
		/// A Unity Event listener used to update the text field
		/// </summary>
		/// <param name="t"></param>
		private void UpdateText(string t)
		{
			text = t;
		}

		private void UpdateColor(Color c)
		{
			color = c;
		}
	}
}