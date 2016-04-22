using UnityEngine;

public class Campfire : MonoBehaviour 
{		
	[SerializeField] private bool _lit;

	void Start () 
	{
		GetComponent<Animator>().SetBool("Lit", _lit);
	}
}
