using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Attribute {
    readonly float _value;
    readonly float _min;
    readonly float _max;

    public Attribute(float value, float min=float.NegativeInfinity, float max = float.PositiveInfinity) {
        _value = value > min? value < max? value: max: min;
        _min = min;
        _max = max;
    }
    public static implicit operator float(Attribute a) {
        return a._value;
    }
    public static Attribute operator +(Attribute a, float b) => new(a._value + b, a._min, a._max);
    public static Attribute operator +(float a, Attribute b) => new(b._value+a, b._min, b._max);
    public static Attribute operator +(Attribute a, Attribute b) {
        if (a._min != b._min || a._max != b._max) throw new AttributeMismatchException();
        return new(a._value+b._value, a._min, a._max);
    }
    public static Attribute operator -(Attribute a, float b) => new(a._value - b, a._min, a._max);
    public static Attribute operator -(float a, Attribute b) => new(a - b._value, b._min, b._max);
    public static Attribute operator -(Attribute a, Attribute b) {
        if (a._min != b._min || a._max != b._max) throw new AttributeMismatchException();
        return new(a._value - b._value, a._min, a._max);
    }
    public static Attribute operator *(Attribute a, float b) => new(a._value * b, a._min, a._max);
    public static Attribute operator *(float a, Attribute b) => new(b._value * a, b._min, b._max);
    public static Attribute operator *(Attribute a, Attribute b) {
        if (a._min != b._min || a._max != b._max) throw new AttributeMismatchException();
        return new(a._value * b._value, a._min, a._max);
    }
    public static Attribute operator /(Attribute a, float b) => new(a._value / b, a._min, a._max);
    public static Attribute operator /(float a, Attribute b) => new(b._value / a, b._min, b._max);
    public static Attribute operator /(Attribute a, Attribute b) {
        if (a._min != b._min || a._max != b._max) throw new AttributeMismatchException();
        return new(a._value / b._value, a._min, a._max);
    }

    public static bool operator ==(Attribute a, Attribute b) => a._value == b._value;
    public static bool operator !=(Attribute a, Attribute b) => a._value != b._value;

    public override bool Equals(object obj) {
        if (obj is float f)
            return _value == f;
        if (obj is Attribute a) return _value == a._value;
        return false;
    }
    public override int GetHashCode() {
        return _value.GetHashCode();
    }
    public override string ToString() {
        return _value.ToString();
    }


    public static void Driver() {
        Attribute a = new(666);
        Attribute b = new(333);
        Attribute c = a + b;
        if (a != c) {
            
        }
    }

}

public class AttributeMismatchException: System.Exception {
    public AttributeMismatchException(): base("The two attributes' min and max are not the same") {

    }
}
