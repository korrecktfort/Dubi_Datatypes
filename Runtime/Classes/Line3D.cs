using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line3D
{
    Vector3 forward, right, up;
    Vector3 p0, p1;
    float length, lengthSqr;

    Line3D previous, next;

    public Line3D(Vector3 p0, Vector3 p1)
    {
        this.p0 = p0;
        this.p1 = p1;

        this.forward = (p1 - p0).normalized;

        if(this.forward == Vector3.up)        
            this.right = Vector3.right;        
        else        
            this.right = Vector3.Cross(this.forward, Vector3.up).normalized;

        this.up = Vector3.Cross(this.right, this.forward).normalized;

        this.length = Vector3.Distance(p0, p1);
        this.lengthSqr = this.length * this.length;
    }
}
