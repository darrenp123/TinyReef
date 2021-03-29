using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishStatShop : MonoBehaviour {
    [SerializeField]
    Player User;

    public void IncreaseLifespan() {
        User.PurchasePoints(1, 1);
    }

    public void DecreaseLifespan() {
        User.PurchasePoints(1, -1);
    }

    public void IncreaseSize() {
        User.PurchasePoints(2, 1);
    }

    public void DecreaseSize() {
        User.PurchasePoints(2, -1);
    }

    public void IncreaseSpeed() {
        User.PurchasePoints(3, 1);
    }

    public void DecreaseSpeed() {
        User.PurchasePoints(3, -1);
    }

    public void IncreaseSensoryRadious() {
        User.PurchasePoints(4, 1);
    }

    public void DecreaseSensoryRadious() {
        User.PurchasePoints(4, -1);
    }

    public void IncreaseCamouflage() {
        User.PurchasePoints(5, 1);
    }

    public void DecreaseCamouflage() {
        User.PurchasePoints(5, -1);
    }

    public void IncreaseMatingUrge() {
        User.PurchasePoints(6, 1);
    }

    public void DecreaseMatingUrge() {
        User.PurchasePoints(6, -1);
    }

    public void IncreaseGestationPeriod() {
        User.PurchasePoints(7, 1);
    }

    public void DecreaseGestationPeriod() {
        User.PurchasePoints(7, -1);
    }
}
