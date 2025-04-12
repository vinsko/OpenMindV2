using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public bool prologueSeen;       // becomes true when player has read the prologue
    public bool playedBefore;       // becomes true when player has played game before, regardless of win/loss, but must have finished the game.
    
    // Stories that have been beaten, which unlock the other stories.
    public bool storyAWon;          
    public bool storyBWon;
    public bool storyCWon;
    
    // Intros that have been seen, gives option to skip when true
    public bool storyAIntroSeen;
    public bool storyBIntroSeen;
    public bool storyCIntroSeen;
}
