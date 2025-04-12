using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class used to manage and track acknowledgements and determine when they timed out.
/// Only used in <see cref="DataSender"/>
/// </summary>
public class AcknowledgementTime : IEquatable<AcknowledgementTime>
{
    /// <summary>
    /// The signature of the message this object belongs to.
    /// </summary>
    public readonly string   Signature;
    private         float    timeoutSeconds;
    private         DateTime start;
    
    public AcknowledgementTime(string signature, float timeoutSeconds)
    {
        Signature = signature;
        this.timeoutSeconds = timeoutSeconds;
        start = DateTime.Now;
    }
    
    /// <summary>
    /// If timeoutSeconds was smaller than 0, assume an infinite timeout amount, thus a timeout never occurred.
    /// Otherwise, look at the difference between now and the creation of this object and test whether this exceeds the timeout amount.
    /// </summary>
    /// <returns></returns>
    public bool HasTimedOut() => !(timeoutSeconds < 0) && (DateTime.Now - start).TotalMilliseconds / 1000f > timeoutSeconds;
    
    public bool Equals(AcknowledgementTime other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }
        
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        
        return Signature == other.Signature && timeoutSeconds.Equals(other.timeoutSeconds) && start.Equals(other.start);
    }
    
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        
        if (obj.GetType() != this.GetType())
        {
            return false;
        }
        
        AcknowledgementTime o = (AcknowledgementTime)obj;
        return o.Signature == Signature && o.timeoutSeconds == timeoutSeconds && o.start == start;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Signature, timeoutSeconds, start);
    }
}
