using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public SUIT MySuit { get; set; }
    public VALUE MyValue { get; set; }
   
}

public enum SUIT { H, S, D, C }
public enum VALUE { V2 = 2, V3, V4, V5, V6, V7, V8, V9, V10, VJ, VQ, VK, VA}
