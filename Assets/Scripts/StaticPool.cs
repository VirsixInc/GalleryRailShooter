using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaticPool : MonoBehaviour {

	public static StaticPool s_instance;
	
	public Dictionary<GameObject, List<GameObject>> objLists;

	const int DEFAULT_SIZE = 10;
	const int SIZE_INCREMENT = 5;

	[System.NonSerialized]
	public GameObject parent;

	#region Singleton Initialization	
	void Awake() {
		if(s_instance == null) {
			//If I am the fist instance, make me the first Singleton
			s_instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			//If a Singleton already exists and you find another reference in scene, destroy it
			if(s_instance != this)
				DestroyImmediate(gameObject);
		}
	}
	
	#endregion

	void Start () {
		objLists = new Dictionary<GameObject, List<GameObject>>();
	}

	/// <summary>
	/// Not neccesary. Preload prefab for the pool.
	/// </summary>
	/// <param name="prefab">Prefab.</param>
	public static void InitObj(GameObject prefab) {
		if(s_instance.objLists.ContainsKey(prefab) == false) {
			GameObject holder = new GameObject(prefab.name);
			//holder.AddComponent<NetworkView>();
			//holder.networkView.viewID = Network.AllocateViewID();
			holder.transform.parent = s_instance.transform;
			//s_instance.networkView.RPC( "NetworkInitObj", RPCMode.OthersBuffered, holder.name, holder.networkView.viewID );

			s_instance.objLists.Add (prefab, new List<GameObject>());
			AddToList(prefab, DEFAULT_SIZE, holder.transform);
		}
	}

	/// <summary>
	/// Gets the object of type prefab.
	/// </summary>
	/// <returns>The object.</returns>
	/// <param name="prefab">Prefab.</param>
	public static GameObject GetObj(GameObject prefab) {
		InitObj(prefab);

		// Find inactive in scene
		for(int i = 0, n = s_instance.objLists[prefab].Count; i < n; i++) {
			if(s_instance.objLists[prefab][i].GetComponent<StaticPoolActive>().m_activeInScene == false) {
				s_instance.objLists[prefab][i].GetComponent<StaticPoolActive>().m_activeInScene = true;
				return s_instance.objLists[prefab][i];
			}
		}

		// None found
		AddToList(prefab, SIZE_INCREMENT, s_instance.transform.FindChild(prefab.name));

		// Zero indexed and minus what we just added
		GameObject newObj = s_instance.objLists[prefab][s_instance.objLists[prefab].Count - SIZE_INCREMENT];
		newObj.GetComponent<StaticPoolActive>().m_activeInScene = true;
		return newObj;

	}

	static void AddToList(GameObject prefab, int count, Transform holder) {
		for(int i = 0; i < count; i++) {
			GameObject obj = (GameObject)Network.Instantiate(prefab, Vector3.one*1000f, Quaternion.identity, 0 );
			s_instance.objLists[prefab].Add(obj);
			obj.transform.parent = holder;

			//s_instance.networkView.RPC( "NetworkAddToList", RPCMode.OthersBuffered, holder.networkView.viewID, obj.networkView.viewID );
		}
	}

	[RPC]
	public void Reset() {
		Transform[] objs = GetComponentsInChildren<Transform>();
		foreach( Transform obj in objs )
			obj.SendMessage( "StaticReset", SendMessageOptions.DontRequireReceiver );
	}

	[RPC]
	private void NetworkInitObj( string holderName, NetworkViewID holderID ) {
		GameObject holder = new GameObject( holderName );
		holder.AddComponent<NetworkView>();
		holder.networkView.viewID = holderID;
		holder.transform.parent = s_instance.transform;
	}

	[RPC]
	private void NetworkAddToList( NetworkViewID holderID, NetworkViewID objID ) {
		Transform holder = NetworkView.Find( holderID ).observed.transform;
		GameObject obj = NetworkView.Find( objID ).observed.gameObject;

		obj.transform.parent = holder;
	}
}