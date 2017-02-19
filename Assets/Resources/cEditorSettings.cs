using UnityEngine;


[ExecuteInEditMode]
public class cEditorSettings : MonoBehaviour {
	public float snapValue = 0.32f;
	public float depth = 0;    

	void Update() {
		float snapInverse = 1/snapValue;

		float x, y, z;

		// if snapValue = .5, x = 1.45 -> snapInverse = 2 -> x*2 => 2.90 -> round 2.90 => 3 -> 3/2 => 1.5
		// so 1.45 to nearest .5 is 1.5
		x = Mathf.Round(transform.position.x * snapInverse) * snapValue;
		y = Mathf.Round(transform.position.y * snapInverse) * snapValue;   
		z = depth;  // depth from camera

		transform.position = new Vector3(x, y, z);
	}
}

 
//public class SortChildren : ScriptableObject
//{

//    [MenuItem("GameObject/Sort Children")]

//    static void MenuAddChild()
//    {
//        Sort(Selection.activeTransform);
//    }

//    static void Sort(Transform current)
//    {
//        foreach (Transform child in current)
//            Sort(child);
//        current.parent = current.parent;
//    }
//}