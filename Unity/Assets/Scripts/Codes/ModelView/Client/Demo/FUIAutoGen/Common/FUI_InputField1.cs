/** This is an automatically generated class by FUICodeSpawner. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace ET.Client.Common
{
	public partial class FUI_InputField1: GComponent
	{
		public GImage n0;
		public GTextInput Input;
		public GTextField simpleText;
		public GRichTextField richText;
		public const string URL = "ui://f2boiu4iab9e2";

		public static FUI_InputField1 CreateInstance()
		{
			return (FUI_InputField1)UIPackage.CreateObject("Common", "InputField1");
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			n0 = (GImage)GetChildAt(0);
			Input = (GTextInput)GetChildAt(1);
			simpleText = (GTextField)GetChildAt(2);
			richText = (GRichTextField)GetChildAt(3);
		}
	}
}
