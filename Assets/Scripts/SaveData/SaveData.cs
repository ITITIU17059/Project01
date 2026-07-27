using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int stageIndex;
    public int bossIndex;

    public List<string> bossSequence = new();

    public List<string> handCards = new();
    public List<string> deckCards = new();
    public List<string> graveyardCards = new();

    public List<string> ownedRewards = new();
    public List<string> equippedRewards = new();

}