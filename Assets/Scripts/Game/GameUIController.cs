using System;
using UnityEngine;
using QFramework;
using QFramework.Example;

namespace Gambling
{
	public partial class GameUIController : ViewController
	{
		void Start()
		{
			UIKit.OpenPanel<UIGamePanel>();
			UIKit.Root.SetResolution(1920, 1080,1);

		}

		private void OnDestroy()
		{
			UIKit.ClosePanel<UIGamePanel>();
		}
	}
}
