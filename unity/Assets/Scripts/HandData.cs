using System;

[Serializable]
public class Landmark
{
    public float x, y, z;
}

[Serializable]
public class HandPacket
{
    public string type;
    public Landmark[] landmarks;
    public bool is_grabbing;
}