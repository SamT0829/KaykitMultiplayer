using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameCoinPrefab : MonoBehaviour
{
    public CoinInfo CoinInfo;
    [SerializeField] float rotationSpeed;

    private bool canTouch = false;

    private void FixedUpdate()
    {
        CoinRotationAnimation();
    }
    public void Init(CoinInfo coinInfo)
    {
        CoinInfo = coinInfo;
        gameObject.SetActive(coinInfo.active);
        CoinAppearAnimation();
    }
    public void NetworkUpdate(CoinInfo coinInfo)
    {
        CoinInfo = coinInfo;
        gameObject.SetActive(coinInfo.active);
    }
    private void CoinAppearAnimation()
    {
        Sequence coinAppearAnimation = DOTween.Sequence();
        transform.position = Vector3.zero;
        coinAppearAnimation.Append(transform.DOLocalMove(new Vector3(CoinInfo.Position.x / 2f, 1, CoinInfo.Position.z / 2f), 0.5f));
        coinAppearAnimation.Append(transform.DOLocalMove(CoinInfo.Position, 0.5f).SetEase(Ease.InQuad));
        coinAppearAnimation.OnComplete(() => canTouch = true);
    }
    private void CoinRotationAnimation()
    {
        transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.fixedDeltaTime, Vector3.up);
    }
}
