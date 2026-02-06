using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;

namespace QFramework.Example
{
	public class UIGameOverPanelData : UIPanelData
	{
	}
	public partial class UIGameOverPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameOverPanelData ?? new UIGameOverPanelData();
			// please add init code here
			ActionKit.OnUpdate.Register(() =>
			{
				if (Input.anyKeyDown)
				{
					SceneManager.LoadScene("GamblingMain");
					this.CloseSelf();
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
			
		}
		
		protected override void OnShow()
		{

		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
		
		
	}
}
