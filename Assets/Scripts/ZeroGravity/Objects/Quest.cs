using System.Collections.Generic;
using System.Linq;
using OpenHellion.ProviderSystem;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Quest
	{
		public uint ID;

		public List<QuestTrigger> QuestTriggers;

		public QuestTriggerDependencyTpe ActivationDependencyTpe;

		public QuestTriggerDependencyTpe CompletionDependencyTpe;

		public QuestStatus Status;

		public List<uint> DependencyQuests;

		public QuestObject QuestObject;

		public bool CanSkip;

		public string Name
		{
			get
			{
				if (QuestObject != null)
				{
					return Localization.GetLocalizedField(QuestObject.Name, useDefault: true);
				}
				Dbg.Error("Quest has missing QuestObject (Quest ID: " + ID);
				return string.Empty;
			}
		}

		public string Description
		{
			get
			{
				if (QuestObject != null)
				{
					return Localization.GetLocalizedField(QuestObject.Description, useDefault: true);
				}
				Dbg.Error("Quest has missing QuestObject (Quest ID: " + ID);
				return string.Empty;
			}
		}

		public bool IsFinished => Status == QuestStatus.Completed || Status == QuestStatus.Failed;

		public Quest(QuestData data)
		{
			Quest quest = this;
			ID = data.ID;
			QuestObject = Client.Instance.QuestCollection.Quests.FirstOrDefault((QuestObject m) => m.ID == data.ID);
			QuestTriggers = data.QuestTriggers.Select((QuestTriggerData m) => new QuestTrigger(quest, m)).ToList();
			ActivationDependencyTpe = data.ActivationDependencyTpe;
			CompletionDependencyTpe = data.CompletionDependencyTpe;
			DependencyQuests = data.DependencyQuests;
			QuestObject.Quest = this;
			if (QuestObject != null && QuestObject.Achivement != 0)
			{
				ProviderManager.MainProvider.GetAchievement(QuestObject.Achivement, out CanSkip);
			}
		}

		public void SetDetails(QuestDetails details, bool showNotifications = true, bool playCutScenes = true)
		{
			Status = details.Status;
			QuestTrigger questTrigger = null;
			foreach (QuestTriggerDetails qtd in details.QuestTriggers)
			{
				QuestTrigger questTrigger2 = QuestTriggers.FirstOrDefault((QuestTrigger m) => m.ID == qtd.ID);
				if (questTrigger2 != null)
				{
					if (questTrigger2.Status != qtd.Status && qtd.Status == QuestStatus.Completed)
					{
						questTrigger = questTrigger2;
					}
					questTrigger2.SetDetails(qtd, showNotifications);
				}
			}
			if (questTrigger != null)
			{
				Client.Instance.CanvasManager.CanvasUI.QuestTriggerUpdate(questTrigger, playCutScenes);
			}
			if (Status == QuestStatus.Completed && QuestObject != null && QuestObject.Achivement != 0)
			{
				ProviderManager.MainProvider.SetAchievement(QuestObject.Achivement);
			}
		}
	}
}
