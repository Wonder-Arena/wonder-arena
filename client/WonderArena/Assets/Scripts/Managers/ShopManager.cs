using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    GameObject confirmationWindow;
    [SerializeField]
    TextMeshProUGUI waitingText;

    public string idOfBeast;

    private void Start()
    {
        StartCoroutine(GetBeastsForListing());
    }

    private IEnumerator GetBeastsForListing()
    {
        while (true)
        {
            CoroutineHelper.Instance.RunCoroutine("GetListingBeasts",
                FlowInterfaceBB.Instance.GetListingBeasts());
            yield return new WaitForSeconds(10f);
        }
    }

    public void OnBuyClicked()
    {
        Debug.Log(idOfBeast);
        confirmationWindow.SetActive(true);
    }

    public void OnYesClicked()
    {
        StartCoroutine(WaitForStripe());
    }

    public void OnNoClicked()
    {
        confirmationWindow.SetActive(false);
    }

    private IEnumerator WaitForStripe()
    {
        yield return (NetworkManager.Instance.GetStripeCheckout(idOfBeast));
        Application.OpenURL(NetworkManager.Instance.responseStripeURL);
    }
}
