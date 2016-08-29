using System;
using System.Collections.Generic;
using System.Linq;
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
			ToggleAlwaysOn,
			Label,
			VertScroll,
			Slider,
			SliderBackground
		}

		[SerializeField]
		private ElementTypes elementType = ElementTypes.None;

		public ElementTypes ElementType
		{
			get { return elementType; }
		}

		private void setSelectable(Styles style, Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setText(style, GetComponentInChildren<Text>());

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

		private void setText(Styles style, Text text)
		{
			if (style == null)
				return;

			if (text == null)
				return;

			if (style.Font != null)
				text.font = style.Font;

			text.fontSize = style.Size;
			text.fontStyle = style.Style;
			text.color = style.Color;
		}

		public void setText(Styles style)
		{
			setText(style, GetComponent<Text>());
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
			setSelectable(null, normal, highlight, active, inactive);
		}

		public void setToggle(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setSelectable(null, normal, highlight, active, inactive);

			Toggle toggle = GetComponent<Toggle>();

			if (toggle == null)
				return;

			Image toggleImage = toggle.graphic as Image;

			if (toggleImage == null)
				return;

			toggleImage.sprite = active;
			toggleImage.type = Image.Type.Sliced;
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
