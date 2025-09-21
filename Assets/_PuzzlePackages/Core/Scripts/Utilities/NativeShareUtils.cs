using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NativeShareUtils
{
    public static NativeShare nativeShare = new NativeShare();

    public static void ShareLinkGame()
    {
        nativeShare.Clear();
        nativeShare.SetSubject("Goods Sorting");
        nativeShare.SetText(GetLink());

        nativeShare.Share();
    }

    private static string GetLink()
    {
        return $"Let's play Goods Sorting together! {LinkGame()}";
    }

    private static string LinkGame()
    {
        return "https://goodssorting.onelink.me/voK4/wukvu5k5";
    }

}
