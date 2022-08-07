using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelTransaction : MonoBehaviour {

	public GameObject cancelTransaction;

	void OnEnable()
	{
		StartCoroutine (cancelTransactionTookToLong());
	}

	IEnumerator cancelTransactionTookToLong()
	{
		yield return new WaitForSeconds (8f);
		cancelTransaction.SetActive (true);
	}
    
}
