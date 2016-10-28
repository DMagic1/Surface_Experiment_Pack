#region license
/*The MIT License (MIT)
SEP_Style - Unity element for marking UI styles

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

using UnityEngine;
using UnityEngine.UI;

namespace SEPScience.Unity.Unity
{
	public class SEP_Style : MonoBehaviour
	{
		public enum ElementTypes
		{
			None,
			Window,
			Box,
			Button,
			ToggleButton,
			VertScroll,
			Slider,
			SliderBackground,
		}

		[SerializeField]
		private ElementTypes elementType = ElementTypes.None;

		public ElementTypes ElementType
		{
			get { return elementType; }
		}

		private void setSelectable(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			Selectable select = GetComponent<Selectable>();

			if (select == null)
				return;

			select.image.sprite = normal;
			select.image.type = Image.Type.Sliced;
			select.transition = Selectable.Transition.SpriteSwap;

			SpriteState spriteState = select.spriteState;
			spriteState.highlightedSprite = highlight;
			spriteState.pressedSprite = active;
			spriteState.disabledSprite = inactive;
			select.spriteState = spriteState;
		}

		public void setScrollbar(Sprite background, Sprite thumb)
		{
			Image back = GetComponent<Image>();

			if (back == null)
				return;

			back.sprite = background;

			Scrollbar scroll = GetComponent<Scrollbar>();

			if (scroll == null)
				return;

			if (scroll.targetGraphic == null)
				return;

			Image scrollThumb = scroll.targetGraphic.GetComponent<Image>();

			if (scrollThumb == null)
				return;

			scrollThumb.sprite = thumb;
		}

		public void setImage(Sprite sprite, Image.Type type)
		{
			Image image = GetComponent<Image>();

			if (image == null)
				return;

			image.sprite = sprite;
			image.type = type;
		}

		public void setButton(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setSelectable(normal, highlight, active, inactive);
		}

		public void setToggle(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setSelectable(normal, highlight, active, inactive);

			Image onImage = GetComponentsInChildren<Image>(true)[1];

			if (onImage == null)
				return;

			onImage.sprite = active;
			onImage.type = Image.Type.Sliced;
		}

		public void setSlider(Sprite background, Sprite foreground, Color backColor, Color foreColor)
		{
			if (background == null || foreground == null)
				return;

			Slider slider = GetComponent<Slider>();

			if (slider == null)
				return;

			Image back = slider.GetComponentInChildren<Image>();

			if (back == null)
				return;

			back.sprite = background;
			back.color = backColor;
			back.type = Image.Type.Sliced;

			RectTransform fill = slider.fillRect;

			if (fill == null)
				return;

			Image front = fill.GetComponentInChildren<Image>();

			if (front == null)
				return;

			front.sprite = foreground;
			front.color = foreColor;
			front.type = Image.Type.Sliced;
		}

		public void setSlider(Sprite foreground, Color foreColor)
		{
			if (foreground == null)
				return;

			Slider slider = GetComponent<Slider>();

			if (slider == null)
				return;

			RectTransform fill = slider.fillRect;

			if (fill == null)
				return;

			Image front = fill.GetComponentInChildren<Image>();

			if (front == null)
				return;

			front.sprite = foreground;
			front.color = foreColor;
			front.type = Image.Type.Sliced;
		}

	}
}
