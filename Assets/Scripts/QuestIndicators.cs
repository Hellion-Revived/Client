using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.Helpers;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

public class QuestIndicators : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CAddMarkersOnMap_003Ec__AnonStorey0
	{
		internal QuestTrigger trigger;

		internal bool _003C_003Em__0(KeyValuePair<IMapMainObject, MapObject> m)
		{
			return (m.Key is Ship && (m.Key as Ship).CustomName == trigger.TaskObject.Location.Name) || (m.Key is Asteroid && (m.Key as Asteroid).CustomName == trigger.TaskObject.Location.Name);
		}

		internal bool _003C_003Em__1(KeyValuePair<IMapMainObject, MapObject> m)
		{
			return m.Key is CelestialBody && (m.Key as CelestialBody).GUID == (long)trigger.TaskObject.Celestial;
		}

		internal bool _003C_003Em__2(IMapMainObject m)
		{
			return m is SpaceObjectVessel && (m as SpaceObjectVessel).SceneID == trigger.TaskObject.SceneID && SceneHelper.CompareTags((m as SpaceObjectVessel).VesselData.Tag, trigger.Tag);
		}
	}

	public RectTransform IndicatorParent;

	public GameObject IndicatorPrefab;

	public GameObject NewQuestIndicatorPrefab;

	public GameObject MapQuestIndicatorPrefab;

	public float EdgeOffset = 30f;

	public bool OnMap;

	public Dictionary<SceneQuestTrigger, QuestIndicatorUI> SceneQuestTriggers = new Dictionary<SceneQuestTrigger, QuestIndicatorUI>();

	public Dictionary<QuestTrigger, Tuple<MapObject, QuestIndicatorUI>> MapQuestIndicators = new Dictionary<QuestTrigger, Tuple<MapObject, QuestIndicatorUI>>();

	public void AddQuestIndicator(SceneQuestTrigger sceneQuestTrigger)
	{
		if (!SceneQuestTriggers.ContainsKey(sceneQuestTrigger) && sceneQuestTrigger.Visibility != 0 && sceneQuestTrigger.gameObject.activeInHierarchy)
		{
			QuestIndicatorUI component = Object.Instantiate(IndicatorPrefab, IndicatorParent).GetComponent<QuestIndicatorUI>();
			component.QuestIndicators = this;
			component.SceneQuestTrigger = sceneQuestTrigger;
			component.TaskName.text = sceneQuestTrigger.QuestTrigger.TaskObject.IndicatorName;
			if (sceneQuestTrigger.Visibility == QuestIndicatorVisibility.Proximity)
			{
				component.gameObject.SetActive(false);
			}
			SceneQuestTriggers.Add(sceneQuestTrigger, component);
		}
	}

	public void AddAvailableQuestIndicator(SceneQuestTrigger sceneQuestTrigger)
	{
		if (!SceneQuestTriggers.ContainsKey(sceneQuestTrigger) && sceneQuestTrigger.Visibility != 0 && sceneQuestTrigger.gameObject.activeInHierarchy)
		{
			QuestIndicatorUI component = Object.Instantiate(NewQuestIndicatorPrefab, IndicatorParent).GetComponent<QuestIndicatorUI>();
			component.QuestIndicators = this;
			component.SceneQuestTrigger = sceneQuestTrigger;
			component.TaskName.text = sceneQuestTrigger.QuestTrigger.Quest.Name;
			if (sceneQuestTrigger.Visibility == QuestIndicatorVisibility.Proximity)
			{
				component.gameObject.SetActive(false);
			}
			SceneQuestTriggers.Add(sceneQuestTrigger, component);
		}
	}

	public void RemoveQuestIndicator(SceneQuestTrigger sceneQuestTrigger)
	{
		if (SceneQuestTriggers.ContainsKey(sceneQuestTrigger))
		{
			Object.Destroy(SceneQuestTriggers[sceneQuestTrigger].gameObject);
			SceneQuestTriggers.Remove(sceneQuestTrigger);
		}
	}

	public void StopQuestIndicator(SceneQuestTrigger sceneQuestTrigger)
	{
		if (SceneQuestTriggers.ContainsKey(sceneQuestTrigger))
		{
			if (SceneQuestTriggers[sceneQuestTrigger] != null && !SceneQuestTriggers[sceneQuestTrigger].gameObject.activeInHierarchy)
			{
				Object.Destroy(SceneQuestTriggers[sceneQuestTrigger].gameObject);
				SceneQuestTriggers.Remove(sceneQuestTrigger);
			}
			else if (SceneQuestTriggers[sceneQuestTrigger] != null)
			{
				SceneQuestTriggers[sceneQuestTrigger].Animator.SetTrigger("Stop");
			}
		}
	}

	public void RemoveAllIndicators()
	{
		foreach (SceneQuestTrigger key in SceneQuestTriggers.Keys)
		{
			Object.Destroy(SceneQuestTriggers[key].gameObject);
		}
		SceneQuestTriggers.Clear();
	}

	public void AddMarkersOnMap()
	{
		Quest currentTrackingQuest = Client.Instance.CanvasManager.CanvasUI.CurrentTrackingQuest;
		if (currentTrackingQuest == null)
		{
			return;
		}
		using (List<QuestTrigger>.Enumerator enumerator = currentTrackingQuest.QuestTriggers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				_003CAddMarkersOnMap_003Ec__AnonStorey0 _003CAddMarkersOnMap_003Ec__AnonStorey = new _003CAddMarkersOnMap_003Ec__AnonStorey0();
				_003CAddMarkersOnMap_003Ec__AnonStorey.trigger = enumerator.Current;
				if (_003CAddMarkersOnMap_003Ec__AnonStorey.trigger.Status != QuestStatus.Active)
				{
					continue;
				}
				if (_003CAddMarkersOnMap_003Ec__AnonStorey.trigger.TaskObject.Location != null)
				{
					MapObject value = Client.Instance.Map.AllMapObjects.FirstOrDefault(_003CAddMarkersOnMap_003Ec__AnonStorey._003C_003Em__0).Value;
					if (value != null)
					{
						(value.MainObject as ArtificialBody).RadarVisibilityType = RadarVisibilityType.AlwaysVisible;
						if (!MapQuestIndicators.ContainsKey(_003CAddMarkersOnMap_003Ec__AnonStorey.trigger))
						{
							QuestIndicatorUI component = Object.Instantiate(MapQuestIndicatorPrefab, IndicatorParent).GetComponent<QuestIndicatorUI>();
							component.TaskName.text = _003CAddMarkersOnMap_003Ec__AnonStorey.trigger.Name;
							Tuple<MapObject, QuestIndicatorUI> value2 = new Tuple<MapObject, QuestIndicatorUI>(value, component);
							MapQuestIndicators[_003CAddMarkersOnMap_003Ec__AnonStorey.trigger] = value2;
						}
					}
				}
				else if (_003CAddMarkersOnMap_003Ec__AnonStorey.trigger.TaskObject.Celestial != 0)
				{
					MapObject value3 = Client.Instance.Map.AllMapObjects.FirstOrDefault(_003CAddMarkersOnMap_003Ec__AnonStorey._003C_003Em__1).Value;
					if (value3 != null && !MapQuestIndicators.ContainsKey(_003CAddMarkersOnMap_003Ec__AnonStorey.trigger))
					{
						QuestIndicatorUI component2 = Object.Instantiate(MapQuestIndicatorPrefab, IndicatorParent).GetComponent<QuestIndicatorUI>();
						component2.TaskName.text = _003CAddMarkersOnMap_003Ec__AnonStorey.trigger.Name;
						Tuple<MapObject, QuestIndicatorUI> value4 = new Tuple<MapObject, QuestIndicatorUI>(value3, component2);
						MapQuestIndicators[_003CAddMarkersOnMap_003Ec__AnonStorey.trigger] = value4;
					}
				}
				else
				{
					if (_003CAddMarkersOnMap_003Ec__AnonStorey.trigger.TaskObject.Tags.Count <= 0 || _003CAddMarkersOnMap_003Ec__AnonStorey.trigger.TaskObject.SceneID == GameScenes.SceneID.None)
					{
						continue;
					}
					List<IMapMainObject> list = Client.Instance.Map.AllMapObjects.Keys.Where(_003CAddMarkersOnMap_003Ec__AnonStorey._003C_003Em__2).ToList();
					foreach (IMapMainObject item in list)
					{
						MapObject mapObject = Client.Instance.Map.AllMapObjects[item];
						if (mapObject != null && !MapQuestIndicators.ContainsKey(_003CAddMarkersOnMap_003Ec__AnonStorey.trigger))
						{
							QuestIndicatorUI component3 = Object.Instantiate(MapQuestIndicatorPrefab, IndicatorParent).GetComponent<QuestIndicatorUI>();
							component3.TaskName.text = _003CAddMarkersOnMap_003Ec__AnonStorey.trigger.Name;
							Tuple<MapObject, QuestIndicatorUI> value5 = new Tuple<MapObject, QuestIndicatorUI>(mapObject, component3);
							MapQuestIndicators[_003CAddMarkersOnMap_003Ec__AnonStorey.trigger] = value5;
						}
					}
				}
			}
		}
	}

	public void RemoveMarkersOnMap()
	{
		foreach (QuestTrigger key in MapQuestIndicators.Keys)
		{
			Object.Destroy(MapQuestIndicators[key].Item2.gameObject);
		}
		MapQuestIndicators.Clear();
	}

	public void HideMarkerOnMap(QuestTrigger trigger)
	{
		if (MapQuestIndicators.ContainsKey(trigger))
		{
			MapQuestIndicators[trigger].Item2.Animator.SetTrigger("Stop");
		}
	}

	private void Update()
	{
		if (SceneQuestTriggers.Count > 0 && MyPlayer.Instance.IsAlive && MyPlayer.Instance.FpsController.MainCamera != null)
		{
			foreach (SceneQuestTrigger key in SceneQuestTriggers.Keys)
			{
				Vector3 vector = MyPlayer.Instance.FpsController.MainCamera.WorldToScreenPoint(key.transform.TransformPoint(key.QuestIndicatorPosition));
				if (key.Visibility == QuestIndicatorVisibility.Proximity)
				{
					if (Vector3.Distance(MyPlayer.Instance.FpsController.MainCamera.transform.position, key.transform.position) < key.ProximityDistance)
					{
						if (!SceneQuestTriggers[key].gameObject.activeInHierarchy)
						{
							SceneQuestTriggers[key].gameObject.SetActive(true);
							SceneQuestTriggers[key].Animator.SetBool("Hide", false);
						}
					}
					else if (SceneQuestTriggers[key].gameObject.activeInHierarchy)
					{
						SceneQuestTriggers[key].Animator.SetBool("Hide", true);
					}
				}
				Vector2 localPoint = Vector2.zero;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(IndicatorParent, vector, Client.Instance.CanvasManager.Canvas.worldCamera, out localPoint);
				bool flag = false;
				if (vector.z < 0f)
				{
					localPoint = -localPoint.normalized * IndicatorParent.rect.size.magnitude;
					flag = true;
				}
				if (localPoint.x > IndicatorParent.rect.size.x * 0.5f - EdgeOffset)
				{
					localPoint.x = IndicatorParent.rect.size.x * 0.5f - EdgeOffset;
					flag = true;
				}
				if (localPoint.x < (0f - IndicatorParent.rect.size.x) * 0.5f + EdgeOffset)
				{
					localPoint.x = (0f - IndicatorParent.rect.size.x) * 0.5f + EdgeOffset;
					flag = true;
				}
				if (localPoint.y > IndicatorParent.rect.size.y * 0.5f - EdgeOffset)
				{
					localPoint.y = IndicatorParent.rect.size.y * 0.5f - EdgeOffset;
					flag = true;
				}
				if (localPoint.y < (0f - IndicatorParent.rect.size.y) * 0.5f + EdgeOffset)
				{
					localPoint.y = (0f - IndicatorParent.rect.size.y) * 0.5f + EdgeOffset;
					flag = true;
				}
				SceneQuestTriggers[key].SetOffScreen(flag);
				if (flag)
				{
					SceneQuestTriggers[key].Arrow.up = -localPoint.normalized;
				}
				SceneQuestTriggers[key].transform.localPosition = localPoint;
			}
		}
		if (MapQuestIndicators.Count <= 0)
		{
			return;
		}
		foreach (QuestTrigger key2 in MapQuestIndicators.Keys)
		{
			if (!MapQuestIndicators[key2].Item1.gameObject.activeInHierarchy && MapQuestIndicators[key2].Item1.MainObject.RadarVisibilityType == RadarVisibilityType.AlwaysVisible)
			{
				MapQuestIndicators[key2].Item1.gameObject.SetActive(true);
			}
			Vector3 vector2 = Client.Instance.Map.MapCamera.WorldToScreenPoint(MapQuestIndicators[key2].Item1.Position.transform.position);
			Vector2 localPoint2 = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(IndicatorParent, vector2, Client.Instance.CanvasManager.Canvas.worldCamera, out localPoint2);
			bool flag2 = false;
			if (vector2.z < 0f)
			{
				localPoint2 = -localPoint2.normalized * IndicatorParent.rect.size.magnitude;
				flag2 = true;
			}
			if (localPoint2.x > IndicatorParent.rect.size.x * 0.5f - EdgeOffset)
			{
				localPoint2.x = IndicatorParent.rect.size.x * 0.5f - EdgeOffset;
				flag2 = true;
			}
			if (localPoint2.x < (0f - IndicatorParent.rect.size.x) * 0.5f + EdgeOffset)
			{
				localPoint2.x = (0f - IndicatorParent.rect.size.x) * 0.5f + EdgeOffset;
				flag2 = true;
			}
			if (localPoint2.y > IndicatorParent.rect.size.y * 0.5f - EdgeOffset)
			{
				localPoint2.y = IndicatorParent.rect.size.y * 0.5f - EdgeOffset;
				flag2 = true;
			}
			if (localPoint2.y < (0f - IndicatorParent.rect.size.y) * 0.5f + EdgeOffset)
			{
				localPoint2.y = (0f - IndicatorParent.rect.size.y) * 0.5f + EdgeOffset;
				flag2 = true;
			}
			MapQuestIndicators[key2].Item2.SetOffScreen(flag2);
			if (flag2)
			{
				MapQuestIndicators[key2].Item2.Arrow.up = -localPoint2.normalized;
			}
			MapQuestIndicators[key2].Item2.transform.localPosition = localPoint2;
		}
	}
}
